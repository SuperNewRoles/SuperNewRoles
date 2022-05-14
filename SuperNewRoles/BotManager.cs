using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles
{
    public class BotManager
    {
        public static List<PlayerControl> AllBots = new List<PlayerControl>();
        public static void Spawn(string name = "Bot", byte BotPlayerId = 1)
        {
            byte id = 0;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.PlayerId > id)
                {
                    id = p.PlayerId;
                }
            }
            id++;
            var Bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);

            AllBots.Add(Bot);
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
                AmongUsClient.Instance.Despawn(Bots);
                SuperNewRolesPlugin.Logger.LogInfo("完了！");
            }
            AllBots = new List<PlayerControl>();
        }
    }
}
