using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ghost;

class Cantera : GhostRoleBase<Cantera>
{
    public override GhostRoleId Role => GhostRoleId.Cantera;

    public override Color32 RoleColor => new(255, 237, 194, byte.MaxValue);

    public override List<Func<AbilityBase>> Abilities => [];//[() => new CanteraLightAbility()];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override TeamTag TeamTag => TeamTag.Crewmate;

    public override RoleTag[] RoleTags => [RoleTag.GhostRole];

    public override short IntroNum => 1;

    [CustomOptionFloat("CanteraCooldown", 2.5f, 60f, 2.5f, 15f, translationName: "CoolTime")]
    public static float CanteraCooldown;

    [CustomOptionFloat("CanteraDuration", 2.5f, 60f, 2.5f, 5f, translationName: "DurationTime")]
    public static float CanteraDuration;

    [CustomOptionFloat("CanteraChangedVision", 0.25f, 3f, 0.25f, 1.5f)]
    public static float CanteraChangedVision;

    [CustomOptionBool("CanteraIsLimitUses", false)]
    public static bool CanteraIsLimitUses;

    [CustomOptionInt("CanteraMaxUses", 1, 15, 1, 3, parentFieldName: nameof(CanteraIsLimitUses))]
    public static int CanteraMaxUses;
}
