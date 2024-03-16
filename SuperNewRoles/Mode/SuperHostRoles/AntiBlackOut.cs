using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class AntiBlackOut
{
    private static PlayerData<bool> PlayerDeadData;
    private static PlayerData<bool> PlayerDisconnectedData;
    private static bool CantProcess { get { return _cantProcess; } }
    private static bool _cantProcess;
    private static bool ProcessNow;
    private static bool IsModdedSerialize;

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
        Logger.Info("ONWRAPUPPPPPPP");
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            player.Data.IsDead = PlayerDeadData[player];
            player.Data.Disconnected = PlayerDisconnectedData[player];
        }
        new LateTask(() => {
            IsModdedSerialize = true;
            RPCHelper.RpcSyncGameData();
            IsModdedSerialize = false;
            ProcessNow = false;
        }, 0.5f);
        DestroySavedData();
    }

    private static void SetAllDontDead(GameData.PlayerInfo exiled)
    {
        InitalSavedData();
        bool NeedDesyncSerialize = false;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            var desyncdetail = RoleSelectHandler.GetDesyncRole(player.GetRole());
            if (desyncdetail.IsDesync &&
                desyncdetail.RoleType is
                AmongUs.GameOptions.RoleTypes.Impostor or
                AmongUs.GameOptions.RoleTypes.Shapeshifter or
                AmongUs.GameOptions.RoleTypes.ImpostorGhost
            )
            {
                NeedDesyncSerialize = true;
                break;
            }

        }
        if (NeedDesyncSerialize)
            DesyncDontDead(exiled);
        else
            SyncDontDead(exiled);
    }
    private static void DesyncDontDead(GameData.PlayerInfo exiled)
    {
        Logger.Info("Selected DesyncDontDead");
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            PlayerDeadData[player] = player.Data.IsDead;
            PlayerDisconnectedData[player] = player.Data.Disconnected;
        }
        foreach (PlayerControl seer in PlayerControl.AllPlayerControls)
        {
            bool IsImpoAlived = false;
            var desyncdetail = RoleSelectHandler.GetDesyncRole(seer.GetRole());
            if (!desyncdetail.IsDesync || !(
                desyncdetail.RoleType is
                AmongUs.GameOptions.RoleTypes.Impostor or
                AmongUs.GameOptions.RoleTypes.Shapeshifter or
                AmongUs.GameOptions.RoleTypes.ImpostorGhost)
            ) {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsImpostor() && !IsImpoAlived &&
                        (exiled == null || exiled.PlayerId != player.PlayerId))
                    {
                        IsImpoAlived = true;
                        player.Data.IsDead = false;
                        player.Data.Disconnected = false;
                        continue;
                    }
                    if (!player.IsImpostor())
                    {
                        player.Data.IsDead = false;
                        player.Data.Disconnected = false;
                        continue;
                    }
                    player.Data.Disconnected = true;
                }
            }
            else
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    player.Data.IsDead = false;
                    player.Data.Disconnected = false;
                }
            }
            IsModdedSerialize = true;
            RPCHelper.RpcSyncGameData(seer.GetClientId());
            IsModdedSerialize = false;
        }
    }
    private static void SyncDontDead(GameData.PlayerInfo exiled)
    {
        Logger.Info("Selected SyncDontDead");
        bool IsImpoAlived = false;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            PlayerDeadData[player] = player.Data.IsDead;
            PlayerDisconnectedData[player] = player.Data.Disconnected;
            if (player.IsImpostor() && !IsImpoAlived &&
                (exiled == null || exiled.PlayerId != player.PlayerId))
            {
                IsImpoAlived = true;
                player.Data.IsDead = false;
                player.Data.Disconnected = false;
                continue;
            }
            if (!player.IsImpostor())
            {
                player.Data.IsDead = false;
                player.Data.Disconnected = false;
                continue;
            }
            player.Data.Disconnected = true;
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
    }
}
