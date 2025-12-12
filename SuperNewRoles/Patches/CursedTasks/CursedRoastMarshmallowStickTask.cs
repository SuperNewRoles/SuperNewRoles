using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
namespace SuperNewRoles.Patches.CursedTasks;

public class CursedRoastMarshmallowStickTask
{
    [HarmonyPatch(typeof(RoastMarshmallowStickMinigame))]
    public static class RoastMarshmallowStickMinigamePatch
    {

        [HarmonyPatch(nameof(RoastMarshmallowStickMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(RoastMarshmallowStickMinigame __instance)
        {
            if (!Main.IsCursed) return;
        }
        [HarmonyPatch(nameof(RoastMarshmallowStickMinigame.Update)), HarmonyPostfix]
        public static void UpdatePostfix(RoastMarshmallowStickMinigame __instance)
        {
            if (!Main.IsCursed) return;
            var mousePosition = Input.mousePosition;
            var mouseGlobalPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            if (Vector2.Distance(mouseGlobalPosition, __instance.stickTransform.transform.position) > 0.5f) return;
            __instance.stickTransform.transform.position += (mouseGlobalPosition - __instance.stickTransform.transform.position);
            if (Vector2.Distance(__instance.transform.position, __instance.stickTransform.transform.position) > 3)
            {
                __instance.stickTransform.transform.position =
                new Vector3(
                    Mathf.Clamp(__instance.stickTransform.position.x, __instance.transform.position.x - 3, __instance.transform.position.x + 3),
                    Mathf.Clamp(__instance.stickTransform.position.y, __instance.transform.position.y - 3, __instance.transform.position.y + 3),
                    0);
            }
        }
    }
}
