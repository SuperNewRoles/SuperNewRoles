using System;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedSafeTask
{
    [HarmonyPatch(typeof(SafeMinigame))]
    public static class SafeMinigamePatch
    {
        [HarmonyPatch(nameof(SafeMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(SafeMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.combo = new(25);
            do
            {
                for (int i = 0; i < __instance.combo.Length; i++)
                    __instance.combo[i] = IntRange.Next(8) + 1;
            }
            while (Check());
            for (int i = 0; i < __instance.combo.Length; i++)
                if (i % 2 != 0) __instance.combo[i] *= -1;

            Vector3 eulerAngles = __instance.Tumbler.transform.eulerAngles;
            eulerAngles.z = new FloatRange(0f, 360f).NextMinDistance(__instance.combo[0] * 45, 45f);
            __instance.Tumbler.transform.eulerAngles = eulerAngles;

            __instance.latched = new(25);
            for (int i = 0; i < __instance.latched.Length; i++)
                __instance.latched[i] = false;

            __instance.vibration = new(25);
            for (int i = 0; i < __instance.vibration.Length; i++)
                __instance.vibration[i] = false;

            SpriteRenderer arrow = Object.Instantiate(__instance.Arrows[0]);
            Sprite right = __instance.Arrows[0].sprite;
            Sprite left = __instance.Arrows[1].sprite;
            foreach (SpriteRenderer sprite in __instance.Arrows)
                Object.Destroy(sprite.gameObject);
            __instance.Arrows = new(2);
            for (int i = 0; i < __instance.Arrows.Length; i++)
            {
                __instance.Arrows[i] = Object.Instantiate(arrow, __instance.ComboText.transform.parent);
                __instance.Arrows[i].gameObject.name = "safe_rotationarrow" + (i % 2 == 0 ? "selected" : "");
                __instance.Arrows[i].sprite = i % 2 == 0 ? right : left;
                __instance.Arrows[i].transform.localPosition = new(0f, -0.075f, -1f);
                __instance.Arrows[i].transform.localScale *= 2f;
            }
            Object.Destroy(arrow.gameObject);

            __instance.UpdateComboInstructions();

            bool Check()
            {
                for (int i = 0; i < __instance.combo.Length - 1; i++)
                    if (__instance.combo[i] == __instance.combo[i + 1]) return true;
                return false;
            }
        }

        [HarmonyPatch(nameof(SafeMinigame.UpdateComboInstructions)), HarmonyPrefix]
        public static bool UpdateComboInstructionsPrefix(SafeMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            int num = __instance.latched.LastIndexOf(new Func<bool, bool>((b) => b)) + 1;
            for (int i = 0; i < __instance.Arrows.Length; i++)
                __instance.Arrows[i].enabled = num % 2 == i;
            return false;
        }

        [HarmonyPatch(nameof(SafeMinigame.CheckTumblr)), HarmonyPrefix]
        public static bool CheckTumblrPrefix(SafeMinigame __instance, float delta, float tumRotZ, int unlatched, int expected)
        {
            if (!Main.IsCursed) return true;
            float num = __instance.lastTumDir;
            float num2 = -Mathf.Sign(delta);
            if (num2 != 0f && num != num2)
            {
                __instance.reversalBuffer += delta;
                if (Mathf.Abs(__instance.reversalBuffer) > 0.15f)
                {
                    if (Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(__instance.DialTurnSound, false, 1f, null);
                    }
                    __instance.lastTumDir = -Mathf.Sign(delta);
                    __instance.reversalBuffer = 0f;
                }
            }
            if (num != 0f && __instance.lastTumDir != num)
            {
                if (__instance.AngleNear(tumRotZ + 45f, num, expected, 7f))
                {
                    if (Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(__instance.DialGoodSound, false, 1f, null);
                    }
                    __instance.latched[unlatched] = true;
                }
                else
                {
                    __instance.latched = new(25);
                    __instance.vibration = new(25);
                }
                __instance.UpdateComboInstructions();
                return false;
            }
            if (__instance.AngleNear(tumRotZ + 45f, num, expected, 5f))
            {
                __instance.latched[unlatched] = true;
                __instance.UpdateComboInstructions();
                __instance.latched[unlatched] = false;
                if (__instance.AngleNear(tumRotZ + 45f, num, expected, 7f))
                {
                    __instance.reversalBuffer = 0.15f * -num2;
                    if (!__instance.vibration[unlatched])
                    {
                        VibrationManager.Vibrate(0.5f, 0.5f, 0.2f, VibrationManager.VibrationFalloff.Linear, null, false);
                        __instance.vibration[unlatched] = true;
                    }
                }
            }
            return false;
        }
    }
}