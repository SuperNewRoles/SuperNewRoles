using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Patch
{
    class Usables
    {/*
        [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
        public static class ConsoleCanUsePatch
        {
            public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
            {
                canUse = couldUse = false;
                __result = 0;//float.MaxValue;

                //if (IsBlocked(__instance, pc.Object)) return false;
                //if (__instance.AllowImpostor) return true;
                //if (!pc.Object.hasFakeTasks()) return true;

                return false;
            }
        }*/
    }
}
