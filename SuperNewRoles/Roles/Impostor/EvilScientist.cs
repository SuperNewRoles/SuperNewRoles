using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.CustomOptions;
using UnityEngine;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Impostor;

internal class EvilScientist : RoleBase<EvilScientist>
{
    public override RoleId Role => RoleId.EvilScientist;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.ImpostorTeam];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Scientist;
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("EvilScientistCoolTime", 2.5f, 180f, 2.5f, 30f, translationName: "CoolTime", suffix: "Seconds")]
    public static float EvilScientistCoolTime;
    [CustomOptionFloat("EvilScientistDurationTime", 0.5f, 60f, 0.5f, 10f, translationName: "DurationTime", suffix: "Seconds")]
    public static float EvilScientistDurationTime;
    [CustomOptionBool("EvilScientistCanTheLighterSeeTheScientist", true, translationName: "ScientistCanTheLighterSeeTheScientist")]
    public static bool EvilScientistCanTheLighterSeeTheScientist;

    public override List<Func<AbilityBase>> Abilities => [
        () => new InvisibleAbility(
            EvilScientistCoolTime,
            EvilScientistDurationTime,
            EvilScientistCanTheLighterSeeTheScientist,
            "EvilScientistButton.png"
        )
    ];
}