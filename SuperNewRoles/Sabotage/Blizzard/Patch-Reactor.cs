using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using SuperNewRoles.Roles;
using Hazel;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patch;
using System.Text;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Sabotage.Blizzard
{
    public static class Reactor
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static void Postfix()
        {
            if (main.Timer <= 0 && SabotageManager.thisSabotage == SabotageManager.CustomSabotage.Blizzard)
            {
                SuperNewRolesPlugin.Logger.LogInfo(main.Timer);
                ModHelpers.ShowFlash(new Color(0, 255, 255));
                main.OverlayTimer = DateTime.Now;
                main.Timer = 2f;
            }
            if (main.ReactorTimer <= 0)
            {
                ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
            }
        }
        //ここにリアクター関連を書こう
    }
}
