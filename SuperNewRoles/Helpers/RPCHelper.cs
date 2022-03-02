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
    }
}
