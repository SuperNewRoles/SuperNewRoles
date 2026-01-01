using HarmonyLib;
using SuperNewRoles.Modules;
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
            var sprite = AssetManager.GetAsset<Sprite>("UnlockManifold.png");
            foreach (SpriteRenderer button in __instance.Buttons)
                if (sprite) button.sprite = sprite;
        }
    }
}
