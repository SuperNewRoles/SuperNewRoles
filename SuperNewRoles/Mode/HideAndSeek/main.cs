using SuperNewRoles.EndGame;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class main
    {
        public static bool EndGameCheck(ShipStatus __instance,PlayerStatistics statistics)
        {
            if (statistics.CrewAlive == 0) {
                SuperNewRolesPlugin.Logger.LogInfo("ENDDED!!!");
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            } else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                SuperNewRolesPlugin.Logger.LogInfo("TASKEDD!");
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void ClearAndReload() {
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.IsImpostor)
                    {
                        player.RpcSetName(ModHelpers.cs(Roles.RoleClass.ImpostorRed, player.Data.GetPlayerName(PlayerOutfitType.Default)));
                    }
                    else
                    {
                        player.RpcSetName(ModHelpers.cs(new Color32(0, 255, 0, byte.MaxValue), player.Data.GetPlayerName(PlayerOutfitType.Default)));

                    }
                }
            }
        }
    }
}
