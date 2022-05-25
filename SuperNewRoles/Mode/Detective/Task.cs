﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.Detective
{
    public static class Task
    {
        public static void TaskCountDetective(GameData __instance)
        {
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
