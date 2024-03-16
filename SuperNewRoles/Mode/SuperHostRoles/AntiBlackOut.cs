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

    public static void OnMeetingHudClose()
    {
        if (CantProcess)
            return;
        Logger.Info("Start OnMeetingHudClose");
        ProcessNow = true;
        new LateTask(SetAllDontDead, 4f);
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
        }, 8f);
        DestroySavedData();
    }

    private static void SetAllDontDead()
    {
        InitalSavedData();
        bool IsImpoAlived = false;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            PlayerDeadData[player] = player.Data.IsDead;
            PlayerDisconnectedData[player] = player.Data.Disconnected;
            if (player.IsImpostor() && !IsImpoAlived)
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
