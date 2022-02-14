using SuperNewRoles.EndGame;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class main
    {
        public static void EndGameCheck(ShipStatus __instance, PlayerStatistics statistics)
        {
            SuperNewRolesPlugin.Logger.LogInfo("CHECK!");
            SuperNewRolesPlugin.Logger.LogInfo(statistics.TotalAlive);
            var alives = 0;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p.isAlive()) {
                    alives++;
                }
            }
            if (alives == 1)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            }
            else {
                __instance.enabled = true;
            }
        }
        public static void ClearAndReload()
        {

        }
    }
}
