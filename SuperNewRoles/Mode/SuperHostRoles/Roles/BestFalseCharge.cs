using HarmonyLib;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class BestFalseCharge
    {
        public static void WrapUp() { 
            if (AmongUsClient.Instance.AmHost && !RoleClass.Bestfalsecharge.IsOnMeeting)
            {
                foreach (PlayerControl p in RoleClass.Bestfalsecharge.BestfalsechargePlayer)
                {
                    p.RpcMurderPlayer(p);
                }
                RoleClass.Bestfalsecharge.IsOnMeeting = true;
            }
        }
    }
}
