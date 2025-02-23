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
    public override List<Func<AbilityBase>> Abilities { get; } = [];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Madmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("MadmateCanKnowImpostors", false)]
    public static bool MadmateCanKnowImpostors;
    [CustomOptionTask(
        "MadmateTaskOption",    // id
        1,                      // defaultValue
        0,                      // shortMin
        5,                      // shortMax
        1,                      // shortStep
        1,                      // shortDefault
        0,                      // longMin
        5,                      // longMax
        1,                      // longStep
        1,                      // longDefault
        0,                      // commonMin
        2,                      // commonMax
        1,                      // commonStep
        1,                      // commonDefault
        null,                   // translationName
        null,                   // parentFieldName
        DisplayModeId.All       // displayMode
    )]
    public static TaskOptionData MadmateTaskOption;

    [CustomOptionBool("MadmateHasImpostorVision", false)]
    public static bool MadmateHasImpostorVision;
}
