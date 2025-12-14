using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedLiftWeightsTask
{
    [HarmonyPatch(typeof(LiftWeightsMinigame))]
    public static class LiftWeightsMinigamePatch
    {
        private static bool isUp;
        private static float range = 0.1f;
        private static float speed = 0.9f;

        // 0~1の範囲だが、0の場所で一瞬押すだけでクリアしたり、最大の所で待機したりすることを防ぐために、範囲を設けている。
        private static float maxRange = 0.9f;
        private static float minRange = 0.3f;
        [HarmonyPatch(nameof(LiftWeightsMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(LiftWeightsMinigame __instance)
        {
            if (!Main.IsCursed) return;
            isUp = false;
            __instance.validFillPercentRange.min = __instance.validFillPercentRange.max - range;
        }

        [HarmonyPatch(nameof(LiftWeightsMinigame.Update)), HarmonyPostfix]
        public static void UpdatePostfix(LiftWeightsMinigame __instance)
        {
            if (!Main.IsCursed) return;
            float changeRange = speed * Time.deltaTime;
            if (isUp)
            {
                __instance.validFillPercentRange.max += changeRange;
                __instance.validFillPercentRange.min = __instance.validFillPercentRange.max - range;
                if (__instance.validFillPercentRange.max > maxRange)
                {
                    __instance.validFillPercentRange.max = maxRange;
                    isUp = false;
                }
            }
            else
            {
                __instance.validFillPercentRange.min -= changeRange;
                __instance.validFillPercentRange.max = __instance.validFillPercentRange.min + range;
                if (__instance.validFillPercentRange.min < minRange)
                {
                    __instance.validFillPercentRange.min = minRange;
                    isUp = true;
                }
            }
            __instance.InitializeValidIndicator();
        }
    }
}