using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Modules;

public sealed class TaskSelectionExclusionConfig
{
    private readonly Func<bool> _isEnabled;
    private readonly (TaskTypes TaskType, Func<bool> IsEnabled)[] _rules;
    public bool IsActive =>
        _isEnabled?.Invoke() == true && _rules.Any(rule => rule.IsEnabled?.Invoke() == true);

    public TaskSelectionExclusionConfig(
        Func<bool> isEnabled,
        params (TaskTypes TaskType, Func<bool> IsEnabled)[] rules)
    {
        _isEnabled = isEnabled;
        _rules = rules ?? [];
    }

    public bool IsExcluded(TaskTypes taskType)
    {
        if (_isEnabled?.Invoke() != true) return false;

        foreach (var rule in _rules)
        {
            if (rule.TaskType == taskType && rule.IsEnabled?.Invoke() == true)
                return true;
        }

        return false;
    }
}

public static class TaskSelectionExclusion
{
    private static readonly IReadOnlyDictionary<TaskTypes, Func<bool>> GlobalTaskOptions = new Dictionary<TaskTypes, Func<bool>>
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
        return GameSettingOptions.ExcludeSpecificTasksFromSelection &&
               GlobalTaskOptions.TryGetValue(taskType, out var isEnabled) &&
               isEnabled();
    }

    public static Il2CppSystem.Collections.Generic.List<NormalPlayerTask> FilterCandidates(
        Il2CppSystem.Collections.Generic.List<NormalPlayerTask> candidates,
        int requiredCount = 0)
    {
        if (candidates == null || !GameSettingOptions.ExcludeSpecificTasksFromSelection)
            return candidates;

        var filtered = GetAvailableTasks(null, candidates.ToSystemList()).ToIl2CppList();

        // Vanilla common-task bookkeeping still uses the requested count.
        return ShouldKeepOriginalCandidates(requiredCount, filtered.Count)
            ? candidates
            : filtered;
    }

    internal static bool ShouldKeepOriginalCandidates(int requiredCount, int filteredCount)
    {
        return requiredCount > 0 && filteredCount == 0;
    }

    public static List<NormalPlayerTask> GetAvailableTasks(
        ExPlayerControl player,
        IEnumerable<NormalPlayerTask> candidates)
    {
        if (candidates == null) return [];

        return candidates
            .Where(task =>
                task != null &&
                IsAvailableTask(player, task.TaskType))
            .ToList();
    }

    internal static bool IsAvailableTask(ExPlayerControl player, TaskTypes taskType)
    {
        return !IsExcluded(taskType) && !IsExcludedByPlayer(player, taskType);
    }

    private static bool IsExcludedByPlayer(ExPlayerControl player, TaskTypes taskType)
    {
        return player?.GetAbilities<CustomTaskAbility>()
            .Any(ability => ability?.ShouldExcludeTask(taskType) == true) == true;
    }
}
