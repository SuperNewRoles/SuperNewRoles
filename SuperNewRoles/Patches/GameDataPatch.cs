using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public static class GameDataRecomputeTaskCountsPatch
{
    public static void Postfix(GameData __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        __instance.TotalTasks = 0;
        __instance.CompletedTasks = 0;
        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            NetworkedPlayerInfo playerInfo = player.Data;
            if (player.roleBase?.WinnerTeam == WinnerTeamType.Crewmate && player.IsCountTask())
            {
                var (playerCompleted, playerTotal) = player.GetAllTaskForShowProgress();
                __instance.TotalTasks += playerTotal;
                __instance.CompletedTasks += playerCompleted;
            }
        }
        if (__instance.TotalTasks <= 0)
            __instance.TotalTasks = 1;
        else if (__instance.TotalTasks != __instance.CompletedTasks)
            __instance.TotalTasks += 2;
    }
}
