using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

internal class NiceScientist : RoleBase<NiceScientist>
{
    public override RoleId Role => RoleId.NiceScientist;
    public override Color32 RoleColor => Palette.CrewmateBlue;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Scientist;
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("NiceScientistCoolTime", 2.5f, 60f, 2.5f, 30f, translationName: "CoolTime", suffix: "Seconds")]
    public static float NiceScientistCoolTime;
    [CustomOptionFloat("NiceScientistDurationTime", 2.5f, 20f, 2.5f, 10f, translationName: "DurationTime", suffix: "Seconds")]
    public static float NiceScientistDurationTime;
    [CustomOptionBool("NiceScientistCanTheLighterSeeTheScientist", true, translationName: "ScientistCanTheLighterSeeTheScientist")]
    public static bool NiceScientistCanTheLighterSeeTheScientist;

    public override List<Func<AbilityBase>> Abilities => [
        () => new InvisibleAbility(
            NiceScientistCoolTime,
            NiceScientistDurationTime,
            NiceScientistCanTheLighterSeeTheScientist,
            "NiceScientistButton.png"
        )
    ];
}