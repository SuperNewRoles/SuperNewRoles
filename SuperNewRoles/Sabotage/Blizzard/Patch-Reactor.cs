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
        //ここにリアクター関連を書こう
        //開発楽しいいいいいいいいいいいいいいいいいいいいいいい
    }
}
