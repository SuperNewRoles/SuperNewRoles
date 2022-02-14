using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class StuntMan_Patch
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        class StuntManMurderPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (RoleClass.StuntMan.StuntManPlayer.IsCheckListPlayerControl(target) && PlayerControl.LocalPlayer == target &&!(RoleClass.StuntMan.GuardCount <= 0)) {
                    RoleClass.StuntMan.GuardCount--;
                    target.protectedByGuardian = true;
                }
            }
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (RoleClass.StuntMan.StuntManPlayer.IsCheckListPlayerControl(target))
                {
                    target.RemoveProtection();
                }
            }
        }
        
    }

}
