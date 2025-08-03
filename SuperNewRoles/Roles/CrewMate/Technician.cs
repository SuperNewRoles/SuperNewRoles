using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Crewmate;

class Technician : RoleBase<Technician>
{
    public override RoleId Role { get; } = RoleId.Technician;
    public override Color32 RoleColor { get; } = Color.blue;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CustomVentAbility(() => ModHelpers.IsSabotageAvailable(isMushroomMixAsSabotage: TechnicianCanUseVentOnMushroomMixup)),
        () => new ExitVentOnFixSabotageAbility(),
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
    [CustomOptionBool("TechnicianCanUseVentOnMushroomMixup", true)]
    public static bool TechnicianCanUseVentOnMushroomMixup;
}