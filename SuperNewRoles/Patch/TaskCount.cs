using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Patch
{
    class TaskCount
    {
        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
        public static class NormalPlayerTaskPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                if (__instance.IsComplete && __instance.Arrow?.isActiveAndEnabled == true)
                    __instance.Arrow?.gameObject?.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.FixedUpdate))]
        public static class AirshipUploadTaskPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                if (__instance.IsComplete)
                    __instance.Arrows?.DoIf(x => x != null && x.isActiveAndEnabled, x => x.gameObject?.SetActive(false));
            }
        }
        public static Tuple<int, int> TaskDateNoClearCheck(GameData.PlayerInfo playerInfo)
        {
            int TotalTasks = 0;
            int CompletedTasks = 0;

                for (int j = 0; j < playerInfo.Tasks.Count; j++)
                {
                    TotalTasks++;
                    if (playerInfo.Tasks[j].Complete)
                    {
                        CompletedTasks++;
                    }
                }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }
        public static Tuple<int, int> TaskDate(GameData.PlayerInfo playerInfo)
        {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (!playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
                playerInfo.Role && playerInfo.Role.TasksCountTowardProgress
                )
            {

                for (int j = 0; j < playerInfo.Tasks.Count; j++)
                {
                    TotalTasks++;
                    if (playerInfo.Tasks[j].Complete)
                    {
                        CompletedTasks++;
                    }
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch
        {
            public static bool Prefix(GameData __instance)
            {
                if (!Mode.ModeHandler.isMode(Mode.ModeId.Default))
                {
                    return true;
                }
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                    var (playerCompleted, playerTotal) = TaskDate(playerInfo);
                    if (!RoleHelpers.isClearTask(playerInfo.Object))
                    {
                        __instance.TotalTasks += playerTotal;
                        __instance.CompletedTasks += playerCompleted;
                    }
                }
                return false;
            }
        }
    }
}
