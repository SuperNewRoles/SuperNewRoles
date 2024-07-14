using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Mode.PantsRoyal;
public class TurnData
{
    public int StartPlayerCount;
    public int EndPlayerCount;
    public float TurnTimer;
    public float TurnTimerFirst;
    public float StartTimer;
    public float LastUpdateStartTimer;
    public float LastUpdateCountdownTime;
    public bool CooldowntextReseted = false;
    public bool IsStarted;
    public TurnData(bool IsStart = true)
    {
        StartPlayerCount = 0;
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.IsAlive()) StartPlayerCount++;
        }
        if (StartPlayerCount > 10)
        {
            TurnTimerFirst = StartPlayerCount * 6f;
        }
        else if (StartPlayerCount > 3)
        {
            TurnTimerFirst = StartPlayerCount * 10f;
        }
        else if (StartPlayerCount == 3)
        {
            TurnTimerFirst = StartPlayerCount * 15f;
        }
        else if (StartPlayerCount == 2)
        {
            TurnTimerFirst = StartPlayerCount * 17.5f;
        }
        else
        {
            Logger.Info("エラーが発生しました。:" + StartPlayerCount.ToString());
        }
        TurnTimer = TurnTimerFirst;
        EndPlayerCount = (int)Math.Floor(StartPlayerCount / 2.0);
        StartTimer = 5;
        LastUpdateStartTimer = StartTimer + 1;
        IsStarted = IsStart;
        LastUpdateCountdownTime = TurnTimer + 11;
    }
    public void EndTurn()
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.IsBot()) continue;
            if (player.IsDead()) continue;
            if (main.IsPantsHaver(player)) continue;
            player.RpcSnapTo(new(-30, 30));
            player.RpcMurderPlayer(player, true);
        }
        if (ModHelpers.GetAlivePlayerCount() <= 1)
            main.GameEnd();
        else
        {
            main.PantsHaversId = new();
            main.AssignPants();
            main.AssignAllHaversPants();
            main.CurrentTurnData = new(false);
        }
    }
}