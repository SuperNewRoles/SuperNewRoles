using SuperNewRoles.EndGame;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class main
    {
        public static void EndGameCheck(ShipStatus __instance,PlayerStatistics statistics)
        {
            if (statistics.TeamCrewAlive == 0) {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            } else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
            }
            else
            {
                __instance.enabled = true;
            }
        }
        public static void ClearAndReload() {
            if (AmongUsClient.Instance.AmHost)
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                    if (player.Data.Role.IsImpostor)
                    {
                        player.RpcSetName(ModHelpers.cs(Roles.RoleClass.ImpostorRed, player.Data.GetPlayerName(PlayerOutfitType.Default)));
                    } else
                    {
                        player.RpcSetName(ModHelpers.cs(new Color32(0,255,0,byte.MaxValue), player.Data.GetPlayerName(PlayerOutfitType.Default)));

                    }
            }
        }
    }
}
