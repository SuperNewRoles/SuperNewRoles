using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace SuperNewRoles.Sabotage.Blizzard
{
    class Patch_ClearSabotage
    {
        public static bool[] SabotageClearFlag = new bool[2] { false, false };

        //フラグで解除
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static void Postfix()
        {
            //ブリザードって解除されるの？ん？
            if (SabotageClearFlag[1] && SabotageClearFlag[2])
            {
                SabotageManager.thisSabotage = SabotageManager.CustomSabotage.None;
                SuperNewRolesPlugin.Logger.LogInfo("あれ...？あったかくなってきた....");
            }
        }
    }
}
