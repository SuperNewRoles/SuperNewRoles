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

    public override QuoteMod QuoteMod => QuoteMod.NebulaOnTheShip;

    public override List<AssignedTeamType> AssignedTeams => [];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(RoleColor, "⊕");
    public override bool AssignFilter => true;

    public override RoleId[] DoNotAssignRoles => [RoleId.God, RoleId.NiceGuesser, RoleId.EvilGuesser, RoleId.Balancer];

    public override bool UseTeamSpecificAssignment => true;

    [CustomOptionInt("ModifierGuesserMaxShots", 1, 15, 1, 3, translationName: "EvilGuesserMaxShots")]
    public static int ModifierGuesserMaxShots = 3;

    [CustomOptionInt("ModifierGuesserShotsPerMeeting", 1, 15, 1, 3, translationName: "EvilGuesserShotsPerMeeting")]
    public static int ModifierGuesserShotsPerMeeting = 3;

    [CustomOptionBool("ModifierGuesserCannotShootStar", true, translationName: "EvilGuesserCannotShootStar")]
    public static bool ModifierGuesserCannotShootStar = true;

    [CustomOptionBool("ModifierGuesserLimitedTurns", true, translationName: "EvilGuesserLimitedTurns")]
    public static bool ModifierGuesserLimitedTurns = true;

    [CustomOptionInt("ModifierGuesserLimitedTurnsCount", 1, 15, 1, 3, parentFieldName: nameof(ModifierGuesserLimitedTurns), translationName: "EvilGuesserLimitedTurnsCount")]
    public static int ModifierGuesserLimitedTurnsCount = 3;

    [CustomOptionBool("ModifierGuesserCannotShootCrewmate", true, translationName: "EvilGuesserCannotShootCrewmate")]
    public static bool ModifierGuesserCannotShootCrewmate = true;

    [CustomOptionBool("ModifierGuesserMadmateSuicide", true, translationName: "ModifierGuesserMadmateSuicide")]
    public static bool ModifierGuesserMadmateSuicide = true;
}
