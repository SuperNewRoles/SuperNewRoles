namespace SuperNewRoles.Mode.CopsRobbers;

public static class CheckEndGame
{
    public static bool EndGameCheck(ShipStatus __instance)
    {
        bool impostorwin = true;
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
        {
            if (!p.Data.Disconnected)
            {
                if (!p.IsImpostor() && !p.IsArrest())
                {
                    impostorwin = false;
                }
            }
        }
        if (impostorwin)
        {
            __instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return false;
        }
        else if (CustomOptionHolder.FoxCanHouwaWin.GetBool())
        {
            if (SuperHostRoles.EndGameCheck.CheckAndEndGameForFoxHouwaWin(__instance)) return false;
            return false;
        }
        else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            __instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
            return false;
        }
        else
        {
            if (SuperHostRoles.EndGameCheck.CheckAndEndGameForWorkpersonWin(__instance)) return false;
            return false;
        }
    }
}