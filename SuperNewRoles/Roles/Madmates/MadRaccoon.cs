using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Roles.Madmates;

class MadRaccoon : RoleBase<MadRaccoon>
{
    public override RoleId Role { get; } = RoleId.MadRaccoon;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;

    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MadmateAbility(new(
            MadRaccoonHasImpostorVision,
            MadRaccoonCouldUseVent,
            MadRaccoonCanKnowImpostors,
            MadRaccoonNeededTaskCount,
            MadRaccoonIsSpecialTasks ? MadRaccoonSpecialTasks : null)),
        () => new ShapeshiftButtonAbility(MadRaccoonShapeshiftCooldown, MadRaccoonShapeshiftDuration, "MadRacoonButton.png")
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Madmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    // --- Shapeshift ---
    [CustomOptionFloat("MadRaccoonShapeshiftCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float MadRaccoonShapeshiftCooldown;

    [CustomOptionFloat("MadRaccoonShapeshiftDuration", 2.5f, 60f, 2.5f, 15f)]
    public static float MadRaccoonShapeshiftDuration;

    // --- Madmate Custom Options ---
    [CustomOptionBool("MadRaccoonCouldUseVent", false, translationName: "CanUseVent")]
    public static bool MadRaccoonCouldUseVent;

    [CustomOptionBool("MadRaccoonHasImpostorVision", false, translationName: "HasImpostorVision")]
    public static bool MadRaccoonHasImpostorVision;

    [CustomOptionBool("MadRaccoonCanKnowImpostors", false, translationName: "MadmateCanKnowImpostors")]
    public static bool MadRaccoonCanKnowImpostors;

    [CustomOptionInt("MadRaccoonNeededTaskCount", 0, 30, 1, 6, parentFieldName: nameof(MadRaccoonCanKnowImpostors))]
    public static int MadRaccoonNeededTaskCount;

    [CustomOptionBool("MadRaccoonIsSpecialTasks", false, translationName: "MadmateIsSpecialTasks")]
    public static bool MadRaccoonIsSpecialTasks;
    [CustomOptionTask("MadRaccoonSpecialTasks", 1, 1, 1, translationName: "MadmateSpecialTasks", parentFieldName: nameof(MadRaccoonIsSpecialTasks))]
    public static TaskOptionData MadRaccoonSpecialTasks;
}