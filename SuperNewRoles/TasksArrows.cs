using HarmonyLib;
using System;

namespace SuperNewRoles
{
    [HarmonyPatch]
    public static class TasksArrow
    {
        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
        public static class NormalPlayerTaskPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                bool showArrows = !TasksArrowsOption.hideTaskArrows && !__instance.IsComplete && __instance.TaskStep > 0;
                __instance.Arrow?.gameObject?.SetActive(showArrows);
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.FixedUpdate))]
        public static class AirshipUploadTaskPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                bool showArrows = !TasksArrowsOption.hideTaskArrows && !__instance.IsComplete && __instance.TaskStep > 0;
                __instance.Arrows?.DoIf(x => x != null, x => x.gameObject?.SetActive(showArrows));
            }
        }

        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.UpdateArrow))]
        public static class NormalPlayerTaskUpdateArrowPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                if (TasksArrowsOption.hideTaskArrows)
                {
                    __instance.Arrow?.gameObject?.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.UpdateArrow))]
        public static class AirshipUploadTaskUpdateArrowPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                if (TasksArrowsOption.hideTaskArrows)
                {
                    __instance.Arrows?.DoIf(x => x != null, x => x.gameObject?.SetActive(false));
                }
            }
        }
    }

    static class TasksArrowsOption
    {
        public static bool hideTaskArrows = false;

        public static void clearAndReloadMapOptions()
        {
            hideTaskArrows = ConfigRoles.HideTaskArrows.Value;
        }
    }
}