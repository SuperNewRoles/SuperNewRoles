using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNewRoles
{
    public static class BotManager
    {
        public static List<PlayerControl> Bots = new();
        public static PlayerControl Spawn(string name = "", int id = -1)
        {
            byte newid = 0;
            if (id == -1)
            {
                var temp = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.PlayerId > temp)
                    {
                        temp = p.PlayerId;
                    }
                }
                newid = (byte)((byte)temp + 1);
                SuperNewRolesPlugin.Logger.LogInfo("あいでぃー:" + newid);
            }
            else
            {
                newid = (byte)id;
            }
            PlayerControl bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
            bot.PlayerId = newid;
            GameData.Instance.AddPlayer(bot);
            PlayerControl.AllPlayerControls.Add(bot);
            AmongUsClient.Instance.Spawn(bot, -2, SpawnFlags.None);
            bot.transform.position = PlayerControl.LocalPlayer.transform.position;
            bot.NetTransform.enabled = true;
            GameData.Instance.RpcSetTasks(bot.PlayerId, new byte[0]);


            bot.RpcSetColor((byte)PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
            bot.RpcSetName(PlayerControl.LocalPlayer.name);
            bot.RpcSetPet(PlayerControl.LocalPlayer.CurrentOutfit.PetId);
            bot.RpcSetSkin(PlayerControl.LocalPlayer.CurrentOutfit.SkinId);
            bot.RpcSetNamePlate(PlayerControl.LocalPlayer.CurrentOutfit.NamePlateId);

            new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
            //new LateTask(() => { foreach (var pc in PlayerControl.AllPlayerControls) pc.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
            //new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
            Bots.Add(bot);
            return bot;
        }
        public static void BotDespawn(this PlayerControl player)
        {
            Bots.RemoveAll(x => player.PlayerId == x.PlayerId);
            Il2CppSystem.Collections.Generic.List<PlayerControl> NewControl = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.PlayerId != player.PlayerId)
                {
                    NewControl.Add(p);
                }
            }
            PlayerControl.AllPlayerControls = NewControl;
            player.Despawn();
        }
        public static void AllDespawn()
        {
            foreach (PlayerControl bot in Bots)
            {
                bot.BotDespawn();
            }
            Bots = new();
        }
    }
}
