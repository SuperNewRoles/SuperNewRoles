using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.CopsRobbers
{
    public static class CheckEndGame
    {
        public static bool EndGameCheck(ShipStatus __instance)
        {
            bool impostorwin = true;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
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
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
                return false;
            }
            else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return false;
            }
            else
            {
                if (SuperHostRoles.EndGameCheck.CheckAndEndGameForWorkpersonWin(__instance)) return false;
                return false;
            }
        }
    }
}
