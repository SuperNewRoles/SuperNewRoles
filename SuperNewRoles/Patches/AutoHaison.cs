using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;


[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), [typeof(PlayerControl), typeof(DisconnectReasons)])]
public static class HostDisconnectedPatch
{
    public static void Postfix(PlayerControl player, DisconnectReasons reason)
    {
        if (player.OwnerId == AmongUsClient.Instance.HostId)
        {
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                //恐らくEndGame時にサーバー側でホストが切り替わるため、
                //最初にこの処理を行ったプレイヤー(次のホスト)以外はこの処理に到達しない(既にホストが変わっている)
                EndGamer.RpcHaison();
            }
        }
    }
}
