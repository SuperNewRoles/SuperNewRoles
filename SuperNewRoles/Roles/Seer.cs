using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using SuperNewRoles.Patches;
using System.Reflection;
namespace SuperNewRoles.Roles
{
    class Seer
    {
        public static PlayerControl seer;
        public static List<PlayerControl> SeerPlayer;
        public static List<Vector3> deadBodyPositions;
        public static bool limitSoulDuration;
        public static float soulDuration;
        public static int mode;

        private static Sprite SoulSprite;
        public static Sprite getSoulSprite()
        {
            if (SoulSprite) return SoulSprite;
            SoulSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Soul.png", 500f);
            return SoulSprite;
        }

        class ExileControllerWrapUpPatch
        {

            [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
            class BaseExileControllerPatch
            {
                public static void Postfix(ExileController __instance)
                {
                    WrapUpPostfix(__instance.exiled);
                }
            }

            [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
            class AirshipExileControllerPatch
            {
                public static void Postfix(AirshipExileController __instance)
                {
                    WrapUpPostfix(__instance.exiled);
                }
            }

            static void WrapUpPostfix(GameData.PlayerInfo exiled)
            {

                // Seer spawn souls
                if (Seer.deadBodyPositions != null && Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && (Seer.mode == 0 || Seer.mode == 2))
                {
                    foreach (Vector3 pos in Seer.deadBodyPositions)
                    {
                        GameObject soul = new GameObject();
                        soul.transform.position = pos;
                        soul.layer = 5;
                        var rend = soul.AddComponent<SpriteRenderer>();
                        rend.sprite = Seer.getSoulSprite();

                        if (Seer.limitSoulDuration)
                        {
                            HudManager.Instance.StartCoroutine(Effects.Lerp(Seer.soulDuration, new Action<float>((p) =>
                            {
                                if (rend != null)
                                {
                                    var tmp = rend.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    rend.color = tmp;
                                }
                                if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                            })));
                        }
                    }
                    Seer.deadBodyPositions = new List<Vector3>();
                }

            }
        }


    }
}

