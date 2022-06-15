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
                if (!ModeHandler.isMode(ModeId.HideAndSeek)) return;
                if (AmongUsClient.Instance.AmHost)
                {
                    if (!target.Data.Role.IsImpostor)
                    {
                        if (ZombieOptions.HASDeathTask.getBool())
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
                if (playerInfo.Object.isAlive() && !playerInfo.Object.isImpostor())
                {
                    var (playerCompleted, playerTotal) = SuperNewRoles.Patch.TaskCount.TaskDate(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
            }
        }
    }
}