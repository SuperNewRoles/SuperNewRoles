using System;
using System.IO;
using HarmonyLib;
using SuperNewRoles.Patch;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class Task
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class DeadPlayerTaskPatch
        {
            public static void Postfix([HarmonyArgument(0)] PlayerControl target)
            {
                if (!ModeHandler.IsMode(ModeId.HideAndSeek)) return;
                if (AmongUsClient.Instance.AmHost)
                {
                    if (!target.Data.Role.IsImpostor)
                    {
                        if (HideAndSeekOptions.HASDeathTask.GetBool())
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
        public static void TaskCountHideAndSeek(GameData __instance)
        {
            __instance.TotalTasks = 0;
            __instance.CompletedTasks = 0;
            for (int i = 0; i < __instance.AllPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                if (playerInfo.Object.IsAlive() && !playerInfo.Object.IsImpostor())
                {
                    var (playerCompleted, playerTotal) = TaskCount.TaskDate(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
            }
        }
    }
}