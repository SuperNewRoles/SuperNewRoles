using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class Main
    {
        public static bool EndGameCheck(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.CrewAlive == 0)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[HAS]ENDDED!!!");
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[HAS]TASKEND!");
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void ClearAndReload() { }
    }
}