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
    class EvilSpeedBooster { 
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = RoleClass.EvilSpeedBooster.CoolTime;
        }
        public static void BoostStart()
        {
            SuperNewRolesPlugin.Logger.LogInfo(RoleClass.EvilSpeedBooster.Speed);
            PlayerControl.GameOptions.PlayerSpeedMod = RoleClass.EvilSpeedBooster.Speed;
            RoleClass.EvilSpeedBooster.IsSpeedBoost = true;
            EvilSpeedBooster.ResetCoolDown();
        }
        public static void ResetSpeed()
        {
            PlayerControl.GameOptions.PlayerSpeedMod = RoleClass.EvilSpeedBooster.DefaultSpeed;
            RoleClass.EvilSpeedBooster.IsSpeedBoost = false;
        }

        public static void SpeedBoostCheck()
        {
            if (!RoleClass.EvilSpeedBooster.IsSpeedBoost) return;
            if (HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer + RoleClass.EvilSpeedBooster.DurationTime <= RoleClass.EvilSpeedBooster.CoolTime) SpeedBoostEnd();
        }
        public static void SpeedBoostEnd()
        {
            ResetSpeed();
        }
        public static bool IsEvilSpeedBooster(PlayerControl Player)
        {
            if (RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.IsCheckListPlayerControl(Player))
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
