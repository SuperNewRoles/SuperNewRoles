using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class Task
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class DeadPlayerTaskPatch
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (ModeHandler.thisMode != ModeId.HideAndSeek) return;
                if (AmongUsClient.Instance.AmHost)
                {
                    if (!target.Data.Role.IsImpostor)
                    {
                        if (HASOptions.HASDeathTask.getBool())
                        {
                            foreach (PlayerTask task in target.myTasks)
                            {
                                task.Complete();
                            }
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        public static class GameDataRecomputeTaskCountsPatch
        {
            public static void Postfix(GameData __instance)
            {
                if (ModeHandler.isMode(ModeId.Default)) return;
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                    if (!playerInfo.IsDead && !playerInfo.Disconnected)
                    {
                        var (playerCompleted, playerTotal) = SuperNewRoles.Patch.TaskCount.TaskDate(playerInfo);
                        __instance.TotalTasks += playerTotal;
                        __instance.CompletedTasks += playerCompleted;
                    }
                }
                return;
            }
        }
    }
}
