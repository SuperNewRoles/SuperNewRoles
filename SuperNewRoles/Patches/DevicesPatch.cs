using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace SuperNewRoles.Patches;

/// <summary>
/// 情報機器制限の実装
/// </summary>
public static class DevicesPatch
{
    public static bool IsAdminRestrict;
    public static bool IsVitalRestrict;
    public static bool IsCameraRestrict;
    public static TextMeshPro TimeRemaining;
    static HashSet<string> DeviceTypes = new();
    public static Dictionary<string, HashSet<byte>> UsePlayers = new();
    public static Dictionary<string, float> DeviceTimers = new();
    public static float SyncTimer;
    public enum DeviceType
    {
        Admin,
        Camera,
        Vital
    }

    public static void ClearAndReload()
    {
        FixedUpdateEvent.Instance.AddListener(FixedUpdate);
        UsePlayers = new() { { DeviceType.Admin.ToString(), new() }, { DeviceType.Camera.ToString(), new() }, { DeviceType.Vital.ToString(), new() } };
        DeviceTypes = new();
        DeviceTimers = new();

        IsAdminRestrict = MapSettingOptions.DeviceAdminOption == DeviceOptionType.Restrict;
        IsCameraRestrict = MapSettingOptions.DeviceCameraOption == DeviceOptionType.Restrict;
        IsVitalRestrict = MapSettingOptions.DeviceVitalOrDoorLogOption == DeviceOptionType.Restrict;

        if (MapSettingOptions.DeviceOptions)
        {
            if (IsAdminRestrict)
            {
                DeviceTypes.Add(DeviceType.Admin.ToString());
                DeviceTimers[DeviceType.Admin.ToString()] = MapSettingOptions.DeviceUseAdminTime;
            }
            if (IsCameraRestrict)
            {
                DeviceTypes.Add(DeviceType.Camera.ToString());
                DeviceTimers[DeviceType.Camera.ToString()] = MapSettingOptions.DeviceUseCameraTime;
            }
            if (IsVitalRestrict)
            {
                DeviceTypes.Add(DeviceType.Vital.ToString());
                DeviceTimers[DeviceType.Vital.ToString()] = MapSettingOptions.DeviceUseVitalOrDoorLogTime;
            }
        }
        SyncTimer = 0f;
    }
    private static bool ValidPlayer(byte playerId)
    {
        ExPlayerControl player = ExPlayerControl.ById(playerId);
        return player != null && player.IsAlive();
    }
    public static void FixedUpdate()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        foreach (var deviceType in DeviceTypes)
        {
            if (!DeviceTimers.TryGetValue(deviceType, out float timer))
                continue;
            if (timer <= 0)
                continue;
            UsePlayers[deviceType].RemoveWhere(playerId => !ValidPlayer(playerId));
            if (UsePlayers[deviceType].Count <= 0)
                continue;
            DeviceTimers[deviceType] -= Time.fixedDeltaTime;
            if (SyncTimer <= 0)
            {
                // RPCを送信する
                RpcSetDeviceTime(deviceType, DeviceTimers[deviceType]);
                Logger.Info($"Sync {deviceType}:{DeviceTimers[deviceType]}");
            }
        }
        if (SyncTimer <= 0)
            SyncTimer = 0.2f;
        else
            SyncTimer -= Time.fixedDeltaTime;
    }

    #region アドミン関連
    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
    public static class MapConsoleUsePatch
    {
        public static bool Prefix(MapConsole __instance)
        {
            bool IsUse = !MapSettingOptions.DeviceOptions || MapSettingOptions.DeviceAdminOption != DeviceOptionType.CantUse;
            return IsUse;
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Awake))]
    class MapCountOverlayAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(MapCountOverlay __instance)
        {
            if (IsAdminRestrict)
            {
                if (DeviceTimers[DeviceType.Admin.ToString()] <= 0)
                {
                    MapBehaviour.Instance.Close();
                    return;
                }
                if (TimeRemaining == null)
                {
                    TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                    TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                    TimeRemaining.transform.position = Vector3.zero;
                    TimeRemaining.transform.localPosition = new Vector3(2.75f, 4.65f);
                    TimeRemaining.transform.localScale *= 2f;
                    TimeRemaining.color = Palette.White;
                    TimeRemaining.text = "";
                }

                // RPCを送信
                RpcSetDeviceUseStatus(DeviceType.Admin, PlayerControl.LocalPlayer.PlayerId, true);
            }
            else if (MapSettingOptions.DeviceOptions && MapSettingOptions.DeviceAdminOption == DeviceOptionType.CantUse)
            {
                MapBehaviour.Instance.Close();
            }
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    class MapCountOverlayUpdatePatch
    {
        public static bool Prefix(MapCountOverlay __instance)
        {
            if (IsAdminRestrict && DeviceTimers[DeviceType.Admin.ToString()] <= 0)
            {
                MapBehaviour.Instance.Close();
                return false;
            }
            else if (MapSettingOptions.DeviceOptions && MapSettingOptions.DeviceCameraOption == DeviceOptionType.CantUse)
            {
                MapBehaviour.Instance.Close();
                return false;
            }
            return true;
        }

        public static void Postfix(MapCountOverlay __instance)
        {
            if (!IsAdminRestrict) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (DeviceTimers[DeviceType.Admin.ToString()] <= 0)
            {
                MapBehaviour.Instance.Close();
                return;
            }
            if (!AmongUsClient.Instance.AmHost)
                DeviceTimers[DeviceType.Admin.ToString()] -= Time.deltaTime;
            if (TimeRemaining != null)
                TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Admin.ToString()]).ToString(@"mm\:ss\.ff");
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnDisable))]
    class MapCountOverlayOnDisablePatch
    {
        public static void Postfix()
        {
            if (!IsAdminRestrict)
                return;
            if (TimeRemaining != null)
            {
                GameObject.Destroy(TimeRemaining.gameObject);
                TimeRemaining = null;
            }
            RpcSetDeviceUseStatus(DeviceType.Admin, PlayerControl.LocalPlayer.PlayerId, false);
        }
    }
    #endregion

    #region バイタル関連
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    class VitalsMinigameBeginPatch
    {
        static void Postfix(VitalsMinigame __instance)
        {
            if (MapSettingOptions.DeviceOptions && MapSettingOptions.DeviceVitalOrDoorLogOption == DeviceOptionType.CantUse)
                __instance.Close();
            else if (IsVitalRestrict && DeviceTimers[DeviceType.Vital.ToString()] <= 0)
                __instance.Close();
            else if (IsVitalRestrict)
            {
                if (TimeRemaining == null)
                {
                    TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                    TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                    TimeRemaining.transform.position = Vector3.zero;
                    TimeRemaining.transform.localPosition = new Vector3(1.7f, 4.45f);
                    TimeRemaining.transform.localScale *= 1.8f;
                    TimeRemaining.color = Palette.White;
                    TimeRemaining.text = "";
                }
                // RPCを送信
                RpcSetDeviceUseStatus(DeviceType.Vital, PlayerControl.LocalPlayer.PlayerId, true);
            }
        }
    }

    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new Type[] { })]
    class VitalCloseOpen
    {
        static void Postfix(Minigame __instance)
        {
            if (__instance is not VitalsMinigame || !IsVitalRestrict)
                return;
            // RPCを送信
            RpcSetDeviceUseStatus(DeviceType.Vital, PlayerControl.LocalPlayer.PlayerId, false);
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    class VitalsMinigameUpdatePatch
    {
        static void Postfix(VitalsMinigame __instance)
        {
            if (IsVitalRestrict && DeviceTimers[DeviceType.Vital.ToString()] <= 0)
            {
                __instance.Close();
                return;
            }
            else if (MapSettingOptions.DeviceOptions && MapSettingOptions.DeviceVitalOrDoorLogOption == DeviceOptionType.CantUse)
            {
                __instance.Close();
                return;
            }

            if (!IsVitalRestrict) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (!AmongUsClient.Instance.AmHost)
                DeviceTimers[DeviceType.Vital.ToString()] -= Time.deltaTime;
            if (TimeRemaining != null)
                TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Vital.ToString()]).ToString(@"mm\:ss\.ff");
        }
    }
    #endregion

    #region カメラ関連
    static bool IsCameraCloseNow;
    static void CameraClose()
    {
        IsCameraCloseNow = true;
        if (!IsCameraRestrict)
            return;
        // RPCを送信
        RpcSetDeviceUseStatus(DeviceType.Camera, PlayerControl.LocalPlayer.PlayerId, false);
    }

    static void CameraOpen(Transform instance)
    {
        if (MapSettingOptions.DeviceOptions && MapSettingOptions.DeviceCameraOption != DeviceOptionType.CantUse)
        {
            IsCameraCloseNow = false;
            if (IsCameraRestrict)
            {
                if (TimeRemaining == null)
                {
                    TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, instance);
                    TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                    TimeRemaining.transform.position = Vector3.zero;
                    TimeRemaining.transform.localPosition =
                        GameManager.Instance.LogicOptions.currentGameOptions.MapId == 5 ?
                        new(2.3f, 4.2f, -10) :
                        new(0.95f, 4.45f, -10f);
                    TimeRemaining.transform.localScale *= 1.8f;
                    TimeRemaining.color = Palette.White;
                    TimeRemaining.text = "";
                }
                // RPCを送信
                RpcSetDeviceUseStatus(DeviceType.Camera, PlayerControl.LocalPlayer.PlayerId, true);
            }
        }
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

    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Close))]
    class FungleSurveillanceMinigameClosePatch
    {
        public static void Postfix() => CameraClose();
    }

    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
    class PlanetSurveillanceMinigameBeginPatch
    {
        public static void Postfix(PlanetSurveillanceMinigame __instance) => CameraOpen(__instance.transform);
    }

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    class SurveillanceMinigameBeginPatch
    {
        public static void Postfix(SurveillanceMinigame __instance) => CameraOpen(__instance.transform);
    }

    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
    class FungleSurveillanceMinigameBeginPatch
    {
        public static void Postfix(FungleSurveillanceMinigame __instance) => CameraOpen(__instance.transform);
    }

    private static void UpdateCameraTimer(Transform targetTransform, Action closeAction, Vector3 taskTextLocalPosition)
    {
        if (IsCameraRestrict && DeviceTimers[DeviceType.Camera.ToString()] <= 0)
        {
            closeAction();
            return;
        }
        if (MapSettingOptions.DeviceOptions && MapSettingOptions.DeviceCameraOption == DeviceOptionType.CantUse)
        {
            closeAction();
            return;
        }
        if (!IsCameraRestrict)
            return;
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            if (TimeRemaining != null)
                GameObject.Destroy(TimeRemaining.gameObject);
            return;
        }
        if (IsCameraCloseNow)
            return;
        if (!AmongUsClient.Instance.AmHost)
            DeviceTimers[DeviceType.Camera.ToString()] -= Time.deltaTime;
        if (TimeRemaining != null)
            TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Camera.ToString()]).ToString(@"mm\:ss\.ff");
    }

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
    class SurveillanceMinigameUpdatePatch
    {
        public static void Postfix(SurveillanceMinigame __instance)
        {
            UpdateCameraTimer(__instance.transform, () => __instance.Close(), new Vector3(0.95f, 4.45f, -10f));
        }
    }

    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
    class PlanetSurveillanceMinigameUpdatePatch
    {
        public static void Postfix(PlanetSurveillanceMinigame __instance)
        {
            UpdateCameraTimer(__instance.transform, () => __instance.Close(), new Vector3(0.95f, 4.45f, -10f));
        }
    }

    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Update))]
    class FungleSurveillanceMinigameUpdatePatch
    {
        public static void Postfix(FungleSurveillanceMinigame __instance)
        {
            UpdateCameraTimer(__instance.transform, () => __instance.Close(), new Vector3(2.3f, 4.2f, -10f));
        }
    }
    #endregion

    #region CustomRPC メソッド
    // デバイスの使用時間を設定するRPC
    [CustomRPC(onlyOtherPlayer: true)]
    public static void RpcSetDeviceTime(string deviceType, float time)
    {
        DeviceTimers[deviceType] = time;
        Logger.Info($"SET {deviceType}:{DeviceTimers[deviceType]}");
    }

    // デバイスの使用状態を設定するRPC
    [CustomRPC]
    public static void RpcSetDeviceUseStatus(DeviceType deviceType, byte player, bool isOpen)
    {
        Logger.Info($"SET {deviceType}:{player}:{isOpen}");

        if (isOpen)
        {
            if (!UsePlayers[deviceType.ToString()].Contains(player))
                UsePlayers[deviceType.ToString()].Add(player);
        }
        else
        {
            UsePlayers[deviceType.ToString()].Remove(player);
        }
    }
    #endregion
}