using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Impostor;

class EvilGuesser : RoleBase<EvilGuesser>
{
    public override RoleId Role { get; } = RoleId.EvilGuesser;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new GuesserAbility(EvilGuesserMaxShots, EvilGuesserShotsPerMeeting, EvilGuesserCannotShootCrewmate, EvilGuesserCannotShootStar, EvilGuesserLimitedTurns, EvilGuesserLimitedTurnsCount)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionInt("EvilGuesserMaxShots", 1, 15, 1, 3)]
    public static int EvilGuesserMaxShots = 3;

    [CustomOptionInt("EvilGuesserShotsPerMeeting", 1, 15, 1, 3)]
    public static int EvilGuesserShotsPerMeeting = 3;

    [CustomOptionBool("EvilGuesserCannotShootStar", true)]
    public static bool EvilGuesserCannotShootStar = true;

    [CustomOptionBool("EvilGuesserLimitedTurns", true, parentFieldName: nameof(EvilGuesserCannotShootStar))]
    public static bool EvilGuesserLimitedTurns = true;

    [CustomOptionInt("EvilGuesserLimitedTurnsCount", 1, 15, 1, 3, parentFieldName: nameof(EvilGuesserLimitedTurns))]
    public static int EvilGuesserLimitedTurnsCount = 3;

    [CustomOptionBool("EvilGuesserCannotShootCrewmate", true)]
    public static bool EvilGuesserCannotShootCrewmate = true;
}
