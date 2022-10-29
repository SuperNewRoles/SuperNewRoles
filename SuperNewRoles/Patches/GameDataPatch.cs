using System;
using HarmonyLib;
using SuperNewRoles.Roles;
using InnerNet;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    class HandleDisconnectPatch
    {
        static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
        {
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                Modifier.allModifiers.Do(x => x.HandleDisconnect(player, reason));
            }
        }
    }
}