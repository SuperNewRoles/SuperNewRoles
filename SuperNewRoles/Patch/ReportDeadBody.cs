using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Patch
{
    class ReportDeadBody
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
        class ReportDeadBodyPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
            {
                if (ModeHandler.isMode(ModeId.HideAndSeek)) return false;
                if (ModeHandler.isMode(ModeId.BattleRoyal)) return false;
                if (ModeHandler.isMode(ModeId.SuperHostRoles)) return Mode.SuperHostRoles.ReportDeadBody.ReportDeadBodyPatch(__instance,target);
                if (ModeHandler.isMode(ModeId.Zombie)) return false;
                return true;
            }
        }
    }
}
