using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PowerTools;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.MapOptions
{
    [HarmonyPatch(typeof(ShipStatus))]
    class PolusRandomSpawn
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SpawnPlayer))]
        public static void Postfix(ShipStatus __instance, PlayerControl player, int numPlayers, bool initialSpawn)
        {
            // Polusの湧き位置をランダムにする 無駄に人数分シャッフルが走るのをそのうち直す
            if (PlayerControl.GameOptions.MapId == 2 && MapOption.PolusRandomSpawn.getBool())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    System.Random rand = new();
                    int randVal = rand.Next(0, 6);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RandomSpawn, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)player.Data.PlayerId);
                    writer.Write((byte)randVal);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.randomSpawn((byte)player.Data.PlayerId, (byte)randVal);
                }
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    class MeetingHudClosePatch
    {
        static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.GameOptions.MapId == 2 && MapOption.PolusRandomSpawn.getBool())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        System.Random rand = new();
                        int randVal = rand.Next(0, 6);
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RandomSpawn, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)player.Data.PlayerId);
                        writer.Write((byte)randVal);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.randomSpawn((byte)player.Data.PlayerId, (byte)randVal);
                    }
                }
            }
        }
    }
    public static class Extensions
    {
        public static T Random<T>(this IList<T> self)
        {
            if (self.Count > 0)
            {
                return self[UnityEngine.Random.Range(0, self.Count)];
            }
            return default;
        }
    }
}
