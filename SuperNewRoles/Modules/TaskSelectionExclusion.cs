using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Modules;

public static class TaskSelectionExclusion
{
    private static readonly IReadOnlyDictionary<TaskTypes, Func<bool>> ExcludedTaskOptions = new Dictionary<TaskTypes, Func<bool>>
    {
        [TaskTypes.UnlockSafe] = () => GameSettingOptions.ExcludeUnlockSafeTaskFromSelection,
        [TaskTypes.ResetBreakers] = () => GameSettingOptions.ExcludeResetBreakersTaskFromSelection,
        [TaskTypes.CatchFish] = () => GameSettingOptions.ExcludeCatchFishTaskFromSelection,
        [TaskTypes.UploadData] = () => GameSettingOptions.ExcludeUploadDataTaskFromSelection,
        [TaskTypes.VentCleaning] = () => GameSettingOptions.ExcludeVentCleaningTaskFromSelection,
        [TaskTypes.SubmitScan] = () => GameSettingOptions.ExcludeSubmitScanTaskFromSelection,
    };

    public static bool IsExcluded(TaskTypes taskType)
    {
        if (!GameSettingOptions.ExcludeSpecificTasksFromSelection) return false;

        return ExcludedTaskOptions.TryGetValue(taskType, out var isEnabled) && isEnabled();
    }

    public static Il2CppSystem.Collections.Generic.List<NormalPlayerTask> FilterCandidates(
        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> candidates)
    {
        if (candidates == null || !GameSettingOptions.ExcludeSpecificTasksFromSelection) return candidates;

        var filteredCandidates = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
        foreach (var candidate in candidates)
        {
            if (candidate == null || !IsExcluded(candidate.TaskType))
                filteredCandidates.Add(candidate);
        }

        return filteredCandidates;
    }
}
