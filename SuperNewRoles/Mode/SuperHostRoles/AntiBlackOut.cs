using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class AntiBlackOut
{
    public class GamePlayerData
    {
        public byte PlayerId { get; }
        public RoleTypes roleTypes { get; }
        public bool IsDead { get; }
        public bool Disconnected { get; }
        public GamePlayerData(NetworkedPlayerInfo playerInfo)
        {
            if (playerInfo == null)
                throw new NotImplementedException("PlayerInfo is null");
            PlayerId = playerInfo.PlayerId;
            roleTypes = playerInfo.Role.Role;
            IsDead = playerInfo.IsDead;
            Disconnected = playerInfo.Disconnected;
        }
    }
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
        AllExileWillDobuleVoted,
        OnlyDesyncImpostorDead,
        AliveCanViewDeadPlayerChat,
        EndAliveCanViewDeadPlayerChat
    }
    public static PlayerData<GamePlayerData> GamePlayers;
    private static bool CantProcess { get { return _cantProcess; } }
    private static bool _cantProcess;
    private static bool ProcessNow;
    private static bool IsModdedSerialize;
    public static NetworkedPlayerInfo RealExiled;

    public static void OnDisconnect(NetworkedPlayerInfo exiled)
    {
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles))
            return;
        // う～ん、ここどうしようね。なくても問題なさそうだけど...
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
            .Write(target.Data.NetId)
            .Write(SendName)
            .EndRpc()
            .AutoStartRpc(target.NetId, (byte)RpcCalls.SendChat, tarGetClientId: targetClientId)
            .Write(text)
            .EndRpc()
            .AutoStartRpc(target.NetId, (byte)RpcCalls.SetName, tarGetClientId: targetClientId)
            .Write(target.Data.NetId)
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

    public static SupportType GetSupportType(NetworkedPlayerInfo exiled)
    {
        if (exiled == null)
        {
            RealExiled = null;
            return SupportType.NoneExile;
        }
        RealExiled = exiled;
        return
            PlayerControl.AllPlayerControls.Count >= BlackOutSafetyShowExiledPlayer
            ? SupportType.DefaultExile
            : SupportType.NoneExile;
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

    public static void OnMeetingHudClose(NetworkedPlayerInfo exiled)
    {
        if (CantProcess)
            return;
        Logger.Info("Start OnMeetingHudClose");
        ProcessNow = true;
        new LateTask(() =>
        {
            RoleBaseManager.DoInterfaces<ISHRAntiBlackout>(x => x.StartAntiBlackout());
            StartAntiBlackOutProcess(exiled);
        }, 4f);
    }
    // 最低限ここまで未切断者がいれば正常に追放者を表示できるっていう人数大丈夫
    private const int BlackOutSafetyShowExiledPlayer = 4;
    public static void StartAntiBlackOutProcess(NetworkedPlayerInfo exiled)
    {
        InitalSavedData();
        bool CanShowExiledPlayer = PlayerControl.AllPlayerControls.Count >= BlackOutSafetyShowExiledPlayer;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            GamePlayers[player] = new(player.Data);
        if (exiled?.PlayerId != PlayerControl.LocalPlayer.PlayerId)
        {
            SendOtherCrewmate(null);
        }
        else
        {
            foreach (PlayerControl seer in PlayerControl.AllPlayerControls)
            {
                if (seer.IsMod())
                    continue;
                SendOtherCrewmate(seer);
            }
        }
    }
    private static void SendOtherCrewmate(PlayerControl seer)
    {
        CustomRpcSender sender = CustomRpcSender.Create($"StartAntiBlackOut_To:{(seer?.PlayerId.ToString() ?? "All")}", sendOption: Hazel.SendOption.Reliable);
        int seerClientId = seer?.GetClientId() ?? -1;
        PlayerControl impostorPlayer = seer ?? PlayerControl.LocalPlayer;
        sender.RpcSetRole(impostorPlayer, RoleTypes.Impostor, true, seerClientId);
        foreach (PlayerControl target in PlayerControl.AllPlayerControls)
        {
            if (target == impostorPlayer)
                continue;
            sender.RpcSetRole(target, RoleTypes.Crewmate, true, seerClientId);
        }
        sender.SendMessage();
    }
    public static void OnWrapUp()
    {
        if (CantProcess)
            return;
        if (Chat.IsOldSHR)
        {
            Logger.Info("AntiBlackOut Passed. Reason:Chat.IsOldSHR is true.");
            return;
        }
        Logger.Info("Running AntiBlackOut.");
        if (GamePlayers == null)
            throw new NotImplementedException("GamePlayers is null");
        new LateTask(() => {
            if (RealExiled != null && RealExiled.Object != null)
                RealExiled.Object.Exiled();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsAlive())
                    continue;
                SendAntiBlackOutInformation(player, ABOInformationType.EndAliveCanViewDeadPlayerChat);
            }
            List<(PlayerControl player, RoleTypes role)> DesyncPlayers = new();
            foreach(GamePlayerData gamePlayerData in GamePlayers.Values)
            {
                PlayerControl player = ModHelpers.PlayerById(gamePlayerData.PlayerId);
                if (player == null)
                {
                    Logger.Error($"GamePlayerData({gamePlayerData.PlayerId}) is null.","AntiBlackOutWrapUp");
                    continue;
                }
                RoleTypes ToRoleTypes = (player.IsDead() && !gamePlayerData.IsDead) ?
                         (gamePlayerData.roleTypes.IsImpostorRole() ?
                          RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost) :
                         gamePlayerData.roleTypes;
                ISupportSHR supportSHR = (ISupportSHR)player.GetRoleBase();
                if (supportSHR != null && player.IsAlive() && !supportSHR.IsDesync && !supportSHR.RealRole.IsImpostorRole())
                    ToRoleTypes = supportSHR.RealRole;
                if (player.IsDead() && !RoleManager.IsGhostRole(ToRoleTypes))
                    Logger.Info($"What's this!? {ToRoleTypes} {player.PlayerId}");
                player.RpcSetRole(ToRoleTypes, true);
                var desyncRole = RoleSelectHandler.GetDesyncRole(player.GetRole());
                if (desyncRole.IsDesync && desyncRole.RoleType.IsImpostorRole())
                    DesyncPlayers.Add((player, desyncRole.RoleType));
                else if (supportSHR != null && supportSHR.IsDesync)
                    DesyncPlayers.Add((player, supportSHR.DesyncRole));
            }
            new LateTask(() =>
            {
                foreach (var desyncDetail in DesyncPlayers)
                {
                    if (desyncDetail.player.IsMod() || desyncDetail.player.IsDead())
                        continue;
                    desyncDetail.player.RpcSetRoleDesync(desyncDetail.role, true);
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player == desyncDetail.player)
                            continue;
                        player.RpcSetRoleDesync(
                            player.IsImpostor() ?
                               (player.IsDead() ?
                                RoleTypes.CrewmateGhost :
                                RoleTypes.Crewmate) :
                            player.Data.Role.Role, canOverride: true,
                            desyncDetail.player
                        );
                    }
                }
                IsModdedSerialize = true;
                RPCHelper.RpcSyncAllNetworkedPlayer();
                IsModdedSerialize = false;
                ChangeName.SetRoleNames();
                RoleBaseManager.DoInterfaces<ISHRAntiBlackout>(x => x.EndAntiBlackout());
                new LateTask(() =>
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.IsMod() || player.IsDead())
                            continue;
                        RoleTypes? DesyncRole = RoleSelectHandler.GetDesyncRole(player);
                        if (player.IsImpostor() || (DesyncRole.HasValue && DesyncRole.Value.IsImpostorRole()))
                            player.RpcShowGuardEffect(player);
                    }
                }, 0.5f);
                DestroySavedData();
            }, 0.2f);
            ProcessNow = false;
        }, 0.75f);
    }
    /*
    private static void SetAllDontDead(NetworkedPlayerInfo exiled)
    {
        InitalSavedData();
        bool NeedDesyncSerialize = false;
        foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
        {
            PlayerDeadData[player.PlayerId] = player.IsDead;
            PlayerDisconnectedData[player.PlayerId] = player.Disconnected;
            if (player.Object != null && IsPlayerDesyncImpostorTeam(player.Object))
                NeedDesyncSerialize = true;

        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsAlive())
                continue;
            SendAntiBlackOutInformation(player, ABOInformationType.AliveCanViewDeadPlayerChat);
        }
        if (NeedDesyncSerialize)
            DesyncDontDead(exiled);
        else
            SyncDontDead(exiled);
    }
    private static void DesyncDontDead(NetworkedPlayerInfo exiled)
    {
        Logger.Info("Selected DesyncDontDead");
        foreach (PlayerControl seer in PlayerControl.AllPlayerControls)
        {
            if (seer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                continue;
            bool IsImpoAlived = false;
            if (!IsPlayerDesyncImpostorTeam(seer)) {
                foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
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
                foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
                {
                    player.IsDead = false;
                    player.Disconnected = false;
                }
            }
            IsModdedSerialize = true;
            Logger.Info($"---------SendTo {seer.Data.PlayerName}({seer.GetClientId()})---------");
            foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
            {
                Logger.Info($"{player.PlayerName}({player.Object.GetClientId()}) -> {player.IsDead} : {player.Disconnected}");
            }
            if (PlayerDeadData[seer] && !PlayerDisconnectedData[seer] && !seer.Data.Role.IsImpostor && IsPlayerDesyncImpostorTeam(seer))
            {
                HashSet<(byte, RoleTypes)> seerchanges = [
                    (seer.PlayerId, seer.Data.Role.Role)
                ];
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.Role.IsImpostor)
                        continue;
                    seerchanges.Add((player.PlayerId, player.Data.Role.Role));
                    player.RpcSetRoleDesync(RoleTypes.CrewmateGhost, true, seer);
                }
                seer.RpcSetRoleDesync(RoleTypes.ImpostorGhost, true, seer);
                RoleChangedData.Add((seer.PlayerId, seerchanges));
            }
            RPCHelper.RpcSyncAllNetworkedPlayer(seer.GetClientId());
            IsModdedSerialize = false;
        }
    }*/
    public static void StartMeeting()
    {
        SendAntiBlackOutInformation(null, ABOInformationType.AllExileWillDobuleVoted);
    }
    private static void SyncDontDead(NetworkedPlayerInfo exiled)
    {
        Logger.Info("Selected SyncDontDead");
        bool IsImpoAlived = false;
        foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
        {
            player.IsDead = player.Disconnected = false;
            /*
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
            player.Disconnected = true;*/
        }
        IsModdedSerialize = true;
        RPCHelper.RpcSyncAllNetworkedPlayer();
        IsModdedSerialize = false;
    }
    private static void DestroySavedData()
    {
        GamePlayers = null;
    }
    private static void InitalSavedData()
    {
        GamePlayers = new();
    }
}
