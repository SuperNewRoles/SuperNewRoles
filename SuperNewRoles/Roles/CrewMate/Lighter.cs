using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

internal class Lighter : RoleBase<Lighter>
{
    public override RoleId Role => RoleId.Lighter;
    public override Color32 RoleColor => new(255, 255, 0, 255); // 黄色
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("LighterCoolTime", 2.5f, 60f, 2.5f, 30f, translationName: "CoolTime", suffix: "Seconds")]
    public static float LighterCoolTime;
    [CustomOptionFloat("LighterDurationTime", 2.5f, 20f, 2.5f, 10f, translationName: "DurationTime", suffix: "Seconds")]
    public static float LighterDurationTime;
    [CustomOptionFloat("LighterRadius", 0.1f, 10f, 0.1f, 2.5f)]
    public static float LighterRadius;

    public override List<Func<AbilityBase>> Abilities => [
        () => new LighterAbility(
            LighterCoolTime,
            LighterDurationTime,
            LighterRadius
        )
    ];
}