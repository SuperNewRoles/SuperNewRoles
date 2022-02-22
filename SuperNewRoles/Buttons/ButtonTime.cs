using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Buttons
{
    class ButtonTime
    {
        public static void Update()
        {
            try
            {
                ClergymanDuration();
            }
            catch {}
            SpeedBoosterButton();
            EvilSpeedBoosterButton();
            SheriffKillButton();
            ClergymanButton();
            LighterButton();
            MovingButton();
            DoorrButton();
            TeleporterButton();
        }

        public static void TeleporterButton()
        {
            if (Roles.RoleClass.Teleporter.ButtonTimer == null)
            {
                Roles.RoleClass.Teleporter.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Teleporter.CoolTime);
            Buttons.HudManagerStartPatch.TeleporterButton.Timer = (float)((Roles.RoleClass.Teleporter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.TeleporterButton.Timer <= 0f) Buttons.HudManagerStartPatch.TeleporterButton.Timer = 0f; return;
        }
        public static void DoorrButton()
        {
            if (Buttons.HudManagerStartPatch.DoorrDoorButton.Timer == 0) return;
            if (Roles.RoleClass.Doorr.ButtonTimer == null)
            {
                Roles.RoleClass.Doorr.ButtonTimer = DateTime.Now;
            }
            TimeSpan TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Doorr.CoolTime);
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.EvilDoorr.CoolTime);
            }
            Buttons.HudManagerStartPatch.DoorrDoorButton.Timer = (float)((Roles.RoleClass.Doorr.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.DoorrDoorButton.Timer <= 0f) Buttons.HudManagerStartPatch.DoorrDoorButton.Timer = 0f; return;
        }
        public static void MovingButton()
        {
            if (Buttons.HudManagerStartPatch.MovingTpButton.Timer == 0) return;
            if (Roles.RoleClass.Moving.ButtonTimer == null)
            {
                Roles.RoleClass.Moving.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Moving.CoolTime);
            Buttons.HudManagerStartPatch.MovingTpButton.Timer = (float)((Roles.RoleClass.Moving.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.MovingTpButton.Timer <= 0f) Buttons.HudManagerStartPatch.MovingTpButton.Timer = 0f; return;
        }

        public static void LighterButton()
        {
            if (Roles.RoleClass.Lighter.IsLightOn)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Lighter.DurationTime);
                Buttons.HudManagerStartPatch.LighterLightOnButton.MaxTimer = Roles.RoleClass.Lighter.DurationTime;
                Buttons.HudManagerStartPatch.LighterLightOnButton.Timer = (float)((Roles.RoleClass.Lighter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.LighterLightOnButton.Timer <= 0f)
                {
                    Roles.Lighter.LightOutEnd();
                    Roles.Lighter.ResetCoolDown();
                    Buttons.HudManagerStartPatch.LighterLightOnButton.MaxTimer = Roles.RoleClass.Lighter.CoolTime;
                    Roles.RoleClass.Lighter.IsLightOn = false;
                    Buttons.HudManagerStartPatch.LighterLightOnButton.actionButton.cooldownTimerText.color = Color.white;
                    Roles.RoleClass.Lighter.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                if (Roles.RoleClass.Lighter.ButtonTimer == null)
                {
                    Roles.RoleClass.Lighter.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Lighter.CoolTime);
                Buttons.HudManagerStartPatch.LighterLightOnButton.Timer = (float)((Roles.RoleClass.Lighter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.LighterLightOnButton.Timer <= 0f) Buttons.HudManagerStartPatch.LighterLightOnButton.Timer = 0f; return;
            }
        }
        public static void ClergymanDuration()
        {
            if (RoleClass.Clergyman.OldButtonTime == 0) return;
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Clergyman.DurationTime);
            RoleClass.Clergyman.OldButtonTime = (float)((Roles.RoleClass.Clergyman.OldButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (RoleClass.Clergyman.OldButtonTime <= 0f) RoleClass.Clergyman.OldButtonTime = 0f; return;
        }
        public static void ClergymanButton()
        {
                if (Roles.RoleClass.Clergyman.ButtonTimer == null)
                {
                    Roles.RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Clergyman.CoolTime);
                Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer = (float)((Roles.RoleClass.Clergyman.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer <= 0f) Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer = 0f; return;
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
