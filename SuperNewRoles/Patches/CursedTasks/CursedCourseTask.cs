using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedCourseTask
{
    [HarmonyPatch(typeof(CourseMinigame))]
    public static class CourseMinigamePatch
    {
        [HarmonyPatch(nameof(CourseMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(CourseMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.NumPoints = 25;
        }

        [HarmonyPatch(nameof(CourseMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(CourseMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.Destination.gameObject.SetActive(false);
            __instance.Path.gameObject.SetActive(false);
            foreach (SpriteRenderer sprite in __instance.Dots)
                sprite.gameObject.SetActive(false);
            foreach (CourseStarBehaviour star in __instance.Stars)
                if (star != null) star.gameObject.SetActive(false);
        }
    }
}