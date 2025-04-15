using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;

class ModifierGuesser : ModifierBase<ModifierGuesser>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.ModifierGuesser;

    public override Color32 RoleColor => Crewmate.NiceGuesser.Instance.RoleColor;

    public override List<Func<AbilityBase>> Abilities => [() => new GuesserAbility(2, 2, true, true)];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override TeamTag TeamTag => TeamTag.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override string ModifierMark => "{0}" + ModHelpers.Cs(RoleColor, "⊕");

    [CustomOptionInt("ModifierGuesserMaxShots", 1, 15, 1, 3, translationName: "EvilGuesserMaxShots")]
    public static int ModifierGuesserMaxShots = 3;

    [CustomOptionInt("ModifierGuesserShotsPerMeeting", 1, 15, 1, 3, translationName: "EvilGuesserShotsPerMeeting")]
    public static int ModifierGuesserShotsPerMeeting = 3;

    [CustomOptionBool("ModifierGuesserCannotShootStar", true, translationName: "EvilGuesserCannotShootStar")]
    public static bool ModifierGuesserCannotShootStar = true;

    [CustomOptionBool("ModifierGuesserLimitedTurns", true, parentFieldName: nameof(ModifierGuesserCannotShootStar), translationName: "EvilGuesserLimitedTurns")]
    public static bool ModifierGuesserLimitedTurns = true;

    [CustomOptionInt("ModifierGuesserLimitedTurnsCount", 1, 15, 1, 3, parentFieldName: nameof(ModifierGuesserLimitedTurns), translationName: "EvilGuesserLimitedTurnsCount")]
    public static int ModifierGuesserLimitedTurnsCount = 3;

    [CustomOptionBool("ModifierGuesserCannotShootCrewmate", true, translationName: "EvilGuesserCannotShootCrewmate")]
    public static bool ModifierGuesserCannotShootCrewmate = true;
}
