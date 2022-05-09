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
using SuperNewRoles.Roles;
using System.Text;



namespace SuperNewRoles.Roles
{
    class Seer
    {


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

                foreach (PlayerControl SeerPlayer in RoleClass.Seer.SeerPlayer)
                {
                    if (RoleClass.Seer.deadBodyPositions != null && RoleClass.Seer.SeerPlayer != null && PlayerControl.LocalPlayer == SeerPlayer && (RoleClass.Seer.mode == 0 || RoleClass.Seer.mode == 2))


                    {
                        foreach (Vector3 pos in RoleClass.Seer.deadBodyPositions)
                        {
                            GameObject soul = new GameObject();
                            soul.transform.position = pos;
                            soul.layer = 5;
                            var rend = soul.AddComponent<SpriteRenderer>();
                            rend.sprite = Seer.getSoulSprite();

                            if (RoleClass.Seer.limitSoulDuration)
                            {
                                HudManager.Instance.StartCoroutine(Effects.Lerp(RoleClass.Seer.soulDuration, new Action<float>((p) =>
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
                        RoleClass.Seer.deadBodyPositions = new List<Vector3>();
                    }
                }
            }

            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
            public static class MurderPlayerPatch
            {
                public static bool resetToCrewmate = false;
                public static bool resetToDead = false;

                public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
                {
                    // Allow everyone to murder players
                    resetToCrewmate = !__instance.Data.Role.IsImpostor;
                    resetToDead = __instance.Data.IsDead;
                    __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
                    __instance.Data.IsDead = false;
                }


                public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
                {

                    foreach (PlayerControl SeerPlayer in RoleClass.Seer.SeerPlayer)
                    {
                        // Seer show flash and add dead player position


                        if (SeerPlayer != null && PlayerControl.LocalPlayer == SeerPlayer && !SeerPlayer.Data.IsDead && SeerPlayer != target && RoleClass.Seer.mode <= 1)
                        {
                            RoleHelpers.ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
                        }


                        if (RoleClass.Seer.deadBodyPositions != null) RoleClass.Seer.deadBodyPositions.Add(target.transform.position);
                    }


                }
            }

        }

    }


}


