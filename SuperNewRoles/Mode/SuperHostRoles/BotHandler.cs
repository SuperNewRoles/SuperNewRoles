using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class BotHandler
    {
        public static List<PlayerControl> Bots = new List<PlayerControl>();
        public static PlayerControl Bot;
        public static void AddBot(byte id,string name)
        {
            PlayerControl bot = null;
            SuperNewRolesPlugin.Logger.LogInfo("1");
            if (bot == null)
            {
                bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                bot.PlayerId = id;
                GameData.Instance.AddPlayer(bot);
                AmongUsClient.Instance.Spawn(bot, -2, SpawnFlags.None);
                bot.transform.position = PlayerControl.LocalPlayer.transform.position;
                bot.NetTransform.enabled = true;
                GameData.Instance.RpcSetTasks(id, new byte[0]);
            }

            bot.RpcSetColor((byte)PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
            bot.RpcSetName(name);
            bot.RpcSetPet(PlayerControl.LocalPlayer.CurrentOutfit.PetId);
            bot.RpcSetSkin(PlayerControl.LocalPlayer.CurrentOutfit.SkinId);
            bot.RpcSetNamePlate(PlayerControl.LocalPlayer.CurrentOutfit.NamePlateId);
            bot.RpcSetRole(RoleTypes.Crewmate);
            new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
            new LateTask(() => { foreach (var pc in PlayerControl.AllPlayerControls) pc.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
            new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
            Bots.Add(bot);
            Bot = bot;
        }
    }
}
