using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.MapOptions
{
    class DeviceClass
    {
        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
        public static class MapConsoleUsePatch
        {
            public static bool Prefix(MapConsole __instance)
            {
                return MapOption.UseAdmin;
            }
        }
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsDevice
        {
            static void Postfix(VitalsMinigame __instance)
            {
                if (MapOption.UseVitalOrDoorLog == false)
                {
                    __instance.Close();
                }
            }
        }
        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
        class SurveillanceMinigameUpdatePatch
        {
            public static void Postfix(SurveillanceMinigame __instance)
            {
                if (MapOption.UseCamera == false)
                {
                    __instance.Close();
                }
            }
        }
        [HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
        class SecurityLogGameUpdatePatch
        {
            public static void Postfix(SecurityLogGame __instance)
            {
                    if (MapOption.UseVitalOrDoorLog == false)
                    {
                        __instance.Close();
                    }
                }
            }
        }
    }
