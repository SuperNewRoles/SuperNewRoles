using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.MapOptions
{
    public class DeviceClass
    {
        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
        public static class MapConsoleUsePatch
        {
            public static bool Prefix(MapConsole __instance)
            {
                Roles.CrewMate.Painter.HandleRpc(Roles.CrewMate.Painter.ActionType.CheckAdmin);
                bool IsUse = MapOption.UseAdmin;
                return IsUse;
            }
        }
        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        class MapCountOverlayUpdatePatch
        {
            public static bool Prefix(MapConsole __instance)
            {
                bool IsUse = MapOption.UseAdmin;

                return IsUse || RoleClass.EvilHacker.IsMyAdmin;
            }
        }
        [HarmonyPatch(typeof(MapCountOverlay),nameof(MapCountOverlay.OnDisable))]
        class MapCountOverlayOnDisablePatch
        {
            public static void Postfix()
            {
                RoleClass.EvilHacker.IsMyAdmin = false;
            }
        }
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class CoVitalsOpen
        {
            static void Postfix(VitalsMinigame __instance)
            {
                Roles.CrewMate.Painter.HandleRpc(Roles.CrewMate.Painter.ActionType.CheckVital);
            }
        }
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsDevice
        {
            static void Postfix(VitalsMinigame __instance)
            {
                if (!MapOption.UseVitalOrDoorLog)
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

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
        class PlanetSurveillanceMinigameUpdatePatch
        {
            public static void Postfix(PlanetSurveillanceMinigame __instance)
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