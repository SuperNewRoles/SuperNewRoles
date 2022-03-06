using HarmonyLib;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.Zombie
{
    static class main
    {
        public static Color Zombiecolor = new Color32(143, 188, 143,byte.MaxValue);
        public static Color Policecolor = Color.blue;
        public static List<int> ZombiePlayers;
        public static bool IsZombie(this PlayerControl player) {
            if (player.isImpostor() || ZombiePlayers.Contains(player.PlayerId)) return true;
            return false;
        }
        public static void SetZombie(this PlayerControl player)
        {
            player.RpcSetHat("hat_NoHat");
            player.RpcSetSkin("skin_None");

            player.RpcSetColor(2);
            player.RpcSetVisor("visor_pk01_DumStickerVisor");
            player.AllTasksCompleted();
            if (!ZombiePlayers.Contains(player.PlayerId)) ZombiePlayers.Add(player.PlayerId);
        }
        public static void SetNotZombie(this PlayerControl player)
        {
            if (ZombiePlayers.Contains(player.PlayerId)) ZombiePlayers.Remove(player.PlayerId);
        }
        public static bool EndGameCheck(ShipStatus __instance, PlayerStatistics statistics)
        {
            bool IsZombieWin = true;
            foreach(PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!ZombiePlayers.Contains(p.PlayerId) && p.isAlive())
                {
                    IsZombieWin = false;
                }
            }
            if (IsZombieWin)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        public static void ClearAndReload()
        {
            if (AmongUsClient.Instance.AmHost)
            {
                FixedUpdate.IsStart = false;
                ZombiePlayers = new List<int>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    p.getDefaultName();
                }
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class SetHudActivePatch
        {
            public static void Postfix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
            {
                if (ModeHandler.isMode(ModeId.Zombie))
                {
                    HudManager.Instance.ReportButton.ToggleVisible(false);
                    HudManager.Instance.SabotageButton.ToggleVisible(false);
                    HudManager.Instance.KillButton.ToggleVisible(false);
                    HudManager.Instance.ImpostorVentButton.ToggleVisible(false);
                }
            }
        }
        public static void SetTimer()
        {
            FixedUpdate.IsStart = true;
            FixedUpdate.NameChangeTimer = 20f;
        }
    }
}
