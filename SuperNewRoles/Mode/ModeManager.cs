using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions.Categories;
using UnityEngine;

namespace SuperNewRoles.Mode;

/// <summary>
/// ゲームモードのマネージャー
/// </summary>
public static class ModeManager
{
    private static IModeBase currentMode = null;
    private static readonly Dictionary<ModeId, Func<IModeBase>> modeFactories = new()
    {
        { ModeId.WCBattleRoyal, () => WCBattleRoyalMode.Instance },
        { ModeId.BattleRoyal, () => BattleRoyalMode.Instance }
    };

    /// <summary>
    /// 現在のモードを取得
    /// </summary>
    public static IModeBase CurrentMode => currentMode;

    /// <summary>
    /// モードがアクティブかどうか
    /// </summary>
    public static bool IsModeActive => currentMode != null && Categories.ModeOption != ModeId.Default;

    /// <summary>
    /// ゲーム開始時の処理
    /// </summary>
    public static void OnGameStart()
    {
        // 現在のモード設定を取得
        var selectedMode = Categories.ModeOption;

        if (selectedMode == ModeId.Default)
        {
            currentMode = null;
            return;
        }

        // モードのインスタンスを作成
        if (modeFactories.TryGetValue(selectedMode, out var factory))
        {
            currentMode = factory();
            Logger.Info($"Mode activated: {currentMode.ModeName}");
            currentMode.OnGameStart();
        }
        else
        {
            Logger.Error($"Mode factory not found for: {selectedMode}");
            currentMode = null;
        }
    }

    /// <summary>
    /// ゲーム終了時の処理
    /// </summary>
    public static void OnGameEnd()
    {
        if (currentMode != null)
        {
            currentMode.OnGameEnd();
            currentMode = null;
        }
    }

    /// <summary>
    /// プレイヤー死亡時の処理
    /// </summary>
    public static void OnPlayerDeath(PlayerControl player, PlayerControl killer)
    {
        currentMode?.OnPlayerDeath(player, killer);
    }


    /// <summary>
    /// 勝利条件チェック
    /// </summary>
    public static bool CheckWinCondition()
    {
        return currentMode?.CheckWinCondition() ?? false;
    }

    public static bool IsMode(ModeId mode)
    {
        return Categories.ModeOption == mode;
    }
}