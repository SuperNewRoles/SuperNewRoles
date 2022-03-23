using HarmonyLib;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class CountTask
    {

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch
        {
            public static void Postfix(GameData __instance)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (!Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles)) return;
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                    if (!RoleHelpers.isClearTask(playerInfo.Object))
                    {
                        var (playerCompleted, playerTotal) = TaskCount.TaskDate(playerInfo);
                        __instance.TotalTasks += playerTotal;
                        __instance.CompletedTasks += playerCompleted;
                    }
                }
                return;
            }
        }
    }
}
