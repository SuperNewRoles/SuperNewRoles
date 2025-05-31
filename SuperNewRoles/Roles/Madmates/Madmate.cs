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
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new MadmateAbility(
        new(MadmateHasImpostorVision, MadmateCouldUseVent, MadmateCanKnowImpostors, MadmateNeededTaskCount, MadmateIsSpecialTasks ? MadmateSpecialTasks : null))];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRolesGM;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Madmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("MadmateCanKnowImpostors", false)]
    public static bool MadmateCanKnowImpostors;
    [CustomOptionInt("MadmateNeededTaskCount", 0, 30, 1, 6, parentFieldName: nameof(MadmateCanKnowImpostors))]
    public static int MadmateNeededTaskCount;
    [CustomOptionBool("MadmateIsSpecialTasks", false, parentFieldName: nameof(MadmateCanKnowImpostors))]
    public static bool MadmateIsSpecialTasks;
    [CustomOptionTask("MadmateSpecialTasks", 1, 1, 1, parentFieldName: nameof(MadmateIsSpecialTasks))]
    public static TaskOptionData MadmateSpecialTasks;

    [CustomOptionBool("MadmateCouldUseVent", false, translationName: "CanUseVent")]
    public static bool MadmateCouldUseVent;
    [CustomOptionBool("MadmateHasImpostorVision", false, translationName: "HasImpostorVision")]
    public static bool MadmateHasImpostorVision;
}
