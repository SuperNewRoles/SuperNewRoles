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
    public override Color32 RoleColor { get; } = new(184, 251, 255, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = new List<Func<AbilityBase>>() { () => new GuesserAbility(false, NiceGuesserMaxShots, NiceGuesserShotsPerMeeting, NiceGuesserCannotShootCrewmate, false, NiceGuesserCannotShootStar) };

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("NiceGuesserCanKillNeutral", true)]
    public static bool NiceGuesserCanKillNeutral;

    [CustomOptionInt("NiceGuesserMaxShots", 1, 15, 1, 3)]
    public static int NiceGuesserMaxShots = 3;

    [CustomOptionInt("NiceGuesserShotsPerMeeting", 1, 15, 1, 3)]
    public static int NiceGuesserShotsPerMeeting = 3;

    [CustomOptionBool("NiceGuesserCannotShootCrewmate", true)]
    public static bool NiceGuesserCannotShootCrewmate = true;

    [CustomOptionBool("NiceGuesserCannotShootStar", true)]
    public static bool NiceGuesserCannotShootStar = true;
}
