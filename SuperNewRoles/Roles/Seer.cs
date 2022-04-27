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
                if (Seer.deadBodyPositions != null && Seer.SeerPlayer != null && RoleClass.Seer.SeerPlayer == Seer.SeerPlayer && (Seer.mode == 0 || Seer.mode == 2))
                //
                //TOR�ł�
                //if (Seer.deadBodyPositions != null && Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && (Seer.mode == 0 || Seer.mode == 2)) 
                //�uTOR:seer�v�́uSNR;SeerPlayer�v�Ƃقړ����Ӗ������B
                //����L���O����H��
                //�uSeer.seer�́ASeerPlayer�ł���B�@Seer.seer���ƕ����Ή��ł��Ȃ��̂ŁASeerPlayer�ɂ��Ăق����ł��ˁv�Ƃ̎��B
                //���ׁ̈A[RoleClass.cs]�Ɂupublic static PlayerControl seer;�v��t����K�v�͂Ȃ��B
                //
                //�A���A�uTOR:PlayerControl.LocalPlayer == Seer.seer�v�̕�����[Seer.seer]��u�������邾���ł͕s�\���B
                //[TOR:PlayerControl]��[SNR:RoleClass]
                //[TOR:LocalPlayer]��[SNR:Seer.SeerPlayer]
                //�ƒu��������B
                //


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

                public static void ShowFlash(Color color, float duration = 1f)
                {
                    if (HudManager.Instance == null || HudManager.Instance.FullScreen == null) return;
                    HudManager.Instance.FullScreen.gameObject.SetActive(true);
                    HudManager.Instance.FullScreen.enabled = true;
                    HudManager.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) => {
                        var renderer = HudManager.Instance.FullScreen;

                        if (p < 0.5)
                        {
                            if (renderer != null)
                                renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                        }
                        else
                        {
                            if (renderer != null)
                                renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                        }
                        if (p == 1f && renderer != null) renderer.enabled = false;
                    })));
                }

                public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
                {


                    // Seer show flash and add dead player position

                    int i;
                    for (i = 0; i < 16; i++)
                    {
                        if (Seer.SeerPlayer != null && RoleClass.Seer.SeerPlayer == Seer.SeerPlayer && !Seer.SeerPlayer.Data.IsDead && Seer.SeerPlayer[i] != target && Seer.mode <= 1)

                        //Seer.SeerPlayer != target ���� For �ŏ���!! ���X�g(�z��)���璆�g���o���K�v���L��B�����@1�l�ڂ̃V�[�A�`�@�ŏ�������B
                        //Seer.SeerPlayer[i] != target�ł��Ǝv���@�^�[�Q�b�g�͒m���


                        {
                            ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
                        }
                    }

                    if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(target.transform.position);

                    
                }
            }





        }


    }
}

