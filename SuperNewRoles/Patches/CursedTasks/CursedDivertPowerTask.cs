using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppSystem.Text;
using Rewired;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;
using Random = System.Random;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedDivertPowerTask
{
    public static Dictionary<uint, CursedDivertPower> Data;
    public static List<SystemTypes> SliderOrder;

    [HarmonyPatch(typeof(DivertPowerMinigame))]
    public static class DivertPowerMinigamePatch
    {
        [HarmonyPatch(nameof(DivertPowerMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(DivertPowerMinigame __instance)
        {
            if (!Main.IsCursed) return;
            SliderOrder = new(__instance.SliderOrder);
            __instance.SliderOrder = __instance.SliderOrder.OrderBy(x => new Random().Next()).ToArray();
        }

        [HarmonyPatch(nameof(DivertPowerMinigame.FixedUpdate)), HarmonyPrefix]
        public static bool FixedUpdatePrefix(DivertPowerMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            __instance.myController.Update();
            if (__instance.sliderId >= 0)
            {
                float axisRaw = ReInput.players.GetPlayer(0).GetAxisRaw(__instance.inputJoystick);
                Collider2D collider2D = __instance.Sliders[__instance.sliderId];
                Vector2 vector = collider2D.transform.localPosition;
                if (Mathf.Abs(axisRaw) > 0.01f)
                {
                    __instance.prevHadInput = true;
                    vector.y = __instance.SliderY.Clamp(vector.y + axisRaw * Time.deltaTime * 2f);
                    collider2D.transform.localPosition = vector;
                }
                else
                {
                    if (__instance.prevHadInput && __instance.SliderY.max - vector.y < 0.05f)
                    {
                        ChangeTargetSystem();
                        __instance.MyNormTask.NextStep();
                        __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                        __instance.sliderId = -1;
                        collider2D.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                    }
                    __instance.prevHadInput = false;
                }
            }
            float num = 0f;
            for (int i = 0; i < __instance.Sliders.Length; i++)
            {
                num += __instance.SliderY.ReverseLerp(__instance.Sliders[i].transform.localPosition.y) / __instance.Sliders.Length;
            }
            for (int j = 0; j < __instance.Sliders.Length; j++)
            {
                float num2 = __instance.SliderY.ReverseLerp(__instance.Sliders[j].transform.localPosition.y);
                float num3 = num2 / num / 1.6f;
                __instance.Gauges[j].value = num3 + (Mathf.PerlinNoise(j, Time.time * 51f) - 0.5f) * 0.04f;
                Color color = Color.Lerp(Color.gray, Color.yellow, num2 * num2);
                color.a = num3 < 0.1f ? 0 : 1;
                Vector2 textureOffset = __instance.Wires[j].material.GetTextureOffset("_MainTex");
                textureOffset.x -= Time.fixedDeltaTime * 3f * Mathf.Lerp(0.1f, 2f, num3);
                __instance.Wires[j].material.SetTextureOffset("_MainTex", textureOffset);
                __instance.Wires[j].material.SetColor("_Color", color);
            }
            if (__instance.sliderId < 0) return false;
            Collider2D collider2D2 = __instance.Sliders[__instance.sliderId];
            Vector2 vector2 = collider2D2.transform.localPosition;
            DragState dragState = __instance.myController.CheckDrag(collider2D2);
            if (dragState == DragState.Dragging)
            {
                Vector2 vector3 = (Vector3)__instance.myController.DragPosition - collider2D2.transform.parent.position;
                vector3.y = __instance.SliderY.Clamp(vector3.y);
                vector2.y = vector3.y;
                collider2D2.transform.localPosition = vector2;
                return false;
            }
            if (dragState != DragState.Released) return false;
            if (__instance.SliderY.max - vector2.y < 0.05f)
            {
                ChangeTargetSystem();
                __instance.MyNormTask.NextStep();
                __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                __instance.sliderId = -1;
                collider2D2.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
            }
            return false;

            void ChangeTargetSystem()
            {
                DivertPowerTask task = __instance.MyNormTask.Cast<DivertPowerTask>();
                if (!Data.ContainsKey(task.Id)) Data.Add(task.Id, new(task.TargetSystem));
                List<SystemTypes> types = new(__instance.SliderOrder);
                types.RemoveAll(x => x == task.TargetSystem || x == Data[task.Id].Target);
                task.TargetSystem = types.GetRandom();
            }
        }
    }

    [HarmonyPatch(typeof(AcceptDivertPowerGame))]
    public static class AcceptDivertPowerGamePatch
    {
        [HarmonyPatch(nameof(AcceptDivertPowerGame.DoSwitch)), HarmonyPrefix]
        public static bool CoDoSwitchPrefix(AcceptDivertPowerGame __instance)
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
            if (__instance.MyNormTask)
            {
                if (Data[__instance.MyTask.Id].Count >= 5) __instance.MyNormTask.NextStep();
                else
                {
                    DivertPowerTask task = __instance.MyNormTask.Cast<DivertPowerTask>();
                    if (!Data.ContainsKey(task.Id)) Data.Add(task.Id, new(task.TargetSystem));
                    if (Data[task.Id].Count < 4)
                    {
                        List<SystemTypes> types = new(SliderOrder);
                        types.RemoveAll(x => x == task.TargetSystem || x == Data[task.Id].Target);
                        task.TargetSystem = types.GetRandom();
                    }
                    else task.TargetSystem = Data[task.Id].Target;
                    task.UpdateArrowAndLocation();

                }
            }
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