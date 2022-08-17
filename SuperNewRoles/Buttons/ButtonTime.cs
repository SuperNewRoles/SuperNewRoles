using System;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomRPC;
using UnityEngine;

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
            catch { }
            HawkDuration();
            ClairvoyantDuration();
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
                Buttons.HudManagerStartPatch.ScientistButton.MaxTimer = durationtime;
                Buttons.HudManagerStartPatch.ScientistButton.Timer = (float)((RoleClass.NiceScientist.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.ScientistButton.Timer <= 0f)
                {
                    Roles.Scientist.ScientistEnd();
                    Roles.Scientist.ResetCoolDown();
                    Buttons.HudManagerStartPatch.ScientistButton.MaxTimer = cooltime;
                    RoleClass.NiceScientist.IsScientist = false;
                    Buttons.HudManagerStartPatch.ScientistButton.actionButton.cooldownTimerText.color = Color.white;
                    RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)cooltime);
                Buttons.HudManagerStartPatch.ScientistButton.Timer = (float)((RoleClass.NiceScientist.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.ScientistButton.Timer <= 0f) Buttons.HudManagerStartPatch.ScientistButton.Timer = 0f; return;
            }
        }
        public static void HawkDuration()
        {
            if (RoleClass.Hawk.Timer == 0 && PlayerControl.LocalPlayer.isRole(RoleId.Hawk)) return;
            if (RoleClass.NiceHawk.Timer == 0 && PlayerControl.LocalPlayer.isRole(RoleId.NiceHawk)) return;
            if (RoleClass.MadHawk.Timer == 0 && PlayerControl.LocalPlayer.isRole(RoleId.MadHawk)) return;
            RoleClass.Hawk.IsHawkOn = true;
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Hawk.DurationTime);
            if (PlayerControl.LocalPlayer.isRole(RoleId.NiceHawk))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.NiceHawk.DurationTime);
                RoleClass.NiceHawk.Timer = (float)((RoleClass.NiceHawk.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (RoleClass.NiceHawk.Timer <= 0f) RoleClass.NiceHawk.Timer = 0f; NiceHawk.TimerEnd(); RoleClass.Hawk.IsHawkOn = false; return;
            }
            if (PlayerControl.LocalPlayer.isRole(RoleId.MadHawk))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.MadHawk.DurationTime);
                RoleClass.MadHawk.Timer = (float)((RoleClass.MadHawk.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (RoleClass.MadHawk.Timer <= 0f) RoleClass.MadHawk.Timer = 0f; MadHawk.TimerEnd(); RoleClass.Hawk.IsHawkOn = false; return;
            }
            RoleClass.Hawk.Timer = (float)((RoleClass.Hawk.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (RoleClass.Hawk.Timer <= 0f && PlayerControl.LocalPlayer.isRole(RoleId.Hawk)) RoleClass.Hawk.Timer = 0f; Hawk.TimerEnd(); RoleClass.Hawk.IsHawkOn = false; return;
        }
        public static void ClairvoyantDuration()
        {
            if (MapOptions.MapOption.Timer == 0 && PlayerControl.LocalPlayer.Data.IsDead && MapOptions.MapOption.ClairvoyantZoom) return;
            MapOptions.MapOption.IsZoomOn = true;
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)MapOptions.MapOption.DurationTime);
            TimeSpanDate = new TimeSpan(0, 0, 0, (int)MapOptions.MapOption.DurationTime);
            MapOptions.MapOption.Timer = (float)((MapOptions.MapOption.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (MapOptions.MapOption.Timer <= 0f) MapOptions.MapOption.Timer = 0f; Patch.Clairvoyant.TimerEnd(); MapOptions.MapOption.IsZoomOn = false; return;
        }
        public static void TeleporterButton()
        {
            if (RoleClass.Teleporter.ButtonTimer == null)
            {
                RoleClass.Teleporter.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Teleporter.CoolTime);
            Buttons.HudManagerStartPatch.TeleporterButton.Timer = (float)((RoleClass.Teleporter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.TeleporterButton.Timer <= 0f) Buttons.HudManagerStartPatch.TeleporterButton.Timer = 0f; return;
        }
        public static void DoorrButton()
        {
            if (Buttons.HudManagerStartPatch.DoorrDoorButton.Timer == 0) return;
            if (RoleClass.Doorr.ButtonTimer == null)
            {
                RoleClass.Doorr.ButtonTimer = DateTime.Now;
            }
            TimeSpan TimeSpanDate = new(0, 0, 0, (int)RoleClass.Doorr.CoolTime);
            if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.EvilDoorr.CoolTime);
            }
            Buttons.HudManagerStartPatch.DoorrDoorButton.Timer = (float)((RoleClass.Doorr.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.DoorrDoorButton.Timer <= 0f) Buttons.HudManagerStartPatch.DoorrDoorButton.Timer = 0f; return;
        }
        public static void MovingButton()
        {
            if (Buttons.HudManagerStartPatch.MovingTpButton.Timer == 0) return;
            if (RoleClass.Moving.ButtonTimer == null)
            {
                RoleClass.Moving.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Moving.CoolTime);
            if (PlayerControl.LocalPlayer.isRole(RoleId.EvilMoving))
            {
                TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.EvilMoving.CoolTime);
            }
            Buttons.HudManagerStartPatch.MovingTpButton.Timer = (float)((RoleClass.Moving.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.MovingTpButton.Timer <= 0f) Buttons.HudManagerStartPatch.MovingTpButton.Timer = 0f; return;
        }
        public static void LighterButton()
        {
            if (RoleClass.Lighter.IsLightOn)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Lighter.DurationTime);
                Buttons.HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.DurationTime;
                Buttons.HudManagerStartPatch.LighterLightOnButton.Timer = (float)((RoleClass.Lighter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.LighterLightOnButton.Timer <= 0f)
                {
                    Roles.Lighter.LightOutEnd();
                    Roles.Lighter.ResetCoolDown();
                    Buttons.HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.CoolTime;
                    RoleClass.Lighter.IsLightOn = false;
                    Buttons.HudManagerStartPatch.LighterLightOnButton.actionButton.cooldownTimerText.color = Color.white;
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
                Buttons.HudManagerStartPatch.LighterLightOnButton.Timer = (float)((RoleClass.Lighter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.LighterLightOnButton.Timer <= 0f) Buttons.HudManagerStartPatch.LighterLightOnButton.Timer = 0f; return;
            }
        }
        public static void ClergymanDuration()
        {
            if (RoleClass.Clergyman.OldButtonTime == 0) return;
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Clergyman.DurationTime);
            RoleClass.Clergyman.OldButtonTime = (float)((RoleClass.Clergyman.OldButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (RoleClass.Clergyman.OldButtonTime <= 0f) RoleClass.Clergyman.OldButtonTime = 0f; return;
        }
        public static void ClergymanButton()
        {
            if (RoleClass.Clergyman.ButtonTimer == null)
            {
                RoleClass.Clergyman.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Clergyman.CoolTime);
            Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer = (float)((RoleClass.Clergyman.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer <= 0f) Buttons.HudManagerStartPatch.ClergymanLightOutButton.Timer = 0f; return;
        }
        public static void SheriffKillButton()
        {
            if (Buttons.HudManagerStartPatch.SheriffKillButton.Timer == 0) return;
            if (RoleClass.Sheriff.ButtonTimer == null)
            {
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, PlayerControl.LocalPlayer.isRole(RoleId.Sheriff) ? (int)RoleClass.Sheriff.CoolTime : (int)RoleClass.RemoteSheriff.CoolTime);
            Buttons.HudManagerStartPatch.SheriffKillButton.Timer = (float)((RoleClass.Sheriff.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
            if (Buttons.HudManagerStartPatch.SheriffKillButton.Timer <= 0f) Buttons.HudManagerStartPatch.SheriffKillButton.Timer = 0f; return;
        }
        public static void SpeedBoosterButton()
        {
            if (RoleClass.SpeedBooster.IsSpeedBoost)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.SpeedBooster.DurationTime);
                Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.DurationTime;
                Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)((RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f)
                {
                    Roles.SpeedBooster.SpeedBoostEnd();
                    Roles.SpeedBooster.ResetCoolDown();
                    Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
                    RoleClass.SpeedBooster.IsSpeedBoost = false;
                    Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.actionButton.cooldownTimerText.color = Color.white;
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
                Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)((RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f) Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer = 0f; return;
            }
        }
        public static void EvilSpeedBoosterButton()
        {
            if (RoleClass.EvilSpeedBooster.IsSpeedBoost)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.EvilSpeedBooster.DurationTime);
                Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.DurationTime;
                Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)((RoleClass.EvilSpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f)
                {
                    Roles.EvilSpeedBooster.SpeedBoostEnd();
                    Roles.EvilSpeedBooster.ResetCoolDown();
                    Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.CoolTime;
                    RoleClass.EvilSpeedBooster.IsSpeedBoost = false;
                    Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.actionButton.cooldownTimerText.color = Color.white;
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
                Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)((RoleClass.EvilSpeedBooster.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f) Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = 0f; return;
            }
        }
        public static void DoppelgangerButton()
        {
            HudManagerStartPatch.DoppelgangerButton.actionButton.cooldownTimerText.color = Color.white;
            if (RoleClass.Doppelganger.ShapeButton == null)
            {
                RoleClass.Doppelganger.ShapeButton = DateTime.Now;
            }
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Doppelganger.CoolTime);
            HudManagerStartPatch.DoppelgangerButton.Timer = (float)(RoleClass.Doppelganger.ShapeButton + TimeSpanDate - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.DoppelgangerButton.Timer <= 0f) HudManagerStartPatch.DoppelgangerButton.Timer = 0f; return;
        }
    }
}
