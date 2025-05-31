using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Worshiper : RoleBase<Worshiper>
{
    public override RoleId Role { get; } = RoleId.Worshiper;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new SuicideButtonAbility(WorshiperAbilitySuicideCoolTime, AssetManager.GetAsset<Sprite>("ShermansServantSuicideButton.png")),
        () => new MadmateAbility(
            new(WorshiperHasImpostorVision, WorshiperCanUseVent, WorshiperCanKnowImpostors, WorshiperNeededTaskCount, WorshiperIsSpecialTasks ? WorshiperSpecialTasks : null)
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("WorshiperAbilitySuicideCoolTime", 0f, 60f, 2.5f, 30f)]
    public static float WorshiperAbilitySuicideCoolTime;

    [CustomOptionBool("WorshiperCanUseVent", false, translationName: "CanUseVent")]
    public static bool WorshiperCanUseVent;

    [CustomOptionBool("WorshiperHasImpostorVision", false, translationName: "HasImpostorVision")]
    public static bool WorshiperHasImpostorVision;

    [CustomOptionBool("WorshiperCanKnowImpostors", false)]
    public static bool WorshiperCanKnowImpostors;

    [CustomOptionInt("WorshiperNeededTaskCount", 0, 30, 1, 6, parentFieldName: nameof(WorshiperCanKnowImpostors), translationName: "MadmateNeededTaskCount")]
    public static int WorshiperNeededTaskCount;

    [CustomOptionBool("WorshiperIsSpecialTasks", false, translationName: "MadmateIsSpecialTasks")]
    public static bool WorshiperIsSpecialTasks;
    [CustomOptionTask("WorshiperSpecialTasks", 1, 1, 1, translationName: "MadmateSpecialTasks", parentFieldName: nameof(WorshiperIsSpecialTasks))]
    public static TaskOptionData WorshiperSpecialTasks;
}

