using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppSystem.Text;
using SuperNewRoles.MapCustoms;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedFixShowerTask
{
    public static Dictionary<uint, CursedFixShower> Data;

    [HarmonyPatch(typeof(FixShowerMinigame))]
    public static class FixShowerMinigamePatch
    {
        [HarmonyPatch(nameof(FixShowerMinigame.Start)), HarmonyPostfix]
        public static void StartPostfix(FixShowerMinigame __instance)
        {
            if (!Data.ContainsKey(__instance.MyNormTask.Id)) Data.Add(__instance.MyNormTask.Id, new());
            if (Data[__instance.MyNormTask.Id].Disabled) __instance.Close();
            if (Data[__instance.MyNormTask.Id].Count <= 0)
            {
                __instance.MyNormTask.Data = BitConverter.GetBytes(BoolRange.Next(0.5f) ? FloatRange.Next(0f, 0.1f) : (1f - FloatRange.Next(0f, 0.1f)));
                __instance.showerPos = BitConverter.ToSingle(__instance.MyNormTask.Data, 0);
                __instance.showerHead.transform.localEulerAngles = new Vector3(0f, 0f, __instance.showerAngles.Lerp(__instance.showerPos));
                Data[__instance.MyNormTask.Id].Count = 2;
            }
        }

        [HarmonyPatch(nameof(FixShowerMinigame.Bash)), HarmonyPrefix]
        public static bool BashPrefix(FixShowerMinigame __instance, ref Il2CppSystem.Collections.IEnumerator __result, float power)
        {
            if (!Main.IsCursed) return true;
            __result = Bash(__instance, power).WrapToIl2Cpp();
            return false;
        }

        public static IEnumerator Bash(FixShowerMinigame __instance, float power)
        {
            __instance.animating = true;
            __instance.mallet.transform.localEulerAngles = Vector3.zero;
            if ((double)__instance.showerPos < 0.5)
            {
                __instance.mallet.flipX = false;
                __instance.mallet.transform.localPosition = new Vector3(-2.5f, -0.4f, 0f);
            }
            else
            {
                __instance.mallet.flipX = true;
                __instance.mallet.transform.localPosition = new Vector3(2.5f, -0.4f, 0f);
            }
            __instance.mallet.enabled = true;
            yield return Effects.Wait(0.05f);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.swingSound, false, 1f, null);
            yield return Effects.Lerp(0.15f, new Action<float>((t) =>
            {
                float num = __instance.hammerAngles.Lerp(__instance.hammerAnim.Evaluate(t));
                if (__instance.mallet.flipX) num = -num;
                __instance.mallet.transform.localEulerAngles = new Vector3(0f, 0f, num);
            }));
            if (__instance.showerPos < 0.5f) VibrationManager.Vibrate(power, 0f, 0.3f, VibrationManager.VibrationFalloff.Linear, null, false);
            else VibrationManager.Vibrate(0f, power, 0.3f, VibrationManager.VibrationFalloff.Linear, null, false);
            if ((double)__instance.showerPos > 0.5) power = -power;
            __instance.showerPos += power;
            __instance.showerHead.transform.localEulerAngles = new Vector3(0f, 0f, __instance.showerAngles.Lerp(__instance.showerPos));
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.bashSounds.Random(), false, 1f, null);
            yield return Effects.Wait(0.05f);
            __instance.mallet.enabled = false;
            Data[__instance.MyNormTask.Id].Count--;
            if (Mathf.Abs(__instance.showerPos - 0.5f) < 0.07f)
            {
                __instance.MyNormTask.NextStep();
                yield return __instance.CoStartClose(0.75f);
            }
            else if (Data[__instance.MyNormTask.Id].Count <= 0)
            {
                Data[__instance.MyNormTask.Id].Disabled = true;
                Data[__instance.MyNormTask.Id].Timer = 30.75f;
                yield return __instance.CoStartClose(0.75f);
            }
            __instance.animating = false;
            yield break;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.FixedUpdate)), HarmonyPostfix]
        public static void FixedUpdatePostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (!Data.ContainsKey(__instance.Id)) return;
            if (__instance.TaskType != TaskTypes.FixShower) return;
            if (Data[__instance.Id].Disabled)
            {
                Data[__instance.Id].Timer -= Time.fixedDeltaTime;
                if (Data[__instance.Id].Timer < 0)
                {
                    Data[__instance.Id].Disabled = false;
                    Data[__instance.Id].Timer = 30.75f;
                }
            }
        }

        [HarmonyPatch(nameof(NormalPlayerTask.AppendTaskText)), HarmonyPrefix]
        public static bool AppendTaskTextPrefix(NormalPlayerTask __instance, [HarmonyArgument(0)] StringBuilder sb)
        {
            if (!Main.IsCursed) return true;
            if (__instance.TaskType != TaskTypes.FixShower) return true;
            if (!Data.ContainsKey(__instance.Id)) return true;
            if (!Data[__instance.Id].Disabled) return true;
            string room = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.StartAt);
            string task = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.TaskType);
            string timer = string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecondsAbbv), (int)Data[__instance.Id].Timer);
            sb.AppendLine($"{room}: {task} <color=#B00000>({timer})</color>");
            return false;
        }
    }

    [HarmonyPatch(typeof(Console))]
    public static class ConsolePatch
    {
        [HarmonyPatch(nameof(Console.Use)), HarmonyPrefix]
        public static bool UsePrefix(Console __instance)
        {
            if (!Main.IsCursed) return true;
            return !IsBlocked(__instance);
        }

        public static bool IsBlocked(Console __instance)
        {
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
            if (!canUse) return false;
            PlayerTask task = __instance.FindTask(CachedPlayer.LocalPlayer);
            if (!task) return false;
            if (task.TaskType != TaskTypes.FixShower) return false;
            if (!Data.ContainsKey(task.Id)) return false;
            if (Data[task.Id].Disabled) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(UseButton))]
    public static class UseButtonPatch
    {
        [HarmonyPatch(nameof(UseButton.SetTarget)), HarmonyPrefix]
        public static bool SetTargetPrefix(UseButton __instance, IUsable target)
        {
            if (!Main.IsCursed) return true;
            if (IsBlocked(target))
            {
                __instance.currentTarget = null;
                __instance.graphic.color = Palette.DisabledClear;
                __instance.graphic.material.SetFloat("_Desat", 0f);
                return false;
            }
            __instance.enabled = true;
            __instance.currentTarget = target;
            return true;
        }

        public static bool IsBlocked(IUsable target)
        {
            if (target == null) return false;
            Console console = target.TryCast<Console>();
            if (!console) return false;
            if (ConsolePatch.IsBlocked(console)) return true;
            return false;
        }
    }

    public class CursedFixShower
    {
        public int Count;
        public bool Disabled;
        public float Timer;
        public CursedFixShower()
        {
            Count = 2;
            Disabled = false;
            Timer = 30.75f;
            // 0.75秒は閉じるまでの時間
        }
    }
}