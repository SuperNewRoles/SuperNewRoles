using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(ShipStatus))]
    class PolusRandomSpawn
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SpawnPlayer))]
        public static void Postfix(PlayerControl player)
        {
            // Polusの湧き位置をランダムにする
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Polus) && MapCustom.PolusRandomSpawn.GetBool() && player.PlayerId == CachedPlayer.LocalPlayer.PlayerId && AmongUsClient.Instance.AmHost)
            {
                System.Random rand = new();
                int randVal = rand.Next(0, 11);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RandomSpawn, SendOption.Reliable, -1);
                writer.Write(player.Data.PlayerId);
                writer.Write((byte)randVal);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.RandomSpawn(player.Data.PlayerId, (byte)randVal);
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    class MeetingHudClosePatch
    {
        static void Postfix()
        {
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Polus) && MapCustom.PolusRandomSpawn.GetBool())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        System.Random rand = new();
                        int randVal = rand.Next(0, 11);
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RandomSpawn, SendOption.Reliable, -1);
                        writer.Write(player.Data.PlayerId);
                        writer.Write((byte)randVal);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.RandomSpawn(player.Data.PlayerId, (byte)randVal);
                    }
                }
            }
        }
    }

}