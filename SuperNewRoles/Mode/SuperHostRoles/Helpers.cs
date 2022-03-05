using Hazel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class Helpers
    {
        /// <summary>
        /// 特定のプレイヤーから見て、特定のプレイヤーの名前を変更する巻数
        /// </summary>
        /// <param name="TargetPlayer">変更する名前</param>
        /// <param name="NewName">変更後の名前</param>
        /// <param name="SeePlayer">変更後の名前を見れるプレイヤー</param>
        public static void RpcSetNamePrivate(this PlayerControl TargetPlayer, string NewName, PlayerControl SeePlayer = null)
        {
            if (TargetPlayer == null || NewName == null || !AmongUsClient.Instance.AmHost) return;
            if (SeePlayer == null) SeePlayer = TargetPlayer;
            var clientId = SeePlayer.getClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.SetName, Hazel.SendOption.Reliable, clientId);
            writer.Write(NewName);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void UnCheckedRpcSetRole(this PlayerControl player, RoleTypes role)
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                player.RpcSetRoleDesync(role,p);
            }
        }
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

    }
}
