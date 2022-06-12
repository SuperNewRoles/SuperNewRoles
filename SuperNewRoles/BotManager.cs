using Hazel;
using SuperNewRoles.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles
{
    public static class BotManager
    {
        public static List<PlayerControl> AllBots = new List<PlayerControl>();
        public static bool IsBot(this PlayerControl player)
        {
            try
            {
                if (player == null) return false;
                if (player.Data.Disconnected) return false;
                foreach (PlayerControl p in BotManager.AllBots)
                {
                    if (p.PlayerId == player.PlayerId) return true;
                }
                return false;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        public static bool IsPlayer(this PlayerControl player)
        {
            return !IsBot(player);
        }
        public static PlayerControl Spawn(string name = "Bot", byte BotPlayerId = 1)
        {
            byte id = 0;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.PlayerId > id)
                {
                    id = p.PlayerId;
                }
            }
            var Bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);

            id++;
            /*
            if (id < 14) {
                id = 15;
            }
            */
            Bot.PlayerId = id;
           // Bot.PlayerId = BotPlayerId;
            GameData.Instance.AddPlayer(Bot);
            AmongUsClient.Instance.Spawn(Bot, -2, InnerNet.SpawnFlags.IsClientCharacter);
            Bot.transform.position = new Vector3(9999f, 9999f, 0);
            Bot.NetTransform.enabled = true;

            Bot.RpcSetName(name);
            Bot.RpcSetColor(1);
            Bot.RpcSetHat("hat_NoHat");
            Bot.RpcSetPet("peet_EmptyPet");
            Bot.RpcSetVisor("visor_EmptyVisor");
            Bot.RpcSetNamePlate("nameplate_NoPlate");
            Bot.RpcSetSkin("skin_None");
            GameData.Instance.RpcSetTasks(Bot.PlayerId, new byte[0]);
            SuperNewRolesPlugin.Logger.LogInfo("botスポーン！\nID:" + Bot.PlayerId + "\nBotName:" + Bot.name);
            AllBots.Add(Bot);
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetBot);
            writer.Write(Bot.PlayerId);
            new LateTask(()=>writer.EndRPC(),0.5f);
            return Bot;
        }
        public static void Despawn(PlayerControl Bot)
        {
            SuperNewRolesPlugin.Logger.LogInfo("botデスポーン！\nID:" + Bot.PlayerId + "\nBotName:" + Bot.name);
            GameData.Instance.RemovePlayer(Bot.PlayerId);
            AmongUsClient.Instance.Despawn(Bot);
            SuperNewRolesPlugin.Logger.LogInfo("完了！");
            AllBots.Remove(Bot);
        }
        public static void AllBotDespawn()
        {
            foreach (PlayerControl Bots in AllBots)
            {
                SuperNewRolesPlugin.Logger.LogInfo("botデスポーン！\nID:" + Bots.PlayerId + "\nBotName:" + Bots.name);
                GameData.Instance.RemovePlayer(Bots.PlayerId);
                Bots.Despawn();
                SuperNewRolesPlugin.Logger.LogInfo("完了！");
            }
            AllBots = new List<PlayerControl>();
        }
    }
}
