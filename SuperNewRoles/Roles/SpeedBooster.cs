using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using SuperNewRoles.Patches;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class SpeedBooster { 
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
            RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
        }
        public static void BoostStart()
        {
            PlayerControl.GameOptions.PlayerSpeedMod = RoleClass.SpeedBooster.Speed;
            RoleClass.SpeedBooster.IsSpeedBoost = true;
            SpeedBooster.ResetCoolDown();
        }
        public static void ResetSpeed()
        {
            PlayerControl.GameOptions.PlayerSpeedMod = RoleClass.SpeedBooster.DefaultSpeed;
        }
        public static void SpeedBoostEnd()
        {
            ResetSpeed();
        }
        public static bool IsSpeedBooster(PlayerControl Player)
        {
            if (RoleClass.SpeedBooster.SpeedBoosterPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EndMeeting()
        {

            HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
            RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
            ResetSpeed();

        }
    }
}
