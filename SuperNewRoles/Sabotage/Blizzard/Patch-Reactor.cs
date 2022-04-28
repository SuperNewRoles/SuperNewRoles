using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using SuperNewRoles.Roles;
using Hazel;

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
        }
        /*public static void Update()
        {
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)5f);
            main.Timer = (float)((main.OverlayTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (main.Timer <= 0)
            {
                main.IsOverlay = true;
            }
            if (main.IsOverlay == true)
            {
                ModHelpers.ShowFlash(new Color(0, 255, 255));
                SuperNewRolesPlugin.Logger.LogInfo("@here");
                main.Timer = 5f;
                main.IsOverlay = false;
            }
        }*/
        //ここにリアクター関連を書こう
    }
}
