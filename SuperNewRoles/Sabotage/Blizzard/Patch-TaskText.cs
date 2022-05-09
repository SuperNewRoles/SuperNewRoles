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
using System.Threading;
using System.Linq;

namespace SuperNewRoles.Sabotage.Blizzard
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class TaskText
    {
        public static void Postfix(HudManager __instance)
        {
            if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.Blizzard)
            {
                if (__instance.TaskText.text.Contains("凍結死するまで"))
                {
                          return;
                }
                　　__instance.TaskText.text += "\n" +
                                           "<color=red>凍結死するまで" + Math.Truncate(main.ReactorTimer) + "秒</color>";
            }
        }
    }
}
