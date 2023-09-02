using System.Linq;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedEmptyGarbageTask
{
    [HarmonyPatch(typeof(EmptyGarbageMinigame))]
    public static class EmptyGarbageMinigamePatch
    {
        public static int Count;

        [HarmonyPatch(nameof(EmptyGarbageMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(EmptyGarbageMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.NumObjects = 100;
            Count = 0;
        }

        [HarmonyPatch(nameof(EmptyGarbageMinigame.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(EmptyGarbageMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None) return false;
            __instance.controller.Update();
            Vector3 localPosition = __instance.Handle.transform.localPosition;
            float num = __instance.HandleRange.ReverseLerp(localPosition.y);
            if (Controller.currentTouchType == Controller.TouchType.Joystick)
            {
                if (!__instance.finished)
                {
                    Player player = ReInput.players.GetPlayer(0);
                    if (__instance.touchpad.IsTouching()) __instance.leverInput = -__instance.touchpad.GetTouchVector().y;
                    else __instance.leverInput = Mathf.Clamp01(-player.GetAxis(17));
                    localPosition.y = __instance.HandleRange.Lerp(1f - __instance.leverInput);
                    num = __instance.HandleRange.ReverseLerp(localPosition.y);
                    if (__instance.leverInput >= 0.01f)
                    {
                        __instance.hadInput = true;
                        if (num <= 0.5f && __instance.Blocker.enabled)
                        {
                            if (Constants.ShouldPlaySfx())
                            {
                                SoundManager.Instance.PlaySound(__instance.LeverDown, false, 1f, null);
                                SoundManager.Instance.PlaySound(__instance.GrinderStart, false, 0.8f, null);
                                SoundManager.Instance.StopSound(__instance.GrinderEnd);
                                SoundManager.Instance.StopSound(__instance.GrinderLoop);
                            }
                            __instance.Blocker.enabled = false;
                            __instance.StopCoroutines();
                            __instance.popCoroutine = __instance.StartCoroutine(__instance.PopObjects());
                            __instance.animateCoroutine = __instance.StartCoroutine(__instance.AnimateObjects());
                        }
                    }
                    else
                    {
                        if (__instance.hadInput)
                        {
                            if (!__instance.Blocker.enabled)
                            {
                                __instance.Blocker.enabled = true;
                                if (Constants.ShouldPlaySfx())
                                {
                                    SoundManager.Instance.PlaySound(__instance.LeverUp, false, 1f, null);
                                    SoundManager.Instance.StopSound(__instance.GrinderStart);
                                    SoundManager.Instance.StopSound(__instance.GrinderLoop);
                                    SoundManager.Instance.PlaySound(__instance.GrinderEnd, false, 0.8f, null);
                                }
                            }
                            if (!__instance.finished)
                            {
                                if (__instance.Objects.All(x => !x))
                                {
                                    Count++;
                                    if (Count >= 3)
                                    {
                                        __instance.MyNormTask.NextStep();
                                        __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                                        __instance.finished = true;
                                    }
                                    else Reset();
                                }
                            }
                        }
                        __instance.hadInput = false;
                    }
                }
            }
            else
            {
                switch (__instance.controller.CheckDrag(__instance.Handle))
                {
                    case DragState.NoTouch:
                        localPosition.y = Mathf.Lerp(localPosition.y, __instance.HandleRange.max, num + Time.deltaTime * 15f);
                        break;
                    case DragState.Dragging:
                        if (!__instance.finished)
                        {
                            if (num > 0.5f)
                            {
                                Vector2 vector = __instance.controller.DragPosition - (Vector2)__instance.transform.position;
                                float num2 = __instance.HandleRange.ReverseLerp(__instance.HandleRange.Clamp(vector.y));
                                localPosition.y = __instance.HandleRange.Lerp(num2 / 2f + 0.5f);
                            }
                            else
                            {
                                localPosition.y = Mathf.Lerp(localPosition.y, __instance.HandleRange.min, num + Time.deltaTime * 15f);
                                if (__instance.Blocker.enabled)
                                {
                                    if (Constants.ShouldPlaySfx())
                                    {
                                        SoundManager.Instance.PlaySound(__instance.LeverDown, false, 1f, null);
                                        SoundManager.Instance.PlaySound(__instance.GrinderStart, false, 0.8f, null);
                                        SoundManager.Instance.StopSound(__instance.GrinderEnd);
                                        SoundManager.Instance.StopSound(__instance.GrinderLoop);
                                    }
                                    __instance.Blocker.enabled = false;
                                    __instance.StopCoroutines();
                                    __instance.popCoroutine = __instance.StartCoroutine(__instance.PopObjects());
                                    __instance.animateCoroutine = __instance.StartCoroutine(__instance.AnimateObjects());
                                }
                            }
                        }
                        break;
                    case DragState.Released:
                        if (!__instance.Blocker.enabled)
                        {
                            __instance.Blocker.enabled = true;
                            if (Constants.ShouldPlaySfx())
                            {
                                SoundManager.Instance.PlaySound(__instance.LeverUp, false, 1f, null);
                                SoundManager.Instance.StopSound(__instance.GrinderStart);
                                SoundManager.Instance.StopSound(__instance.GrinderLoop);
                                SoundManager.Instance.PlaySound(__instance.GrinderEnd, false, 0.8f, null);
                            }
                        }
                        if (!__instance.finished)
                        {
                            if (__instance.Objects.All(x => !x))
                            {
                                Count++;
                                if (Count >= 3)
                                {
                                    __instance.MyNormTask.NextStep();
                                    __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                                    __instance.finished = true;
                                }
                                else Reset();
                            }
                        }
                        break;
                }
            }
            if (Constants.ShouldPlaySfx() && !__instance.Blocker.enabled && !SoundManager.Instance.SoundIsPlaying(__instance.GrinderStart))
                SoundManager.Instance.PlaySound(__instance.GrinderLoop, true, 0.8f, SoundManager.Instance.SfxChannel);
            __instance.Handle.transform.localPosition = localPosition;
            Vector3 localScale = __instance.Bars.transform.localScale;
            localScale.y = __instance.HandleRange.ChangeRange(localPosition.y, -1f, 1f);
            __instance.Bars.transform.localScale = localScale;
            return false;

            void Reset()
            {
                int i = 0;
                __instance.Objects = new(__instance.NumObjects);
                RandomFill<SpriteRenderer> random = new();
                if (__instance.MyNormTask.StartAt == SystemTypes.LifeSupp) random.Set(__instance.GarbagePrefabs.Union(__instance.LeafPrefabs).IEnumerableToIl2Cpp());
                else
                {
                    if (__instance.MyNormTask != null && __instance.MyNormTask.taskStep == 0)
                    {
                        if (__instance.MyNormTask.TaskType == TaskTypes.EmptyChute) random.Set(__instance.GarbagePrefabs.IEnumerableToIl2Cpp());
                        else random.Set(__instance.LeafPrefabs.IEnumerableToIl2Cpp());
                    }
                    else
                    {
                        random.Set(__instance.GarbagePrefabs.Union(__instance.LeafPrefabs).IEnumerableToIl2Cpp());
                        while (i < __instance.SpecialObjectPrefabs.Length)
                        {
                            SpriteRenderer sprite = __instance.Objects[i] = Object.Instantiate(__instance.SpecialObjectPrefabs[i]);
                            sprite.transform.SetParent(__instance.transform);
                            sprite.transform.localPosition = __instance.SpawnRange.Next();
                            i++;
                        }
                    }
                }
                while (i < __instance.Objects.Length)
                {
                    SpriteRenderer sprite = __instance.Objects[i] = Object.Instantiate(random.Get());
                    sprite.transform.SetParent(__instance.transform);
                    Vector3 vector = __instance.SpawnRange.Next();
                    vector.z = FloatRange.Next(-0.5f, 0.5f);
                    sprite.transform.localPosition = vector;
                    sprite.color = Color.Lerp(Color.white, Color.black, (vector.z + 0.5f) * 0.7f);
                    i++;
                }
            }
        }
    }

    [HarmonyPatch(typeof(AirshipGarbageGame))]
    public static class AirshipGarbageGamePatch
    {
        public static int Count;
        public static bool IsReset;

        [HarmonyPatch(nameof(AirshipGarbageGame.Begin)), HarmonyPostfix]
        public static void BeginPostfix()
        {
            if (!Main.IsCursed) return;
            Count = 0;
            IsReset = false;
        }

        [HarmonyPatch(nameof(AirshipGarbageGame.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(AirshipGarbageGame __instance)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amOpening) return false;
            if (__instance.amClosing != Minigame.CloseState.None) return false;
            __instance.controller.Update();
            if (Controller.currentTouchType == Controller.TouchType.Joystick)
            {
                Player player = ReInput.players.GetPlayer(0);
                __instance.handCursorObject.position = __instance.can.Handle.transform.position;
                if (player.GetButton(24) && player.GetButton(21))
                {
                    if (!__instance.grabbedHands.activeSelf)
                    {
                        __instance.grabbedHands.SetActive(true);
                        __instance.waitingHands.SetActive(false);
                        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.grabSound, false, 1f, null);
                    }
                    __instance.can.Handle.sprite = __instance.PulledHandle;
                    Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
                    if (axis2DRaw.magnitude > 0.9f)
                    {
                        if (!__instance.prevHadLeftInput) __instance.can.Body.velocity = axis2DRaw.normalized * 6f;
                        __instance.prevHadLeftInput = true;
                    }
                    else __instance.prevHadLeftInput = false;
                }
                else
                {
                    if (!__instance.waitingHands.activeSelf)
                    {
                        __instance.grabbedHands.SetActive(false);
                        __instance.waitingHands.SetActive(true);
                    }
                    __instance.can.Handle.sprite = __instance.RelaxeHandle;
                }
            }
            else
            {
                DragState dragState = __instance.controller.CheckDrag(__instance.can.Hitbox);
                if (dragState != DragState.TouchStart)
                {
                    if (dragState != DragState.Dragging) __instance.can.Handle.sprite = __instance.RelaxeHandle;
                    else
                    {
                        __instance.can.Handle.sprite = __instance.PulledHandle;
                        Vector2 vector = __instance.controller.DragPosition - (Vector2)__instance.can.Handle.transform.position;
                        __instance.can.Body.velocity = 10f * vector;
                    }
                }
                else if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.grabSound, false, 1f, null);
            }
            if (!__instance.can.Body.IsTouching(__instance.can.Success) && !IsReset)
            {
                Count++;
                if (Count >= 3)
                {
                    __instance.MyNormTask.NextStep();
                    __instance.StartCoroutine(__instance.CoStartClose(0.7f));
                }
                else Reset();
                return false;
            }
            IsReset = false;
            return false;

            void Reset()
            {
                // handles
                // garbage_bin1front
                Object.Destroy(__instance.can.gameObject);
                __instance.can = Object.Instantiate(__instance.GarbagePrefabs[__instance.ConsoleId], __instance.transform);
                IsReset = true;
                /*
                Transform can = __instance.can.transform;
                Transform handles = can.Find("handles");
                Transform garbage_bin1front = can.Find("garbage_bin1front");

                handles.eulerAngles = garbage_bin1front.eulerAngles;
                handles.localPosition = garbage_bin1front.localPosition + handles.up * 0.5f;
                //*/
            }
        }
    }
}