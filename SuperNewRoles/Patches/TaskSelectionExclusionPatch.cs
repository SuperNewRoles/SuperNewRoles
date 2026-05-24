using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.AddTasksFromList))]
public static class TaskSelectionExclusionPatch
{
    public static void Prefix(
        [HarmonyArgument(4)] ref Il2CppSystem.Collections.Generic.List<NormalPlayerTask> candidates)
    {
        candidates = TaskSelectionExclusion.FilterCandidates(candidates);
    }
}
