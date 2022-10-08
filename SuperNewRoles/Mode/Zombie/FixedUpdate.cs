using HarmonyLib;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Mode.Zombie
{
    class FixedUpdate
    {
        public static float NameChangeTimer;
        public static bool IsStart;
        public static void ZombieTimerUpdate(HudManager __instance)
        {
            ModeHandler.HudUpdate(__instance);
            if (ModeHandler.IsMode(ModeId.Zombie) && IsStart && NameChangeTimer != -10 && AmongUsClient.Instance.AmHost && !FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)
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
                            p.HideName();
                            if (p.IsImpostor())
                            {
                                Main.SetZombie(p);
                            }
                        }
                        foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        {
                            if (!p.IsZombie())
                            {
                                ZombieOptions.ChengeSetting(p);
                            }
                        }
                        NameChangeTimer = -10;
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