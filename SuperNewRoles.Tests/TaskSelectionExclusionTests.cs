using System;
using FluentAssertions;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class TaskSelectionExclusionTests
{
    [Fact]
    public void IsExcluded_ParentDisabled_DoesNotExcludeTasks()
    {
        WithTaskExclusionOptions(
            parent: false,
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
    public void IsExcluded_UnlockSafeEnabled_ExcludesUnlockSafe()
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
