using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedSweepTask
{
    [HarmonyPatch(typeof(SweepMinigame))]
    public static class SweepMinigamePatch
    {
        [HarmonyPatch(nameof(SweepMinigame.FixedUpdate)), HarmonyPrefix]
        public static bool FixedUpdatePrefix(SweepMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            float num = Mathf.Clamp(Mathf.Sin(__instance.timer / 75f * 2f) / 2f + 0.5f, 5f, 15f);
            __instance.timer += Time.fixedDeltaTime * num;
            if (__instance.spinnerIdx < __instance.Spinners.Length)
            {
                float num2 = __instance.CalcXPerc();
                __instance.Gauges[__instance.spinnerIdx].Value = num2 < 13f ? 0.9f : 0.1f;
                Quaternion localRotation = Quaternion.Euler(0f, 0f, __instance.timer * __instance.SpinRate);
                __instance.Spinners[__instance.spinnerIdx].transform.localRotation = localRotation;
                __instance.Shadows[__instance.spinnerIdx].transform.localRotation = localRotation;
                __instance.Lights[__instance.spinnerIdx].enabled = num2 < 13f;
            }
            for (int i = 0; i < __instance.Gauges.Length; i++)
            {
                HorizontalGauge horizontalGauge = __instance.Gauges[i];
                if (i < __instance.spinnerIdx) horizontalGauge.Value = 0.95f;
                if (i > __instance.spinnerIdx) horizontalGauge.Value = 0.05f;
                horizontalGauge.Value += (Mathf.PerlinNoise(i, Time.time * 51f) - 0.5f) * 0.025f;
            }
            return false;
        }
    }
}
