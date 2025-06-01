using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Teruteru : RoleBase<Teruteru>
{
    public override RoleId Role { get; } = RoleId.Teruteru;
    public override Color32 RoleColor { get; } = new Color32(255, 165, 0, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new TeruteruAbility(new TeruteruData(
            canUseVent: TeruteruCanUseVent,
            requireTaskCompletion: TeruteruRequireTaskCompletion,
            requiredTaskCount: TeruteruRequiredTaskCount,
            customTaskCount: TeruteruCustomTaskCount,
            teruteruTaskOption: TeruteruTaskOption
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    [CustomOptionBool("TeruteruCanUseVent", false, translationName: "CanUseVent")]
    public static bool TeruteruCanUseVent;

    [CustomOptionBool("TeruteruRequireTaskCompletion", false)]
    public static bool TeruteruRequireTaskCompletion;

    [CustomOptionInt("TeruteruRequiredTaskCount", 1, 15, 1, 3, parentFieldName: nameof(TeruteruRequireTaskCompletion))]
    public static int TeruteruRequiredTaskCount;

    [CustomOptionBool("TeruteruCustomTaskCount", false, parentFieldName: nameof(TeruteruRequireTaskCompletion))]
    public static bool TeruteruCustomTaskCount;

    [CustomOptionTask("TeruteruTaskOption", 1, 1, 1, parentFieldName: nameof(TeruteruCustomTaskCount))]
    public static TaskOptionData TeruteruTaskOption;
}

public class TeruteruAbility : AbilityBase
{
    private readonly TeruteruData _data;
    private EventListener<ExileEventData> _playerExiledListener;
    private CustomVentAbility _ventAbility;
    private CustomTaskAbility _customTaskAbility;

    public TeruteruAbility(TeruteruData data)
    {
        _data = data;
    }

    public override void AttachToAlls()
    {
        // ベント使用可能設定
        _ventAbility = new CustomVentAbility(() => _data.CanUseVent);
        _customTaskAbility = new CustomTaskAbility(() => (_data.RequireTaskCompletion, false, _data.RequiredTaskCount), _data.CustomTaskCount ? _data.TeruteruTaskOption : null);

        ExPlayerControl exPlayer = Player;
        exPlayer.AttachAbility(_ventAbility, new AbilityParentAbility(this));
        exPlayer.AttachAbility(_customTaskAbility, new AbilityParentAbility(this));

        // イベントリスナーを設定
        _playerExiledListener = ExileEvent.Instance.AddListener(OnPlayerExiled);
    }

    private void OnPlayerExiled(ExileEventData data)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        // プレイヤーが追放された時の処理
        if (data.exiled == null || data.exiled.PlayerId != Player.PlayerId)
            return;
        // タスク完了が必要な設定がオンの場合
        if (_data.RequireTaskCompletion)
        {
            // タスク数をチェック
            var (tasksCompleted, tasksTotal) = ModHelpers.TaskCompletedData(Player.Data);

            // 設定された必要タスク数を使用
            if (tasksCompleted < _data.RequiredTaskCount)
            {
                // タスク不足で勝利条件を満たさない
                return;
            }
        }

        // 勝利条件を満たした場合、ゲーム終了
        EndGamer.RpcEndGameWithWinner(CustomGameOverReason.TeruteruWin, WinType.SingleNeutral, [Player], Teruteru.Instance.RoleColor, "Teruteru");

    }

    public override void DetachToAlls()
    {
        // イベントリスナーを削除
        _playerExiledListener?.RemoveListener();
    }
}

public class TeruteruData
{
    public bool CanUseVent { get; }
    public bool RequireTaskCompletion { get; }
    public int RequiredTaskCount { get; }
    public bool CustomTaskCount { get; }
    public TaskOptionData TeruteruTaskOption { get; }

    public TeruteruData(bool canUseVent, bool requireTaskCompletion, int requiredTaskCount, bool customTaskCount, TaskOptionData teruteruTaskOption)
    {
        CanUseVent = canUseVent;
        RequireTaskCompletion = requireTaskCompletion;
        RequiredTaskCount = requiredTaskCount;
        CustomTaskCount = customTaskCount;
        TeruteruTaskOption = teruteruTaskOption;
    }
}