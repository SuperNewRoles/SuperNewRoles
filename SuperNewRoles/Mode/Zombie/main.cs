using System;
using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patch;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.Zombie
{
    static class Main
    {
        public static Color Zombiecolor = new Color32(143, 188, 143, byte.MaxValue);
        public static Color Policecolor = Color.blue;
        public static List<int> ZombiePlayers;
        public static bool IsZombie(this PlayerControl player)
        {
            try
            {
                return player.Data.Disconnected || player.Data.Role.IsImpostor || ZombiePlayers.Contains(player.PlayerId);
            }
            catch { return false; }
        }
        public static void CountTaskZombie(GameData __instance)
        {
            __instance.TotalTasks = 0;
            __instance.CompletedTasks = 0;
            for (int i = 0; i < __instance.AllPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                if (!playerInfo.Object.IsZombie())
                {
                    var (playerCompleted, playerTotal) = TaskCount.TaskDate(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
            }
        }
        public static void SetZombie(this PlayerControl player)
        {
            //player.RpcSetHat("");
            /*
            player.RpcSetSkin("");
            */
            player.RpcSetColor(2);
            /*
            player.UncheckSetVisor("visor_pk01_DumStickerVisor");
            */

            foreach (PlayerTask task in player.myTasks)
            {
                task.Complete();
            }
            /*
            var Data = PlayerControl.GameOptions;
            Data.CrewLightMod = ZombieOptions.ZombieLight.GetFloat();
            RPCHelper.RPCGameOptionsPrivate(Data,player);
            */
            if (!ZombiePlayers.Contains(player.PlayerId)) ZombiePlayers.Add(player.PlayerId);
            ZombieOptions.ChengeSetting(player);
        }
        public static void SetNotZombie(this PlayerControl player)
        {
            if (ZombiePlayers.Contains(player.PlayerId)) ZombiePlayers.Remove(player.PlayerId);
        }
        public static bool EndGameCheck(ShipStatus __instance, PlayerStatistics statistics)
        {
            bool IsZombieWin = true;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!ZombiePlayers.Contains(p.PlayerId) && p.IsAlive())
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
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                p.SetHat("", 0);
            }
            /*
            PlayerControl.GameOptions.ImpostorLightMod = ZombieOptions.ZombieLight.GetFloat();
            CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
            */
            SyncSetting.OptionData = PlayerControl.GameOptions;
            ZombieOptions.ZombieLight = ZombieOptions.ZombieLightOption.GetFloat();
            ZombieOptions.ZombieSpeed = ZombieOptions.ZombieSpeedOption.GetFloat();
            ZombieOptions.PoliceLight = ZombieOptions.PoliceLightOption.GetFloat();
            ZombieOptions.PoliceSpeed = ZombieOptions.PoliceSpeedOption.GetFloat();
            if (!AmongUsClient.Instance.AmHost) return;
            ZombiePlayers = new();
            if (AmongUsClient.Instance.AmHost)
            {
                FixedUpdate.IsStart = false;
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    p.GetDefaultName();
                    /*
                    p.UncheckSetVisor("visor_EmptyVisor");
                    */

                    //p.RpcSetHat("");

                    /*
                    p.RpcSetSkin("");
                    */
                }
            }
            ZombieOptions.FirstChangeSettings();
        }
        public static void SetTimer()
        {
            FixedUpdate.IsStart = true;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                {
                    if (p2.PlayerId != p.PlayerId)
                    {
                        p2.RpcSetNamePrivate("Playing on SuperNewRoles!", p);
                    }
                }
            }
            FixedUpdate.NameChangeTimer = ZombieOptions.StartSecondOption.GetFloat();
        }
    }
}