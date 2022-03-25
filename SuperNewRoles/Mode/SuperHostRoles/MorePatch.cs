
using HarmonyLib;
using SuperNewRoles.CustomRPC;
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
        public static bool RepairSystem(ShipStatus __instance,
                SystemTypes systemType,
                PlayerControl player,
                byte amount)
        {
            SyncSetting.CustomSyncSettings();
            if (player.isRole(RoleId.Sheriff) || player.isRole(RoleId.truelover)) return false;
            if (!RoleClass.Minimalist.UseSabo && player.isRole(CustomRPC.RoleId.Minimalist)) return false;
            if (!RoleClass.Egoist.UseSabo && player.isRole(CustomRPC.RoleId.Egoist)) return false;
            return true;
        }
        public static void StartMeeting(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            RoleClass.IsMeeting = false;
            FixedUpdate.SetRoleNames();
            new LateTask(() => {
                RoleClass.IsMeeting = true;
                FixedUpdate.SetDefaultNames();
            }, 5f, "SetMeetingName");
        }
        public static void MeetingEnd()
        {
            FixedUpdate.SetDefaultNames();
        }
    }
}
