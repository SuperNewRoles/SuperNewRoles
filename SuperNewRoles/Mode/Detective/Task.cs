using System;

namespace SuperNewRoles.Mode.Detective;

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
                NetworkedPlayerInfo playerInfo = __instance.AllPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.Object.IsCrew() && (playerInfo.Object.PlayerId != Main.DetectivePlayer.PlayerId || !Main.IsDetectiveNotTask))
                {
                    var (playerCompleted, playerTotal) = Patches.TaskCount.TaskDate(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
            }
            catch { }
        }
    }
}