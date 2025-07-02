using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

/// <summary>
/// 泥棒ロール - キルしたクルーメイトのタスク進捗を巻き戻す
/// </summary>
internal class Robber : RoleBase<Robber>
{
    public override RoleId Role => RoleId.Robber;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new() { () => new RobberAbility(RewindTaskCount) };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionInt("RobberRewindTaskCount", 1, 15, 1, 2, translationName: "RobberRewindTaskCount")]
    public static int RewindTaskCount;
}

/// <summary>
/// 泥棒の能力 - キル時にタスク進捗を巻き戻す
/// </summary>
public class RobberAbility : AbilityBase
{
    private int _rewindTaskCount;
    private EventListener<MurderEventData> _murderListener;

    public RobberAbility(int rewindTaskCount)
    {
        _rewindTaskCount = rewindTaskCount;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _murderListener?.RemoveListener();
    }

    private void OnMurder(MurderEventData data)
    {
        // 自分がキルした場合のみ処理
        if (data.killer != Player) return;
        if (!data.killer.AmOwner) return;

        // ローカルでどのタスクを巻き戻すかを決定
        var taskIdsToRewind = GetTasksToRewind(data.target, _rewindTaskCount);
        if (taskIdsToRewind.Count > 0)
        {
            // RPCで全プレイヤーに巻き戻すタスクIDを送信
            RpcRewindTasks(data.target, taskIdsToRewind.ToArray());
        }
    }

    /// <summary>
    /// 巻き戻すタスクのIDを決定する（ホストでのみ実行）
    /// </summary>
    private List<uint> GetTasksToRewind(ExPlayerControl target, int rewindCount)
    {
        if (target == null || target.Data == null) return new List<uint>();

        // 完了したタスクのリストを取得
        var completedTasks = target.Data.Tasks.ToArray()
            .Where(task => task.Complete)
            .ToList();

        if (completedTasks.Count == 0) return new List<uint>();

        // 巻き戻すタスク数を制限
        int actualRewindCount = Math.Min(rewindCount, completedTasks.Count);
        var taskList = completedTasks.ToList();
        for (int i = taskList.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = taskList[i];
            taskList[i] = taskList[j];
            taskList[j] = temp;
        }
        return taskList.Take(actualRewindCount).Select(task => task.Id).ToList();
    }

    [CustomRPC]
    public void RpcRewindTasks(ExPlayerControl target, uint[] taskIds)
    {
        if (target == null || target.Data == null || taskIds == null) return;

        foreach (uint taskId in taskIds)
        {
            // タスク情報を取得
            var taskInfo = target.Data.Tasks.ToArray()
                .FirstOrDefault(task => task.Id == taskId);

            if (taskInfo != null)
            {
                // タスクを未完了状態に戻す
                taskInfo.Complete = false;

                // プレイヤーのタスクオブジェクトも更新
                var playerTask = target.Player.myTasks.ToArray()
                    .FirstOrDefault(task => task.Id == taskId);

                if (playerTask != null)
                {
                    if (playerTask.TryCast<NormalPlayerTask>() != null)
                    {
                        var normalTask = playerTask.TryCast<NormalPlayerTask>();
                        normalTask.taskStep = 0;
                    }
                }
            }
        }
    }
}