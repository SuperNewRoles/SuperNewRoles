using System;
using System.Reflection;
using FluentAssertions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using Xunit;

namespace SuperNewRoles.Tests;

public class TaskSelectionExclusionTests
{
    [Fact]
    public void ExclusionOptions_DefaultToOff()
    {
        string[] fieldNames =
        [
            nameof(GameSettingOptions.ExcludeSpecificTasksFromSelection),
            nameof(GameSettingOptions.ExcludeUnlockSafeTaskFromSelection),
            nameof(GameSettingOptions.ExcludeResetBreakersTaskFromSelection),
            nameof(GameSettingOptions.ExcludeCatchFishTaskFromSelection),
            nameof(GameSettingOptions.ExcludeUploadDataTaskFromSelection),
            nameof(GameSettingOptions.ExcludeVentCleaningTaskFromSelection),
            nameof(GameSettingOptions.ExcludeSubmitScanTaskFromSelection),
        ];

        foreach (var fieldName in fieldNames)
        {
            var field = typeof(GameSettingOptions).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            field.Should().NotBeNull();
            field!.GetCustomAttribute<CustomOptionBoolAttribute>()!.DefaultValue.Should().BeFalse(fieldName);
        }
    }

    [Fact]
    public void IsExcluded_ParentDisabled_DoesNotExcludeTasks()
    {
        WithTaskExclusionOptions(
            parent: false,
            action: () =>
            {
                TaskSelectionExclusion.IsExcluded(TaskTypes.UnlockSafe).Should().BeFalse();
                TaskSelectionExclusion.IsExcluded(TaskTypes.ResetBreakers).Should().BeFalse();
            });
    }

    [Fact]
    public void IsExcluded_EnabledTask_IsExcluded()
    {
        WithTaskExclusionOptions(
            parent: true,
            unlockSafe: true,
            resetBreakers: false,
            action: () =>
            {
                TaskSelectionExclusion.IsExcluded(TaskTypes.UnlockSafe).Should().BeTrue();
                TaskSelectionExclusion.IsExcluded(TaskTypes.ResetBreakers).Should().BeFalse();
            });
    }

    [Fact]
    public void IsExcluded_ResetBreakersEnabled_ExcludesResetBreakers()
    {
        WithTaskExclusionOptions(
            parent: true,
            unlockSafe: false,
            resetBreakers: true,
            action: () =>
            {
                TaskSelectionExclusion.IsExcluded(TaskTypes.UnlockSafe).Should().BeFalse();
                TaskSelectionExclusion.IsExcluded(TaskTypes.ResetBreakers).Should().BeTrue();
            });
    }

    [Fact]
    public void IsExcluded_AdditionalTasksEnabled_ExcludesConfiguredTasks()
    {
        WithTaskExclusionOptions(
            parent: true,
            catchFish: true,
            uploadData: true,
            ventCleaning: true,
            submitScan: true,
            action: () =>
            {
                TaskSelectionExclusion.IsExcluded(TaskTypes.CatchFish).Should().BeTrue();
                TaskSelectionExclusion.IsExcluded(TaskTypes.UploadData).Should().BeTrue();
                TaskSelectionExclusion.IsExcluded(TaskTypes.VentCleaning).Should().BeTrue();
                TaskSelectionExclusion.IsExcluded(TaskTypes.SubmitScan).Should().BeTrue();
            });
    }

    [Fact]
    public void IsExcluded_ChildDisabled_DoesNotExcludeThatTask()
    {
        WithTaskExclusionOptions(
            parent: true,
            unlockSafe: false,
            resetBreakers: false,
            catchFish: false,
            uploadData: false,
            ventCleaning: false,
            submitScan: false,
            action: () =>
            {
                TaskSelectionExclusion.IsExcluded(TaskTypes.UnlockSafe).Should().BeFalse();
                TaskSelectionExclusion.IsExcluded(TaskTypes.ResetBreakers).Should().BeFalse();
                TaskSelectionExclusion.IsExcluded(TaskTypes.CatchFish).Should().BeFalse();
                TaskSelectionExclusion.IsExcluded(TaskTypes.UploadData).Should().BeFalse();
                TaskSelectionExclusion.IsExcluded(TaskTypes.VentCleaning).Should().BeFalse();
                TaskSelectionExclusion.IsExcluded(TaskTypes.SubmitScan).Should().BeFalse();
            });
    }

    [Fact]
    public void IsAvailableTask_RejectsExcludedTaskAndAllowsOtherTask()
    {
        WithTaskExclusionOptions(
            parent: true,
            resetBreakers: true,
            action: () =>
            {
                TaskSelectionExclusion.IsAvailableTask(null, TaskTypes.ResetBreakers).Should().BeFalse();
                TaskSelectionExclusion.IsAvailableTask(null, TaskTypes.ClearAsteroids).Should().BeTrue();
            });
    }

