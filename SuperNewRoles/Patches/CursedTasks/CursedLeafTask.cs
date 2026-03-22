using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedLeafTask
{
    public static int LeavesNum;
    public static int LeafDoneCount;

    [HarmonyPatch(typeof(LeafMinigame))]
    public static class LeafMinigamePatch
    {
        [HarmonyPatch(nameof(LeafMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(LeafMinigame __instance)
        {
            if (!Main.IsCursed) return;
            GameObject pointer = new("Pointer");
            pointer.transform.SetParent(__instance.transform);
            pointer.layer = 4;
            CircleCollider2D collider2D = pointer.AddComponent<CircleCollider2D>();
            collider2D.radius = 0.5f;
        }

        [HarmonyPatch(nameof(LeafMinigame.FixedUpdate)), HarmonyPostfix]
        public static void FixedUpdatePostfix(LeafMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.transform.FindChild("Pointer").position = __instance.myController.HoverPosition;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.Initialize)), HarmonyPostfix]
        public static void InitializePostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.TaskType != TaskTypes.CleanO2Filter) return;
            __instance.MaxStep = Main.Num;
        }
    }
}