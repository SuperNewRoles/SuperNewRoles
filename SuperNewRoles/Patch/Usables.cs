using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    class Usables
    {
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
        class OnPlayerLeftPatch
        {
            public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                {
                }
            }
        }
        /*
        [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
        public static class ConsoleCanUsePatch
        {
            public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
            {
                canUse = true;
                couldUse = false;
                __result = Vector3.Distance(CachedPlayer.LocalPlayer.transform.position,__instance.transform.position);//float.MaxValue;

                //if (IsBlocked(__instance, pc.Object)) return false;
                if (__instance.AllowImpostor) return true;
                //if (!pc.Object.hasFakeTasks()) return true;

                return false;
            }
        }*/
    }
}
