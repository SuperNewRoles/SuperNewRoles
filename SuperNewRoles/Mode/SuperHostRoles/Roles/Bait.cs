
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections;
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
                new LateTask(() => {
                    RoleClass.Bait.ReportedPlayer.Add(target.PlayerId);
                    __instance.CmdReportDeadBody(target.Data);
                }, CustomOptions.BaitReportTime.getFloat(), "ReportBaitBody");
            }
        }
    }
}
