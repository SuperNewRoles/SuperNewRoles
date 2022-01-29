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
            HudManagerStartPatch.SpeedBoosterBoostButton.isEffectActive = true;
            HudManagerStartPatch.SpeedBoosterBoostButton.Timer = HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
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
