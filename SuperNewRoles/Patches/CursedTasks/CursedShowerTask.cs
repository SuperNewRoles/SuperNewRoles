using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedShowerTask
{
    public static Dictionary<uint, float> Timer;

    [HarmonyPatch(typeof(ShowerMinigame))]
    public static class ShowerMinigamePatch
    {
        [HarmonyPatch(nameof(ShowerMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(ShowerMinigame __instance)
        {
            if (!Main.IsCursed) return;
            if (!Timer.ContainsKey(__instance.MyTask.Id)) Timer.Add(__instance.MyTask.Id, 0f);

            __instance.timer = 0;
            __instance.MaxTime = 180;
            __instance.PercentText.text = $"{(int)((__instance.MaxTime - Timer[__instance.MyTask.Id]) / __instance.MaxTime * 1000)}%";
        }

        [HarmonyPatch(nameof(ShowerMinigame.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(ShowerMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None) return false;
            Timer[__instance.MyTask.Id] += Time.deltaTime;
            __instance.Gauge.value = 1f - Timer[__instance.MyTask.Id] / __instance.MaxTime;
            __instance.PercentText.text = $"{(int)((__instance.MaxTime - Timer[__instance.MyTask.Id]) / __instance.MaxTime * 1000)}%";
            if (Timer[__instance.MyTask.Id] >= __instance.MaxTime)
            {
                __instance.MyNormTask.NextStep();
                __instance.StartCoroutine(__instance.CoStartClose(0.5f));
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        [HarmonyPatch(nameof(ShipStatus.Start)), HarmonyPostfix]
        public static void StartPostfix(ShipStatus __instance)
        {
            if (!Main.IsCursed) return;
            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId != 4) return;

            Transform particles = __instance.transform.Find("HallwayMain/ShowerParticles");
            particles.localScale *= 10;
            Vector3 pos1 = particles.position;
            pos1.y = -0.4f;
            pos1.z = -4.9f;
            particles.position = pos1;

            Transform halls_photos = __instance.transform.Find("HallwayMain/halls_photos");
            Vector3 pos2 = halls_photos.position;
            pos2.z = -4.8f;
            halls_photos.position = pos2;
        }
    }
}
