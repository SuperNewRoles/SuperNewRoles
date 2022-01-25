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
    class Speeder
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SpeedBoosterBoostButton.Timer = RoleClass.SpeedBooster.CoolTime;
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
            RoleClass.SpeedBooster.IsSpeedBoost = false;
        }

        public static void SpeedBoostCheck()
        {
            if (!RoleClass.SpeedBooster.IsSpeedBoost) return;
            if (HudManagerStartPatch.SpeedBoosterBoostButton.Timer + RoleClass.SpeedBooster.DurationTime <= RoleClass.SpeedBooster.CoolTime) SpeedBoostEnd();
        }
        public static void SpeedBoostEnd()
        {
            ResetSpeed();
        }
        public static bool IsSpeedBooster(PlayerControl Player)
        {
            return true;
            if (RoleClass.SpeedBooster.SpeedBoosterPlayer.Contains(Player))
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

            ResetCoolDown();
            ResetSpeed();

        }
    }
}
