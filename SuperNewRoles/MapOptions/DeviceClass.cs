using HarmonyLib;
using SuperNewRoles.Roles;
using UnityEngine;

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
            public static bool Prefix(MapCountOverlay __instance)
            {
                bool IsUse = MapOption.UseAdmin || RoleClass.EvilHacker.IsMyAdmin;
                if (IsUse)
                {
                    bool commsActive = false;
                    foreach (PlayerTask task in CachedPlayer.LocalPlayer.PlayerControl.myTasks)
                        if (task.TaskType == TaskTypes.FixComms) commsActive = true;

                    if (!__instance.isSab && commsActive)
                    {
                        __instance.isSab = true;
                        __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                        __instance.SabotageText.gameObject.SetActive(true);
                        return false;
                    }

                    if (__instance.isSab && !commsActive)
                    {
                        __instance.isSab = false;
                        __instance.BackgroundColor.SetColor(Color.green);
                        __instance.SabotageText.gameObject.SetActive(false);
                    }

                    for (int i = 0; i < __instance.CountAreas.Length; i++)
                    {
                        CounterArea counterArea = __instance.CountAreas[i];

                        if (!commsActive && counterArea.RoomType > SystemTypes.Hallway)
                        {
                            PlainShipRoom plainShipRoom = MapUtilities.CachedShipStatus.FastRooms[counterArea.RoomType];

                            if (plainShipRoom != null && plainShipRoom.roomArea)
                            {
                                int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                                int num2 = num;

                                // ロミジュリと絵画の部屋をアドミンの対象から外す
                                for (int j = 0; j < num; j++)
                                {
                                    Collider2D collider2D = __instance.buffer[j];
                                    if (!(collider2D.tag == "DeadBody"))
                                    {
                                        PlayerControl component = collider2D.GetComponent<PlayerControl>();
                                        if (!component || component.IsDead())
                                        {
                                            num2--;
                                        }
                                        else if (!CustomOptions.CrackerIsAdminView.GetBool() && RoleClass.Cracker.CrackedPlayers.Contains(component.PlayerId) && (component.PlayerId != CachedPlayer.LocalPlayer.PlayerId || !CustomOptions.CrackerIsSelfNone.GetBool()))
                                        {
                                            num2--;
                                        }
                                    }
                                }
                                if (num2 < 0) num2 = 0;
                                counterArea.UpdateCount(num2);
                            }
                            else
                            {
                                Debug.LogWarning($"Couldn't find counter for:{counterArea.RoomType}");
                            }
                        }
                        else
                        {
                            counterArea.UpdateCount(0);
                        }
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnDisable))]
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