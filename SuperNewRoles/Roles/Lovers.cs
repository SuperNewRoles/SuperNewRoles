using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class Lovers
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        class HandleDisconnectPatch
        {
            public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    if (player.IsLovers())
                    {
                        RoleClass.Lovers.LoversPlayer.RemoveAll(x => x.TrueForAll(x2 => x2.PlayerId == player.PlayerId));
                    }
                }
            }
        }
    }
}
