using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedShieldTask
{
    [HarmonyPatch(typeof(ShieldMinigame))]
    public static class ShieldMinigamePatch
    {
        [HarmonyPatch(nameof(ShieldMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(ShieldMinigame __instance)
        {
            if (!Main.IsCursed) return;
            GameObject copyshield = Object.Instantiate(__instance.Shields[0].gameObject);
            for (int i = 0; i < __instance.Shields.Length; i++) Object.Destroy(__instance.Shields[i].gameObject);
            __instance.Shields = new SpriteRenderer[9];
            for (int i = 0; i < __instance.Shields.Length; i++)
            {
                GameObject shield = Object.Instantiate(copyshield);
                shield.name = $"ShieldButton {i + 1}";
                shield.transform.SetParent(__instance.transform);
                shield.transform.localPosition = new(-1 + (i % 3), 1 - (i / 3), -1);
                shield.GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cursed.Tetragon.png", 100f);
                Object.Destroy(shield.GetComponent<PolygonCollider2D>());
                BoxCollider2D box = shield.AddComponent<BoxCollider2D>();
                box.size = new(0.88f, 0.88f);
                ButtonBehavior button = shield.GetComponent<ButtonBehavior>();
                button.colliders = new[] { box };
                button.OnClick = new();
                int key = i;
                button.OnClick.AddListener((Action)(() => __instance.ToggleShield(key)));

                __instance.Shields[i] = shield.GetComponent<SpriteRenderer>();
            }
            Object.Destroy(copyshield);
            __instance.UpdateButtons();
        }

        [HarmonyPatch(nameof(ShieldMinigame.ToggleShield)), HarmonyPrefix]
        public static bool ToggleShieldPrefix(ShieldMinigame __instance, int i)
        {
            if (!Main.IsCursed) return true;
            if (__instance.MyNormTask.IsComplete) return false;
            ToggleShield(i);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.MyNormTask.Data[i] == 1 ? __instance.ShieldOnSound : __instance.ShieldOffSound, false, 1f, null);

            switch (i)
            {
                // 上段
                case 0:
                    ToggleShield(1);
                    ToggleShield(3);
                    break;
                case 1:
                    ToggleShield(0);
                    ToggleShield(2);
                    ToggleShield(4);
                    break;
                case 2:
                    ToggleShield(1);
                    ToggleShield(5);
                    break;
                // 中段
                case 3:
                    ToggleShield(0);
                    ToggleShield(4);
                    ToggleShield(6);
                    break;
                case 4:
                    ToggleShield(1);
                    ToggleShield(3);
                    ToggleShield(5);
                    ToggleShield(7);
                    break;
                case 5:
                    ToggleShield(2);
                    ToggleShield(4);
                    ToggleShield(8);
                    break;
                // 下段
                case 6:
                    ToggleShield(3);
                    ToggleShield(7);
                    break;
                case 7:
                    ToggleShield(4);
                    ToggleShield(6);
                    ToggleShield(8);
                    break;
                case 8:
                    ToggleShield(5);
                    ToggleShield(7);
                    break;
            }

            if (__instance.MyNormTask.Data.All((byte b) => b == 1))
            {
                __instance.MyNormTask.NextStep();
                __instance.StartCoroutine(__instance.CoStartClose(0.75f));
                if (ShipStatus.Instance.ShieldsImages.Length != 0 && !ShipStatus.Instance.ShieldsImages[0].IsPlaying())
                    PlayerControl.LocalPlayer.RpcPlayAnimation(1);
            }
            return false;

            void ToggleShield(int number) => __instance.MyNormTask.Data[number] = (byte)(__instance.MyNormTask.Data[number] == 0 ? 1 : 0);
        }

        [HarmonyPatch(nameof(ShieldMinigame.UpdateButtons)), HarmonyPrefix]
        public static bool UpdateButtonsPrefix(ShieldMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            int num = 0;
            for (int i = 0; i < __instance.MyNormTask.Data.Length; i++)
            {
                bool flag = __instance.MyNormTask.Data[i] == 0;
                if (!flag) num++;
                if (__instance.Shields.Count <= i) continue;
                __instance.Shields[i].color = flag ? __instance.OffColor : __instance.OnColor;
            }
            if (__instance.MyNormTask.Data.All((byte b) => b == 1))
            {
                __instance.Gauge.transform.Rotate(0f, 0f, Time.fixedDeltaTime * 45f);
                __instance.Gauge.color = new Color(1f, 1f, 1f, 1f);
                return false;
            }
            float num2 = Mathf.Lerp(0.1f, 0.5f, num / 9f);
            __instance.Gauge.color = new Color(1f, num2, num2, 1f);
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.Initialize)), HarmonyPostfix]
        public static void InitializePostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.TaskType != TaskTypes.PrimeShields) return;
            __instance.Data = new byte[9];
            for (int i = 0; i < __instance.Data.Length; i++) __instance.Data[i] = (byte)(BoolRange.Next(0.7f) ? 0 : 1);
            if (__instance.Data.All((byte b) => b == 1)) __instance.Data[Random.Range(0, __instance.Data.Length)] = 0;
        }
    }
}