    [Theory]
    [InlineData(3, 2, false)]
    [InlineData(3, 1, false)]
    [InlineData(3, 0, true)]
    [InlineData(0, 0, false)]
    public void ShouldKeepOriginalCandidates_OnlyWhenRequiredPoolIsEmpty(
        int requiredCount,
        int filteredCount,
        bool expected)
    {
        TaskSelectionExclusion.ShouldKeepOriginalCandidates(requiredCount, filteredCount)
            .Should().Be(expected);
    }

    [Theory]
    [InlineData(20, 5, 20)]
    [InlineData(3, 1, 3)]
    [InlineData(3, 0, 0)]
    public void GetTaskCountToAssign_OnlyDropsCountForAnEmptyPool(
        int requestedCount,
        int candidateCount,
        int expectedCount)
    {
        CustomTaskAbility.GetTaskCountToAssign(requestedCount, candidateCount)
            .Should().Be(expectedCount);
    }

    [Fact]
    public void CreateVanillaTaskData_AllCountsZero_AssignsOneShortTask()
    {
        var result = CustomTaskAbility.CreateVanillaTaskData(0, 0, 0);

        result.Short.Should().Be(1);
        result.Long.Should().Be(0);
        result.Common.Should().Be(0);
    }

    [Fact]
    public void CreateVanillaTaskData_NonZeroCounts_ArePreserved()
    {
        var result = CustomTaskAbility.CreateVanillaTaskData(2, 3, 1);

        result.Short.Should().Be(2);
        result.Long.Should().Be(3);
        result.Common.Should().Be(1);
    }

    [Fact]
    public void RoleConfig_RequiresParentAndChildSettings()
    {
        bool parent = true;
        bool lever = false;
        var config = new TaskSelectionExclusionConfig(
            () => parent,
            (TaskTypes.ResetBreakers, () => lever));

        config.IsExcluded(TaskTypes.ResetBreakers).Should().BeFalse();
        lever = true;
        config.IsExcluded(TaskTypes.ResetBreakers).Should().BeTrue();
        parent = false;
        config.IsExcluded(TaskTypes.ResetBreakers).Should().BeFalse();
    }

    [Fact]
    public void CustomTaskAbility_AppliesConfigOnlyInPrefabReplacementMode()
    {
        var config = new TaskSelectionExclusionConfig(
            () => true,
            (TaskTypes.ResetBreakers, () => true));
        var ability = new CustomTaskAbility(
            () => (true, true, null),
            taskSelectionExclusionConfig: config);

        ability.ShouldExcludeTask(TaskTypes.ResetBreakers, isPrefabReplacementMode: false).Should().BeFalse();
        ability.ShouldExcludeTask(TaskTypes.ResetBreakers, isPrefabReplacementMode: true).Should().BeTrue();
    }

    [Fact]
    public void RoleFactories_RegisterLeverAndShootingTasks()
    {
        AssertRoleFactory(
            typeof(HamburgerShop),
            HamburgerShop.CreateTaskSelectionExclusion,
            nameof(HamburgerShop.HamburgerShopExcludeSpecificTasksFromPrefabReplacement),
            nameof(HamburgerShop.HamburgerShopExcludeResetBreakersTaskFromPrefabReplacement),
            nameof(HamburgerShop.HamburgerShopExcludeClearAsteroidsTaskFromPrefabReplacement));
        AssertRoleFactory(
            typeof(BodyBuilder),
            BodyBuilder.CreateTaskSelectionExclusion,
            nameof(BodyBuilder.BodyBuilderExcludeSpecificTasksFromPrefabReplacement),
            nameof(BodyBuilder.BodyBuilderExcludeResetBreakersTaskFromPrefabReplacement),
            nameof(BodyBuilder.BodyBuilderExcludeClearAsteroidsTaskFromPrefabReplacement));
        AssertRoleFactory(
            typeof(Safecracker),
            Safecracker.CreateTaskSelectionExclusion,
            nameof(Safecracker.SafecrackerExcludeSpecificTasksFromPrefabReplacement),
            nameof(Safecracker.SafecrackerExcludeResetBreakersTaskFromPrefabReplacement),
            nameof(Safecracker.SafecrackerExcludeClearAsteroidsTaskFromPrefabReplacement));
    }

    [Fact]
    public void RoleOptions_HaveTheExpectedParentAndDisplayMode()
    {
        AssertRoleOptions(
            typeof(HamburgerShop),
            nameof(HamburgerShop.HamburgerShopExcludeSpecificTasksFromPrefabReplacement),
            nameof(HamburgerShop.HamburgerShopExcludeResetBreakersTaskFromPrefabReplacement),
            nameof(HamburgerShop.HamburgerShopExcludeClearAsteroidsTaskFromPrefabReplacement),
            DisplayModeId.Default);
        AssertRoleOptions(
            typeof(BodyBuilder),
            nameof(BodyBuilder.BodyBuilderExcludeSpecificTasksFromPrefabReplacement),
            nameof(BodyBuilder.BodyBuilderExcludeResetBreakersTaskFromPrefabReplacement),
            nameof(BodyBuilder.BodyBuilderExcludeClearAsteroidsTaskFromPrefabReplacement),
            DisplayModeId.All);
        AssertRoleOptions(
            typeof(Safecracker),
            nameof(Safecracker.SafecrackerExcludeSpecificTasksFromPrefabReplacement),
            nameof(Safecracker.SafecrackerExcludeResetBreakersTaskFromPrefabReplacement),
            nameof(Safecracker.SafecrackerExcludeClearAsteroidsTaskFromPrefabReplacement),
            DisplayModeId.All);
    }

