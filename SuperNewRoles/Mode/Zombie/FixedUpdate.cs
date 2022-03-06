using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.Zombie
{
    class FixedUpdate {
        /*
        [HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.SetColor))]
        class Setcolor
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref int colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo(__instance.nameText.text+":"+colorid);
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHat))]
        class Sethat
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref string colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[SetHat]"+__instance.nameText.text + ":" + colorid);
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetVisor))]
        class Setvisor
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref string colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[SetVisor]" + __instance.nameText.text + ":" + colorid);
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPet))]
        class SetPet
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref string colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[SetPet]" + __instance.nameText.text + ":" + colorid);
            }
        }
        */
        public static float NameChangeTimer;
        public static bool IsStart;
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class TimerUpdate
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance.AmHost && ModeHandler.isMode(ModeId.Zombie) && AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && !HudManager.Instance.isIntroDisplayed)
                {
                    if (NameChangeTimer >= 0f)
                    {
                        NameChangeTimer -= Time.deltaTime;
                    } else
                    {
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            p.RpcSetName(p.getDefaultName());
                            if (!p.IsZombie())
                            {
                                p.RpcSetVisor("visor_EmptyVisor");

                                p.RpcSetColor(1);
                                p.RpcSetHat("hat_police");
                                p.RpcSetSkin("skin_Police");
                            }
                            NameChangeTimer = -10;
                        }
                    }
                }
            }
        }
        public static void Update()
        {
            if (!IsStart) return;
            byte BlueIndex = 1;
            byte greenIndex = 2;
            if (main.ZombiePlayers.Count == 0)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor())
                    {
                        main.ZombiePlayers.Add(p.PlayerId);
                    }
                }
            }
            if (NameChangeTimer >= 0f)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    p.RpcSetNamePrivate(string.Format(ModTranslation.getString("ZombieTimerText"),(int)NameChangeTimer+1));
                    foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                    {
                        if (p2.PlayerId != p.PlayerId)
                        {
                            p2.RpcSetNamePrivate("Playing on SuperNewRoles!", p);
                        }
                    }
                }
            } else
            {
                foreach(PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isAlive())
                    {
                        foreach(PlayerControl p3 in PlayerControl.AllPlayerControls)
                        {
                            if (!p3.IsZombie())
                            {
                                foreach (int pint in main.ZombiePlayers)
                                {
                                    var p4 = ModHelpers.playerById((byte)pint);
                                    if (p4 != null && p4.isAlive())
                                    {
                                        var DistanceData = Vector3.Distance(p3.transform.position,p4.transform.position);
                                        SuperNewRolesPlugin.Logger.LogInfo("DISTANCE:"+ DistanceData);
                                        if (DistanceData <= 0.5f) {
                                            main.SetZombie(p3);
                                        }
                                    }
                                }
                            }
                        }

                        //コスメ設定
                        if (p.IsZombie())
                        {
                            p.RpcSetHat("hat_NoHat");
                            p.RpcSetSkin("skin_None");

                            p.RpcSetColor(greenIndex);
                            p.RpcSetVisor("visor_pk01_DumStickerVisor");
                        }
                        //コスメ設定終わり

                    }
                }
            }
        }
    }
}
