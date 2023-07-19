using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppSystem.Text;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;
using Random = System.Random;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedDivertPowerTask
{
    public static Dictionary<uint, CursedDivertPower> Data;
    public static List<SystemTypes> SliderOrder;
    public static bool Change;

    [HarmonyPatch(typeof(DivertPowerMinigame))]
    public static class DivertPowerMinigamePatch
    {
        [HarmonyPatch(nameof(DivertPowerMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(DivertPowerMinigame __instance)
        {
            if (!Main.IsCursed) return;
            SliderOrder = __instance.SliderOrder.ToList();
            __instance.SliderOrder = __instance.SliderOrder.OrderBy(x => new Random().Next()).ToArray();
            Change = true;
        }
    }

    [HarmonyPatch(typeof(AcceptDivertPowerGame))]
    public static class AcceptDivertPowerGamePatch
    {
        [HarmonyPatch(nameof(AcceptDivertPowerGame.DoSwitch)), HarmonyPrefix]
        public static bool DoSwitchPrefix(AcceptDivertPowerGame __instance)
        {
            if (!Main.IsCursed) return true;
            if (__instance.done) return false;
            __instance.done = true;
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.SwitchSound, false, 1f, null);
            __instance.StartCoroutine(CoDoSwitch(__instance));
            return false;
        }

        public static IEnumerator CoDoSwitch(AcceptDivertPowerGame __instance)
        {
            yield return Effects.Lerp(0.25f, new Action<float>((t) =>
            {
                __instance.Switch.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, 90f, t));
            }));
            __instance.LeftWires[0].SetPosition(1, new Vector3(1.265f, 0f, 0f));
            for (int i = 0; i < __instance.RightWires.Length; i++)
            {
                __instance.RightWires[i].enabled = true;
                __instance.RightWires[i].material.SetColor("_Color", Color.yellow);
            }
            for (int j = 0; j < __instance.LeftWires.Length; j++)
            {
                __instance.LeftWires[j].material.SetColor("_Color", Color.yellow);
            }
            Data[__instance.MyTask.Id].Count++;
            Change = true;
            if (Data[__instance.MyTask.Id].Count >= 5 && __instance.MyNormTask) __instance.MyNormTask.NextStep();
            yield return __instance.CoStartClose(0.75f);
            yield break;
        }
    }

    [HarmonyPatch(typeof(DivertPowerTask))]
    public static class DivertPowerTaskPatch
    {
        [HarmonyPatch(nameof(DivertPowerTask.AppendTaskText)), HarmonyPrefix]
        public static bool AppendTaskTextPrefix(DivertPowerTask __instance, [HarmonyArgument(0)] StringBuilder sb)
        {
            if (!Main.IsCursed) return true;
            if (__instance.TaskType != TaskTypes.DivertPower) return true;

            if (!Data.ContainsKey(__instance.Id)) Data.Add(__instance.Id, new(__instance.TargetSystem));
            if (Change)
            {
                Change = false;
                if (Data[__instance.Id].Count < 4)
                {
                    List<SystemTypes> types = new(SliderOrder);
                    types.Remove(Data[__instance.Id].Target);
                    if (types.Contains(__instance.TargetSystem)) types.Remove(__instance.TargetSystem);
                    __instance.TargetSystem = types.GetRandom();
                }
                else __instance.TargetSystem = Data[__instance.Id].Target;

                __instance.UpdateArrow();
            }

            if (__instance.taskStep > 0)
            {
                if (__instance.IsComplete) sb.Append("<color=#00DD00FF>");
                else sb.Append("<color=#FFFF00FF>");
            }
            if (__instance.taskStep == 0)
            {
                sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(__instance.StartAt));
                sb.Append(": ");
                sb.Append(string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DivertPowerTo),
                                        DestroyableSingleton<TranslationController>.Instance.GetString(Data[__instance.Id].Target)));
            }
            else
            {
                sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(Data[__instance.Id].Target));
                sb.Append(": ");
                sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AcceptDivertedPower));
            }
            sb.Append(" (");
            sb.Append(__instance.taskStep);
            sb.Append("/");
            sb.Append(__instance.MaxStep);
            sb.AppendLine(")");
            if (__instance.taskStep > 0) sb.Append("</color>");

            return false;
        }
    }

    public class CursedDivertPower
    {
        public SystemTypes Target;
        public int Count;
        public CursedDivertPower(SystemTypes target)
        {
            this.Target = target;
            this.Count = 0;
        }
    }
}
