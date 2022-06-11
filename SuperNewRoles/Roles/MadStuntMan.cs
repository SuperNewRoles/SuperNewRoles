using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    class MadStuntMan
    {
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (!p.isRole(RoleId.MadStuntMan)) return false;
            SuperNewRolesPlugin.Logger.LogInfo("有効を返しました");
            return true;

            // SuperNewRolesPlugin.Logger.LogInfo("一番下まで通過");

            //return false;
        }



        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            {
                if (AmongUsClient.Instance.AmHost && __instance.PlayerId != target.PlayerId)
                {
                    if (target.isRole(CustomRPC.RoleId.MadStuntMan))
                    {
                        if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.MadStuntmanGuard, __instance))
                        {

                        }


                    }
                }
            }
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) { }
    }
}

