using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Patch
{
    class AprilFoolPatch
    {
        [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
        public static class ConstPatch
        {
            public static void Postfix(ref bool __result)
            {
                __result = ConfigRoles.IsHorseMode.Value;
            }
        }
    }
}
