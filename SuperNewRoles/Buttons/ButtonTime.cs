using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace SuperNewRoles.Buttons
{
    class ButtonTime
    {
        public static void Update()
        {
            SpeedBoosterButton();
            EvilSpeedBoosterButton();
            SheriffKillButton();
            ClergymanButton();
        }
        public static void ClergymanDuration()
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor) return;
            if (!(Roles.RoleClass.Clergyman.OldButtonTimer == new DateTime(2000, 1, 1, 1, 1, 1)))
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Clergyman.DurationTime);
                if ((float)((Roles.RoleClass.Clergyman.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds <= 0f)
                {
                    Roles.Clergyman.LightOutEndRPC();
                    Roles.RoleClass.Clergyman.OldButtonTimer = new DateTime(2000, 1, 1, 1, 1, 1);
                }
            }
                
        }
        public static void ClergymanButton()
        {
            if (Roles.RoleClass.Clergyman.IsLightOff)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Clergyman.DurationTime);
                Buttons.HudManagerStartPatch.ClergymanLightOutButton.MaxTimer = Roles.RoleClass.Clergyman.DurationTime;
                Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer = (float)((Roles.RoleClass.Clergyman.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer <= 0f)
                {
                    Roles.Clergyman.LightOutEnd();
                    Roles.Clergyman.ResetCoolDown();
                    Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = Roles.RoleClass.Clergyman.CoolTime;
                    Roles.RoleClass.Clergyman.IsLightOff = false;
                    Buttons.HudManagerStartPatch.ClergymanLightOutButton.actionButton.cooldownTimerText.color = Color.white;
                    Roles.RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                if (Roles.RoleClass.Clergyman.ButtonTimer == null)
                {
                    Roles.RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Clergyman.CoolTime);
                Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer = (float)((Roles.RoleClass.Clergyman.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer <= 0f) Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer = 0f; return;
            }
        }
        public static void SheriffKillButton()
        {
            if (Buttons.HudManagerStartPatch.SheriffKillButton.Timer == 0) return;
            if (Roles.RoleClass.Sheriff.ButtonTimer == null)
            {
                Roles.RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Sheriff.CoolTime);
            Buttons.HudManagerStartPatch.SheriffKillButton.Timer = (float)((Roles.RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.SheriffKillButton.Timer <= 0f) Buttons.HudManagerStartPatch.SheriffKillButton.Timer = 0f; return;
        }
        public static void SpeedBoosterButton()
        {

            if (Roles.RoleClass.SpeedBooster.IsSpeedBoost)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.SpeedBooster.DurationTime);
                Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = Roles.RoleClass.SpeedBooster.DurationTime;
                Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)((Roles.RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f)
                {
                    Roles.SpeedBooster.SpeedBoostEnd();
                    Roles.SpeedBooster.ResetCoolDown();
                    Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = Roles.RoleClass.SpeedBooster.CoolTime;
                    Roles.RoleClass.SpeedBooster.IsSpeedBoost = false;
                    Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.actionButton.cooldownTimerText.color = Color.white;
                    Roles.RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                }
            } else
            {
                if (Roles.RoleClass.SpeedBooster.ButtonTimer == null)
                {
                    Roles.RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.SpeedBooster.CoolTime);
                Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)((Roles.RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f) Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer = 0f; return;
            }
        }
        public static void EvilSpeedBoosterButton()
        {

            if (Roles.RoleClass.EvilSpeedBooster.IsSpeedBoost)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.EvilSpeedBooster.DurationTime);
                Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = Roles.RoleClass.EvilSpeedBooster.DurationTime;
                Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)((Roles.RoleClass.EvilSpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f)
                {
                    Roles.EvilSpeedBooster.SpeedBoostEnd();
                    Roles.EvilSpeedBooster.ResetCoolDown();
                    Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = Roles.RoleClass.EvilSpeedBooster.CoolTime;
                    Roles.RoleClass.EvilSpeedBooster.IsSpeedBoost = false;
                    Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.actionButton.cooldownTimerText.color = Color.white;
                    Roles.RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                if (Roles.RoleClass.EvilSpeedBooster.ButtonTimer == null)
                {
                    Roles.RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.EvilSpeedBooster.CoolTime);
                Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)((Roles.RoleClass.EvilSpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f) Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = 0f; return;
            }
        }
    }
}
