using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.CrewMate;

class Madmate : RoleBase<Madmate>
{
    public override RoleId Role { get; } = RoleId.Madmate;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new MadmateAbility(new(MadmateHasImpostorVision, MadmateCouldUseVent, MadmateCanKnowImpostors, MadmateNeededTaskCount))];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Madmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("MadmateCanKnowImpostors", false, "MadmateCanKnowImpostorsOption")]
    public static bool MadmateCanKnowImpostors;
    [CustomOptionInt("MadmateNeededTaskCount", 0, 30, 1, 6, "MadmateNeededTaskCountOption", parentFieldName: nameof(MadmateCanKnowImpostors))]
    public static int MadmateNeededTaskCount;
    [CustomOptionBool("MadmateCouldUseVent", false, "MadmateCouldUseVentOption")]
    public static bool MadmateCouldUseVent;
    [CustomOptionBool("MadmateHasImpostorVision", false, "MadmateHasImpostorVisionOption")]
    public static bool MadmateHasImpostorVision;
}
