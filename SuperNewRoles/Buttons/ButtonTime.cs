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
    }
}
