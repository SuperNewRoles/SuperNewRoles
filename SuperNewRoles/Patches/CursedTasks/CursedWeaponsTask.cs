using HarmonyLib;
using SuperNewRoles.MapCustoms;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedWeaponsTask
{
    [HarmonyPatch(typeof(WeaponsMinigame))]
    public static class WeaponsMinigamePatch
    {
        [HarmonyPatch(nameof(WeaponsMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(WeaponsMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.MyNormTask.MaxStep = 50;
            __instance.ScoreText.text = string.Format(ModTranslation.GetString("CursedWeaponsTaskScoreText"), __instance.MyNormTask.TaskStep, __instance.MyNormTask.MaxStep);

            GameObject cursor = new("cursor");
            cursor.transform.SetParent(__instance.transform);
            cursor.layer = 4;
            CircleCollider2D circleCollider2D = cursor.AddComponent<CircleCollider2D>();
            circleCollider2D.radius = 0.025f;
        }

        [HarmonyPatch(nameof(WeaponsMinigame.FixedUpdate)), HarmonyPostfix]
        public static void FixedUpdatePostfix(WeaponsMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.transform.FindChild("cursor").position = __instance.myController.HoverPosition;
        }

        [HarmonyPatch(nameof(WeaponsMinigame.BreakApart)), HarmonyPrefix]
        public static bool BreakApartPrefix(WeaponsMinigame __instance, Asteroid ast)
        {
            if (!Main.IsCursed) return true;
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.ExplodeSounds.Random<AudioClip>(), false, 1f, null).pitch = FloatRange.Next(0.8f, 1.2f);
            if (!__instance.MyNormTask.IsComplete)
            {
                __instance.StartCoroutine(ast.CoBreakApart());
                if (__instance.MyNormTask)
                {
                    __instance.MyNormTask.NextStep();
                    __instance.ScoreText.text = string.Format(ModTranslation.GetString("CursedWeaponsTaskScoreText"), __instance.MyNormTask.TaskStep, __instance.MyNormTask.MaxStep);
                }
                if (__instance.MyNormTask && __instance.MyNormTask.IsComplete)
                {
                    __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                    foreach (PoolableBehavior poolableBehavior in __instance.asteroidPool.activeChildren)
                    {
                        Asteroid asteroid = (Asteroid)poolableBehavior;
                        if (!(asteroid == ast)) __instance.StartCoroutine(asteroid.CoBreakApart());
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Asteroid))]
    public static class AsteroidPatch
    {
        [HarmonyPatch(nameof(Asteroid.Reset)), HarmonyPostfix]
        public static void AwakePostfix(Asteroid __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.gameObject.GetComponent<Rigidbody2D>()) return;
            Rigidbody2D rigidbody2D = __instance.gameObject.AddComponent<Rigidbody2D>();
            rigidbody2D.gravityScale = 0f;
        }
    }
}
