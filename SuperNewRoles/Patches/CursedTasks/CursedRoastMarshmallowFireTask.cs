using HarmonyLib;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedRoastMarshmallowFireTask
{
    [HarmonyPatch(typeof(RoastMarshmallowFireMinigame))]
    public static class RoastMarshmallowFireMinigamePatch
    {
        [HarmonyPatch(nameof(RoastMarshmallowFireMinigame.OnBagMarshmallowPressed)), HarmonyPostfix]
        public static void BeginPostfix(RoastMarshmallowFireMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.timeToToasted = ModHelpers.GetRandomFloat(0.1f, 10f);
            __instance.timeToBurnt = ModHelpers.GetRandomFloat(0.45f, 0.9f);
        }
        [HarmonyPatch(nameof(RoastMarshmallowFireMinigame.OnStickMarshmallowPressed)), HarmonyPostfix]
        public static void OnStickMarshmallowPressedPostfix(RoastMarshmallowFireMinigame __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.state != RoastMarshmallowFireMinigame.State.Toasting)
            {
                return;
            }
            if (__instance.marshmallowState == RoastMarshmallowFireMinigame.MarshmallowState.New)
            {
                // 早すぎてもダメにする
                __instance.SetMarshmallowState(RoastMarshmallowFireMinigame.MarshmallowState.Burnt);
            }
        }
    }
}