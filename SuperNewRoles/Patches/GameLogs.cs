/*using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches;

/// <summary>
/// ゲームの重要なイベントをログ出力するクラス
/// バグの特定やデバッグに役立つ情報を提供します
/// </summary>
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
public static class GameStartLogPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        Logger.Info("=== ゲーム開始 ===", "GameLogs");
        Logger.Info($"プレイヤー数: {PlayerControl.AllPlayerControls.Count}", "GameLogs");
        Logger.Info($"マップ: {GameOptionsManager.Instance.CurrentGameOptions.MapId}", "GameLogs");
        Logger.Info($"ローカルプレイヤー: {PlayerControl.LocalPlayer?.Data?.PlayerName ?? "Unknown"} (ID: {PlayerControl.LocalPlayer?.PlayerId ?? 255})", "GameLogs");

        // ローカルプレイヤーの役職情報をログ出力
        var localPlayer = ExPlayerControl.LocalPlayer;
        if (localPlayer != null)
        {
            Logger.Info($"ローカルプレイヤー役職: {localPlayer.Role}", "GameLogs");
            Logger.Info($"ローカルプレイヤー陣営: {(localPlayer.IsImpostor() ? "インポスター" : localPlayer.IsCrewmate() ? "クルー" : "第三陣営")}", "GameLogs");
        }

        Logger.Info("=== 全プレイヤー情報 ===", "GameLogs");
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            ExPlayerControl exPlayer = player;
            Logger.Info($"プレイヤー: {player.Data?.PlayerName ?? "Unknown"} (ID: {player.PlayerId}) - 役職: {exPlayer?.Role ?? RoleId.None}", "GameLogs");
        }
    }
}

/// <summary>
/// プレイヤーの死亡イベントをログ出力
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
public static class PlayerDeathLogPatch
{
    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        Logger.Info($"プレイヤー死亡: {__instance.Data?.PlayerName ?? "Unknown"} (ID: {__instance.PlayerId}) - 死因: {reason}", "GameLogs");
        Logger.Info($"残り生存者数: {PlayerControl.AllPlayerControls.Count(p => !p.Data.IsDead)}", "GameLogs");
    }
}

/// <summary>
/// 会議の開始をログ出力
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingStartLogPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        Logger.Info("=== 会議開始 ===", "GameLogs");
        Logger.Info($"生存者数: {PlayerControl.AllPlayerControls.Count(p => !p.Data.IsDead)}", "GameLogs");

        var reportedBody = __instance;
        if (reportedBody != null)
        {
            Logger.Info($"発見された死体: {reportedBody.PlayerName} (ID: {reportedBody.PlayerId})", "GameLogs");
        }
        else
        {
            Logger.Info("緊急会議が召集されました", "GameLogs");
        }
    }
}

/// <summary>
/// 投票結果をログ出力
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class VotingCompleteLogPatch
{
    public static void Postfix(MeetingHud __instance, MeetingHud.VoterState[] states, ExileController.State __result)
    {
        Logger.Info("=== 投票結果 ===", "GameLogs");

        foreach (var state in states)
        {
            var voter = ModHelpers.GetPlayerById(state.VoterId);
            var votedFor = state.VotedForId == 253 ? "スキップ" :
                          state.VotedForId == 254 ? "未投票" :
                          ModHelpers.GetPlayerById(state.VotedForId)?.Data?.PlayerName ?? "Unknown";

            Logger.Info($"投票者: {voter?.Data?.PlayerName ?? "Unknown"} → {votedFor}", "GameLogs");
        }

        var exiled = __result.exiled;
        if (exiled != null)
        {
            Logger.Info($"追放されたプレイヤー: {exiled.PlayerName} (ID: {exiled.PlayerId})", "GameLogs");
        }
        else
        {
            Logger.Info("引き分けまたは追放なし", "GameLogs");
        }
    }
}

/// <summary>
/// ゲーム終了をログ出力
/// </summary>
[HarmonyPatch(typeof(GameManager), nameof(GameManager.EndGame))]
public static class GameEndLogPatch
{
    public static void Postfix(GameManager __instance, GameOverReason reason)
    {
        Logger.Info("=== ゲーム終了 ===", "GameLogs");
        Logger.Info($"終了理由: {reason}", "GameLogs");

        Logger.Info("=== 最終生存者 ===", "GameLogs");
        foreach (var player in PlayerControl.AllPlayerControls.Where(p => !p.Data.IsDead))
        {
            var exPlayer = player.ToExPlayer();
            Logger.Info($"生存者: {player.Data?.PlayerName ?? "Unknown"} (ID: {player.PlayerId}) - 役職: {exPlayer?.Role ?? RoleId.Default}", "GameLogs");
        }
    }
}

/// <summary>
/// キル イベントをログ出力
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class MurderLogPatch
{
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        Logger.Info($"キル発生: {__instance.Data?.PlayerName ?? "Unknown"} が {target.Data?.PlayerName ?? "Unknown"} をキル", "GameLogs");
        Logger.Info($"キル場所: {target.transform.position}", "GameLogs");
    }
}

/// <summary>
/// ベント使用をログ出力
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.EnterVent))]
public static class VentEnterLogPatch
{
    public static void Postfix(PlayerControl __instance, int id)
    {
        Logger.Info($"ベント侵入: {__instance.Data?.PlayerName ?? "Unknown"} がベント {id} に入りました", "GameLogs");
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ExitVent))]
public static class VentExitLogPatch
{
    public static void Postfix(PlayerControl __instance, int id)
    {
        Logger.Info($"ベント退出: {__instance.Data?.PlayerName ?? "Unknown"} がベント {id} から出ました", "GameLogs");
    }
}

/// <summary>
/// サボタージュ発生をログ出力
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartCoroutine))]
public static class SabotageLogPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == PlayerControl.LocalPlayer && MapBehaviour.Instance != null)
        {
            Logger.Info($"サボタージュ検出: プレイヤー {__instance.Data?.PlayerName ?? "Unknown"} による操作", "GameLogs");
        }
    }
}

/// <summary>
/// エラー情報を詳細にログ出力
/// </summary>
public static class ErrorLogger
{
    public static void LogException(System.Exception ex, string context = "")
    {
        Logger.Error($"例外発生 {context}: {ex.GetType().Name} - {ex.Message}", "GameLogs");
        Logger.Error($"スタックトレース: {ex.StackTrace}", "GameLogs");

        if (ex.InnerException != null)
        {
            Logger.Error($"内部例外: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}", "GameLogs");
        }
    }

    public static void LogPlayerState(PlayerControl player, string context = "")
    {
        if (player == null)
        {
            Logger.Warning($"プレイヤーがnullです {context}", "GameLogs");
            return;
        }

        Logger.Info($"プレイヤー状態 {context}: {player.Data?.PlayerName ?? "Unknown"} (ID: {player.PlayerId})", "GameLogs");
        Logger.Info($"  位置: {player.transform.position}", "GameLogs");
        Logger.Info($"  生存状態: {!player.Data.IsDead}", "GameLogs");
        Logger.Info($"  移動可能: {player.CanMove}", "GameLogs");

        ExPlayerControl exPlayer = player;
        if (exPlayer != null)
        {
            Logger.Info($"  役職: {exPlayer.Role}", "GameLogs");
            Logger.Info($"  陣営: {(exPlayer.IsImpostor() ? "インポスター" : exPlayer.IsCrewmate() ? "クルー" : "第三陣営")}", "GameLogs");
        }
    }
}
*/