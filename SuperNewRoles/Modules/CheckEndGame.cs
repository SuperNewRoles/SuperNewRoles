using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
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
    JackalDomination,
    PavlovsWin,
}

public enum SabotageSystemType
{
    LifeSupport,
    Critical
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class CoStartGamePatch
{
    public static void Postfix()
    {
        CheckGameEndPatch.CouldCheckEndGame = true;
    }
}
[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class CheckGameEndPatch
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(0.15);
    private static float _nextCheckTime;
    public static bool CouldCheckEndGame = true;

    public static bool Prefix()
    {
        if (!CouldCheckEndGame) return false;
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

        // パブロフ勝利
        if (stats.IsPavlovsWin)
            return VictoryType.PavlovsWin;

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
        (var winners, var color, var upperText) = GetEndGameData(victoryType);
        EndGamer.EndGame(reason, WinType.Default, winners, color, upperText);
    }

    private static (HashSet<ExPlayerControl> winners, Color32 color, string upperText) GetEndGameData(VictoryType victoryType)
    {
        switch (victoryType)
        {
            case VictoryType.ImpostorKill:
            case VictoryType.ImpostorVote:
            case VictoryType.ImpostorSabotage:
                return (ExPlayerControl.ExPlayerControls.Where(player => player.IsImpostorWinTeam()).ToHashSet(), Palette.ImpostorRed, "ImpostorWin");
            case VictoryType.CrewmateTask:
            case VictoryType.CrewmateVote:
                return (ExPlayerControl.ExPlayerControls.Where(player => player.IsCrewmate()).ToHashSet(), Palette.CrewmateBlue, "CrewmateWin");
            case VictoryType.JackalDomination:
                return (ExPlayerControl.ExPlayerControls.Where(player => player.IsJackalTeam()).ToHashSet(), Jackal.Instance.RoleColor, "JackalWin");
            case VictoryType.PavlovsWin:
                return (ExPlayerControl.ExPlayerControls.Where(player => player.IsPavlovsTeam()).ToHashSet(), PavlovsDog.Instance.RoleColor, "PavlovsWin");
            default:
                throw new ArgumentException($"Invalid victory type: {victoryType}");
        }
    }

    private static GameOverReason MapVictoryTypeToGameOverReason(VictoryType victoryType) => victoryType switch
    {
        VictoryType.ImpostorKill => GameOverReason.ImpostorsByKill,
        VictoryType.ImpostorVote => GameOverReason.ImpostorsByVote,
        VictoryType.ImpostorSabotage => GameOverReason.ImpostorsBySabotage,
        VictoryType.CrewmateTask => GameOverReason.CrewmatesByTask,
        VictoryType.CrewmateVote => GameOverReason.CrewmatesByVote,
        VictoryType.JackalDomination => (GameOverReason)CustomGameOverReason.JackalWin,
        VictoryType.PavlovsWin => (GameOverReason)CustomGameOverReason.PavlovsWin,
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
    public int PavlovsDogAlive { get; }
    public int PavlovsOwnerAlive { get; }
    public int TeamPavlovsAlive { get; }
    public int TotalKiller { get; }
    public int ArsonistAlive { get; }

    public bool IsKillerExist => TotalKiller > 0;

    public bool IsImpostorDominating => IsKillerWin(TeamImpostorsAlive);
    public bool IsJackalDominating => IsKillerWin(TeamJackalAlive);
    public bool IsPavlovsWin => IsKillerWin(TeamPavlovsAlive); public bool IsCrewmateVictory => !IsKillerExist;

    public PlayerStatistics()
    {
        // 生存中のプレイヤーのみを抽出
        var alivePlayers = GetAlivePlayers();

        // 各チームの生存者数をLINQで計算（条件が増えた場合もここを修正しやすい）
        TeamImpostorsAlive = alivePlayers.Count(player => player.IsImpostor());
        CrewAlive = alivePlayers.Count(player => player.IsCrewmate());
        TeamJackalAlive = alivePlayers.Count(player => player.IsJackalTeam());

        PavlovsDogAlive = alivePlayers.Count(player => player.Role == RoleId.PavlovsDog);
        PavlovsOwnerAlive = alivePlayers.Count(player => player.Role == RoleId.PavlovsOwner);
        if (PavlovsDogAlive > 0)
            TeamPavlovsAlive = PavlovsDogAlive + PavlovsOwnerAlive;
        else
            TeamPavlovsAlive = alivePlayers.Count(player => player.Role == RoleId.PavlovsOwner && player.GetAbility<PavlovsOwnerAbility>()?.HasRemainingDogCount() == true);
        TotalAlive = alivePlayers.Count();
        TotalKiller = alivePlayers.Count(player => player.IsNonCrewKiller());
    }

    // ExPlayerControl配列から生存しているプレイヤーを返すヘルパーメソッド
    private static IEnumerable<ExPlayerControl> GetAlivePlayers()
    {
        return ExPlayerControl.ExPlayerControlsArray
            .Where(player => player != null && !player.IsDead());
    }

    // 勝利条件の判定ロジックを切り出し、変更しやすいようにしている
    // 対象チームのキラー数が、全体の半数以上かつ全キラーがそのチームである場合、勝利と判定
    private bool IsKillerWin(int teamAlive)
    {
        return teamAlive >= TotalAlive - teamAlive && TotalKiller <= teamAlive && teamAlive != 0 && !(PavlovsDogAlive <= 0 && TeamPavlovsAlive > 0);
    }
}