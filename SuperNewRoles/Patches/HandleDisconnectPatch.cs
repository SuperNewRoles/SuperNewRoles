using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
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
                //なので廃村のCustomRPCを他プレイヤーに送る必要がある
                RPCHelper.StartRPC(CustomRPC.SetHaison).EndRPC();
                RPCProcedure.SetHaison();
                Logger.Info("===================== ホスト切断のため廃村 ======================", "End Game");
                GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                MapUtilities.CachedShipStatus.enabled = false;
            }
        }
    }
}

