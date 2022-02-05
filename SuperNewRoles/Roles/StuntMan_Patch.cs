using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class StuntMan_Patch
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
        class CheckMurderPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (RoleClass.StuntMan.StuntManPlayer.IsCheckListPlayerControl(target)) {
                    SuperNewRolesPlugin.Logger.LogInfo("checkkkkkkkkkk");
                    __instance.RpcProtectPlayer(target, 0);
                    __instance.RpcMurderPlayer(target);
                    return false;
                }
                return false;
            }
        }
        
    }

}
