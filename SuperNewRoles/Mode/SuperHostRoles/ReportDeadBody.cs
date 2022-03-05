using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class ReportDeadBody
    {
        public static bool ReportDeadBodyPatch(PlayerControl __instance,GameData.PlayerInfo target)
        {
            if (__instance.isRole(CustomRPC.RoleId.Minimalist)) return RoleClass.Minimalist.UseReport;
            return true;
        }
    }
}
