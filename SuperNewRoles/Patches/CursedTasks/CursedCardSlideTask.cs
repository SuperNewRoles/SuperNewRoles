using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedCardSlideTask
{
    public static float Accepted;
    public static float Timer;

    [HarmonyPatch(typeof(CardSlideGame))]
    public static class CardSlideGamePatch
    {
        [HarmonyPatch(nameof(CardSlideGame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(CardSlideGame __instance)
        {
            if (!Main.IsCursed) return;
            int random = Random.RandomRange(0, 25 + 1);
            Accepted = 0.25f + (0.05f * random);
            Timer = 60f;
            __instance.AcceptedTime = new(Accepted - 0.025f, Accepted + 0.025f);
        }

        [HarmonyPatch(nameof(CardSlideGame.Update)), HarmonyPrefix]
        public static void UpdatePrefix(CardSlideGame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.AcceptedTime = new(Accepted - (0.025f * (Timer / 60f)), Accepted + (0.025f * (Timer / 60f)));
            Timer -= Time.fixedDeltaTime;
            if (Timer <= 0) Timer = 0;
        }
    }
}
