using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedVentCleaningTask
{
    [HarmonyPatch(typeof(VentCleaningMinigame))]
    public static class VentCleaningMinigamePatch
    {
        [HarmonyPatch(nameof(VentCleaningMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(VentCleaningMinigame __instance)
        {
            if (!Main.IsCursed) return;
            Transform TaskParent = __instance.transform.parent;
            for (int i = 0; i < TaskParent.childCount; i++)
            {
                Transform child = TaskParent.GetChild(i);
                if (child.name == "VentDirt(Clone)") Object.Destroy(child);
            }

            __instance.numberOfDirts = Main.Num;
            for (int i = 0; i < __instance.numberOfDirts; i++) __instance.SpawnDirt();
        }
    }
}
