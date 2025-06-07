using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

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

    [CustomOptionInt("PhosphorusPuttingLimit", 1, 10, 1, 1, translationName: "PhosphorusPuttingLimit")]
    public static int PuttingLimit;
    [CustomOptionFloat("PhosphorusLightingCooltime", 2.5f, 60f, 2.5f, 30f, translationName: "PhosphorusLightingCooltime", suffix: "Seconds")]
    public static float LightingCooltime;
    [CustomOptionFloat("PhosphorusLightRange", 0.1f, 5f, 0.1f, 0.5f, translationName: "PhosphorusLightRange")]
    public static float LightRange;
    [CustomOptionFloat("PhosphorusCoolTime", 2.5f, 60f, 2.5f, 30f, translationName: "CoolTime", suffix: "Seconds")]
    public static float CoolTime;
    [CustomOptionFloat("PhosphorusDurationTime", 2.5f, 120f, 2.5f, 10f, translationName: "DurationTime", suffix: "Seconds")]
    public static float DurationTime;

    public override List<Func<AbilityBase>> Abilities => [
        () => new PhosphorusPutAbility(
            CoolTime,
            PuttingLimit
        ),
        () => new PhosphorusLightingAbility(
            LightingCooltime,
            DurationTime,
            LightRange
        )
    ];
}