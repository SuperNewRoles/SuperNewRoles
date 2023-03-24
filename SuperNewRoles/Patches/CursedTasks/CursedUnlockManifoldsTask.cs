using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedUnlockManifoldsTask
{
    [HarmonyPatch(typeof(UnlockManifoldsMinigame))]
    public static class UnlockManifoldsMinigamePatch
    {
        [HarmonyPatch(nameof(UnlockManifoldsMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(UnlockManifoldsMinigame __instance)
        {
            if (!Main.IsCursed) return;
            foreach (SpriteRenderer button in __instance.Buttons)
                button.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cursed.UnlockManifold.png", 100f);
        }
    }
}
