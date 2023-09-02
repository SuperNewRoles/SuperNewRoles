using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedStowArmsTask
{
    [HarmonyPatch(typeof(StowArms))]
    public static class StowArmsPatch
    {
        [HarmonyPatch(nameof(StowArms.Begin)), HarmonyPostfix]
        public static void BeginPostfix(StowArms __instance)
        {
            GameObject pointer = new("Pointer");
            pointer.transform.SetParent(__instance.transform);
            pointer.layer = 4;
            CircleCollider2D circleCollider2D = pointer.AddComponent<CircleCollider2D>();
            circleCollider2D.radius = 0.25f;

            Collider2D[] colliders = __instance.ConsoleId == 1 ? __instance.GunColliders : __instance.RifleColliders;
            foreach (Collider2D collider in colliders)
            {
                Rigidbody2D rigidbody = collider.gameObject.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 0f;
            }
        }

        [HarmonyPatch(nameof(StowArms.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(StowArms __instance)
        {
            if (!Main.IsCursed) return true;
            __instance.cont.Update();
            __instance.transform.FindChild("Pointer").position = __instance.cont.HoverPosition;
            ValidateSelectorActive(__instance.selectorObject.gameObject, Controller.currentTouchType == Controller.TouchType.Joystick);
            __instance.DoUpdate(null, null);
            if (__instance.currentGrabbedObject)
            {
                __instance.selectorObject.position = __instance.currentGrabbedObject.transform.position;
                __instance.selectorObject.SetLocalZ(0f);
                ValidateSelectorActive(__instance.selectorSubobjects[0], false);
                ValidateSelectorActive(__instance.selectorSubobjects[1], __instance.ConsoleId == 1);
                ValidateSelectorActive(__instance.selectorSubobjects[2], __instance.ConsoleId != 1);
                return false;
            }
            __instance.selectorObject.position = VirtualCursor.currentPosition;
            __instance.selectorObject.SetLocalZ(0f);
            ValidateSelectorActive(__instance.selectorSubobjects[0], true);
            ValidateSelectorActive(__instance.selectorSubobjects[1], false);
            ValidateSelectorActive(__instance.selectorSubobjects[2], false);
            return false;
        }

        [HarmonyPatch(nameof(StowArms.DoUpdate)), HarmonyPrefix]
        public static bool DoUpdatePrefix(StowArms __instance)
        {
            if (!Main.IsCursed) return true;
            __instance.currentGrabbedObject = null;
            Collider2D[] colliders = __instance.ConsoleId == 1 ? __instance.GunColliders : __instance.RifleColliders;
            DragSlot[] slots = __instance.ConsoleId == 1 ? __instance.GunsSlots : __instance.RifleSlots;
            Vector3 angles = __instance.ConsoleId == 1 ? new(-0f, -0f, 25f) : new(-0f, -0f, 18.69f);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];
                DragSlot slot = slots[i];
                if (!slot.Occupant)
                {
                    if (Vector2.Distance(collider.transform.position, slot.TargetPosition) < 0.25f && Vector3.Distance(collider.transform.eulerAngles, angles) < 0.5f)
                    {
                        collider.transform.position = slot.TargetPosition;
                        collider.transform.eulerAngles = angles;
                        Object.Destroy(collider.GetComponent<Rigidbody2D>());
                        slot.Occupant = collider;
                        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.PlaceSound, false, 1f, null).pitch = FloatRange.Next(0.8f, 1.2f);
                        __instance.CheckForWin(null, null);
                    }
                }
            }
            return false;
        }

        [HarmonyPatch(nameof(StowArms.CheckForWin)), HarmonyPrefix]
        public static bool CheckForWinPrefix(StowArms __instance)
        {
            if (!Main.IsCursed) return true;
            Collider2D[] colliders = __instance.ConsoleId == 1 ? __instance.GunColliders : __instance.RifleColliders;
            DragSlot[] slots = __instance.ConsoleId == 1 ? __instance.GunsSlots : __instance.RifleSlots;
            Vector3 angles = __instance.ConsoleId == 1 ? new(-0f, -0f, 25f) : new(-0f, -0f, 18.69f);
            bool flag = true;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];
                DragSlot slot = slots[i];
                if (Vector2.Distance(collider.transform.position, slot.TargetPosition) >= 0.25f || Vector3.Distance(collider.transform.eulerAngles, angles) >= 0.5f)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                __instance.MyNormTask.NextStep();
                __instance.StartCoroutine(__instance.CoStartClose(0.75f));
            }
            return false;
        }

        public static void ValidateSelectorActive(GameObject selector, bool shouldBeActive)
        {
            if (selector.gameObject.activeSelf != shouldBeActive)
                selector.gameObject.SetActive(shouldBeActive);
        }
    }
}