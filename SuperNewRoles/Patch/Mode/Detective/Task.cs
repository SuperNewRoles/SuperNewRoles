using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.Detective
{
    class Task
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        public static class GameDataRecomputeTaskCountsPatch
        {
            public static void Postfix(GameData __instance)
            {
                if (!ModeHandler.isMode(ModeId.Detective)) return;
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++)
                {
                    try
                    {
                        GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                        if (!playerInfo.Disconnected && playerInfo.Object.isCrew() && (playerInfo.Object.PlayerId != main.DetectivePlayer.PlayerId || !main.IsDetectiveNotTask))
                        {
                            var (playerCompleted, playerTotal) = Patch.TaskCount.TaskDate(playerInfo);
                            __instance.TotalTasks += playerTotal;
                            __instance.CompletedTasks += playerCompleted;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
