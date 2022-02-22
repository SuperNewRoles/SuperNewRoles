using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
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
                if (RoleClass.StuntMan.StuntManPlayer.IsCheckListPlayerControl(target)) {
                        if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId)) { 
                            RoleClass.StuntMan.GuardCount[target.PlayerId] = (int)CustomOptions.StuntManMaxGuardCount.getFloat() - 1;
                        target.protectedByGuardian = true;
                    } else
                        {
                        if (!(RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0)) {
                            RoleClass.StuntMan.GuardCount[target.PlayerId]--;
                            target.protectedByGuardian = true;
                        }
                        }
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
