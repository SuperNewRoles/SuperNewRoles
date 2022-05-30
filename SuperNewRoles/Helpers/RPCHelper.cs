using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Helpers
{
    public static class RPCHelper
    {
        public static MessageWriter StartRPC(CustomRPC.CustomRPC RPCId, PlayerControl SendTarget = null)
        {
            return StartRPC((byte)RPCId, SendTarget);
        }
        public static MessageWriter StartRPC(byte RPCId,PlayerControl SendTarget = null) {
            var target = SendTarget != null ? SendTarget.getClientId() : -1;
           return AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, RPCId, Hazel.SendOption.Reliable, target);
        }
        public static void EndRPC(this MessageWriter Writer)
        {
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static void RPCGameOptionsPrivate(GameOptionsData Data,PlayerControl target)
        {
            MessageWriter messageWriter = StartRPC((byte)2,target);
            messageWriter.WriteBytesAndSize(Data.ToBytes((byte)5));
            messageWriter.EndMessage();
        }
        /// <summary>
        /// 特定のプレイヤーから見て、特定のプレイヤーの名前を変更する関数
        /// </summary>
        /// <param name="TargetPlayer">変更する名前</param>
        /// <param name="NewName">変更後の名前</param>
        /// <param name="SeePlayer">変更後の名前を見れるプレイヤー</param>
        public static void RpcSetNamePrivate(this PlayerControl TargetPlayer, string NewName, PlayerControl SeePlayer = null)
        {
            if (TargetPlayer == null || NewName == null || !AmongUsClient.Instance.AmHost) return;
            if (SeePlayer == null) SeePlayer = TargetPlayer;
            var clientId = SeePlayer.getClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, clientId);
            writer.Write(NewName);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcProtectPlayerPrivate(this PlayerControl SourcePlayer, PlayerControl target, int colorId, PlayerControl SeePlayer = null)
        {
            if (SourcePlayer == null || target == null || !AmongUsClient.Instance.AmHost) return;
            if (SeePlayer == null) SeePlayer = SourcePlayer;
            var clientId = SeePlayer.getClientId();
            MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(SourcePlayer.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.Reliable, clientId);
            val.WriteNetObject(target);
            val.Write(colorId);
            AmongUsClient.Instance.FinishRpcImmediately(val);
        }

        public static void RPCSendChatPrivate(this PlayerControl TargetPlayer,string Chat,PlayerControl SeePlayer = null)
        {
            if (TargetPlayer == null || Chat == null) return;
            if (SeePlayer == null) SeePlayer = TargetPlayer;
            var clientId = SeePlayer.getClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.None, clientId);
            writer.Write(Chat);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void UncheckSetVisor(this PlayerControl p, string id)
        {
            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(p.NetId, (byte)RpcCalls.SetVisor, Hazel.SendOption.Reliable, p2.getClientId());
                writer.Write(id);
                writer.EndRPC();
            }
        }
        /// <summary>
        /// 通常のRPCのExiled
        /// </summary>
        public static void RpcInnerExiled(this PlayerControl TargetPlayer)
        {
            if (TargetPlayer == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.Exiled, Hazel.SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            TargetPlayer.Exiled();
        }
        public static void RPCSetColorModOnly(this PlayerControl player,byte color)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.CustomRPC.UncheckedSetColor, SendOption.Reliable);
            writer.Write(color);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            player.SetColor(color);
        }
        public static void RPCSetRoleUnchecked(this PlayerControl player, RoleTypes roletype)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.UncheckedSetVanilaRole, SendOption.Reliable);
            writer.Write(player.PlayerId);
            writer.Write((byte)roletype);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.UncheckedSetVanilaRole(player.PlayerId,(byte)roletype);
        }
    }
}
