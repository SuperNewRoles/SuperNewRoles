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

    public override List<Func<AbilityBase>> Abilities => [
        () => new GuesserAbility(
            maxShots: ModifierGuesserMaxShots,
            shotsPerMeeting: ModifierGuesserShotsPerMeeting,
            cannotShootCrewmate: ModifierGuesserCannotShootCrewmate,
            cannotShootCelebrity: ModifierGuesserCannotShootStar,
            celebrityLimitedTurns: ModifierGuesserLimitedTurns,
            celebrityLimitedTurnsCount: ModifierGuesserLimitedTurnsCount,
            madmateSuicide: ModifierGuesserMadmateSuicide
        )];

    public override int? PercentageOption => 100;

    public override int? NumberOfCrews => 1;

    public override CustomOption[] Options => ModifierGuesserCategory.Options;

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override bool HiddenOption => true;

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(RoleColor, "⊕");

    public override RoleId[] DoNotAssignRoles => [RoleId.God, RoleId.NiceGuesser, RoleId.EvilGuesser, RoleId.Balancer];

    [Modifier(ModifierRoleId.ModifierGuesser)]
    [AssignFilter([], [RoleId.God, RoleId.NiceGuesser, RoleId.EvilGuesser, RoleId.Balancer])]
    public static CustomOptionCategory ModifierGuesserCategory;

    [CustomOptionInt("ModifierGuesserMaxImpostors", 0, 15, 1, 0, translationName: "ModifierGuesserMaxImpostors", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserMaxImpostors = 0;

    [CustomOptionInt("ModifierGuesserImpostorChance", 5, 100, 5, 100, translationName: "ModifierGuesserImpostorChance", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserImpostorChance = 100;

    [CustomOptionInt("ModifierGuesserMaxNeutrals", 0, 15, 1, 0, translationName: "ModifierGuesserMaxNeutrals", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserMaxNeutrals = 0;

    [CustomOptionInt("ModifierGuesserNeutralChance", 5, 100, 5, 100, translationName: "ModifierGuesserNeutralChance", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserNeutralChance = 100;

    [CustomOptionInt("ModifierGuesserMaxCrewmates", 0, 15, 1, 0, translationName: "ModifierGuesserMaxCrewmates", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserMaxCrewmates = 0;

    [CustomOptionInt("ModifierGuesserCrewmateChance", 5, 100, 5, 100, translationName: "ModifierGuesserCrewmateChance", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserCrewmateChance = 100;

    [CustomOptionInt("ModifierGuesserMaxShots", 1, 15, 1, 3, translationName: "EvilGuesserMaxShots", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserMaxShots = 3;

    [CustomOptionInt("ModifierGuesserShotsPerMeeting", 1, 15, 1, 3, translationName: "EvilGuesserShotsPerMeeting", parentFieldName: nameof(ModifierGuesserCategory))]
    public static int ModifierGuesserShotsPerMeeting = 3;

    [CustomOptionBool("ModifierGuesserCannotShootStar", true, translationName: "EvilGuesserCannotShootStar", parentFieldName: nameof(ModifierGuesserCategory))]
    public static bool ModifierGuesserCannotShootStar = true;

    [CustomOptionBool("ModifierGuesserLimitedTurns", true, parentFieldName: nameof(ModifierGuesserCannotShootStar), translationName: "EvilGuesserLimitedTurns")]
    public static bool ModifierGuesserLimitedTurns = true;

    [CustomOptionInt("ModifierGuesserLimitedTurnsCount", 1, 15, 1, 3, parentFieldName: nameof(ModifierGuesserLimitedTurns), translationName: "EvilGuesserLimitedTurnsCount")]
    public static int ModifierGuesserLimitedTurnsCount = 3;

    [CustomOptionBool("ModifierGuesserCannotShootCrewmate", true, translationName: "EvilGuesserCannotShootCrewmate", parentFieldName: nameof(ModifierGuesserCategory))]
    public static bool ModifierGuesserCannotShootCrewmate = true;

    [CustomOptionBool("ModifierGuesserMadmateSuicide", true, translationName: "ModifierGuesserMadmateSuicide", parentFieldName: nameof(ModifierGuesserCategory))]
    public static bool ModifierGuesserMadmateSuicide = true;
}
