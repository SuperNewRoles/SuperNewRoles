using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public static class GameDataRecomputeTaskCountsPatch
{
    public static bool Prefix()
    {
        // ホストでは下のPostfixが両カウンターを全て再集計するため、本体の走査は不要。
        // 集計をPostfixに残し、本体をスキップした場合も従来と同じ結果を設定する。
        return !AmongUsClient.Instance.AmHost;
    }

    public static void Postfix(GameData __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        __instance.TotalTasks = 0;
        __instance.CompletedTasks = 0;
        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            NetworkedPlayerInfo playerInfo = player.Data;
            // 切断していないクルーメイトのタスクのみカウント
            if (!playerInfo.Disconnected && player.roleBase?.WinnerTeam == WinnerTeamType.Crewmate && player.IsCountTask())
            {
                var (playerCompleted, playerTotal) = player.GetAllTaskForShowProgress();
                __instance.TotalTasks += playerTotal;
                __instance.CompletedTasks += playerCompleted;
            }
        }
        // タスクが存在しない場合のフォールバック
        if (__instance.TotalTasks <= 0)
            __instance.TotalTasks = 1;
        else if (__instance.TotalTasks != __instance.CompletedTasks)
            __instance.TotalTasks += 2;
    }
}
