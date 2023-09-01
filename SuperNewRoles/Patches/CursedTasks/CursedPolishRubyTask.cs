using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedPolishRubyTask
{
    [HarmonyPatch(typeof(PolishRubyGame))]
    public static class PolishRubyGamePatch
    {
        [HarmonyPatch(nameof(PolishRubyGame.Start)), HarmonyPostfix]
        public static void StartPostfix(PolishRubyGame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.swipesToClean = 10;
            foreach (PassiveButton button in __instance.Buttons)
                button.gameObject.SetActive(true);
        }

        [HarmonyPatch(nameof(PolishRubyGame.Update)), HarmonyPostfix]
        public static void UpdatePostfix(PolishRubyGame __instance)
        {
            if (!Main.IsCursed) return;
            Transform button = __instance.transform.Find("CloseButton");
            Vector3 pos = button.position;
            __instance.transform.Rotate(Vector3.forward * (Time.fixedDeltaTime * 20f));
            button.rotation = Quaternion.Euler(0f, 0f, 0f);
            button.position = pos;
        }
    }
}
