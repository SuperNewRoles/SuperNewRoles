using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Sabotage.Blizzard
{
    public static class SlowSpeed
    {
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsSlowSpeedPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.Blizzard)
                {
                    __instance.body.velocity *= main.BlizzardSlowSpeedmagnification;
                }
            }
        }
    }
}        //スピード関連
