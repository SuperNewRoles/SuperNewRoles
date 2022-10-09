using System;

using SuperNewRoles.MapOptions;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Buttons
{
    class ButtonTime
    {
        public static void Update()
        {
            SpeedBoosterButton();
            EvilSpeedBoosterButton();
            LighterButton();
            MovingButton();
            TeleporterButton();
            HawkDuration();
            ScientistButton();
            CamouflagerButton();
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
            if (RoleClass.NiceScientist.IsScientist)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)durationtime);
                HudManagerStartPatch.ScientistButton.MaxTimer = durationtime;
                HudManagerStartPatch.ScientistButton.Timer = (float)(RoleClass.NiceScientist.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.ScientistButton.Timer <= 0f)
                {
                    Scientist.ScientistEnd();
                    Scientist.ResetCoolDown();
                    HudManagerStartPatch.ScientistButton.MaxTimer = cooltime;
                    RoleClass.NiceScientist.IsScientist = false;
                    HudManagerStartPatch.ScientistButton.actionButton.cooldownTimerText.color = Color.white;
                    RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)cooltime);
                HudManagerStartPatch.ScientistButton.Timer = (float)(RoleClass.NiceScientist.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.ScientistButton.Timer <= 0f) HudManagerStartPatch.ScientistButton.Timer = 0f; return;
            }
        }
        public static void HawkDuration()
        {
            if (RoleClass.Hawk.Timer == 0 && PlayerControl.LocalPlayer.IsRole(RoleId.Hawk)) return;
            if (RoleClass.NiceHawk.Timer == 0 && PlayerControl.LocalPlayer.IsRole(RoleId.NiceHawk)) return;
            if (RoleClass.MadHawk.Timer == 0 && PlayerControl.LocalPlayer.IsRole(RoleId.MadHawk)) return;
            RoleClass.Hawk.IsHawkOn = true;
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Hawk.DurationTime);
            if (PlayerControl.LocalPlayer.IsRole(RoleId.NiceHawk))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.NiceHawk.DurationTime);
                RoleClass.NiceHawk.Timer = (float)(RoleClass.NiceHawk.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
                if (RoleClass.NiceHawk.Timer <= 0f) RoleClass.NiceHawk.Timer = 0f; RoleClass.Hawk.IsHawkOn = false; return;
            }
            if (PlayerControl.LocalPlayer.IsRole(RoleId.MadHawk))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.MadHawk.DurationTime);
                RoleClass.MadHawk.Timer = (float)(RoleClass.MadHawk.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
                if (RoleClass.MadHawk.Timer <= 0f) RoleClass.MadHawk.Timer = 0f; RoleClass.Hawk.IsHawkOn = false; return;
            }
            RoleClass.Hawk.Timer = (float)(RoleClass.Hawk.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
            if (RoleClass.Hawk.Timer <= 0f && PlayerControl.LocalPlayer.IsRole(RoleId.Hawk)) RoleClass.Hawk.Timer = 0f; RoleClass.Hawk.IsHawkOn = false; return;
        }
        public static void ClairvoyantDuration()
        {
            if (MapOption.Timer == 0 && PlayerControl.LocalPlayer.Data.IsDead && MapOption.ClairvoyantZoom) return;
            MapOption.IsZoomOn = true;
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)MapOption.DurationTime);
            TimeSpanDate = new TimeSpan(0, 0, 0, (int)MapOption.DurationTime);
            MapOption.Timer = (float)(MapOption.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
            if (MapOption.Timer <= 0f) MapOption.Timer = 0f; MapOption.IsZoomOn = false; return;
        }
        public static void TeleporterButton()
        {
            if (RoleClass.Teleporter.ButtonTimer == null)
            {
                RoleClass.Teleporter.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Teleporter.CoolTime);
            HudManagerStartPatch.TeleporterButton.Timer = (float)(RoleClass.Teleporter.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.TeleporterButton.Timer <= 0f) HudManagerStartPatch.TeleporterButton.Timer = 0f; return;
        }
        public static void MovingButton()
        {
            if (HudManagerStartPatch.MovingTpButton.Timer == 0) return;
            if (RoleClass.Moving.ButtonTimer == null)
            {
                RoleClass.Moving.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Moving.CoolTime);
            if (PlayerControl.LocalPlayer.IsRole(RoleId.EvilMoving))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.EvilMoving.CoolTime);
            }
            HudManagerStartPatch.MovingTpButton.Timer = (float)(RoleClass.Moving.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.MovingTpButton.Timer <= 0f) HudManagerStartPatch.MovingTpButton.Timer = 0f; return;
        }
        public static void LighterButton()
        {
            if (RoleClass.Lighter.IsLightOn)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Lighter.DurationTime);
                HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.DurationTime;
                HudManagerStartPatch.LighterLightOnButton.Timer = (float)((RoleClass.Lighter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.LighterLightOnButton.Timer <= 0f)
                {
                    Lighter.LightOutEnd();
                    Lighter.ResetCoolDown();
                    HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.CoolTime;
                    RoleClass.Lighter.IsLightOn = false;
                    HudManagerStartPatch.LighterLightOnButton.actionButton.cooldownTimerText.color = Color.white;
                    RoleClass.Lighter.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                if (RoleClass.Lighter.ButtonTimer == null)
                {
                    RoleClass.Lighter.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Lighter.CoolTime);
                HudManagerStartPatch.LighterLightOnButton.Timer = (float)(RoleClass.Lighter.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.LighterLightOnButton.Timer <= 0f) HudManagerStartPatch.LighterLightOnButton.Timer = 0f; return;
            }
        }
        public static void SheriffKillButton()
        {
            if (HudManagerStartPatch.SheriffKillButton.Timer == 0) return;
            if (RoleClass.Sheriff.ButtonTimer == null)
            {
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff) ? (int)RoleClass.Sheriff.CoolTime : (int)RoleClass.RemoteSheriff.CoolTime);
            HudManagerStartPatch.SheriffKillButton.Timer = (float)(RoleClass.Sheriff.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.SheriffKillButton.Timer <= 0f) HudManagerStartPatch.SheriffKillButton.Timer = 0f; return;
        }
        public static void SpeedBoosterButton()
        {
            if (RoleClass.SpeedBooster.IsSpeedBoost)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.SpeedBooster.DurationTime);
                HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.DurationTime;
                HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)((RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f)
                {
                    SpeedBooster.SpeedBoostEnd();
                    SpeedBooster.ResetCoolDown();
                    HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
                    RoleClass.SpeedBooster.IsSpeedBoost = false;
                    HudManagerStartPatch.SpeedBoosterBoostButton.actionButton.cooldownTimerText.color = Color.white;
                    RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                if (RoleClass.SpeedBooster.ButtonTimer == null)
                {
                    RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.SpeedBooster.CoolTime);
                HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)(RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f) HudManagerStartPatch.SpeedBoosterBoostButton.Timer = 0f; return;
            }
        }
        public static void EvilSpeedBoosterButton()
        {
            if (RoleClass.EvilSpeedBooster.IsSpeedBoost)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.EvilSpeedBooster.DurationTime);
                HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.DurationTime;
                HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)((RoleClass.EvilSpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f)
                {
                    EvilSpeedBooster.SpeedBoostEnd();
                    EvilSpeedBooster.ResetCoolDown();
                    HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.CoolTime;
                    RoleClass.EvilSpeedBooster.IsSpeedBoost = false;
                    HudManagerStartPatch.EvilSpeedBoosterBoostButton.actionButton.cooldownTimerText.color = Color.white;
                    RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                if (RoleClass.EvilSpeedBooster.ButtonTimer == null)
                {
                    RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.EvilSpeedBooster.CoolTime);
                HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)(RoleClass.EvilSpeedBooster.ButtonTimer + TimeSpanDate - DateTime.Now).TotalSeconds;
                if (HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f) Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = 0f; return;
            }
        }
        public static void CamouflagerButton()
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                if (RoleClass.Camouflager.IsCamouflage)
                {
                    var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Camouflager.DurationTime);
                    HudManagerStartPatch.CamouflagerButton.actionButton.cooldownTimerText.color = Color.green;
                    HudManagerStartPatch.CamouflagerButton.MaxTimer = RoleClass.Camouflager.DurationTime;
                    HudManagerStartPatch.CamouflagerButton.Timer = (float)((RoleClass.Camouflager.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                    if (HudManagerStartPatch.CamouflagerButton.Timer <= 0f)
                    {
                        Roles.Impostor.Camouflager.ResetCamouflageSHR();
                        Roles.Impostor.Camouflager.ResetCoolTime();
                        HudManagerStartPatch.CamouflagerButton.MaxTimer = RoleClass.Camouflager.CoolTime;
                        RoleClass.Camouflager.IsCamouflage = false;
                        HudManagerStartPatch.CamouflagerButton.actionButton.cooldownTimerText.color = Color.white;
                        RoleClass.Camouflager.ButtonTimer = DateTime.Now;
                    }
                }
            }
        }
    }
}