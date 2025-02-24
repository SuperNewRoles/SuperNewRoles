using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.CrewMate;

class BestFalseCharge : RoleBase<BestFalseCharge>
{
    public override RoleId Role { get; } = RoleId.BestFalseCharge;
    public override Color32 RoleColor { get; } = Color.white;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new AutoExileAfterMeeting()];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

public class AutoExileAfterMeeting : AbilityBase
{
    public EventListener<WrapUpEventData> wrapUpEventListener;
    public override void AttachToLocalPlayer()
    {
        wrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        PlayerControl.LocalPlayer.RpcExiledCustom();
        if (wrapUpEventListener != null)
        {
            WrapUpEvent.Instance.RemoveListener(wrapUpEventListener);
            wrapUpEventListener = null;
        }
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (wrapUpEventListener != null)
        {
            WrapUpEvent.Instance.RemoveListener(wrapUpEventListener);
            wrapUpEventListener = null;
        }
    }
}
