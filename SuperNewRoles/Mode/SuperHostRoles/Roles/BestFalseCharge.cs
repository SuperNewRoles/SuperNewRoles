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
                PlayerControl impostor = null;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor()) {
                        impostor = p;
                    }
                }
                foreach (PlayerControl p in RoleClass.Bestfalsecharge.BestfalsechargePlayer)
                {
                        impostor.RpcMurderPlayer(p);
                        p.Data.IsDead = true;
                }
                RoleClass.Bestfalsecharge.IsOnMeeting = true;
            }
        }
    }
}
