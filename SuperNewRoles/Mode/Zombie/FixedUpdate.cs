using HarmonyLib;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Mode.Zombie
{
    class FixedUpdate
    {
        /*
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetSkin))]
        class Setcolorskin
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref string skinid)
            {
                SuperNewRolesPlugin.Logger.LogInfo(__instance.NameText().text + ":" + skinid);
            }
        }
        [HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.SetColor))]
        class Setcolor
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref int colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo(__instance.NameText().text+":"+colorid);
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHat))]
        class Sethat
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref string colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[SetHat]"+__instance.NameText().text + ":" + colorid);
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetVisor))]
        class Setvisor
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] ref string colorid)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[SetVisor]" + __instance.NameText().text + ":" + colorid);
            }
        }
        */
        public static float NameChangeTimer;
        public static bool IsStart;
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class TimerUpdate
        {
            public static void Postfix(HudManager __instance)
            {
                if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
                Mode.ModeHandler.HudUpdate(__instance);
                if (IsStart && NameChangeTimer != -10 && AmongUsClient.Instance.AmHost && ModeHandler.IsMode(ModeId.Zombie) && !FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)
                    if (ModeHandler.IsMode(ModeId.Zombie) && IsStart && NameChangeTimer != -10 && AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && !FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)
                    {
                        if (NameChangeTimer >= 0f)
                        {
                            NameChangeTimer -= Time.deltaTime;
                        }
                        else if (NameChangeTimer != -10)
                        {
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                p.RpcSetName("　");
                                if (p.IsImpostor())
                                {
                                    Main.SetZombie(p);
                                }
                            }
                            byte BlueIndex = 1;
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
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
                                    ZombieOptions.ChengeSetting(p);
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
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        p.RpcSetNamePrivate(string.Format(ModTranslation.GetString("ZombieTimerText"), (int)NameChangeTimer + 1));
                    }
                }
                else
                {
                    foreach (int pint in Main.ZombiePlayers)
                    {
                        var p1 = ModHelpers.PlayerById((byte)pint);
                        foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        {
                            if (!p.IsZombie())
                            {
                                if (p != null && p.IsAlive() && !p.Data.Disconnected)
                                {
                                    var DistanceData = Vector2.Distance(p.transform.position, p1.transform.position);
                                    if (DistanceData <= 0.5f)
                                    {
                                        Main.SetZombie(p);
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