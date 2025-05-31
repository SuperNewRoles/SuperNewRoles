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
    public override RoleId Role { get; } = RoleId.Workperson;
    public override Color32 RoleColor { get; } = new(210, 180, 140, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new CustomTaskAbility(
        () => (true, false, WorkpersonTaskData.Total),
        WorkpersonTaskData
    ),
    () => new WorkpersonAbility(WorkpersonNeedAliveToWin),
    () => new CustomVentAbility(
        canUseVent: () => WorkpersonCanUseVent
    )];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    [CustomOptionBool("WorkpersonNeedAliveToWin", false)]
    public static bool WorkpersonNeedAliveToWin;
    [CustomOptionBool("WorkpersonUseCustomTaskSetting", true)]
    public static bool WorkpersonUseCustomTaskSetting;
    [CustomOptionTask("WorkpersonTask", 8, 8, 8, parentFieldName: nameof(WorkpersonUseCustomTaskSetting))]
    public static TaskOptionData WorkpersonTaskData;
    [CustomOptionBool("WorkpersonCanUseVent", false, translationName: "CanUseVent")]
    public static bool WorkpersonCanUseVent;
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
            EndGamer.RpcEndGameWithWinner(CustomGameOverReason.WorkpersonWin, WinType.SingleNeutral, [Player], Workperson.Instance.RoleColor, "Workperson", string.Empty);
        }
    }
    public override void DetachToLocalPlayer()
    {
        TaskCompleteEvent.Instance.RemoveListener(_taskCompleteListener);
    }
}