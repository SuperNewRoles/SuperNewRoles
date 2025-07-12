using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using UnityEngine;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.Roles.Ghost;

class GhostMechanic : GhostRoleBase<GhostMechanic>
{
    public override GhostRoleId Role { get; } = GhostRoleId.GhostMechanic;
    public override Color32 RoleColor { get; } = new(138, 142, 153, 255);
    public override List<Func<AbilityBase>> Abilities { get; } = new List<Func<AbilityBase>>
    {
        () => new RepairSabotageAbility(GhostMechanicCoolTime, true, GhostMechanicCannotFixMushroomMixup)
    };

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = { RoleTag.GhostRole };

    [CustomOptionFloat("GhostMechanicCoolTime", 2.5f, 60f, 2.5f, 30f, "CoolTime")]
    public static float GhostMechanicCoolTime;
    [CustomOptionBool("GhostMechanicCannotFixMushroomMixup", true)]
    public static bool GhostMechanicCannotFixMushroomMixup;
}