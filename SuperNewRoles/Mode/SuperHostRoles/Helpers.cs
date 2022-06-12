using Hazel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class Helpers
    {
        public static void UnCheckedRpcSetRole(this PlayerControl player, RoleTypes role)
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                player.RpcSetRoleDesync(role,p);
            }
        }
        //TownOfHostより！！
        public static void RpcSetRoleDesync(this PlayerControl player, RoleTypes role, PlayerControl seer = null)
        {
            //player: 名前の変更対象
            //seer: 上の変更を確認することができるプレイヤー

            if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.getClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcSetRoleDesync(this PlayerControl player, CustomRpcSender sender, RoleTypes role, PlayerControl seer = null)
        {
            //player: 名前の変更対象
            //seer: 上の変更を確認することができるプレイヤー

            if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.getClientId();
            SuperNewRolesPlugin.Logger.LogInfo("(Desync => "+ seer.Data.PlayerName+" ) "+ player.Data.PlayerName + " => " + role);
            sender.StartRpc(player.NetId, RpcCalls.SetRole, targetClientId : clientId);
            sender.Write((ushort)role);
            sender.EndRpc();
        }
        public static void RpcSetRole(this PlayerControl player, CustomRpcSender sender, RoleTypes role)
        {
            SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " => "+role);
            if (player == null) return;
            sender.StartRpc(player.NetId, RpcCalls.SetRole);
            sender.Write((ushort)role);
            sender.EndRpc();
            player.SetRole(role);
        }
    }
}
