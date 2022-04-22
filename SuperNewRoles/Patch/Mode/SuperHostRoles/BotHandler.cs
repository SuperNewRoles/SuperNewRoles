using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class BotHandler
    {
        public static PlayerControl bot;
        public static void CreateBot()//byte id,string name)
        {
            if (bot == null)
            {
                bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                bot.PlayerId = 15;
                GameData.Instance.AddPlayer(bot);
                AmongUsClient.Instance.Spawn(bot, -2, SpawnFlags.None);
                bot.transform.position = PlayerControl.LocalPlayer.transform.position;
                bot.NetTransform.enabled = true;
                GameData.Instance.RpcSetTasks(bot.PlayerId, new byte[0]);
            }

            bot.RpcSetColor((byte)PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
            bot.RpcSetName("(SNR自動生成BOT)");// PlayerControl.LocalPlayer.name);
            bot.RpcSetPet(PlayerControl.LocalPlayer.CurrentOutfit.PetId);
            bot.RpcSetSkin(PlayerControl.LocalPlayer.CurrentOutfit.SkinId);
            bot.RpcSetNamePlate(PlayerControl.LocalPlayer.CurrentOutfit.NamePlateId);

            new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
            new LateTask(() => { foreach (var pc in PlayerControl.AllPlayerControls) pc.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
            new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
        }
        public static bool IsBot(this PlayerControl p) {
            if (p.PlayerId == bot.PlayerId) {
                return true;
            }
            return false;
        }
        public static bool IsNoBot(this PlayerControl p)
        {
            if (p.PlayerId != bot.PlayerId)
            {
                return true;
            }
            return false;
        }
    }
}
