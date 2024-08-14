using System;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Buttons;

class ButtonTime
{
    public static void Update()
    {
        RoleId currentRole = PlayerControl.LocalPlayer.GetRole();
        switch (currentRole)
        {
            case RoleId.SpeedBooster:
                SpeedBoosterButton();
                break;
            case RoleId.EvilSpeedBooster:
                EvilSpeedBoosterButton();
                break;
            case RoleId.Lighter:
                LighterButton();
                break;
            case RoleId.Moving:
            case RoleId.EvilMoving:
                MovingButton();
                break;
            case RoleId.Teleporter:
            case RoleId.NiceTeleporter:
                TeleporterButton();
                break;
            case RoleId.Hawk:
            case RoleId.MadHawk:
                HawkDuration();
                break;
            case RoleId.Camouflager:
                CamouflagerButton();
                break;
        }
        Patches.Clairvoyant.ClairvoyantDuration();
    }
    public static void HawkDuration()
    {
        if (RoleClass.Hawk.Timer == 0 && PlayerControl.LocalPlayer.IsRole(RoleId.Hawk)) return;
        if (RoleClass.NiceHawk.Timer == 0 && PlayerControl.LocalPlayer.IsRole(RoleId.NiceHawk)) return;
        if (RoleClass.MadHawk.Timer == 0 && PlayerControl.LocalPlayer.IsRole(RoleId.MadHawk)) return;
        RoleClass.Hawk.IsHawkOn = true;
        var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.Hawk.DurationTime);
        if (PlayerControl.LocalPlayer.IsRole(RoleId.NiceHawk))
        {
            timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.NiceHawk.DurationTime);
            RoleClass.NiceHawk.Timer = (float)(RoleClass.NiceHawk.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
            if (RoleClass.NiceHawk.Timer <= 0f) RoleClass.NiceHawk.Timer = 0f; RoleClass.Hawk.IsHawkOn = false; return;
        }
        if (PlayerControl.LocalPlayer.IsRole(RoleId.MadHawk))
        {
            timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.MadHawk.DurationTime);
            RoleClass.MadHawk.Timer = (float)(RoleClass.MadHawk.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
            if (RoleClass.MadHawk.Timer <= 0f) RoleClass.MadHawk.Timer = 0f; RoleClass.Hawk.IsHawkOn = false; return;
        }
        RoleClass.Hawk.Timer = (float)(RoleClass.Hawk.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
        if (RoleClass.Hawk.Timer <= 0f && PlayerControl.LocalPlayer.IsRole(RoleId.Hawk)) RoleClass.Hawk.Timer = 0f; RoleClass.Hawk.IsHawkOn = false; return;
    }
    public static void TeleporterButton()
    {
        if (RoleClass.Teleporter.ButtonTimer == null)
        {
            RoleClass.Teleporter.ButtonTimer = DateTime.Now;
        }
        var timeSpanData = new TimeSpan(0, 0, 0, (int)Teleporter.CoolTime);
        HudManagerStartPatch.TeleporterButton.Timer = (float)(RoleClass.Teleporter.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
        if (HudManagerStartPatch.TeleporterButton.Timer <= 0f) HudManagerStartPatch.TeleporterButton.Timer = 0f; return;
    }
    public static void MovingButton()
    {
        if (HudManagerStartPatch.MovingTpButton.Timer == 0) return;
        if (RoleClass.Moving.ButtonTimer == null)
        {
            RoleClass.Moving.ButtonTimer = DateTime.Now;
        }
        var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.Moving.CoolTime);
        if (PlayerControl.LocalPlayer.IsRole(RoleId.EvilMoving))
        {
            timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.EvilMoving.CoolTime);
        }
        HudManagerStartPatch.MovingTpButton.Timer = (float)(RoleClass.Moving.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
        if (HudManagerStartPatch.MovingTpButton.Timer <= 0f) HudManagerStartPatch.MovingTpButton.Timer = 0f; return;
    }
    public static void LighterButton()
    {
        if (RoleClass.Lighter.IsLightOn)
        {
            var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.Lighter.DurationTime);
            HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.DurationTime;
            HudManagerStartPatch.LighterLightOnButton.Timer = (float)((RoleClass.Lighter.ButtonTimer + timeSpanData) - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.LighterLightOnButton.Timer <= 0f)
            {
                Lighter.LightOutEnd();
                Lighter.ResetCooldown();
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
            var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.Lighter.CoolTime);
            HudManagerStartPatch.LighterLightOnButton.Timer = (float)(RoleClass.Lighter.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
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
        var timeSpanData = new TimeSpan(0, 0, 0, PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff) ? (int)RoleClass.Sheriff.CoolTime : (int)RoleClass.RemoteSheriff.CoolTime);
        HudManagerStartPatch.SheriffKillButton.Timer = (float)(RoleClass.Sheriff.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
        if (HudManagerStartPatch.SheriffKillButton.Timer <= 0f) HudManagerStartPatch.SheriffKillButton.Timer = 0f; return;
    }
    public static void SpeedBoosterButton()
    {
        if (RoleClass.SpeedBooster.IsSpeedBoost)
        {
            var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.SpeedBooster.DurationTime);
            HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.DurationTime;
            HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)((RoleClass.SpeedBooster.ButtonTimer + timeSpanData) - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f)
            {
                SpeedBooster.SpeedBoostEnd();
                SpeedBooster.ResetCooldown();
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
            var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.SpeedBooster.CoolTime);
            HudManagerStartPatch.SpeedBoosterBoostButton.Timer = (float)(RoleClass.SpeedBooster.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.SpeedBoosterBoostButton.Timer <= 0f) HudManagerStartPatch.SpeedBoosterBoostButton.Timer = 0f; return;
        }
    }
    public static void EvilSpeedBoosterButton()
    {
        if (RoleClass.EvilSpeedBooster.IsSpeedBoost)
        {
            var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.EvilSpeedBooster.DurationTime);
            HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.DurationTime;
            HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)((RoleClass.EvilSpeedBooster.ButtonTimer + timeSpanData) - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f)
            {
                EvilSpeedBooster.SpeedBoostEnd();
                EvilSpeedBooster.ResetCooldown();
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
            var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.EvilSpeedBooster.CoolTime);
            HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = (float)(RoleClass.EvilSpeedBooster.ButtonTimer + timeSpanData - DateTime.Now).TotalSeconds;
            if (HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer <= 0f) Buttons.HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer = 0f; return;
        }
    }
    public static void CamouflagerButton()
    {
        if (RoleClass.Camouflager.IsCamouflage)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.Camouflager.DurationTime);
                HudManagerStartPatch.CamouflagerButton.actionButton.cooldownTimerText.color = Color.green;
                HudManagerStartPatch.CamouflagerButton.MaxTimer = RoleClass.Camouflager.DurationTime;
                HudManagerStartPatch.CamouflagerButton.Timer = (float)((RoleClass.Camouflager.ButtonTimer + timeSpanData) - DateTime.Now).TotalSeconds;
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
            else
            {
                var timeSpanData = new TimeSpan(0, 0, 0, (int)RoleClass.Camouflager.DurationTime);
                RoleClass.Camouflager.CamoDurationData = (float)((RoleClass.Camouflager.ButtonTimer + timeSpanData) - DateTime.Now).TotalSeconds;
                if (RoleClass.Camouflager.CamoDurationData <= 0f)
                {
                    Roles.Impostor.Camouflager.ResetCamouflage();
                }
            }
        }
    }
}