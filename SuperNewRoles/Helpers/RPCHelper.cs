using Hazel;
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
            if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                PlayerControl.GameOptions = Data;
            }
            else
            {
                MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 2, (SendOption)1, target.getClientId());
                Writer.WriteBytesAndSize(Data.ToBytes(5));
                AmongUsClient.Instance.FinishRpcImmediately(Writer);
            }
        }
    }
}
