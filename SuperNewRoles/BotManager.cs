using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles
{
    public class BotManager
    {
        internal static BepInEx.Logging.ManualLogSource Logger;
        public static List<PlayerControl> AllBots = new List<PlayerControl>();
        public static void Spawn(string name = "Bot", byte BotPlayerId = 1)
        {
            var Bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
           
            AllBots.Add(Bot);
            GameData.Instance.AddPlayer(Bot);
            AmongUsClient.Instance.Spawn(Bot, -2, InnerNet.SpawnFlags.IsClientCharacter);
            Bot.PlayerId = BotPlayerId;
            Bot.transform.position = new Vector3(9999f, 9999f, 0);
            Bot.NetTransform.enabled = false;
            
            Bot.SetName(name);
            Bot.SetColor(1);
            Bot.SetHat("hat_Empty", 1);
            Bot.SetPet("pet_Empty", 1);
            Bot.SetVisor("visor_Empty");
            Bot.SetNamePlate("NamePlate_Empty");
            Bot.SetSkin("skin_Empty", 1);
            GameData.Instance.RpcSetTasks(Bot.PlayerId, new byte[0]);
            SuperNewRolesPlugin.Logger.LogInfo("<color=#00ff00>botスポーン！\nID:" + Bot.PlayerId + "\nBotName:" + Bot.name);
        }
        public static void Despawn(PlayerControl Bot)
        {
            SuperNewRolesPlugin.Logger.LogInfo("<color=#00ff00>botデスポーン！\nID:" + Bot.PlayerId + "\nBotName:" + Bot.name);
            Bot.Despawn();
            GameData.Instance.RemovePlayer(Bot.PlayerId);
            AllBots.Remove(Bot);
        }
        public static void AllBotDespawn()
        {
            foreach (PlayerControl Bots in AllBots)
            {
                Bots.Despawn();
                GameData.Instance.RemovePlayer(Bots.PlayerId);
                SuperNewRolesPlugin.Logger.LogInfo("<color=#00ff00>botデスポーン！\nID:" + Bots.PlayerId + "\nBotName:" + Bots.name);
            }
            AllBots = new List<PlayerControl>();
        }
    }
}
