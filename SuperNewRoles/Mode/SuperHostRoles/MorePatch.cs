using BepInEx.IL2CPP.Utils;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class MorePatch
    {
        public static bool RepairSystem(ShipStatus __instance,SystemTypes systemType,PlayerControl player,byte amount)
        {
            if (!RoleClass.Minimalist.UseSabo && player.isRole(CustomRPC.RoleId.Minimalist)) return false;
            if (!RoleClass.Egoist.UseSabo && player.isRole(CustomRPC.RoleId.Egoist)) return false;
            return true;
        }
        public static void StartMeeting(MeetingHud __instance)
        {
            FixedUpdate.SetRoleNames();
            AmongUsClient.Instance.StartCoroutine(SetNameCoro());
            IEnumerator SetNameCoro()
            {
                yield return new WaitForSeconds(5);
                FixedUpdate.SetDefaultNames();
            }
        }
        public static void MeetingEnd()
        {
            FixedUpdate.SetDefaultNames();
        }
    }
}
