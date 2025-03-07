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
    public static Dictionary<string, HashSet<PlayerControl>> UsePlayers = new();
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
            UsePlayers[deviceType].RemoveWhere(player => player == null || player.Data == null || player.Data.Disconnected || player.Data.IsDead);
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

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
    class MapCountOverlayAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(MapCountOverlay __instance)
        {
            if (IsAdminRestrict)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, __instance.transform);
                TimeRemaining.alignment = TMPro.TextAlignmentOptions.Center;
                TimeRemaining.transform.localPosition = new Vector3(0, -1, -100f);
                TimeRemaining.transform.localScale = Vector3.one * 0.5f;
                TimeRemaining.gameObject.SetActive(true);

                // RPCを送信
                RpcSetDeviceUseStatus((byte)DeviceType.Admin, PlayerControl.LocalPlayer, true);
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
            if (TimeRemaining == null)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                TimeRemaining.transform.position = Vector3.zero;
                TimeRemaining.transform.localPosition = new Vector3(3.25f, 5.25f);
                TimeRemaining.transform.localScale *= 2f;
                TimeRemaining.color = Palette.White;
            }
            TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Admin.ToString()]).ToString(@"mm\:ss\.ff");
            TimeRemaining.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnDisable))]
    class MapCountOverlayOnDisablePatch
    {
        public static void Postfix()
        {
            if (!IsAdminRestrict)
                return;
            if (!AmongUsClient.Instance.AmHost)
            {
                // RPCを送信
                RpcSetDeviceUseStatus((byte)DeviceType.Admin, PlayerControl.LocalPlayer, false);
            }
            else
            {
                UsePlayers[DeviceType.Admin.ToString()].Remove(PlayerControl.LocalPlayer);
            }
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
            if (IsVitalRestrict)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, __instance.transform);
                TimeRemaining.alignment = TMPro.TextAlignmentOptions.Center;
                TimeRemaining.transform.localPosition = new Vector3(0, 0, -50f);
                TimeRemaining.transform.localScale = Vector3.one * 0.3f;
                TimeRemaining.gameObject.SetActive(true);

                // RPCを送信
                RpcSetDeviceUseStatus((byte)DeviceType.Vital, PlayerControl.LocalPlayer, true);
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
            if (!AmongUsClient.Instance.AmHost)
            {
                // RPCを送信
                RpcSetDeviceUseStatus((byte)DeviceType.Vital, PlayerControl.LocalPlayer, false);
            }
            else
            {
                UsePlayers[DeviceType.Vital.ToString()].Remove(PlayerControl.LocalPlayer);
            }
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

            if (!IsVitalRestrict) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (!AmongUsClient.Instance.AmHost)
                DeviceTimers[DeviceType.Vital.ToString()] -= Time.deltaTime;
            if (TimeRemaining == null)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                TimeRemaining.transform.position = Vector3.zero;
                TimeRemaining.transform.localPosition = new Vector3(1.7f, 4.45f);
                TimeRemaining.transform.localScale *= 1.8f;
                TimeRemaining.color = Palette.White;
            }
            TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Vital.ToString()]).ToString(@"mm\:ss\.ff");
            TimeRemaining.gameObject.SetActive(true);
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
        if (!AmongUsClient.Instance.AmHost)
        {
            // RPCを送信
            RpcSetDeviceUseStatus((byte)DeviceType.Camera, PlayerControl.LocalPlayer, false);
        }
        else
        {
            UsePlayers[DeviceType.Camera.ToString()].Remove(PlayerControl.LocalPlayer);
        }
    }

    static void CameraOpen()
    {
        if (MapSettingOptions.DeviceOptions && MapSettingOptions.DeviceCameraOption != DeviceOptionType.CantUse)
        {
            IsCameraCloseNow = false;
            if (IsCameraRestrict)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, Camera.main.transform);
                TimeRemaining.alignment = TMPro.TextAlignmentOptions.Center;
                TimeRemaining.transform.localPosition = new Vector3(0, -1.8f, -250f);
                TimeRemaining.transform.localScale = Vector3.one * 0.3f;
                TimeRemaining.gameObject.SetActive(true);

                // RPCを送信
                RpcSetDeviceUseStatus((byte)DeviceType.Camera, PlayerControl.LocalPlayer, true);
            }
        }
        else
        {
            CameraClose();
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

    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
    class PlanetSurveillanceMinigameBeginPatch
    {
        public static void Postfix() => CameraOpen();
    }

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    class SurveillanceMinigameBeginPatch
    {
        public static void Postfix() => CameraOpen();
    }

    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
    class FungleSurveillanceMinigameBeginPatch
    {
        public static void Postfix() => CameraOpen();
    }

    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Close))]
    class FungleSurveillanceMinigameClosePatch
    {
        public static void Postfix() => CameraClose();
    }

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
    class SurveillanceMinigameUpdatePatch
    {
        public static void Postfix(SurveillanceMinigame __instance)
        {
            if (IsCameraRestrict && DeviceTimers[DeviceType.Camera.ToString()] <= 0)
            {
                __instance.Close();
                return;
            }

            if (!IsCameraRestrict) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (IsCameraCloseNow) return;
            if (!AmongUsClient.Instance.AmHost)
                DeviceTimers[DeviceType.Camera.ToString()] -= Time.deltaTime;
            if (TimeRemaining == null)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                TimeRemaining.transform.position = Vector3.zero;
                TimeRemaining.transform.localPosition = new Vector3(0.95f, 4.45f, -10f);
                TimeRemaining.transform.localScale *= 1.8f;
                TimeRemaining.color = Palette.White;
            }
            TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Camera.ToString()]).ToString(@"mm\:ss\.ff");
            TimeRemaining.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
    class PlanetSurveillanceMinigameUpdatePatch
    {
        public static void Postfix(PlanetSurveillanceMinigame __instance)
        {
            if (IsCameraRestrict && DeviceTimers[DeviceType.Camera.ToString()] <= 0)
            {
                __instance.Close();
                return;
            }

            if (!IsCameraRestrict) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (IsCameraCloseNow) return;
            if (!AmongUsClient.Instance.AmHost)
                DeviceTimers[DeviceType.Camera.ToString()] -= Time.deltaTime;
            if (TimeRemaining == null)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                TimeRemaining.transform.position = Vector3.zero;
                TimeRemaining.transform.localPosition = new Vector3(0.95f, 4.45f, -10f);
                TimeRemaining.transform.localScale *= 1.8f;
                TimeRemaining.color = Palette.White;
            }
            TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Camera.ToString()]).ToString(@"mm\:ss\.ff");
            TimeRemaining.gameObject.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Update))]
    class FungleSurveillanceMinigameUpdatePatch
    {
        public static void Postfix(FungleSurveillanceMinigame __instance)
        {
            if (IsCameraRestrict && DeviceTimers[DeviceType.Camera.ToString()] <= 0)
            {
                __instance.Close();
                return;
            }

            if (!IsCameraRestrict) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (TimeRemaining != null) GameObject.Destroy(TimeRemaining.gameObject);
                return;
            }
            if (IsCameraCloseNow) return;
            if (!AmongUsClient.Instance.AmHost)
                DeviceTimers[DeviceType.Camera.ToString()] -= Time.deltaTime;
            if (TimeRemaining == null)
            {
                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                TimeRemaining.transform.position = Vector3.zero;
                TimeRemaining.transform.localPosition = new Vector3(2.3f, 4.2f, -10f);
                TimeRemaining.transform.localScale *= 1.8f;
                TimeRemaining.color = Palette.White;
            }
            TimeRemaining.text = TimeSpan.FromSeconds(DeviceTimers[DeviceType.Camera.ToString()]).ToString(@"mm\:ss\.ff");
            TimeRemaining.gameObject.SetActive(true);
        }
    }
    #endregion

    #region CustomRPC メソッド
    // デバイスの使用時間を設定するRPC
    [CustomRPC]
    public static void RpcSetDeviceTime(string deviceType, float time)
    {
        DeviceTimers[deviceType] = time;
        Logger.Info($"SET {deviceType}:{DeviceTimers[deviceType]}");
    }

    // デバイスの使用状態を設定するRPC
    [CustomRPC]
    public static void RpcSetDeviceUseStatus(byte deviceTypeByte, PlayerControl player, bool isOpen)
    {
        DeviceType deviceType = (DeviceType)deviceTypeByte;
        if (player == null)
            return;

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