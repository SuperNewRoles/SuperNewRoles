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
            HawkDuration();
            ScientistButton();
        }
        public static void ScientistButton()
        {

            float durationtime;
            float cooltime;
            if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
            {
                durationtime = RoleClass.EvilScientist.DurationTime;
                cooltime = RoleClass.EvilScientist.CoolTime;
            }
            else
            {
                durationtime = RoleClass.NiceScientist.DurationTime;
                cooltime = RoleClass.NiceScientist.CoolTime;
            }
            if (Roles.RoleClass.NiceScientist.IsScientist)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)durationtime);
                Buttons.HudManagerStartPatch.ScientistButton.MaxTimer = durationtime;
                Buttons.HudManagerStartPatch.ScientistButton.Timer = (float)((Roles.RoleClass.NiceScientist.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.ScientistButton.Timer <= 0f)
                {
                    Roles.Scientist.ScientistEnd();
                    Roles.Scientist.ResetCoolDown();
                    Buttons.HudManagerStartPatch.ScientistButton.MaxTimer = cooltime;
                    Roles.RoleClass.NiceScientist.IsScientist = false;
                    Buttons.HudManagerStartPatch.ScientistButton.actionButton.cooldownTimerText.color = Color.white;
                    Roles.RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)cooltime);
                Buttons.HudManagerStartPatch.ScientistButton.Timer = (float)((Roles.RoleClass.NiceScientist.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.ScientistButton.Timer <= 0f) Buttons.HudManagerStartPatch.ScientistButton.Timer = 0f; return;
            }
        }
        public static void HawkDuration()
        {
            if (RoleClass.Hawk.Timer == 0 && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk)) return;
            if (RoleClass.NiceHawk.Timer == 0 && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceHawk)) return;
            if (RoleClass.MadHawk.Timer == 0 && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadHawk)) return;
            RoleClass.Hawk.IsHawkOn = true;
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.Hawk.DurationTime);
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceHawk))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.NiceHawk.DurationTime);
                RoleClass.NiceHawk.Timer = (float)((Roles.RoleClass.NiceHawk.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (RoleClass.NiceHawk.Timer <= 0f) RoleClass.NiceHawk.Timer = 0f; NiceHawk.TimerEnd(); RoleClass.Hawk.IsHawkOn = false; return;
            }
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadHawk))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.MadHawk.DurationTime);
                RoleClass.MadHawk.Timer = (float)((Roles.RoleClass.MadHawk.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (RoleClass.MadHawk.Timer <= 0f) RoleClass.MadHawk.Timer = 0f; MadHawk.TimerEnd(); RoleClass.Hawk.IsHawkOn = false; return;
            }
            RoleClass.Hawk.Timer = (float)((Roles.RoleClass.Hawk.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (RoleClass.Hawk.Timer <= 0f && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk)) RoleClass.Hawk.Timer = 0f; Hawk.TimerEnd(); RoleClass.Hawk.IsHawkOn = false; return;
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
            if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
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
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Moving.CoolTime);
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.EvilMoving))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.EvilMoving.CoolTime);
            }
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
            var TimeSpanDate = new TimeSpan(0, 0, 0, PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Sheriff) ? (int)Roles.RoleClass.Sheriff.CoolTime : (int)Roles.RoleClass.RemoteSheriff.CoolTime);
            Buttons.HudManagerStartPatch.SheriffKillButton.Timer = (float)((Roles.RoleClass.Sheriff.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
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
