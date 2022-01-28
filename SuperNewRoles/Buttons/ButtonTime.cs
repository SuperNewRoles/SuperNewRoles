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
        }
        public static void SpeedBoosterButton()
        {

            if (Roles.RoleClass.SpeedBooster.IsSpeedBoost)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Math.Ceiling(Roles.RoleClass.SpeedBooster.DurationTime));
                SuperNewRolesPlugin.Logger.LogInfo("-----");
                SuperNewRolesPlugin.Logger.LogInfo((DateTime.Now - Roles.RoleClass.SpeedBooster.ButtonTimer).Seconds);
                SuperNewRolesPlugin.Logger.LogInfo(DateTime.Now - Roles.RoleClass.SpeedBooster.ButtonTimer);
                SuperNewRolesPlugin.Logger.LogInfo("-----");
                Buttons.HudManagerStartPatch.SpeedBoosterBoostButton.Timer = 60-((DateTime.Now - Roles.RoleClass.SpeedBooster.ButtonTimer).Seconds);
                if (Roles.RoleClass.SpeedBooster.ButtonTimer + TimeSpanDate >= DateTime.Now)
                {
                    Roles.SpeedBooster.SpeedBoostEnd();
                }
            } else
            {
                new TimeSpan(0, 0, 0, (int)Math.Ceiling(Roles.RoleClass.SpeedBooster.CoolTime));
            }
        }
    }
}
