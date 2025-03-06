using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Workperson : RoleBase<Workperson>
{
    [CustomOptionBool("WorkpersonNeedAliveToWin", false)]
    public static bool WorkpersonNeedAliveToWin;
    [CustomOptionBool("WorkpersonUseCustomTaskSetting", true)]
    public static bool WorkpersonUseCustomTaskSetting;
    [CustomOptionTask("WorkpersonTask", 8, 8, 8, parentFieldName: nameof(WorkpersonUseCustomTaskSetting))]
    public static TaskOptionData WorkpersonTaskData;

    public override RoleId Role { get; } = RoleId.Workperson;
    public override Color32 RoleColor { get; } = new(210, 180, 140, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new CustomTaskAbility(
        () => {
            var exPlayer = ExPlayerControl.LocalPlayer;
            if (exPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId) return (false, 0);
            var taskData = ModHelpers.TaskCompletedData(exPlayer.Data);
            if (taskData.completed == -1 || taskData.total == -1) return (false, 0);

            return (true, taskData.total);
        },
        WorkpersonTaskData
    ),
    () => new WorkpersonAbility(WorkpersonNeedAliveToWin)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];
}
public class WorkpersonAbility : AbilityBase
{
    private EventListener<TaskCompleteEventData> _taskCompleteListener;
    private bool _needAliveToWin { get; }
    public WorkpersonAbility(bool needAliveToWin)
    {
        _needAliveToWin = needAliveToWin;
    }
    public override void AttachToLocalPlayer()
    {
        _taskCompleteListener = TaskCompleteEvent.Instance.AddListener(OnTaskComplete);
    }
    private void OnTaskComplete(TaskCompleteEventData data)
    {
        if (data.player.PlayerId != PlayerControl.LocalPlayer.PlayerId) return;
        if (ExPlayerControl.LocalPlayer.IsTaskComplete())
        {
            if (_needAliveToWin && !ExPlayerControl.LocalPlayer.IsAlive()) return;
            new LateTask(() => CustomRpcExts.RpcEndGameForHost((GameOverReason)CustomGameOverReason.WorkpersonWin), 0.2f);
        }
    }
    public override void DetachToLocalPlayer()
    {
        TaskCompleteEvent.Instance.RemoveListener(_taskCompleteListener);
    }
}