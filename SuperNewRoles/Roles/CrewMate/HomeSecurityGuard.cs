using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.CrewMate;

class HomeSecurityGuard : RoleBase<HomeSecurityGuard>
{
    public override RoleId Role { get; } = RoleId.HomeSecurityGuard;
    public override Color32 RoleColor { get; } = new(0, 255, 0, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new CustomTaskAbility(
        () => {
            var exPlayer = ExPlayerControl.LocalPlayer;
            return (false, null, 0);
        },
        new TaskOptionData(1, 0, 0)
    )];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}