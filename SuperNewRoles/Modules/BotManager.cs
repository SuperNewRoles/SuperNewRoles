using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Modules
{
    public static class BotManager
    {
        public static List<PlayerControl> AllBots = new();
        public static bool IsBot(this PlayerControl player)
        {
            try
            {
                if (player == null) return false;
                if (player.Data.Disconnected) return false;
                foreach (PlayerControl p in AllBots)
                {
                    if (p.PlayerId == player.PlayerId) return true;
                }
                return false;
            }
            catch { return false; }
        }
        public static PlayerControl Spawn(string name = "Bot")
        {
            byte id = 0;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.PlayerId > id)
                {
                    id = p.PlayerId;
                }
            }
            var bot = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);

            id++;

            bot.PlayerId = id;
            // Bot.PlayerId = BotPlayerId;
            GameData.Instance.AddPlayer(bot);
            AmongUsClient.Instance.Spawn(bot, -2, InnerNet.SpawnFlags.IsClientCharacter);
            bot.transform.position = new Vector3(9999f, 9999f, 0);
            bot.NetTransform.enabled = true;

            bot.RpcSetName(name);
            bot.RpcSetColor(1);
            bot.RpcSetHat("hat_NoHat");
            bot.RpcSetPet("peet_EmptyPet");
            bot.RpcSetVisor("visor_EmptyVisor");
            bot.RpcSetNamePlate("nameplate_NoPlate");
            bot.RpcSetSkin("skin_None");
            GameData.Instance.RpcSetTasks(bot.PlayerId, new byte[0]);
            Logger.Info($"botスポーン!\nID:{bot.PlayerId}\nBotName:{bot.name}", "Bot Manager");
            AllBots.Add(bot);
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetBot);
            writer.Write(bot.PlayerId);
            new LateTask(() => writer.EndRPC(), 0.5f, "Bot Spawn-End");
            return bot;
        }
        public static void Despawn(PlayerControl bot)
        {
            Logger.Info($"botデスポーン!\nID:{bot.PlayerId}\nBotName:{bot.name}", "Bot Manager");
            GameData.Instance.RemovePlayer(bot.PlayerId);
            AmongUsClient.Instance.Despawn(bot);
            Logger.Info("botデスポーン完了！", "Bot Manager");
            AllBots.Remove(bot);
        }
        public static void AllBotDespawn()
        {
            foreach (PlayerControl bots in AllBots)
            {
                Logger.Info($"Allbotデスポーン!\nID:{bots.PlayerId}\nBotName:{bots.name}", "Bot Manager");
                GameData.Instance.RemovePlayer(bots.PlayerId);
                bots.Despawn();
                Logger.Info("botデスポーン完了！", "Bot Manager");
            }
            AllBots = new();
        }
    }
}