    private static void AssertRoleFactory(
        Type roleType,
        Func<TaskSelectionExclusionConfig> createConfig,
        string parentName,
        string leverName,
        string shootingName)
    {
        var parent = roleType.GetField(parentName, BindingFlags.Public | BindingFlags.Static)!;
        var lever = roleType.GetField(leverName, BindingFlags.Public | BindingFlags.Static)!;
        var shooting = roleType.GetField(shootingName, BindingFlags.Public | BindingFlags.Static)!;
        bool[] original = [(bool)parent.GetValue(null)!, (bool)lever.GetValue(null)!, (bool)shooting.GetValue(null)!];

        try
        {
            parent.SetValue(null, true);
            lever.SetValue(null, true);
            shooting.SetValue(null, true);
            var config = createConfig();

            config.IsExcluded(TaskTypes.ResetBreakers).Should().BeTrue(roleType.Name);
            config.IsExcluded(TaskTypes.ClearAsteroids).Should().BeTrue(roleType.Name);
        }
        finally
        {
            parent.SetValue(null, original[0]);
            lever.SetValue(null, original[1]);
            shooting.SetValue(null, original[2]);
        }
    }

    private static void AssertRoleOptions(
        Type roleType,
        string parentName,
        string leverName,
        string shootingName,
        DisplayModeId displayMode)
    {
        var parent = roleType.GetField(parentName, BindingFlags.Public | BindingFlags.Static)!;
        var parentOption = parent.GetCustomAttribute<CustomOptionBoolAttribute>()!;
        parentOption.DefaultValue.Should().BeFalse();
        parentOption.DisplayMode.Should().Be(displayMode);

        foreach (var childName in new[] { leverName, shootingName })
        {
            var child = roleType.GetField(childName, BindingFlags.Public | BindingFlags.Static)!;
            var childOption = child.GetCustomAttribute<CustomOptionBoolAttribute>()!;
            childOption.DefaultValue.Should().BeFalse();
            childOption.ParentFieldName.Should().Be(parentName);
            childOption.DisplayMode.Should().Be(displayMode);
        }
    }

    private static void WithTaskExclusionOptions(
        bool parent,
        Action action,
        bool unlockSafe = true,
        bool resetBreakers = true,
        bool catchFish = true,
        bool uploadData = true,
        bool ventCleaning = true,
        bool submitScan = true)
    {
        var originalParent = GameSettingOptions.ExcludeSpecificTasksFromSelection;
        var originalUnlockSafe = GameSettingOptions.ExcludeUnlockSafeTaskFromSelection;
        var originalResetBreakers = GameSettingOptions.ExcludeResetBreakersTaskFromSelection;
        var originalCatchFish = GameSettingOptions.ExcludeCatchFishTaskFromSelection;
        var originalUploadData = GameSettingOptions.ExcludeUploadDataTaskFromSelection;
        var originalVentCleaning = GameSettingOptions.ExcludeVentCleaningTaskFromSelection;
        var originalSubmitScan = GameSettingOptions.ExcludeSubmitScanTaskFromSelection;

        try
        {
            GameSettingOptions.ExcludeSpecificTasksFromSelection = parent;
            GameSettingOptions.ExcludeUnlockSafeTaskFromSelection = unlockSafe;
            GameSettingOptions.ExcludeResetBreakersTaskFromSelection = resetBreakers;
            GameSettingOptions.ExcludeCatchFishTaskFromSelection = catchFish;
            GameSettingOptions.ExcludeUploadDataTaskFromSelection = uploadData;
            GameSettingOptions.ExcludeVentCleaningTaskFromSelection = ventCleaning;
            GameSettingOptions.ExcludeSubmitScanTaskFromSelection = submitScan;
            action();
        }
        finally
        {
            GameSettingOptions.ExcludeSpecificTasksFromSelection = originalParent;
            GameSettingOptions.ExcludeUnlockSafeTaskFromSelection = originalUnlockSafe;
            GameSettingOptions.ExcludeResetBreakersTaskFromSelection = originalResetBreakers;
            GameSettingOptions.ExcludeCatchFishTaskFromSelection = originalCatchFish;
            GameSettingOptions.ExcludeUploadDataTaskFromSelection = originalUploadData;
            GameSettingOptions.ExcludeVentCleaningTaskFromSelection = originalVentCleaning;
            GameSettingOptions.ExcludeSubmitScanTaskFromSelection = originalSubmitScan;
        }
    }

}
