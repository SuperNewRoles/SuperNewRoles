using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

class BodyBuilder : RoleBase<BodyBuilder>
{
    public override RoleId Role { get; } = RoleId.BodyBuilder;
    public override Color32 RoleColor { get; } = new(214, 143, 94, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new BodyBuilderAbility(),
        () => new LiftWeightsMinigameAbility(),
        () => new CustomTaskTypeAbility(
            TaskTypes.LiftWeights,
            BodyBuilderChangeAllTaskLiftWeights,
            MapNames.Fungle
        ),
        () => new CustomTaskAbility(
            isTaskTrigger: () => true,
            countsForCrewWin: () => BodyBuilderTaskOptionAvailable,
            requiredTaskCount: () => BodyBuilderTaskOptionAvailable ? BodyBuilderTaskOption.Total : null,
            taskOptions: () => BodyBuilderTaskOptionAvailable ? BodyBuilderTaskOption : null,
            taskSelectionExclusionConfig: CreateTaskSelectionExclusion()
        )
    ];

    internal static TaskSelectionExclusionConfig CreateTaskSelectionExclusion() => new(
        () => BodyBuilderExcludeSpecificTasksFromSelection,
        (TaskTypes.ResetBreakers, () => BodyBuilderExcludeResetBreakersTaskFromSelection),
        (TaskTypes.ClearAsteroids, () => BodyBuilderExcludeClearAsteroidsTaskFromSelection),
        (TaskTypes.SortRecords, () => BodyBuilderExcludeSortRecordsTaskFromSelection));

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("BodyBuilderChangeAllTaskLiftWeights", false)]
    public static bool BodyBuilderChangeAllTaskLiftWeights;

    [CustomOptionBool("BodyBuilderExcludeSpecificTasksFromSelection", false, translationName: "ExcludeSpecificTasksFromSelection")]
    public static bool BodyBuilderExcludeSpecificTasksFromSelection;

    [CustomOptionBool("BodyBuilderExcludeResetBreakersTaskFromSelection", false, translationName: "ExcludeResetBreakersTaskFromSelection", parentFieldName: nameof(BodyBuilderExcludeSpecificTasksFromSelection))]
    public static bool BodyBuilderExcludeResetBreakersTaskFromSelection;

    [CustomOptionBool("BodyBuilderExcludeClearAsteroidsTaskFromSelection", false, translationName: "ExcludeClearAsteroidsTaskFromSelection", parentFieldName: nameof(BodyBuilderExcludeSpecificTasksFromSelection))]
    public static bool BodyBuilderExcludeClearAsteroidsTaskFromSelection;

    [CustomOptionBool("BodyBuilderExcludeSortRecordsTaskFromSelection", false, translationName: "ExcludeSortRecordsTaskFromSelection", parentFieldName: nameof(BodyBuilderExcludeSpecificTasksFromSelection))]
    public static bool BodyBuilderExcludeSortRecordsTaskFromSelection;

    [CustomOptionBool("BodyBuilderTaskOptionAvailable", false)]
    public static bool BodyBuilderTaskOptionAvailable;

    [CustomOptionTask("BodyBuilderTaskOption", 2, 3, 1, parentFieldName: nameof(BodyBuilderTaskOptionAvailable), parentActiveValue: true)]
    public static TaskOptionData BodyBuilderTaskOption;

    [CustomOptionBool("BodyBuilderMoreMuscular", false)]
    public static bool BodyBuilderMoreMuscular;
}
