using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Sabotage.Blizzard
{
    public static class SlowSpeed
    {
        //ここにスピード関連を書こう
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsSprinterPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.Blizzard)
                {
                    __instance.body.velocity /= main.BlizzardSlowSpeedmagnification;
                }
                SuperNewRolesPlugin.Logger.LogInfo(main.IsOverlay);
                SuperNewRolesPlugin.Logger.LogInfo(main.Timer);
            }
        }
    }
}