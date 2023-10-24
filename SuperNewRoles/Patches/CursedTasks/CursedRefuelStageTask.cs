using HarmonyLib;
using Rewired;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedRefuelStageTask
{
    [HarmonyPatch(typeof(RefuelStage))]
    public static class RefuelStagePatch
    {
        [HarmonyPatch(nameof(RefuelStage.FixedUpdate)), HarmonyPrefix]
        public static bool FixedUpdatePrefix(RefuelStage __instance)
        {
            if (!Main.IsCursed) return true;
            if (ReInput.players.GetPlayer(0).GetButton(21))
            {
                if (!__instance.isDown)
                {
                    __instance.usingController = true;
                    __instance.Refuel();
                }
            }
            else if (__instance.isDown && __instance.usingController)
            {
                __instance.usingController = false;
                __instance.Refuel();
            }
            if (__instance.complete) return false;
            if (__instance.isDown && __instance.timer < 1f)
            {
                __instance.timer += Time.fixedDeltaTime / __instance.RefuelDuration;
                __instance.MyNormTask.Data[0] = (byte)Mathf.Min(255f, __instance.timer * 255f);
                if (__instance.timer >= 1f)
                {
                    __instance.complete = true;
                    if (__instance.greenLight) __instance.greenLight.color = __instance.green;
                    if (__instance.redLight) __instance.redLight.color = __instance.darkRed;
                    if (__instance.MyNormTask.MaxStep / 3 == 1)
                    {
                        __instance.MyNormTask.Data[0] = 0;
                        __instance.MyNormTask.NextStep();
                    }
                    else if (__instance.MyNormTask.StartAt == SystemTypes.CargoBay || __instance.MyNormTask.StartAt == SystemTypes.Engine)
                    {
                        __instance.MyNormTask.Data[0] = 0;
                        __instance.MyNormTask.Data[1] = (byte)(BoolRange.Next(0.5f) ? 1 : 2);
                        __instance.MyNormTask.NextStep();
                    }
                    else
                    {
                        __instance.MyNormTask.Data[0] = 0;
                        __instance.MyNormTask.Data[1] += 1;
                        if (__instance.MyNormTask.Data[1] % 2 == 0) __instance.MyNormTask.NextStep();
                    }
                    __instance.MyNormTask.UpdateArrowAndLocation();
                }
            }
            __instance.destGauge.value = __instance.timer;
            if (__instance.srcGauge) __instance.srcGauge.value = 1f - __instance.timer;
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.Initialize)), HarmonyPostfix]
        public static void InitializePostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.TaskType != TaskTypes.FuelEngines) return;
            __instance.MaxStep *= 3;
        }

        [HarmonyPatch(nameof(NormalPlayerTask.NextStep)), HarmonyPostfix]
        public static void NextStepPostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.TaskType != TaskTypes.FuelEngines) return;
            if (__instance.TaskStep % (__instance.MaxStep / 3) == 0) __instance.Data[1] = 0;
        }
    }
}