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
                //SuperNewRolesPlugin.Logger.LogInfo(main.Timer);
                ModHelpers.ShowFlash(new Color(0, 255, 255));
                //SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Blizzard.Sounds.Alarm_sabotage.wav"), false, 1f);
                main.OverlayTimer = DateTime.Now;
                main.Timer = 2f;
            }
            if (main.ReactorTimer <= 0)//終わり終わりぃィっ！
            {
                ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
            }
        }
        private static float count;
        //ここにリアクター関連を書こう     
    }
}
