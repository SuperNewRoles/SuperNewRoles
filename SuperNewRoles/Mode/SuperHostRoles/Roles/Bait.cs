using Hazel;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Bait
    {
        public static void MurderPostfix(PlayerControl __instance,PlayerControl target)
        {
            if (target.isRole(CustomRPC.RoleId.Bait))
            {
                __instance.CmdReportDeadBody(target.Data);
            }
        }
    }
}
