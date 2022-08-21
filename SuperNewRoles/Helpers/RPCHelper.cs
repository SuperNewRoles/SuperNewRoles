using System.Linq;
using Hazel;
using InnerNet;
using SuperNewRoles.CustomRPC;
using UnityEngine;
using static MeetingHud;

namespace SuperNewRoles.Helpers
{
    public static class RPCHelper
    {
        public static MessageWriter StartRPC(RpcCalls RPCId, PlayerControl SendTarget = null)
        {
            return StartRPC(PlayerControl.LocalPlayer.NetId, (byte)RPCId, SendTarget);
        }
        public static MessageWriter StartRPC(uint NetId, RpcCalls RPCId, PlayerControl SendTarget = null)
        {
            return StartRPC(NetId, (byte)RPCId, SendTarget);
        }
        public static MessageWriter StartRPC(CustomRPC.CustomRPC RPCId, PlayerControl SendTarget = null)
        {
            return StartRPC(PlayerControl.LocalPlayer.NetId, (byte)RPCId, SendTarget);
        }
        public static MessageWriter StartRPC(uint NetId, CustomRPC.CustomRPC RPCId, PlayerControl SendTarget = null)
        {
            return StartRPC(NetId, (byte)RPCId, SendTarget);
        }
        public static MessageWriter StartRPC(byte RPCId, PlayerControl SendTarget = null)
        {
            return StartRPC(PlayerControl.LocalPlayer.NetId, RPCId, SendTarget);
        }
        public static MessageWriter StartRPC(uint NetId, byte RPCId, PlayerControl SendTarget = null)
        {
            var target = SendTarget != null ? SendTarget.GetClientId() : -1;
            return AmongUsClient.Instance.StartRpcImmediately(NetId, RPCId, SendOption.Reliable, target);
        }
        public static void EndRPC(this MessageWriter Writer)
        {
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static void RpcSetDoorway(byte id, bool Open)
        {
            ShipStatus.Instance.AllDoors.FirstOrDefault((a) => a.Id == id).SetDoorway(Open);
        }
        public static void RpcSetDoorway(this PlainDoor door, bool Open)
        {
            door.SetDoorway(Open);
            MessageWriter writer = StartRPC(CustomRPC.CustomRPC.RpcSetDoorway);
            writer.Write((byte)door.Id);
            writer.Write(Open);
            writer.EndRPC();
        }
        public static void RPCGameOptionsPrivate(GameOptionsData Data, PlayerControl target)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 2, SendOption.None, target.GetClientId());
            messageWriter.WriteBytesAndSize(Data.ToBytes(5));
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
            var clientId = SeePlayer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, clientId);
            writer.Write(NewName);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSnapTo(this PlayerControl __instance, Vector2 position)
        {
            Logger.Info("CustomRpcSnapToが呼び出されました");
            if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                __instance.NetTransform.RpcSnapTo(position);
                return;
            }
            ushort minSid = (ushort)(__instance.NetTransform.lastSequenceId + 5);
            if (AmongUsClient.Instance.AmClient)
            {
                __instance.NetTransform.SnapTo(position, minSid);
            }
            MessageWriter val = AmongUsClient.Instance.StartRpc(__instance.NetTransform.NetId, 21, SendOption.None);
            __instance.NetTransform.WriteVector2(position, val);
            val.Write(__instance.NetTransform.lastSequenceId);
            val.EndMessage();
        }

        public static void RpcProtectPlayerPrivate(this PlayerControl SourcePlayer, PlayerControl target, int colorId, PlayerControl SeePlayer = null)
        {
            if (SourcePlayer == null || target == null || !AmongUsClient.Instance.AmHost) return;
            if (SeePlayer == null) SeePlayer = SourcePlayer;
            var clientId = SeePlayer.GetClientId();
            MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(SourcePlayer.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.Reliable, clientId);
            val.WriteNetObject(target);
            val.Write(colorId);
            AmongUsClient.Instance.FinishRpcImmediately(val);
        }

        public static void RpcProtectPlayerPrivate(this PlayerControl SourcePlayer, CustomRpcSender sender, PlayerControl target, int colorId, PlayerControl SeePlayer = null)
        {
            if (SourcePlayer == null || target == null || !AmongUsClient.Instance.AmHost) return;
            if (SeePlayer == null) SeePlayer = SourcePlayer;
            var clientId = SeePlayer.GetClientId();
            sender.StartMessage(clientId)
                .StartRpc(SourcePlayer.NetId, RpcCalls.ProtectPlayer)
                .WriteNetObject(target)
                .Write(colorId)
                .EndRpc()
                .EndMessage();
        }

        public static void RPCSendChatPrivate(this PlayerControl TargetPlayer, string Chat, PlayerControl SeePlayer = null)
        {
            if (TargetPlayer == null || Chat == null) return;
            if (SeePlayer == null) SeePlayer = TargetPlayer;
            var clientId = SeePlayer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.None, clientId);
            writer.Write(Chat);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcVotingCompletePrivate(MeetingHud __instance, VoterState[] states, GameData.PlayerInfo exiled, bool tie, PlayerControl SeePlayer)
        {
            MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 23, SendOption.None, SeePlayer.GetClientId());
            val.WritePacked(states.Length);
            foreach (VoterState voterState in states)
            {
                voterState.Serialize(val);
            }
            val.Write(exiled?.PlayerId ?? byte.MaxValue);
            val.Write(tie);
            val.EndMessage();
        }
        /// <summary>
        /// 通常のRPCのExiled
        /// </summary>
        public static void RpcInnerExiled(this PlayerControl TargetPlayer)
        {
            if (TargetPlayer == null) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.Exiled, SendOption.None);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            TargetPlayer.Exiled();
        }
        public static void RPCSetColorModOnly(this PlayerControl player, byte color)
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
            RPCProcedure.UncheckedSetVanilaRole(player.PlayerId, (byte)roletype);
        }

        public static void RpcResetAbilityCooldown(this PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PlayerControl.LocalPlayer.PlayerId == target.PlayerId)
            {
                PlayerControl.LocalPlayer.Data.Role.SetCooldown();
            }
            else
            {
                MessageWriter writer = StartRPC(target.NetId, RpcCalls.ProtectPlayer, target);
                writer.Write(0);
                writer.Write(0);
                writer.EndRPC();
            }
        }
    }
}