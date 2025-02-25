using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Crewmate;

class NiceGuesser : RoleBase<NiceGuesser>
{
    public override RoleId Role { get; } = RoleId.NiceGuesser;
    public override Color32 RoleColor { get; } = Color.yellow;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new GuesserAbility(NiceGuesserMaxShots, NiceGuesserShotsPerMeeting, NiceGuesserCannotShootCrewmate, false, NiceGuesserCannotShootStar)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionInt("NiceGuesserMaxShots", 1, 15, 1, 3)]
    public static int NiceGuesserMaxShots;

    [CustomOptionInt("NiceGuesserShotsPerMeeting", 1, 15, 1, 3)]
    public static int NiceGuesserShotsPerMeeting;

    [CustomOptionBool("NiceGuesserCannotShootCrewmate", true)]
    public static bool NiceGuesserCannotShootCrewmate = true;

    [CustomOptionBool("NiceGuesserCannotShootStar", true)]
    public static bool NiceGuesserCannotShootStar = true;
}
