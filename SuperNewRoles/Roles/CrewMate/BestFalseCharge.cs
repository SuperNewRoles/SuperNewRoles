using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.CrewMate;

class BestFalseCharge : RoleBase<BestFalseCharge>
{
    public override RoleId Role { get; } = RoleId.BestFalseCharge;
    public override Color32 RoleColor { get; } = Color.white;
    public override List<Type> Abilities { get; } = [typeof(AutoExileAfterMeeting)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

class AutoExileAfterMeeting : AbilityBase
{
    public override void AttachToLocalPlayer()
    {
        WrapUpEvent.AddWrapUpListener(OnWrapUp);
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        PlayerControl.LocalPlayer.RpcExiledCustom();
    }
    public override void Detach()
    {
        base.Detach();
    }
}
