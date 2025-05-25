using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;
using Object = UnityEngine.Object;
using Action = System.Action;
using IEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace SuperNewRoles.Roles.Ability;

/// <summary>
/// LiftWeightsミニゲームの動作をカスタマイズするAbility
/// </summary>
public class LiftWeightsMinigameAbility : AbilityBase
{
    public int CustomMaxStep { get; }

    public LiftWeightsMinigameAbility(int customMaxStep = 3)
    {
        CustomMaxStep = customMaxStep;
    }
}

// LiftWeightsミニゲームのパッチ
[HarmonyPatch]
public static class LiftWeightsMinigamePatches
{
    [HarmonyPatch(typeof(LiftWeightsMinigame))]
    public static class LiftWeightsMinigamePatch
    {
        [HarmonyPatch(nameof(LiftWeightsMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(PlayerTask task, ref int __state)
        {
            if (task.Il2CppIs(out NormalPlayerTask normal)) __state = normal.MaxStep;
            else __state = 1;
        }

        [HarmonyPatch(nameof(LiftWeightsMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(LiftWeightsMinigame __instance, ref int __state)
        {
            var liftWeightsAbility = ExPlayerControl.LocalPlayer.GetAbility<LiftWeightsMinigameAbility>();
            if (liftWeightsAbility == null) return;

            __instance.MyNormTask.MaxStep = __state;
            List<SpriteRenderer> sprites = new(__instance.counters);

            while (__instance.MyNormTask.MaxStep > sprites.Count)
            {
                SpriteRenderer sprite = Object.Instantiate(sprites[0], sprites[0].transform.parent);
                sprite.name = $"Counter {sprites.Count + 1}";
                sprites.Add(sprite);
            }

            while (__instance.MyNormTask.MaxStep < sprites.Count)
            {
                SpriteRenderer sprite = sprites.Last();
                sprites.RemoveAt(sprites.Count - 1);
                Object.Destroy(sprite.gameObject);
            }

            __instance.counters = sprites.ToArray();

            for (int i = 0; i < __instance.counters.Length; i++)
            {
                Vector3 pos = new(0.282f * (i / 5), 0.564f - 0.282f * (i % 5));
                __instance.counters[i].transform.localPosition = pos;
                __instance.counters[i].color = __instance.MyNormTask.TaskStep > i ? Color.green : new(0.5849f, 0.5849f, 0.5849f);
            }
            __instance.OnValidate();
        }

        [HarmonyPatch(nameof(LiftWeightsMinigame.EndLifting)), HarmonyPrefix]
        public static bool EndLiftingPrefix(LiftWeightsMinigame __instance)
        {
            var liftWeightsAbility = ExPlayerControl.LocalPlayer.GetAbility<LiftWeightsMinigameAbility>();
            if (liftWeightsAbility == null) return true;

            if (__instance.state != LiftWeightsMinigame.State.Lifting) return false;

            if (__instance.validFillPercentRange.Contains(__instance.currentBarFillPercent))
            {
                __instance.counters[__instance.MyNormTask.taskStep].color = Color.green;
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.completeRepSound, false, 1f, null);
                __instance.StartCoroutine(Effects.Bloop(0f, __instance.counters[__instance.MyNormTask.taskStep].transform, __instance.counters[__instance.MyNormTask.taskStep].transform.localScale.x, 0.5f));
                __instance.MyNormTask.NextStep();
                VibrationManager.Vibrate(0.7f, 0.7f, 0.2f, VibrationManager.VibrationFalloff.None, null, false, "");

                if (__instance.MyNormTask.IsComplete)
                {
                    if (Constants.ShouldPlaySfx())
                    {
                        __instance.StartCoroutine(Effects.Sequence(new IEnumerator[]
                        {
                            Effects.Wait(0.1f),
                            Effects.Action((Action)(() => SoundManager.Instance.PlaySound(__instance.completeAllRepsSound, false, 1f, null)))
                        }));
                    }
                    __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                }
                else
                {
                    PlayerControl player = __instance.MyTask.Owner;
                    Console console = __instance.Console;
                    Vector2 truePosition = player.GetTruePosition();
                    Vector3 position = console.transform.position;
                    bool use = player.Data.Role.CanUse(console.TryCast<IUsable>()) && (!console.onlySameRoom || console.InRoom(truePosition)) &&
                              (!console.onlyFromBelow || truePosition.y < position.y) && console.FindTask(player);

                    if (use)
                    {
                        float distance = Vector2.Distance(truePosition, console.transform.position);
                        use &= distance <= console.UsableDistance;
                        if (console.checkWalls) use &= !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShadowMask, false);
                    }

                    if (!use) __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                }
            }
            else
            {
                __instance.fillBar.color = Color.red;
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.failRepSound, false, 1f, null);
            }

            __instance.barfillAudioSource.Stop();
            __instance.barfillAudioSource.volume = 0f;
            __instance.state = LiftWeightsMinigame.State.Dropping;
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.Initialize)), HarmonyPostfix]
        public static void InitializePostfix(NormalPlayerTask __instance)
        {
            var liftWeightsAbility = ExPlayerControl.LocalPlayer.GetAbility<LiftWeightsMinigameAbility>();
            if (liftWeightsAbility == null) return;
            if (__instance.TaskType != TaskTypes.LiftWeights) return;
            __instance.MaxStep = liftWeightsAbility.CustomMaxStep;
        }
    }
}