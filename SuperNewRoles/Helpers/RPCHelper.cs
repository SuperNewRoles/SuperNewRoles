using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using Steamworks;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;
using UnityEngine.UIElements.StyleSheets;
using static MeetingHud;

namespace SuperNewRoles.Helpers;

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
    public static MessageWriter StartRPC(CustomRPC RPCId, PlayerControl SendTarget = null)
    {
        return StartRPC(PlayerControl.LocalPlayer.NetId, (byte)RPCId, SendTarget);
    }
    public static MessageWriter StartRPC(uint NetId, CustomRPC RPCId, PlayerControl SendTarget = null)
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
    public static void SendSingleRpc(byte RPCId, uint NetId, PlayerControl SendTarget = null) => StartRPC(NetId, RPCId, SendTarget).EndRPC();
    public static void SendSingleRpc(CustomRPC RPCId, uint NetId, PlayerControl SendTarget = null) => StartRPC(NetId, RPCId, SendTarget).EndRPC();
    public static void SendSingleRpc(CustomRPC RPCId, PlayerControl SendTarget = null) => StartRPC(RPCId, SendTarget).EndRPC();

    public static void SendSinglePlayerRpc(CustomRPC RPCId, byte Target, PlayerControl SendTarget = null)
    {
        var writer = StartRPC(RPCId, SendTarget);
        writer.Write(Target);
        writer.EndRPC();
    }

    //Source And Target
    public static void SendSTRpc(CustomRPC RPCId, byte Source, byte Target, PlayerControl SendTarget = null)
    {
        var writer = StartRPC(RPCId, SendTarget);
        writer.Write(Source);
        writer.Write(Target);
        writer.EndRPC();
    }

    public static void RpcSetDoorway(byte id, bool Open)
    {
        MapUtilities.CachedShipStatus.AllDoors.FirstOrDefault((a) => a.Id == id).SetDoorway(Open);
    }
    public static void RpcSetDoorway(this PlainDoor door, bool Open)
    {
        door.SetDoorway(Open);
        MessageWriter writer = StartRPC(CustomRPC.RpcSetDoorway);
        writer.Write((byte)door.Id);
        writer.Write(Open);
        writer.EndRPC();
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
        MessageWriter val = AmongUsClient.Instance.StartRpc(__instance.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None);
        NetHelpers.WriteVector2(position, val);
        val.Write(__instance.NetTransform.lastSequenceId);
        val.EndMessage();
    }
    public static void RpcSyncOption(this IGameOptions gameOptions, int TargetClientId = -1)
    {
        GameManager gm = NormalGameManager.Instance;
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        // 書き込み {}は読みやすさのためです。
        if (TargetClientId < 0)
        {
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);
            return;
        }
        else
        {
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            if (TargetClientId == PlayerControl.LocalPlayer.GetClientId()) return;
            writer.WritePacked(TargetClientId);
        }
        Logger.Info($"共有なうーーー:{TargetClientId} : {gameOptions.GetFloat(FloatOptionNames.CrewLightMod)}");
        {
            writer.StartMessage(1); //0x01 Data
            {
                writer.WritePacked(gm.NetId);
                writer.StartMessage((byte)4);
                writer.WriteBytesAndSize(gm.LogicOptions.gameOptionsFactory.ToBytes(gameOptions));
                writer.EndMessage();
            }
            writer.EndMessage();
        }
        writer.EndMessage();

        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
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
    public static void RpcExiledUnchecked(this PlayerControl player)
    {
        MessageWriter RPCWriter = StartRPC(CustomRPC.ExiledRPC);
        RPCWriter.Write(player.PlayerId);
        RPCWriter.EndRPC();
        RPCProcedure.ExiledRPC(player.PlayerId);
    }
    public static void RPCSetColorModOnly(this PlayerControl player, byte color)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.UncheckedSetColor, SendOption.Reliable);
        writer.Write(color);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        player.SetColor(color);
    }
    public static void RPCSetRoleUnchecked(this PlayerControl player, RoleTypes roletype)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedSetVanillaRole, SendOption.Reliable);
        writer.Write(player.PlayerId);
        writer.Write((byte)roletype);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.UncheckedSetVanillaRole(player.PlayerId, (byte)roletype);
    }
    /// <summary>
    /// 役職をリセットし、新しい役職に変更します。
    /// </summary>
    /// <param name="target">役職が変更される対象(PlayerControl)</param>
    /// <param name="Id">変更先の役職(RoleId)</param>
    public static void ResetAndSetRole(this PlayerControl target, RoleId Id)
    {
        target.RPCSetRoleUnchecked(RoleTypes.Crewmate);
        target.SetRoleRPC(Id);
        Logger.Info($"[{target.GetDefaultName()}] の役職を [{Id}] に変更しました。");
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

    public static void RpcOpenToilet()
    {
        foreach (var i in new[] { 79, 80, 81, 82 })
        {
            Logger.Info($"amount:{i}", "RpcOpenToilet");
            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Doors, i);
        }
    }
}