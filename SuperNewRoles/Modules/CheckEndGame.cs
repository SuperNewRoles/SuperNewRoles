using System;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Modules;

public enum VictoryType
{
    None,
    ImpostorKill,
    ImpostorVote,
    ImpostorSabotage,
    CrewmateTask,
    CrewmateVote,
    JackalDomination
}

public enum SabotageSystemType
{
    LifeSupport,
    Critical
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class CheckGameEndPatch
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(0.15);
    private static float _nextCheckTime;

    public static bool Prefix()
    {
        try
        {
            return HandleGameEndCheck();
        }
        catch (Exception ex)
        {
            Logger.Error($"ゲーム終了チェック中にエラーが発生: {ex}");
            return true; // エラー時はバニラの処理に任せる
        }
    }

    private static bool HandleGameEndCheck()
    {
        if (!ShouldCheckEndGame()) return false;

        using var gameState = new GameState();
        if (!gameState.IsValid) return false;

        var victoryType = DetermineVictoryType(gameState);
        if (victoryType != VictoryType.None)
        {
            EndGame(victoryType);
            return false;
        }

        return false;
    }

    private static bool ShouldCheckEndGame()
    {
        if (Time.time < _nextCheckTime) return false;

        _nextCheckTime = Time.time + (float)CheckInterval.TotalSeconds;

        return GameData.Instance != null &&
               !(CustomOptionManager.DebugMode && CustomOptionManager.DebugModeNoGameEnd) &&
               !DestroyableSingleton<TutorialManager>.InstanceExists;
    }

    private static VictoryType DetermineVictoryType(GameState state)
    {
        if (!state.IsValid) return VictoryType.None;

        return DeterminePriorityVictory(state) ??
               DeterminePlayerBasedVictory(state) ??
               VictoryType.None;
    }

    private static VictoryType? DeterminePriorityVictory(GameState state)
    {
        // サボタージュとタスク完了は他の勝利条件より優先
        // このメソッドは、サボタージュがアクティブであるか、すべてのタスクが完了しているかをチェックします。
        // これらの条件が満たされた場合、他の勝利条件よりも優先されます。
        if (state.IsSabotageActive())
            return VictoryType.ImpostorSabotage;

        if (state.IsAllTasksCompleted())
            return VictoryType.CrewmateTask;

        return null;
    }
    private static VictoryType? DeterminePlayerBasedVictory(GameState state)
    {
        var stats = new PlayerStatistics();

        // インポスター勝利
        if (stats.IsImpostorDominating)
            return GameData.LastDeathReason == DeathReason.Kill
                ? VictoryType.ImpostorKill
                : VictoryType.ImpostorVote;

        // ジャッカル勝利
        if (stats.IsJackalDominating)
            return VictoryType.JackalDomination;

        // クルーメイト勝利
        if (stats.IsCrewmateVictory)
            return VictoryType.CrewmateVote;

        return null;
    }

    private static void EndGame(VictoryType victoryType)
    {
        if (ShipStatus.Instance != null)
            ShipStatus.Instance.enabled = false;

        var reason = MapVictoryTypeToGameOverReason(victoryType);
        GameManager.Instance.RpcEndGame(reason, false);
    }

    private static GameOverReason MapVictoryTypeToGameOverReason(VictoryType victoryType) => victoryType switch
    {
        VictoryType.ImpostorKill => GameOverReason.ImpostorByKill,
        VictoryType.ImpostorVote => GameOverReason.ImpostorByVote,
        VictoryType.ImpostorSabotage => GameOverReason.ImpostorBySabotage,
        VictoryType.CrewmateTask => GameOverReason.HumansByTask,
        VictoryType.CrewmateVote => GameOverReason.HumansByVote,
        VictoryType.JackalDomination => (GameOverReason)CustomGameOverReason.JackalWin,
        _ => throw new ArgumentException($"無効な勝利タイプ: {victoryType}")
    };
}

public sealed class GameState : IDisposable
{
    private readonly ShipStatus _shipStatus;
    private bool _disposed;

    public bool IsValid => !_disposed && _shipStatus != null && _shipStatus.Systems != null;

    public GameState()
    {
        _shipStatus = ShipStatus.Instance;
    }

    public bool IsSabotageActive() =>
        IsValid && (IsSabotageActiveOfType(SabotageSystemType.LifeSupport) ||
                   IsSabotageActiveOfType(SabotageSystemType.Critical));

    public bool IsAllTasksCompleted() =>
        GameData.Instance != null &&
        GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks;

    private bool IsSabotageActiveOfType(SabotageSystemType type)
    {
        return type switch
        {
            SabotageSystemType.LifeSupport => CheckLifeSupportSabotage(),
            SabotageSystemType.Critical => CheckCriticalSabotage(),
            _ => false
        };
    }

    private bool CheckLifeSupportSabotage()
    {
        if (!_shipStatus.Systems.ContainsKey(SystemTypes.LifeSupp)) return false;

        var lifeSupp = _shipStatus.Systems[SystemTypes.LifeSupp].TryCast<LifeSuppSystemType>();
        return lifeSupp != null && lifeSupp.Countdown < 0f;
    }

    private bool CheckCriticalSabotage()
    {
        var systemTypes = new[] { SystemTypes.HeliSabotage, SystemTypes.Laboratory, SystemTypes.Reactor };
        return systemTypes.Any(type => IsCriticalSystemSabotaged(type));
    }

    private bool IsCriticalSystemSabotaged(SystemTypes type)
    {
        if (!_shipStatus.Systems.ContainsKey(type)) return false;

        var system = _shipStatus.Systems[type].TryCast<ICriticalSabotage>();
        return system != null && system.Countdown < 0f;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}

public class PlayerStatistics
{
    public int TeamImpostorsAlive { get; }
    public int CrewAlive { get; }
    public int TotalAlive { get; }
    public int TeamJackalAlive { get; }
    public int TotalKiller { get; }
    public bool IsKillerExist => TotalKiller > 0;

    public bool IsImpostorDominating =>
        IsKillerWin(TeamImpostorsAlive);

    public bool IsJackalDominating =>
        IsKillerWin(TeamJackalAlive);

    public bool IsCrewmateVictory =>
        !IsKillerExist;

    private bool IsKillerWin(int killerAlive)
        => killerAlive >= TotalAlive - killerAlive && TotalKiller == killerAlive && killerAlive != 0;

    public PlayerStatistics()
    {
        var stats = CalculatePlayerStats();
        TeamImpostorsAlive = stats.impostors;
        CrewAlive = stats.crew;
        TeamJackalAlive = stats.jackal;
        TotalAlive = stats.total;
        TotalKiller = stats.impostors + stats.jackal;
    }

    private static (int impostors, int crew, int jackal, int total) CalculatePlayerStats()
    {
        int impostors = 0;
        int crew = 0;
        int jackal = 0;
        int total = 0;

        foreach (var player in ExPlayerControl.ExPlayerControlsArray)
        {
            if (player == null || player.IsDead()) continue;

            if (player.IsImpostor())
            {
                impostors++;
            }
            else if (player.IsCrewmate())
            {
                crew++;
            }
            else if (player.IsJackalTeam())
            {
                jackal++;
            }

            total++;
        }

        return (impostors, crew, jackal, total);
    }
}