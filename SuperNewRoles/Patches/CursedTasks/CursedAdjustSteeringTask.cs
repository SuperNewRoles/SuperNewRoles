using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedAdjustSteeringTask
{
    [HarmonyPatch(typeof(NavigationMinigame))]
    public static class NavigationMinigamePatch
    {
        public static GameObject Target;
        public static float Timer;

        [HarmonyPatch(nameof(NavigationMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(NavigationMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.transform.Find("Background").GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cursed.Radar.png", 100f);
            Target = Object.Instantiate(__instance.transform.Find("Border").gameObject, __instance.transform);
            Target.name = "Target";
            Target.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cursed.Target.png", 100f);

            Timer = Random.Range(0f, Mathf.PI * 2);
            float cos = Mathf.Cos(Timer);
            float sin = Mathf.Sin(Timer);
            __instance.half = new(cos, sin);
            Target.transform.localPosition = new(cos, sin, 0f);
        }

        [HarmonyPatch(nameof(NavigationMinigame.FixedUpdate)), HarmonyPrefix]
        public static bool FixedUpdatePostfix(NavigationMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            if (__instance.MyNormTask && __instance.MyNormTask.IsComplete) return false;

            Timer += Time.fixedDeltaTime;
            float cos = Mathf.Cos(Timer);
            float sin = Mathf.Sin(Timer);
            __instance.half = new(cos, sin);
            Target.transform.localPosition = new(cos, sin, 0f);

            __instance.myController.Update();
            if (Controller.currentTouchType != Controller.TouchType.Joystick)
            {
                switch (__instance.myController.CheckDrag(__instance.hitbox))
                {
                    case DragState.TouchStart:
                    case DragState.Dragging:
                        Vector2 drag = __instance.myController.DragPosition;
                        Vector3 local = drag - (Vector2)__instance.transform.position;
                        __instance.crossHair = local;
                        if (__instance.crossHair.magnitude < 2.2f)
                        {
                            local.z = -2f;
                            __instance.CrossHairImage.transform.localPosition = local;
                            Vector2 div = drag - (Vector2)(__instance.TwoAxisImage.transform.position - __instance.TwoAxisImage.bounds.size / 2f);
                            __instance.TwoAxisImage.material.SetVector("_CrossHair", div.Div(__instance.TwoAxisImage.bounds.size));
                        }
                        break;
                    case DragState.Released:
                        if ((__instance.crossHair - __instance.half).magnitude < 0.025f)
                        {
                            __instance.StartCoroutine(__instance.CompleteGame());
                            __instance.MyNormTask.NextStep();
                        }
                        break;
                    default:
                        break;
                }
            }
            // コントローラー対応はめんどくさそうだったので放置!
            return false;
        }

        [HarmonyPatch(nameof(NavigationMinigame.CompleteGame)), HarmonyPrefix]
        public static bool CompleteGamePrefix(NavigationMinigame __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!Main.IsCursed) return true;
            __result = CompleteGame(__instance).WrapToIl2Cpp();
            return false;
        }

        public static IEnumerator CompleteGame(NavigationMinigame __instance)
        {
            WaitForSeconds wait = new(0.1f);
            Color green = new(0f, 0.8f, 0f, 1f);
            Color32 yellow = new(byte.MaxValue, 202, 0, byte.MaxValue);
            Vector2 half = __instance.half + (Vector2)__instance.transform.position;
            Vector3 local = __instance.half;
            local.z = -2f;
            __instance.CrossHairImage.transform.localPosition = local;
            Vector2 div = half - (Vector2)(__instance.TwoAxisImage.transform.position - __instance.TwoAxisImage.bounds.size / 2f);
            __instance.TwoAxisImage.material.SetVector("_CrossHair", div.Div(__instance.TwoAxisImage.bounds.size));
            __instance.CrossHairImage.color = yellow;
            __instance.TwoAxisImage.material.SetColor("_CrossColor", yellow);
            yield return wait;
            __instance.CrossHairImage.color = Color.white;
            __instance.TwoAxisImage.material.SetColor("_CrossColor", Color.white);
            yield return wait;
            __instance.CrossHairImage.color = yellow;
            __instance.TwoAxisImage.material.SetColor("_CrossColor", yellow);
            yield return wait;
            __instance.CrossHairImage.color = Color.white;
            __instance.TwoAxisImage.material.SetColor("_CrossColor", Color.white);
            yield return wait;
            __instance.CrossHairImage.color = green;
            __instance.TwoAxisImage.material.SetColor("_CrossColor", green);
            yield return __instance.CoStartClose(0.75f);
            yield break;
        }
    }

    [HarmonyPatch(typeof(AdjustSteeringGame))]
    public static class AdjustSteeringGamePatch
    {
        public static float ThrustTimer;
        public static float SteeringTimer;

        [HarmonyPatch(nameof(AdjustSteeringGame.Begin)), HarmonyPostfix]
        public static void BeginPostfix()
        {
            if (!Main.IsCursed) return;
            ThrustTimer = Random.Range(0f, 1f);
            SteeringTimer = Random.Range(0f, 1f);
        }

        [HarmonyPatch(nameof(AdjustSteeringGame.Update)), HarmonyPostfix]
        public static void UpdatePostfix(AdjustSteeringGame __instance)
        {
            if (!Main.IsCursed) return;
            float time = Time.fixedDeltaTime / 2;
            if (!__instance.thrustLocked)
            {
                if (ThrustTimer > 1f) ThrustTimer = 0f;
                ThrustTimer += time;
            }
            if (!__instance.steeringLocked)
            {
                if (SteeringTimer > 1f) SteeringTimer = 0f;
                SteeringTimer += time;
            }

            __instance.TargetThrustY = Mathf.Lerp(AdjustSteeringGame.ThrustRange.min, AdjustSteeringGame.ThrustRange.max, ThrustTimer);
            __instance.ThrustTarget.transform.localPosition = new Vector3(-3.25f, __instance.TargetThrustY);
            __instance.TargetSteeringRot = Mathf.Lerp(AdjustSteeringGame.SteeringRange.min, AdjustSteeringGame.SteeringRange.max, SteeringTimer);
            __instance.SteeringTarget.transform.localEulerAngles = new Vector3(0f, 0f, __instance.TargetSteeringRot);
        }
    }
}