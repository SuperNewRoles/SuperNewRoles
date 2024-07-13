using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazel;
using InnerNet;

namespace SuperNewRoles.Helpers;
public static class MurderHelpers
{
    public static void RpcMurderPlayerFlags(this PlayerControl player, PlayerControl target, MurderResultFlags flags, PlayerControl SendTarget = null)
    {
        if (player == null || target == null) return;
        MessageWriter writer = RPCHelper.StartRPC(player.NetId, RpcCalls.MurderPlayer, SendTarget);
        writer.WriteNetObject(target);
        writer.Write((int)flags);
        writer.EndRPC();
        player.MurderPlayer(target, flags);
    }
    public static void RpcMurderPlayerFlags(this PlayerControl player, CustomRpcSender sender, PlayerControl target, MurderResultFlags flags, PlayerControl SendTarget = null)
    {
        if (sender == null)
        {
            RpcMurderPlayerFlags(player, target, flags, SendTarget);
            return;
        }
        if (player == null || target == null) return;

        sender
            .AutoStartRpc(player.NetId, (byte)RpcCalls.MurderPlayer, SendTarget?.GetClientId() ?? -1)
            .WriteNetObject(target)
            .Write((int)flags)
            .EndRpc();

        player.MurderPlayer(target, flags);
    }
    public static void RpcMurderPlayerForce(this PlayerControl player, PlayerControl target)
    {
        RpcMurderPlayerFlags(player, target,
            MurderResultFlags.Succeeded);
    }
    public static void RpcMurderPlayerFailed(this PlayerControl player, PlayerControl target)
    {
        RpcMurderPlayerFlags(player, target,
            MurderResultFlags.FailedError);
    }
    public static void RpcForceGuard(this PlayerControl shower, PlayerControl target, PlayerControl SendTarget = null, CustomRpcSender sender = null)
    {
        if (sender != null)
            sender.RpcProtectPlayer(shower, target, 0, SendTarget?.GetClientId() ?? -1);
        else if (SendTarget != null)
            shower.RpcProtectPlayerPrivate(target, 0, SendTarget);
        else
            shower.RpcProtectPlayer(target, 0);
        // RpcMurderPlayerFlags(shower, sender, target,
        //   MurderResultFlags.FailedProtected, SendTarget);
    }
    public static void RpcForceMurderAndGuard(this PlayerControl shower, PlayerControl target, PlayerControl SendTarget = null)
    {
        RpcMurderPlayerFlags(shower, target,
            MurderResultFlags.FailedProtected, SendTarget);
    }
    public static void RpcMurderPlayerOnCheck(this PlayerControl player, PlayerControl target)
    {
        MurderResultFlags flags = MurderResultFlags.Succeeded;
        if (target.protectedByGuardianId > -1)
        {
            flags = MurderResultFlags.FailedProtected;
        }
        RpcMurderPlayerFlags(player, target,
            flags);
    }
}