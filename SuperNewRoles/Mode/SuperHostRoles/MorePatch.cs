
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
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
        class PlayerControlCompleteTaskPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                FixedUpdate.SetRoleName(__instance);
            }
        }

        public static bool RepairSystem(ShipStatus __instance,
                SystemTypes systemType,
                PlayerControl player,
                byte amount)
        {
            if (systemType == SystemTypes.Comms)
            {
                FixedUpdate.SetRoleNames();
            }
            SyncSetting.CustomSyncSettings();
            if (systemType == SystemTypes.Sabotage && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
            {
                if ((player.isRole(RoleId.Jackal) && !RoleClass.Jackal.IsUseSabo) || player.isRole(RoleId.Demon) || player.isRole(RoleId.Arsonist) || player.isRole(RoleId.RemoteSheriff) || player.isRole(RoleId.Sheriff) || player.isRole(RoleId.truelover) || player.isRole(RoleId.FalseCharges) || player.isRole(RoleId.MadMaker)) return false;
                if (!RoleClass.Minimalist.UseSabo && player.isRole(CustomRPC.RoleId.Minimalist)) return false;
                if (!RoleClass.Samurai.UseSabo && player.isRole(CustomRPC.RoleId.Samurai)) return false;
                if (!RoleClass.Egoist.UseSabo && player.isRole(CustomRPC.RoleId.Egoist)) return false;
            }
            if (PlayerControl.LocalPlayer.IsUseVent() && RoleHelpers.IsComms())
            {
                if (BattleRoyal.main.VentData.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))
                {
                    var data = BattleRoyal.main.VentData[CachedPlayer.LocalPlayer.PlayerId];
                    if (data != null)
                    {
                        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent((int)data);
                    }
                }
            }
            return true;
        }
        public static void StartMeeting(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            FixedUpdate.SetRoleNames(true);
            RoleClass.IsMeeting = true;
            new LateTask(() => {
                FixedUpdate.SetDefaultNames();
            }, 5f, "SetMeetingName");
        }
        public static void MeetingEnd()
        {
        }
    }
}
