using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Generator.Extensions;
using InnerNet;
using Steamworks;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.BattleRoyal;
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

    public static void SetDoorway(byte id, bool Open)
    {
        MapUtilities.CachedShipStatus.AllDoors.FirstOrDefault((a) => a.Id == id).SetDoorway(Open);
    }
    public static void RpcSetDoorway(this OpenableDoor door, bool Open)
    {
        door.SetDoorway(Open);
        MessageWriter writer = StartRPC(CustomRPC.RpcSetDoorway);
        writer.Write((byte)door.Id);
        writer.Write(Open);
        writer.EndRPC();
    }
    public static void RpcSetNamePrivate(this PlayerControl TargetPlayer, CustomRpcSender sender, string NewName, PlayerControl SeePlayer = null)
    {
        if (sender == null)
        {
            TargetPlayer.RpcSetNamePrivate(NewName, SeePlayer);
            return;
        }
        if (TargetPlayer == null || NewName == null || !AmongUsClient.Instance.AmHost) return;
        if (SeePlayer == null) SeePlayer = TargetPlayer;
        if (SeePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            TargetPlayer.SetName(NewName);
            return;
        }
        var clientId = SeePlayer.GetClientId();
        sender.AutoStartRpc(TargetPlayer.NetId, (byte)RpcCalls.SetName, clientId)
            .Write(TargetPlayer.Data.NetId)
            .Write(NewName)
            .EndRpc();
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
        if (SeePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            TargetPlayer.SetName(NewName);
            return;
        }
        var clientId = SeePlayer.GetClientId();
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, clientId);
        writer.Write(TargetPlayer.Data.NetId);
        writer.Write(NewName);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static CustomRpcSender RpcSetColor(this CustomRpcSender sender, PlayerControl target, byte color, int seer = -1)
    {
        sender.AutoStartRpc(target.NetId, (byte)RpcCalls.SetColor, seer)
            .Write(target.Data.NetId)
            .Write(color)
            .EndRpc();
        if (seer < 0)
            target.SetColor(color);
        return sender;
    }
    public static CustomRpcSender RpcSetHat(this CustomRpcSender sender, PlayerControl player, string hatId, int seer = -1)
    {
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetHatStr, seer)
            .Write(hatId)
            .Write(player.GetNextRpcSequenceId(RpcCalls.SetHatStr))
            .EndRpc();
        if (seer < 0)
        {
            int valueOrDefault = (player.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
            player.SetHat(hatId, valueOrDefault);
        }
        return sender;
    }
    public static CustomRpcSender RpcSetVisor(this CustomRpcSender sender, PlayerControl player, string visorId, int seer = -1)
    {
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetVisorStr, seer)
            .Write(visorId)
            .Write(player.GetNextRpcSequenceId(RpcCalls.SetVisorStr))
            .EndRpc();
        if (seer < 0)
        {
            int valueOrDefault = (player.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
            player.SetVisor(visorId, valueOrDefault);
        }
        return sender;
    }
    public static CustomRpcSender RpcSetName(this CustomRpcSender sender, PlayerControl player, string name, int seer = -1)
    {
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetName, seer)
            .Write(player.Data.NetId)
            .Write(name)
            .EndRpc();
        if (seer < 0)
            player.SetName(name);
        return sender;
    }
    public static CustomRpcSender RpcSetSkin(this CustomRpcSender sender, PlayerControl player, string skinId, int seer = -1)
    {
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetSkinStr, seer)
            .Write(skinId)
            .Write(player.GetNextRpcSequenceId(RpcCalls.SetSkinStr))
            .EndRpc();
        if (seer < 0)
        {
            int valueOrDefault = (player.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
            player.SetSkin(skinId, valueOrDefault);
        }
        return sender;
    }
    public static CustomRpcSender RpcSetPet(this CustomRpcSender sender, PlayerControl player, string petId, int seer = -1)
    {
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetPetStr, seer)
            .Write(petId)
            .Write(player.GetNextRpcSequenceId(RpcCalls.SetPetStr))
            .EndRpc();
        if (seer < 0)
            player.SetPet(petId);
        return sender;
    }

    public static void RpcVotingCompleteDesync(VoterState[] states, NetworkedPlayerInfo exiled, bool tie, PlayerControl seer)
    {
        if (MeetingHud.Instance == null)
            throw new System.Exception("MeetingHud.Instance is null");
        if (seer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            Instance.VotingComplete(states, exiled, tie);
        else
        {
            Logger.Info("Desync Send Now ->"+seer.GetClientId().ToString());
            MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(Instance.NetId, (byte)RpcCalls.VotingComplete, SendOption.Reliable, targetClientId: seer.GetClientId());
            val.WritePacked(states.Length);
            foreach (VoterState voterState in states)
            {
                voterState.Serialize(val);
            }
            if (exiled == null)
                val.Write(byte.MaxValue);
            else
                val.Write(exiled.PlayerId);
            val.Write(tie);
            AmongUsClient.Instance.FinishRpcImmediately(val);
        }
    }

    public static void RpcSnapTo(this PlayerControl __instance, Vector2 position, PlayerControl seer = null)
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        //SuperNewRolesPlugin.Logger.LogInfo("[SHR:RpcSnapTo] CustomSyncSettingsが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        //Logger.Info("CustomRpcSnapToが呼び出されました");
        if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId && seer is null)
        {
            __instance.NetTransform.RpcSnapTo(position);
            return;
        }
        ushort minSid = (ushort)(__instance.NetTransform.lastSequenceId + 5);
        if (AmongUsClient.Instance.AmClient && (seer is null || seer.PlayerId == PlayerControl.LocalPlayer.PlayerId))
        {
            __instance.NetTransform.SnapTo(position, minSid);
        }
        if (seer is null || seer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
        {
            MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(__instance.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None, seer is null ? -1 : seer.GetClientId());
            NetHelpers.WriteVector2(position, val);
            val.Write(__instance.NetTransform.lastSequenceId);
            val.EndRPC();
        }
    }
    public static void RpcSyncOption()
    {
        GameManager gm = NormalGameManager.Instance;
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(5);
        writer.Write(AmongUsClient.Instance.GameId);
        {
            writer.StartMessage(1); //0x01 Data
            {
                writer.WritePacked(gm.NetId);
                writer.StartMessage((byte)4);
                writer.WriteBytesAndSize(gm.LogicOptions.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.CurrentGameOptions, AprilFoolsMode.IsAprilFoolsModeToggledOn));
                writer.EndMessage();
            }
            writer.EndMessage();
        }
        writer.EndMessage();

        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }
    public static void RpcSyncMeetingHud(int TargetClientId = -1)
    {
        if (Instance is null) return;
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);

        // 書き込み {}は読みやすさのためです。
        if (TargetClientId < 0)
        {
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);
        }
        else
        {
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            if (TargetClientId == PlayerControl.LocalPlayer.GetClientId()) return;
            writer.WritePacked(TargetClientId);
        }
        writer.StartMessage(1); //0x01 Data
        {
            writer.WritePacked(Instance.NetId);
            Instance.Serialize(writer, false);

        }
        writer.EndMessage();
        writer.EndMessage();

        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }
    public static void RpcSyncNetworkedPlayer(NetworkedPlayerInfo playerInfo, int TargetClientId = -1)
    {
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        // 書き込み {}は読みやすさのためです。
        if (TargetClientId < 0)
        {
            Logger.Info("Send=>All");
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);
        }
        else
        {
            if (TargetClientId == PlayerControl.LocalPlayer.GetClientId()) return;
            Logger.Info("Send=>" + TargetClientId.ToString());
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(TargetClientId);
        }
        writer.StartMessage(1); //0x01 Data
        {
            writer.WritePacked(playerInfo.NetId);
            GameDataSerializePatch.Is = true;
            playerInfo.Serialize(writer, false);
            GameDataSerializePatch.Is = false;

        }
        writer.EndMessage();
        writer.EndMessage();

        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }
    public static void RpcSyncNetworkedPlayer(CustomRpcSender sender, NetworkedPlayerInfo playerInfo, int TargetClientId = -1)
    {
        if (sender == null)
        {
            RpcSyncNetworkedPlayer(playerInfo, TargetClientId);
            return;
        }
        sender.StartMessage(TargetClientId);

        sender.Write((writer) =>
        {
            writer.StartMessage(1); //0x01 Data
            {
                writer.WritePacked(playerInfo.NetId);
                playerInfo.Serialize(writer, false);
            }
            writer.EndMessage();
        });
        sender.EndMessage();
    }
    public static void RpcSetTasks(CustomRpcSender sender, NetworkedPlayerInfo player, byte[] tasks, int TargetClientId = -1)
    {
        if (sender == null)
        {
            throw new System.NotImplementedException("RpcSetTask sender");
        }
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetTasks, TargetClientId);
        sender.WriteBytesAndSize(tasks);
        sender.EndRpc();
        if (TargetClientId < 0)
            player.SetTasks(tasks);
    }
    public static void RpcSyncAllNetworkedPlayer(CustomRpcSender sender, int TargetClientId = -1)
    {
        if (sender == null)
        {
            RpcSyncAllNetworkedPlayer(TargetClientId);
            return;
        }
        sender.StartMessage(TargetClientId);
        sender.Write((writer) =>
        {
            GameDataSerializePatch.Is = true;
            foreach (var player in GameData.Instance.AllPlayers)
            {
                writer.StartMessage(1); //0x01 Data
                {
                    writer.WritePacked(player.NetId);
                    player.Serialize(writer, false);
                }
                writer.EndMessage();
            }
            GameDataSerializePatch.Is = false;
        });
        sender.EndMessage();
    }
    public static void RpcSyncAllNetworkedPlayer(int TargetClientId = -1)
    {
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        if (TargetClientId < 0)
        {
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);
        }
        else
        {
            if (TargetClientId == PlayerControl.LocalPlayer.GetClientId()) return;
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(TargetClientId);
        }
        GameDataSerializePatch.Is = true;
        foreach (var player in GameData.Instance.AllPlayers)
        {
            // データを分割して送信
            if (writer.Length > 1000)
            {
                writer.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();

                writer = MessageWriter.Get(SendOption.Reliable);
                if (TargetClientId < 0)
                {
                    writer.StartMessage(5);
                    writer.Write(AmongUsClient.Instance.GameId);
                }
                else
                {
                    writer.StartMessage(6);
                    writer.Write(AmongUsClient.Instance.GameId);
                    writer.WritePacked(TargetClientId);
                }
            }

            writer.StartMessage(1); //0x01 Data
            {
                writer.WritePacked(player.NetId);
                player.Serialize(writer, false);
            }
            writer.EndMessage();
        }
        GameDataSerializePatch.Is = false;
        writer.EndMessage();

        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }
    public static void RpcSyncOption(this IGameOptions gameOptions, int TargetClientId = -1, SendOption sendOption = SendOption.Reliable)
    {
        GameManager gm = NormalGameManager.Instance;
        MessageWriter writer = MessageWriter.Get(sendOption);
        // 書き込み {}は読みやすさのためです。
        if (TargetClientId < 0)
        {
            writer.StartMessage(5);
            writer.Write(AmongUsClient.Instance.GameId);
        }
        else
        {
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            if (TargetClientId == PlayerControl.LocalPlayer.GetClientId()) return;
            writer.WritePacked(TargetClientId);
        }

        {
            writer.StartMessage(1); //0x01 Data
            {
                writer.WritePacked(gm.NetId);
                writer.StartMessage(4);
                writer.WriteBytesAndSize(gm.LogicOptions.gameOptionsFactory.ToBytes(gameOptions, AprilFoolsMode.IsAprilFoolsModeToggledOn));
                writer.EndMessage();
            }
            writer.EndMessage();
        }
        writer.EndMessage();

        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }
    public static void RpcSyncOption(this IGameOptions gameOptions, CustomRpcSender sender, int TargetClientId = -1)
    {
        if (sender == null)
        {
            RpcSyncOption(gameOptions, TargetClientId);
            return;
        }
        sender.StartMessage(TargetClientId);
        sender.Write((writer) =>
        {
            writer.StartMessage(1); //0x01 Data
            {
                writer.WritePacked(NormalGameManager.Instance.NetId);
                writer.StartMessage(4);
                writer.WriteBytesAndSize(NormalGameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(gameOptions, AprilFoolsMode.IsAprilFoolsModeToggledOn));
                writer.EndMessage();
            }
            writer.EndMessage();
        });
        sender.EndMessage();
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

    public static void RPCSendChatPrivate(this PlayerControl TargetPlayer, string Chat, PlayerControl SeePlayer = null, CustomRpcSender sender = null)
    {
        if (TargetPlayer == null || Chat == null) return;
        if (SeePlayer == null) SeePlayer = TargetPlayer;
        var clientId = SeePlayer.GetClientId();
        if (sender == null)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(TargetPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.None, clientId);
            writer.Write(Chat);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            return;
        }
        sender.AutoStartRpc(TargetPlayer.NetId, (byte)RpcCalls.SendChat, clientId);
        sender.Write(Chat);
        sender.EndRpc();
    }


    public static void RpcSetHatUnchecked(this PlayerControl player, string hatId, PlayerControl seePlayer = null)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            int valueOrDefault = (player.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
            player.SetHat(hatId, valueOrDefault);
        }
        MessageWriter messageWriter = StartRPC(player.NetId, RpcCalls.SetHat, seePlayer);
        messageWriter.Write(hatId);
        messageWriter.EndRPC();
    }
    public static void RpcSetVisorUnchecked(this PlayerControl player, string visorId, PlayerControl seePlayer = null)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            int valueOrDefault = (player.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
            player.SetVisor(visorId, valueOrDefault);
        }
        MessageWriter messageWriter = StartRPC(player.NetId, RpcCalls.SetVisor, seePlayer);
        messageWriter.Write(visorId);
        messageWriter.EndRPC();
    }
    public static void RpcSetSkinUnchecked(this PlayerControl player, string skinId, PlayerControl seePlayer = null)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            int valueOrDefault = (player.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
            player.SetSkin(skinId, valueOrDefault);
        }
        MessageWriter messageWriter = StartRPC(player.NetId, RpcCalls.SetSkin, seePlayer);
        messageWriter.Write(skinId);
        messageWriter.EndRPC();
    }
    public static void RpcVotingCompletePrivate(MeetingHud __instance, VoterState[] states, NetworkedPlayerInfo exiled, bool tie, PlayerControl SeePlayer)
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
    public static CustomRpcSender RpcShapeshift(this CustomRpcSender sender, PlayerControl player, PlayerControl target, bool shouldAnimate, int seer = -1)
    {
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.Shapeshift, seer)
            .WriteNetObject(target)
            .Write(shouldAnimate)
            .EndRpc();
        if (seer < 0)
            player.Shapeshift(target, shouldAnimate);
        return sender;
    }
    public static void RpcResetAbilityCooldown(this PlayerControl target, CustomRpcSender sender = null)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (PlayerControl.LocalPlayer.PlayerId == target.PlayerId)
        {
            PlayerControl.LocalPlayer.Data.Role.SetCooldown();
        }
        else
        {
            MurderHelpers.RpcForceGuard(target, target, target, sender);
        }
    }

    public static void RpcRevertShapeshiftUnchecked(this PlayerControl player, bool shouldAnimate, PlayerControl seer = null)
    {
        if (seer is not null)
        {
            if (AmongUsClient.Instance.AmClient && PlayerControl.LocalPlayer == seer)
            {
                player.Shapeshift(player, shouldAnimate);
            }
            else
            {
                MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(player.NetId, 46, SendOption.None, seer.GetClientId());
                messageWriter.WriteNetObject(player);
                messageWriter.Write(shouldAnimate);
                AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
            }
        }
        else
        {
            if (AmongUsClient.Instance.AmClient)
            {
                player.Shapeshift(player, shouldAnimate);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(player.NetId, 46, SendOption.None);
            messageWriter.WriteNetObject(player);
            messageWriter.Write(shouldAnimate);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }

    public static void RpcExitVentUnchecked(this PlayerPhysics player, int id)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            ((MonoBehaviour)player).StopAllCoroutines();
            ((MonoBehaviour)player).StartCoroutine(player.CoExitVent(id));
        }
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(player.NetId, 20, SendOption.None);
        messageWriter.WritePacked(id);
        messageWriter.EndMessage();
    }

    public static void RpcOpenToilet()
    {
        foreach (byte i in new[] { 79, 80, 81, 82 })
        {
            Logger.Info($"amount:{i}", "RpcOpenToilet");
            MapUtilities.CachedShipStatus.RpcUpdateSystem(SystemTypes.Doors, i);
        }
    }
}