using HarmonyLib;

namespace SuperNewRoles.Sabotage.CognitiveDeficit
{
    public static class TaskBar
    {
        public static ProgressTracker Instance;
        [HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.FixedUpdate))]
        class TaskBarPatch
        {
            public static void Postfix(ProgressTracker __instance)
            {
                Instance = __instance;
                if (PlayerControl.GameOptions.TaskBarMode != TaskBarMode.Invisible)
                {
                    if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.CognitiveDeficit)
                    {
                        __instance.gameObject.SetActive(Main.IsLocalEnd);
                    }
                }
            }
        }
    }
}