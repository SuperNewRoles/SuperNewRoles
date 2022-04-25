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
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            SuperNewRolesPlugin.Logger.LogInfo(main.Timer);
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)1f);
            if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.Blizzard && main.Timer >= 0.1 && __instance == PlayerControl.LocalPlayer)
            {
                main.Timer = (float) ((DateTime.Now + TimeSpanDate) - DateTime.Now).TotalSeconds;
                ModHelpers.ShowFlash(new Color(204f / 255f, 102f / 255f, 0f / 255f));
            }
            if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.Blizzard && main.Timer >= 0.1)
            {
                main.Timer = 1f;
            }
            }
        //ここにリアクター関連を書こう
    }
}
