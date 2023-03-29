using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace SuperNewRoles.Roles.RoleBases;
public static class CustomRoles
{
    public static void FixedUpdate(PlayerControl player)
    {
        if (player.IsAlive()) Role.allRoles.DoIf(x => x.player == player, x => x.MeFixedUpdateAlive());
        else Role.allRoles.DoIf(x => x.player == player, x => x.MeFixedUpdateDead());
        Role.allRoles.Do((x) => x.FixedUpdate());
    }

    public static void OnMeetingStart()
    {
        Role.allRoles.Do(x => x.OnMeetingStart());
    }

    public static void OnWrapUp()
    {
        Role.allRoles.Do(x => x.OnWrapUp());
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
    class HandleDisconnectPatch
    {
        public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
            }
        }
    }

    public static void OnKill(this PlayerControl player, PlayerControl target)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnKill(target));
    }

    public static void OnDeath(this PlayerControl player, PlayerControl killer)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnDeath(killer));
    }
}