using System;
using System.Collections.Generic;
using System.Text;
using Hazel;
using InnerNet;

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
