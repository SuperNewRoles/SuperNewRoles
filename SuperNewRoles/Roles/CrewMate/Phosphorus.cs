using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

// うるとらあつさん
internal class Phosphorus : RoleBase<Phosphorus>
{
    public override RoleId Role => RoleId.Phosphorus;
    public override Color32 RoleColor => new(249, 188, 81, byte.MaxValue);
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override QuoteMod QuoteMod => QuoteMod.NebulaOnTheShip;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("PhosphorusCoolTime", 2.5f, 60f, 2.5f, 30f, translationName: "CoolTime", suffix: "Seconds")]
    public static float PhosphorusCoolTime;
    [CustomOptionInt("PhosphorusPuttingLimit", 1, 10, 1, 1)]
    public static int PhosphorusPuttingLimit;
    [CustomOptionFloat("PhosphorusLightingCooltime", 2.5f, 60f, 2.5f, 30f, suffix: "Seconds")]
    public static float PhosphorusLightingCooltime;
    [CustomOptionFloat("PhosphorusDurationTime", 2.5f, 120f, 2.5f, 10f, translationName: "DurationTime", suffix: "Seconds")]
    public static float PhosphorusDurationTime;
    [CustomOptionFloat("PhosphorusLightRange", 0.1f, 5f, 0.1f, 0.5f)]
    public static float PhosphorusLightRange;

    public override List<Func<AbilityBase>> Abilities => [
        () => new PhosphorusPutAbility(
            PhosphorusCoolTime,
            PhosphorusPuttingLimit
        ),
        () => new PhosphorusLightingAbility(
            PhosphorusLightingCooltime,
            PhosphorusDurationTime,
            PhosphorusLightRange
        )
    ];
}