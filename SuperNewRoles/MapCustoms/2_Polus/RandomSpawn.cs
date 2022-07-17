using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PowerTools;
using SuperNewRoles.CustomRPC;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(ShipStatus))]
    class PolusRandomSpawn
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SpawnPlayer))]
        public static void Postfix(ShipStatus __instance, PlayerControl player, int numPlayers, bool initialSpawn)
        {
            // Polusの湧き位置をランダムにする
            if (MapCustomHandler.isMapCustom(MapCustomHandler.MapCustomId.Polus) && MapCustoms.MapCustom.PolusRandomSpawn.GetBool() && player.PlayerId == CachedPlayer.LocalPlayer.PlayerId && AmongUsClient.Instance.AmHost)
            {
                System.Random rand = new();
                int randVal = rand.Next(0, 11);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RandomSpawn, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)player.Data.PlayerId);
                writer.Write((byte)randVal);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.randomSpawn((byte)player.Data.PlayerId, (byte)randVal);
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    class MeetingHudClosePatch
    {
        static void Postfix(MeetingHud __instance)
        {
            if (MapCustomHandler.isMapCustom(MapCustomHandler.MapCustomId.Polus) && MapCustoms.MapCustom.PolusRandomSpawn.GetBool())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        System.Random rand = new();
                        int randVal = rand.Next(0, 11);
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

}