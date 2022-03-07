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
                if (IsStart && NameChangeTimer != -10 && AmongUsClient.Instance.AmHost && ModeHandler.isMode(ModeId.Zombie) && AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && !HudManager.Instance.isIntroDisplayed)
                {
                    HideAndSeek.Patch.RepairSystemPatch.Postfix(PlayerControl.LocalPlayer);
                    if (NameChangeTimer >= 0f)
                    {
                        NameChangeTimer -= Time.deltaTime;
                    } else if(NameChangeTimer != -10)
                    {

                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            if (p.isImpostor())
                            {
                                main.SetZombie(p);
                            }
                        }
                        byte BlueIndex = 1;
                        byte greenIndex = 2;
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                p2.RpcSetNamePrivate("　",p);//p.getDefaultName(),p2);
                            }
                            if (!p.IsZombie())
                            {
                                /*
                                p.UncheckSetVisor("visor_EmptyVisor");
                                */
                                p.RpcSetColor(BlueIndex);
                                /*
                                p.RpcSetHat("hat_police");
                                p.RpcSetSkin("skin_Police");
                                */
                            }
                        }
                        NameChangeTimer = -10;
                    }
                }
            }
        }
        public static int FixedUpdateTimer = 0;
        public static void Update()
        {
            if (!IsStart || !AmongUsClient.Instance.AmHost) return;
            FixedUpdateTimer--;
            if (FixedUpdateTimer <= 0)
            {
                FixedUpdateTimer = 15;
                if (NameChangeTimer >= 0f)
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        p.RpcSetNamePrivate(string.Format(ModTranslation.getString("ZombieTimerText"), (int)NameChangeTimer + 1));
                    }
                }
                else
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.isAlive())
                        {
                            foreach (PlayerControl p3 in PlayerControl.AllPlayerControls)
                            {
                                if (!p3.IsZombie())
                                {
                                    foreach (int pint in main.ZombiePlayers)
                                    {
                                        var p4 = ModHelpers.playerById((byte)pint);
                                        if (p4 != null && p4.isAlive())
                                        {
                                            var DistanceData = Vector2.Distance(p3.transform.position, p4.transform.position);
                                            SuperNewRolesPlugin.Logger.LogInfo("DISTANCE:" + DistanceData);
                                            if (DistanceData <= 0.5f)
                                            {
                                                main.SetZombie(p3);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }
    }
}
