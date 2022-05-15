using HarmonyLib;
using Hazel;
using System;
using SuperNewRoles.Patches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class TimeMaster
    {

        public static void ResetCoolDown()
        {
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.MaxTimer = RoleClass.TimeMaster.Cooldown;
            RoleClass.TimeMaster.ButtonTimer = DateTime.Now;
        }
        public static bool IsTimeMaster(PlayerControl Player)
        {
            if (RoleClass.TimeMaster.TimeMasterPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void TimeShieldStart()
        {
            RoleClass.TimeMaster.ShieldActive = true;
            CustomRPC.RPCProcedure.TimeMasterShield();
        }

        public static void TimeShieldEnd()
        {
            if (!RoleClass.TimeMaster.ShieldActive) return;
            RoleClass.TimeMaster.ShieldActive = false;
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.MaxTimer = RoleClass.TimeMaster.Cooldown;
            RoleClass.TimeMaster.ButtonTimer = DateTime.Now;
            RoleClass.TimeMaster.ShieldActive = false;
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.isEffectActive = false;
        }
    }
}
