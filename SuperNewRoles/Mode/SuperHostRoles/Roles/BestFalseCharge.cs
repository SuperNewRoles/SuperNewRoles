using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class BestFalseCharge
    {
        public static void WrapUp() { 
            if (AmongUsClient.Instance.AmHost && !RoleClass.Bestfalsecharge.IsOnMeeting && RoleClass.IsMeeting)
            {
                foreach (PlayerControl p in RoleClass.Bestfalsecharge.BestfalsechargePlayer)
                {
                        p.CheckMurder(p);
                }
                RoleClass.Bestfalsecharge.IsOnMeeting = true;
            }
        }
    }
}
