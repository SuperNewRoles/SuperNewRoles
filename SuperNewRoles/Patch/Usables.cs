using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using InnerNet;
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

    }
}
