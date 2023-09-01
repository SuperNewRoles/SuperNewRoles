using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedToiletTask
{
    public static Dictionary<uint, int> Count;
    public static bool Animation;

    [HarmonyPatch(typeof(ToiletMinigame))]
    public static class ToiletMinigamePatch
    {
        [HarmonyPatch(nameof(ToiletMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(ToiletMinigame __instance)
        {
            if (!Main.IsCursed) return;
            if (!Count.ContainsKey(__instance.MyTask.Id)) Count.Add(__instance.MyTask.Id, 0);
            Animation = false;
        }

        [HarmonyPatch(nameof(ToiletMinigame.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(ToiletMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None || __instance.pressure >= 1f) return false;
            __instance.pressure -= Time.deltaTime * __instance.plungeScale / 2f;
            if (__instance.pressure < 0f) __instance.pressure = 0f;
            __instance.controller.Update();
            Vector3 localPosition = __instance.Stick.transform.localPosition;
            if (Controller.currentTouchType == Controller.TouchType.Joystick)
            {
                float axisRaw = ReInput.players.GetPlayer(0).GetAxisRaw(14);
                if (axisRaw < 0f)
                {
                    if (__instance.controllerStickPos > 1f + axisRaw)
                        __instance.controllerStickPos = Mathf.Lerp(__instance.controllerStickPos, 1f + axisRaw, Time.deltaTime * 30f);
                }
                else if (axisRaw > 0f)
                {
                    if (__instance.controllerStickPos < axisRaw)
                        __instance.controllerStickPos = Mathf.Lerp(__instance.controllerStickPos, axisRaw, Time.deltaTime * 30f);
                }
                else __instance.controllerStickPos = Mathf.Lerp(__instance.controllerStickPos, 1f, Time.deltaTime);
                localPosition.y = __instance.StickRange.Lerp(__instance.controllerStickPos);
                __instance.Stick.transform.localPosition = localPosition;
                if (__instance.lastY > localPosition.y)
                {
                    if (!Animation) __instance.pressure += (__instance.lastY - localPosition.y) * __instance.plungeScale;
                    if (Constants.ShouldPlaySfx() && !__instance.plungerSource.isPlaying)
                    {
                        __instance.plungerSource.clip = __instance.plungeSounds.ToList().GetRandom();
                        __instance.plungerSource.Play();
                        VibrationManager.Vibrate(0.3f, 0.3f, 0.2f, VibrationManager.VibrationFalloff.Linear, null, false);
                    }
                }
                __instance.lastY = localPosition.y;
            }
            else
            {
                switch (__instance.controller.CheckDrag(__instance.Stick))
                {
                    case DragState.TouchStart:
                        __instance.lastY = localPosition.y;
                        break;
                    case DragState.Dragging:
                        localPosition.y = __instance.StickRange.Clamp(__instance.StickRange.max + (__instance.controller.DragPosition.y - __instance.controller.DragStartPosition.y));
                        __instance.Stick.transform.localPosition = localPosition;
                        if (__instance.lastY > localPosition.y)
                        {
                            if (!Animation) __instance.pressure += (__instance.lastY - localPosition.y) * __instance.plungeScale;
                            if (Constants.ShouldPlaySfx() && !__instance.plungerSource.isPlaying)
                            {
                                __instance.plungerSource.clip = __instance.plungeSounds.ToList().GetRandom();
                                __instance.plungerSource.Play();
                            }
                        }
                        __instance.lastY = localPosition.y;
                        break;
                    case DragState.Released:
                        localPosition.y = Mathf.Lerp(localPosition.y, __instance.StickRange.max, Time.deltaTime);
                        __instance.Stick.transform.localPosition = localPosition;
                        break;
                }
            }
            __instance.Plunger.sprite = (localPosition.y < -0.75f) ? __instance.PlungerDown : __instance.PlungerUp;
            __instance.Needle.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, -230f, __instance.pressure));
            if (__instance.pressure >= 1f) __instance.StartCoroutine(__instance.Finish());
            return false;
        }

        [HarmonyPatch(nameof(ToiletMinigame.Finish)), HarmonyPrefix]
        public static bool FinishPrefix(ToiletMinigame __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!Main.IsCursed) return true;
            __result = Finish(__instance).WrapToIl2Cpp();
            return false;
        }

        public static IEnumerator Finish(ToiletMinigame __instance)
        {
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.flushSound, false, 1f, null);
            Animation = true;
            Count[__instance.MyTask.Id] += 1;
            __instance.pressure = 0f;
            VibrationManager.Vibrate(2.5f, 2.5f, 0f, VibrationManager.VibrationFalloff.None, __instance.flushSound, false);
            if (Count[__instance.MyTask.Id] >= 5) __instance.MyNormTask.NextStep();
            yield return Effects.All(Effects.Shake(__instance.Pipes.transform, 0.65f, 0.05f, true), Effects.Rotate2D(__instance.Needle.transform, 230f, 0f, 0.6f));
            if (Count[__instance.MyTask.Id] >= 5) __instance.Close();
            Animation = false;
            yield break;
        }
    }
}
