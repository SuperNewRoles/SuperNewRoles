using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Helpers
{
    public static class DesyncHelpers
    {
        public static void RPCMurderPlayerPrivate(this PlayerControl source, PlayerControl target, PlayerControl see = null)
        {
            PlayerControl SeePlayer = see;
            if (see == null) SeePlayer = source;
            MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(source.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, SeePlayer.getClientId());
            MessageExtensions.WriteNetObject(MurderWriter, target);
            AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
        }
    }
}