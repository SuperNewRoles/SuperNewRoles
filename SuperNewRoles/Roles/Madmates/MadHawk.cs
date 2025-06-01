using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Roles.Madmates;

class MadHawk : RoleBase<MadHawk>
{
    public override RoleId Role { get; } = RoleId.MadHawk;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;

    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MadmateAbility(new(
            MadHawkHasImpostorVision,
            MadHawkCouldUseVent,
            MadHawkCanKnowImpostors,
            MadHawkNeededTaskCount,
            MadHawkIsSpecialTasks ? MadHawkSpecialTasks : null)),
        () => new HawkAbility(
            MadHawkCoolTime,
            MadHawkDurationTime,
            MadHawkZoomMagnification,
            MadHawkCannotWalkInEffect)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Madmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    // --- Hawk Ability ---
    [CustomOptionFloat("MadHawkCoolTime", 0f, 120f, 2.5f, 30f, translationName: "CoolTime")]
    public static float MadHawkCoolTime;

    [CustomOptionFloat("MadHawkDurationTime", 0f, 60f, 0.5f, 10f, translationName: "DurationTime")]
    public static float MadHawkDurationTime;

    [CustomOptionFloat("MadHawkZoomMagnification", 1.25f, 10f, 0.25f, 2f, translationName: "HawkZoomMagnification")]
    public static float MadHawkZoomMagnification;

    [CustomOptionBool("MadHawkCannotWalkInEffect", true, translationName: "HawkCannotWalkInEffect")]
    public static bool MadHawkCannotWalkInEffect;

    // --- Madmate Custom Options ---
    [CustomOptionBool("MadHawkCouldUseVent", false, translationName: "CanUseVent")]
    public static bool MadHawkCouldUseVent;

    [CustomOptionBool("MadHawkHasImpostorVision", false, translationName: "HasImpostorVision")]
    public static bool MadHawkHasImpostorVision;

    [CustomOptionBool("MadHawkCanKnowImpostors", false, translationName: "MadmateCanKnowImpostors")]
    public static bool MadHawkCanKnowImpostors;

    [CustomOptionInt("MadHawkNeededTaskCount", 0, 30, 1, 6, parentFieldName: nameof(MadHawkCanKnowImpostors), translationName: "MadmateNeededTaskCount")]
    public static int MadHawkNeededTaskCount;

    [CustomOptionBool("MadHawkIsSpecialTasks", false, translationName: "MadmateIsSpecialTasks")]
    public static bool MadHawkIsSpecialTasks;
    [CustomOptionTask("MadHawkSpecialTasks", 1, 1, 1, translationName: "MadmateSpecialTasks", parentFieldName: nameof(MadHawkIsSpecialTasks))]
    public static TaskOptionData MadHawkSpecialTasks;
}