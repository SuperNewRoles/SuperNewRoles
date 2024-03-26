using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class AntiBlackOut
{
    public enum SupportType
    {
        NoneExile,
        DeadExile,
        DoubleVotedAfterExile,
        DefaultExile
    }
    public enum ABOInformationType
    {
        FirstDead,
        OnlyDesyncImpostorDead,
        AliveCanViewDeadPlayerChat,
        EndAliveCanViewDeadPlayerChat
    }
    private static PlayerData<bool> PlayerDeadData;
    private static PlayerData<bool> PlayerDisconnectedData;
    private static HashSet<(byte, RoleTypes)> RoleChangedData;
    private static bool CantProcess { get { return _cantProcess; } }
    private static bool _cantProcess;
    private static bool ProcessNow;
    private static bool IsModdedSerialize;
    public static GameData.PlayerInfo RealExiled;

    public static void OnDisconnect(GameData.PlayerInfo exiled)
    {
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles))
            return;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            player.Data.IsDead = PlayerDeadData[player];
            player.Data.Disconnected = PlayerDisconnectedData[player];
        }
        SetAllDontDead(exiled);
    }
    public static void SendAntiBlackOutInformation(PlayerControl target, ABOInformationType informationType, params string[] formatstrings)
    {
        CustomRpcSender crs = CustomRpcSender.Create();
        int targetClientId = target == null ? - 1 : target.GetClientId();
        if (target == null)
            target = PlayerControl.LocalPlayer;
        string SendName = CustomOptionHolder.Cs(RoleClass.JackalBlue, "AntiBlackOutChatTitle");
        string name = target.Data.PlayerName;
        string text = string.Format(ModTranslation.GetString($"AntiBlackOut{informationType}"), formatstrings);
        if (targetClientId == -1)
        {
            target.SetName(SendName);
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(target, text);
            target.SetName(name);
        }
        crs.AutoStartRpc(target.NetId, (byte)RpcCalls.SetName, tarGetClientId: targetClientId)
            .Write(SendName)
            .EndRpc()
            .AutoStartRpc(target.NetId, (byte)RpcCalls.SendChat, tarGetClientId: targetClientId)
            .Write(text)
            .EndRpc()
            .AutoStartRpc(target.NetId, (byte)RpcCalls.SetName, tarGetClientId: targetClientId)
            .Write(name)
            .EndRpc()
            .SendMessage();
    }

    public static void ClearAndReload()
    {
        _cantProcess = !AmongUsClient.Instance.AmHost || !ModeHandler.IsMode(ModeId.SuperHostRoles);
        ProcessNow = IsModdedSerialize = false;
        DestroySavedData();
    }
    public static bool CantSendGameData()
    {
        return ProcessNow && !IsModdedSerialize;
    }

    public static SupportType GetSupportType(GameData.PlayerInfo exiled)
    {
        if (exiled == null)
        {
            RealExiled = null;
            return SupportType.NoneExile;
        }
        int numImpostor = 0;
        int numCrewmate = 0;
        int deadPlayers = 0;
        int deadImpostorsOnDesync = 0;
        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
        {
            if (player.Role.IsImpostor)
                numImpostor++;
            else
                numCrewmate++;
            if (player.IsDead)
            {
                deadPlayers++;
                PlayerControl deadPlayer = player.Object;
                if (deadPlayer != null && IsPlayerDesyncImpostorTeam(deadPlayer))
                    deadImpostorsOnDesync++;
            }
        }
        RealExiled = exiled;
        if (deadPlayers <= 0)
            return SupportType.DoubleVotedAfterExile;
        return SupportType.DeadExile;
    }
    public static bool IsPlayerDesyncImpostorTeam(PlayerControl player)
    {
        var desyncdetail = RoleSelectHandler.GetDesyncRole(player.GetRole());
        return desyncdetail.IsDesync &&
            desyncdetail.RoleType is
            RoleTypes.Impostor or
            RoleTypes.Shapeshifter or
            RoleTypes.ImpostorGhost;
    }

    public static void OnMeetingHudClose(GameData.PlayerInfo exiled)
    {
        if (CantProcess)
            return;
        Logger.Info("Start OnMeetingHudClose");
        ProcessNow = true;
        new LateTask(() => SetAllDontDead(exiled), 4f);
    }
    public static void OnWrapUp()
    {
        if (CantProcess)
            return;
        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
        {
            player.IsDead = PlayerDeadData[player.PlayerId];
            player.Disconnected = PlayerDisconnectedData[player.PlayerId];
        }
        new LateTask(() => {
            if (RealExiled != null && RealExiled.Object != null)
                RealExiled.Object.RpcInnerExiled();
            IsModdedSerialize = true;
            RPCHelper.RpcSyncGameData();
            SendAntiBlackOutInformation(null, ABOInformationType.EndAliveCanViewDeadPlayerChat);
            foreach ((byte playerId, RoleTypes role) in RoleChangedData)
            {
                PlayerControl player = ModHelpers.PlayerById(playerId);
                if (player != null)
                    player.RpcSetRoleDesync(role, player);
            }
            IsModdedSerialize = false;
            ProcessNow = false;
        }, 2f);
        DestroySavedData();
    }

    private static void SetAllDontDead(GameData.PlayerInfo exiled)
    {
        InitalSavedData();
        bool NeedDesyncSerialize = false;
        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
        {
            PlayerDeadData[player.PlayerId] = player.IsDead;
            PlayerDisconnectedData[player.PlayerId] = player.Disconnected;
            if (player.Object != null && IsPlayerDesyncImpostorTeam(player.Object))
                NeedDesyncSerialize = true;

        }
        if (NeedDesyncSerialize)
            DesyncDontDead(exiled);
        else
            SyncDontDead(exiled);
        SendAntiBlackOutInformation(null, ABOInformationType.AliveCanViewDeadPlayerChat);
    }
    private static void DesyncDontDead(GameData.PlayerInfo exiled)
    {
        Logger.Info("Selected DesyncDontDead");
        foreach (PlayerControl seer in PlayerControl.AllPlayerControls)
        {
            if (seer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                continue;
            bool IsImpoAlived = false;
            if (!IsPlayerDesyncImpostorTeam(seer)) {
                foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
                {
                    if (!player.Role.IsImpostor || !IsImpoAlived || exiled == player)
                    {
                        if (player.Role.IsImpostor && !IsImpoAlived && (exiled == null || exiled.PlayerId != player.PlayerId))
                            IsImpoAlived = true;
                        player.IsDead = false;
                        player.Disconnected = false;
                        continue;
                    }
                    player.Disconnected = true;
                }
            }
            else
            {
                foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
                {
                    player.IsDead = false;
                    player.Disconnected = false;
                }
            }
            IsModdedSerialize = true;
            Logger.Info($"---------SendTo {seer.Data.PlayerName}({seer.GetClientId()})---------");
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                Logger.Info($"{player.PlayerName}({player.Object.GetClientId()}) -> {player.IsDead} : {player.Disconnected}");
            }
            if (PlayerDeadData[seer] && !PlayerDisconnectedData[seer] && !seer.Data.Role.IsImpostor && IsPlayerDesyncImpostorTeam(seer))
            {
                RoleChangedData.Add((seer.PlayerId, seer.Data.Role.Role));
                seer.RpcSetRoleDesync(RoleTypes.ImpostorGhost, seer);
            }
            RPCHelper.RpcSyncGameData(seer.GetClientId());
            IsModdedSerialize = false;
        }
    }
    public static void StartMeeting()
    {
        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
        {
            if (player.IsDead || player.Disconnected)
                continue;
            SendAntiBlackOutInformation(null, ABOInformationType.FirstDead);
            break;
        }
    }
    private static void SyncDontDead(GameData.PlayerInfo exiled)
    {
        Logger.Info("Selected SyncDontDead");
        bool IsImpoAlived = false;
        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
        {
            if (player.Role.IsImpostor && !IsImpoAlived &&
                (exiled == null || exiled.PlayerId != player.PlayerId))
            {
                IsImpoAlived = true;
                player.IsDead = false;
                player.Disconnected = false;
                continue;
            }
            if (!player.Role.IsImpostor)
            {
                player.IsDead = false;
                player.Disconnected = false;
                continue;
            }
            player.Disconnected = true;
        }
        IsModdedSerialize = true;
        RPCHelper.RpcSyncGameData();
        IsModdedSerialize = false;
    }
    private static void DestroySavedData()
    {
        PlayerDeadData = null;
        PlayerDisconnectedData = null;
    }
    private static void InitalSavedData()
    {
        PlayerDeadData = new();
        PlayerDisconnectedData = new();
        RoleChangedData = new();
    }
}
