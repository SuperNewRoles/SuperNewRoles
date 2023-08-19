using Hazel;
using InnerNet;

namespace SuperNewRoles.Helpers;

public static class DesyncHelpers
{
    public static void RPCMurderPlayerPrivate(this PlayerControl source, PlayerControl target, PlayerControl see = null)
    {
        PlayerControl SeePlayer = see;
        if (see == null) SeePlayer = source;
        if (SeePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            source.MurderPlayer(target);
            return;
        }
        MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(source.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, SeePlayer.GetClientId());
        MessageExtensions.WriteNetObject(MurderWriter, target);
        AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
    }
    public static void RPCMurderPlayerPrivate(this PlayerControl source, CustomRpcSender sender, PlayerControl target, PlayerControl see = null)
    {
        PlayerControl SeePlayer = see;
        if (see == null) SeePlayer = source;
        sender.StartMessage(SeePlayer.GetClientId())
            .StartRpc(source.NetId, RpcCalls.MurderPlayer)
            .WriteNetObject(target)
            .EndRpc()
            .EndMessage();
    }
}