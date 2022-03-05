using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class MorePatch
    {
        public static bool RepairSystem(ShipStatus __instance,SystemTypes systemType,PlayerControl player,byte amount)
        {
            if (!RoleClass.Minimalist.UseSabo && player.isRole(CustomRPC.RoleId.Minimalist)) return false;
            return true;
        }
    }
}
