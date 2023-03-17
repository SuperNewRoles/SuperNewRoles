using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.MapOption;

public static class DeviceClass
{
    public static bool IsAdminRestrict;
    public static bool IsVitalRestrict;
    public static bool IsCameraRestrict;
    public static float AdminTimer;
    public static float VitalTimer;
    public static float CameraTimer;
    public static DateTime AdminStartTime;
    public static DateTime VitalStartTime;
    public static DateTime CameraStartTime;
    public static Dictionary<DeviceType, PlayerControl> DeviceUsePlayer;
    public static Dictionary<DeviceType, DateTime> DeviceUserUseTime;
    public static TextMeshPro TimeRemaining;
    public enum DeviceType
    {
        Admin,
        Camera,
        Vital
    }

    public static void ClearAndReload()
    {
        /*
        IsAdminLimit = MapOption.Admin&& MapOption.IsAdminLimit.GetBool();
        AdminTimer = MapOption.AdminTimerOption.GetFloat();
        */
        if (MapOption.IsUsingRestrictDevicesTime)
        {
            IsAdminRestrict = MapOption.RestrictAdmin.GetBool();
            AdminTimer = IsAdminRestrict ? MapOption.DeviceUseAdminTime.GetFloat() : 0;
            IsCameraRestrict = MapOption.RestrictCamera.GetBool();
            CameraTimer = IsCameraRestrict ? MapOption.DeviceUseCameraTime.GetFloat() : 0;
            IsVitalRestrict = MapOption.RestrictVital.GetBool();
            VitalTimer = IsVitalRestrict ? MapOption.DeviceUseVitalOrDoorLogTime.GetFloat() : 0;
        }
        else
        {
            IsAdminRestrict = false;
            AdminTimer = 0;
            IsCameraRestrict = false;
            CameraTimer = 0;
            IsVitalRestrict = false;
            VitalTimer = 0;
        }
        DeviceUsePlayer = new() { { DeviceType.Admin, null }, { DeviceType.Camera, null }, { DeviceType.Vital, null } };
        DeviceUserUseTime = new() { { DeviceType.Admin, new() }, { DeviceType.Camera, new() }, { DeviceType.Vital, new() } };
    }
    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
    public static class MapConsoleUsePatch
    {
        public static bool Prefix(MapConsole __instance)
        {
            if (ConfigRoles.DebugMode.Value)
            {
                Logger.Info($"Admin Coordinate(x):{__instance.transform.position.x}", "Debug Mode");
                Logger.Info($"Admin Coordinate(y):{__instance.transform.position.y}", "Debug Mode");
                Logger.Info($"Admin Coordinate(Z):{__instance.transform.position.z}", "Debug Mode");
            }
            Roles.Crewmate.Painter.HandleRpc(Roles.Crewmate.Painter.ActionType.CheckAdmin);
            bool IsUse = MapOption.CanUseAdmin && !PlayerControl.LocalPlayer.IsRole(RoleId.Vampire, RoleId.Dependents);
            return IsUse;
        }
    }
    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
    class MapCountOverlayAwakePatch
    {
        public static void Postfix()
        {
            if (IsAdminRestrict && CachedPlayer.LocalPlayer.IsAlive() && !RoleClass.EvilHacker.IsMyAdmin) AdminStartTime = DateTime.UtcNow;
        }
    }
    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    class MapCountOverlayUpdatePatch
    {
        public static bool Prefix(MapCountOverlay __instance)
        {
            if (IsAdminRestrict && !RoleClass.EvilHacker.IsMyAdmin && AdminTimer <= 0)
            {
                MapBehaviour.Instance.Close();
                return false;
            }
            bool IsUse = (MapOption.CanUseAdmin && !PlayerControl.LocalPlayer.IsRole(RoleId.Vampire, RoleId.Dependents)) || RoleClass.EvilHacker.IsMyAdmin;
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

                    // ロミジュリと絵画の部屋をアドミンの対象から外す
                    if (!commsActive && counterArea.RoomType > SystemTypes.Hallway)
                    {
                        PlainShipRoom plainShipRoom = MapUtilities.CachedShipStatus.FastRooms[counterArea.RoomType];

                        if (plainShipRoom != null && plainShipRoom.roomArea)
                        {
                            HashSet<int> hashSet = new();
                            int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                            int count = 0;

                            for (int j = 0; j < num; j++)
                            {
                                Collider2D collider2D = __instance.buffer[j];
                                if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies) count++;
                                else
                                {
                                    PlayerControl component = collider2D.GetComponent<PlayerControl>();
                                    if (!component) continue;
                                    if (component.Data == null || component.Data.Disconnected || component.Data.IsDead) continue;
                                    if (!__instance.showLivePlayerPosition && component.AmOwner) continue;
                                    if (!hashSet.Add(component.PlayerId)) continue;

                                    if (component.IsRole(RoleId.Vampire, RoleId.Dependents)) continue;
                                    if (!CustomOptionHolder.CrackerIsAdminView.GetBool() && RoleClass.Cracker.CrackedPlayers.Contains(component.PlayerId) &&
                                       (component.PlayerId != CachedPlayer.LocalPlayer.PlayerId || !CustomOptionHolder.CrackerIsSelfNone.GetBool()))
                                        continue;

                                    count++;
                                }
                            }
                            counterArea.UpdateCount(count);
                        }
                        else Debug.LogWarning($"Couldn't find counter for:{counterArea.RoomType}");
                    }
                    else counterArea.UpdateCount(0);
                }
            }
            return false;
        }
        public static void Postfix(MapCountOverlay __instance)
        {
            if (RoleClass.EvilHacker.IsMyAdmin) return;
            if (!IsAdminRestrict) return;
            if (CachedPlayer.LocalPlayer.IsDead())
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (AdminTimer <= 0)
            {
                MapBehaviour.Instance.Close();
                return;
            }
            MessageWriter writer;
            if (DeviceUsePlayer[DeviceType.Admin] == null)
            {
                string dateTimeString = AdminStartTime.ToString("yyyy/MM/dd HH:mm:ss");
                writer = RPCHelper.StartRPC(CustomRPC.SetDeviceUseStatus);
                writer.Write((byte)DeviceType.Admin);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(true);
                writer.Write(dateTimeString);
                writer.EndRPC();
                RPCProcedure.SetDeviceUseStatus((byte)DeviceType.Admin, CachedPlayer.LocalPlayer.PlayerId, true, dateTimeString);
            }
            if (DeviceUsePlayer[DeviceType.Admin].PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                AdminTimer -= Time.deltaTime;
                writer = RPCHelper.StartRPC(CustomRPC.SetDeviceTime);
                writer.Write((byte)DeviceType.Admin);
                writer.Write(AdminTimer);
                writer.EndRPC();
                RPCProcedure.SetDeviceTime((byte)DeviceType.Admin, AdminTimer);
            }
            if (TimeRemaining == null)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                TimeRemaining.transform.position = Vector3.zero;
                TimeRemaining.transform.localPosition = new Vector3(3.25f, 5.25f);
                TimeRemaining.transform.localScale *= 2f;
                TimeRemaining.color = Palette.White;
            }
            TimeRemaining.text = TimeSpan.FromSeconds(AdminTimer).ToString(@"mm\:ss\.ff");
            TimeRemaining.gameObject.SetActive(true);
        }
    }
    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnDisable))]
    class MapCountOverlayOnDisablePatch
    {
        public static void Postfix()
        {
            if (RoleClass.EvilHacker.IsMyAdmin) return;
            RoleClass.EvilHacker.IsMyAdmin = false;
            if (!IsAdminRestrict) return;
            if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
            if (CachedPlayer.LocalPlayer.IsDead()) return;
            if (AdminTimer <= 0) return;
            if (DeviceUsePlayer[DeviceType.Admin] != null && DeviceUsePlayer[DeviceType.Admin].PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetDeviceUseStatus);
                writer.Write((byte)DeviceType.Admin);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(false);
                writer.Write("");
                writer.EndRPC();
                RPCProcedure.SetDeviceUseStatus((byte)DeviceType.Admin, CachedPlayer.LocalPlayer.PlayerId, false, "");
            }
        }
    }
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    class CoVitalsOpen
    {
        static void Postfix(VitalsMinigame __instance)
        {
            if (IsVitalRestrict && CachedPlayer.LocalPlayer.IsAlive() && RoleClass.Doctor.Vital == null) VitalStartTime = DateTime.UtcNow;
            Roles.Crewmate.Painter.HandleRpc(Roles.Crewmate.Painter.ActionType.CheckVital);
        }
    }
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new Type[] { })]
    class VitalCloseOpen
    {
        static void Postfix(Minigame __instance)
        {
            if (__instance is VitalsMinigame && IsVitalRestrict && CachedPlayer.LocalPlayer.IsAlive() && RoleClass.Doctor.Vital == null)
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                if (VitalTimer <= 0) return;
                if (DeviceUsePlayer[DeviceType.Vital] != null && DeviceUsePlayer[DeviceType.Vital].PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetDeviceUseStatus);
                    writer.Write((byte)DeviceType.Vital);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer.Write(false);
                    writer.Write("");
                    writer.EndRPC();
                    RPCProcedure.SetDeviceUseStatus((byte)DeviceType.Vital, CachedPlayer.LocalPlayer.PlayerId, false, "");
                }
            }
        }
    }
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    class VitalsDevice
    {
        static void Postfix(VitalsMinigame __instance)
        {
            if (!MapOption.CanUseVitalOrDoorLog || PlayerControl.LocalPlayer.IsRole(RoleId.Vampire) || PlayerControl.LocalPlayer.IsRole(RoleId.Dependents))
            {
                __instance.Close();
            }
            if (!IsVitalRestrict || RoleClass.Doctor.Vital != null) return;
            if (CachedPlayer.LocalPlayer.IsDead())
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (VitalTimer <= 0)
            {
                __instance.Close();
                return;
            }
            MessageWriter writer;
            if (DeviceUsePlayer[DeviceType.Vital] == null)
            {
                string dateTimeString = VitalStartTime.ToString("yyyy/MM/dd HH:mm:ss");
                writer = RPCHelper.StartRPC(CustomRPC.SetDeviceUseStatus);
                writer.Write((byte)DeviceType.Vital);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(true);
                writer.Write(dateTimeString);
                writer.EndRPC();
                RPCProcedure.SetDeviceUseStatus((byte)DeviceType.Vital, CachedPlayer.LocalPlayer.PlayerId, true, dateTimeString);
            }
            if (DeviceUsePlayer[DeviceType.Vital].PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                VitalTimer -= Time.deltaTime;
                writer = RPCHelper.StartRPC(CustomRPC.SetDeviceTime);
                writer.Write((byte)DeviceType.Vital);
                writer.Write(VitalTimer);
                writer.EndRPC();
                RPCProcedure.SetDeviceTime((byte)DeviceType.Vital, VitalTimer);
            }
            if (TimeRemaining == null)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                TimeRemaining.transform.position = Vector3.zero;
                TimeRemaining.transform.localPosition = new Vector3(1.7f, 4.45f);
                TimeRemaining.transform.localScale *= 1.8f;
                TimeRemaining.color = Palette.White;
            }
            TimeRemaining.text = TimeSpan.FromSeconds(VitalTimer).ToString(@"mm\:ss\.ff");
            TimeRemaining.gameObject.SetActive(true);
        }
    }
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
    class SurveillanceMinigameUpdatePatch
    {
        public static void Postfix(SurveillanceMinigame __instance)
        {
            if (!MapOption.CanUseCamera || PlayerControl.LocalPlayer.IsRole(RoleId.Vampire, RoleId.Dependents))
            {
                __instance.Close();
            }
            CameraUpdate(__instance);
        }
    }
    static bool IsCameraCloseNow;
    static void CameraClose()
    {
        if (!IsCameraRestrict || CachedPlayer.LocalPlayer.IsDead()) return;
        IsCameraCloseNow = true;
        if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
        if (CameraTimer <= 0) return;
        if (DeviceUsePlayer[DeviceType.Camera] != null && DeviceUsePlayer[DeviceType.Camera].PlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetDeviceUseStatus);
            writer.Write((byte)DeviceType.Camera);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write(false);
            writer.Write("");
            writer.EndRPC();
            RPCProcedure.SetDeviceUseStatus((byte)DeviceType.Camera, CachedPlayer.LocalPlayer.PlayerId, false, "");
        }
    }
    static void CameraUpdate(Minigame __instance)
    {
        if (!IsCameraRestrict) return;
        if (CachedPlayer.LocalPlayer.IsDead())
        {
            if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
            return;
        }
        if (IsCameraCloseNow) return;
        if (CameraTimer <= 0)
        {
            __instance.Close();
            return;
        }
        MessageWriter writer;
        if (DeviceUsePlayer[DeviceType.Camera] == null)
        {
            string dateTimeString = CameraStartTime.ToString("yyyy/MM/dd HH:mm:ss");
            writer = RPCHelper.StartRPC(CustomRPC.SetDeviceUseStatus);
            writer.Write((byte)DeviceType.Camera);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write(true);
            writer.Write(dateTimeString);
            writer.EndRPC();
            RPCProcedure.SetDeviceUseStatus((byte)DeviceType.Camera, CachedPlayer.LocalPlayer.PlayerId, true, dateTimeString);
        }
        if (DeviceUsePlayer[DeviceType.Camera].PlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            CameraTimer -= Time.deltaTime;
            writer = RPCHelper.StartRPC(CustomRPC.SetDeviceTime);
            writer.Write((byte)DeviceType.Camera);
            writer.Write(CameraTimer);
            writer.EndRPC();
            RPCProcedure.SetDeviceTime((byte)DeviceType.Camera, CameraTimer);
        }
        if (TimeRemaining == null)
        {
            TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
            TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
            TimeRemaining.transform.position = Vector3.zero;
            TimeRemaining.transform.localPosition = new Vector3(0.95f, 4.45f);
            TimeRemaining.transform.localScale *= 1.8f;
            TimeRemaining.color = Palette.White;
        }
        TimeRemaining.text = TimeSpan.FromSeconds(CameraTimer).ToString(@"mm\:ss\.ff");
        TimeRemaining.gameObject.SetActive(true);
    }
    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Close))]
    class PlanetSurveillanceMinigameClosePatch
    {
        public static void Postfix() => CameraClose();
    }
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Close))]
    class SurveillanceMinigameClosePatch
    {
        public static void Postfix() => CameraClose();
    }
    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
    class PlanetSurveillanceMinigameBeginPatch
    {
        public static void Postfix() => IsCameraCloseNow = false;
    }
    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    class SurveillanceMinigameBeginPatch
    {
        public static void Postfix() => IsCameraCloseNow = false;
    }

    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
    class PlanetSurveillanceMinigameUpdatePatch
    {
        public static void Postfix(PlanetSurveillanceMinigame __instance)
        {
            if (!MapOption.CanUseCamera || PlayerControl.LocalPlayer.IsRole(RoleId.Vampire, RoleId.Dependents))
            {
                __instance.Close();
            }
            CameraUpdate(__instance);
        }
    }

    [HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
    class SecurityLogGameUpdatePatch
    {
        public static void Postfix(SecurityLogGame __instance)
        {
            if (!MapOption.CanUseVitalOrDoorLog || PlayerControl.LocalPlayer.IsRole(RoleId.Vampire, RoleId.Dependents))
            {
                __instance.Close();
            }
            CameraUpdate(__instance);
        }
    }
}