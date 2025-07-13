using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Roles.Ability;
using static SuperNewRoles.CustomOptions.Categories.MapSettingOptions;
using System.Linq;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Patches;

/// <summary>
/// 情報機器制限の実装
/// </summary>
public static class DevicesPatch
{
    public static bool DontCountBecausePortableAdmin;
    public static bool DontCountBecausePortableVitals;
    private static bool isAdminRestrictOption;
    private static bool isAdminDisabledOption;
    public static bool IsAdminRestrict => DontCountBecausePortableAdmin ? false : isAdminRestrictOption;
    public static bool IsAdminDisabled => isAdminDisabledOption;
    public static bool IsVitalRestrict;
    public static bool IsVitalDisabled;
    public static bool IsCameraRestrict;
    public static bool IsCameraDisabled;
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

        isAdminRestrictOption = false;
        isAdminDisabledOption = false;
        IsCameraRestrict = false;
        IsCameraDisabled = false;
        IsVitalRestrict = false;
        IsVitalDisabled = false;

        if (MapSettingOptions.DeviceOptions)
        {
            if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit)
            {
                isAdminRestrictOption = MapSettingOptions.DeviceAdminTimeLimit > 0;
                IsCameraRestrict = MapSettingOptions.DeviceCameraTimeLimit > 0;
                IsVitalRestrict = MapSettingOptions.DeviceVitalTimeLimit > 0;

                if (isAdminRestrictOption)
                {
                    DeviceTypes.Add(DeviceType.Admin.ToString());
                    DeviceTimers[DeviceType.Admin.ToString()] = MapSettingOptions.DeviceAdminTimeLimit;
                }
                if (IsCameraRestrict)
                {
                    DeviceTypes.Add(DeviceType.Camera.ToString());
                    DeviceTimers[DeviceType.Camera.ToString()] = MapSettingOptions.DeviceCameraTimeLimit;
                }
                if (IsVitalRestrict)
                {
                    DeviceTypes.Add(DeviceType.Vital.ToString());
                    DeviceTimers[DeviceType.Vital.ToString()] = MapSettingOptions.DeviceVitalTimeLimit;
                }
            }
            else if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.OnOff)
            {
                isAdminDisabledOption = !MapSettingOptions.DeviceAdminEnabled;
                IsCameraDisabled = !MapSettingOptions.DeviceCameraEnabled;
                IsVitalDisabled = !MapSettingOptions.DeviceVitalEnabled;
            }
        }
        SyncTimer = 0f;
    }
    private static bool ValidPlayer(byte playerId)
    {
        ExPlayerControl player = ExPlayerControl.ById(playerId);
        return player != null && player.IsAlive();
    }
    private static void CreateRemainingTextIfNeeded(Transform Parent)
    {
        if (TimeRemaining != null) return;
        TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, Parent);
        TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
        TimeRemaining.transform.position = Vector3.zero;
        TimeRemaining.transform.localPosition = new Vector3(2.75f, 4.65f);
        TimeRemaining.transform.localScale *= 2f;
        TimeRemaining.color = Palette.White;
        TimeRemaining.text = "";
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
                RpcSetDeviceTime(deviceType, DeviceTimers[deviceType]);
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
            DontCountBecausePortableAdmin = false;
            bool canUse = !(MapSettingOptions.DeviceOptions &&
                            ((MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.OnOff && IsAdminDisabled) ||
                             (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsAdminRestrict && DeviceTimers[DeviceType.Admin.ToString()] <= 0)));
            return canUse;
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
    class MapCountOverlayAwakePatch
    {
        public static void Postfix(MapCountOverlay __instance)
        {
            if (IsAdminRestrict)
            {
                if (DeviceTimers[DeviceType.Admin.ToString()] <= 0)
                {
                    MapBehaviour.Instance.Close();
                    return;
                }
                CreateRemainingTextIfNeeded(__instance.transform);

                Logger.Info($"[Admin Open] RpcSetDeviceUseStatus Called: Player={PlayerControl.LocalPlayer.PlayerId}, IsOpen=true");
                RpcSetDeviceUseStatus(DeviceType.Admin, PlayerControl.LocalPlayer.PlayerId, true);
            }
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    class MapCountOverlayUpdatePatch
    {
        public static bool Prefix(MapCountOverlay __instance)
        {
            if (MapSettingOptions.DeviceOptions)
            {
                if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.OnOff && IsAdminDisabled)
                {
                    MapBehaviour.Instance.Close();
                    return false;
                }
                if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsAdminRestrict && DeviceTimers[DeviceType.Admin.ToString()] <= 0)
                {
                    MapBehaviour.Instance.Close();
                    return false;
                }
            }
            CreateAdmins(__instance);
            return false;
        }

        private static void CreateAdmins(MapCountOverlay __instance)
        {
            AdvancedAdminAbility advancedAdminAbility = ExPlayerControl.LocalPlayer.GetAbility<AdvancedAdminAbility>();
            bool commsActive = !(advancedAdminAbility?.Data.canUseAdminDuringComms ?? false) && ModHelpers.IsComms();

            if (!__instance.isSab && commsActive)
            {
                __instance.isSab = true;
                __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                __instance.SabotageText.gameObject.SetActive(true);
                return;
            }

            if (__instance.isSab && !commsActive)
            {
                __instance.isSab = false;
                __instance.BackgroundColor.SetColor(Color.green);
                __instance.SabotageText.gameObject.SetActive(false);
            }

            bool canSeeImpostorIcon = advancedAdminAbility?.Data.distinctionImpostor ?? false;
            bool canSeeDeadIcon = advancedAdminAbility?.Data.distinctionDead ?? false;

            for (int i = 0; i < __instance.CountAreas.Length; i++)
            {
                CounterArea counterArea = __instance.CountAreas[i];

                if (!commsActive && counterArea.RoomType > SystemTypes.Hallway)
                {
                    PlainShipRoom plainShipRoom = ShipStatus.Instance.FastRooms.TryGetValue(counterArea.RoomType, out var room) ? room : null;

                    if (plainShipRoom != null && plainShipRoom.roomArea)
                    {
                        HashSet<int> hashSet = new();
                        int num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                        int count = 0;
                        List<int> colors = new();
                        int numDeadIcons = 0;
                        int numImpostorIcons = 0;

                        for (int j = 0; j < num; j++)
                        {
                            Collider2D collider2D = __instance.buffer[j];
                            if (collider2D.CompareTag("DeadBody") && __instance.includeDeadBodies)
                            {
                                if (canSeeDeadIcon)
                                {
                                    numDeadIcons++;
                                }

                                count++;
                                colors.Add(ExPlayerControl.ById(collider2D.GetComponent<DeadBody>().ParentId).Player.CurrentOutfit.ColorId);
                            }
                            else
                            {
                                ExPlayerControl component = collider2D.GetComponent<PlayerControl>();
                                if (component?.Player == null) continue;
                                if (component.Data == null || component.Data.Disconnected || component.Data.IsDead) continue;
                                if (!__instance.showLivePlayerPosition && component.AmOwner) continue;
                                if (!hashSet.Add(component.PlayerId)) continue;

                                if (((ExPlayerControl)component).GetAbility<HideInAdminAbility>()?.IsHideInAdmin ?? false) continue;

                                // BlackHatHackerのフィルター
                                var blackHatHacker = ExPlayerControl.LocalPlayer.GetAbility<BlackHatHackerAbility>();
                                if (DevicesPatch.DontCountBecausePortableAdmin && blackHatHacker != null && blackHatHacker.AdminAbility != null &&
                                    !blackHatHacker.InfectedPlayerId.Contains(component.PlayerId) && !component.AmOwner) continue;
                                if (DevicesPatch.DontCountBecausePortableAdmin && blackHatHacker != null && blackHatHacker.AdminAbility != null && BlackHatHacker.BlackHatHackerIsAdminColor)
                                    colors.Add(component.Player.CurrentOutfit.ColorId);
                                count++;
                                if (canSeeImpostorIcon && component.IsImpostor())
                                {
                                    numImpostorIcons++;
                                }
                            }
                        }
                        counterArea.UpdateCount(count);

                        foreach (PoolableBehavior icon in counterArea.myIcons)
                        {
                            Material material = icon.GetComponent<SpriteRenderer>().material;
                            Color iconColor = numImpostorIcons-- > 0 ? Palette.ImpostorRed : numDeadIcons-- > 0 ? Color.gray : Color.yellow;
                            material.SetColor(PlayerMaterial.BackColor, iconColor);
                            material.SetColor(PlayerMaterial.BodyColor, iconColor);
                        }
                        if (BlackHatHackerAbility.LocalInstance?.AdminAbility != null && DontCountBecausePortableAdmin)
                        {
                            foreach (PoolableBehavior icon in counterArea.myIcons)
                            {
                                if (colors.Count <= 0) continue;
                                Material material = icon.GetComponent<SpriteRenderer>().material;
                                Color iconColor = Palette.PlayerColors[colors.FirstOrDefault()];
                                material.SetColor(PlayerMaterial.BackColor, iconColor);
                                material.SetColor(PlayerMaterial.BodyColor, iconColor);
                                colors.RemoveAt(0);
                            }
                        }
                    }
                    else Debug.LogWarning($"Couldn't find counter for:{counterArea.RoomType}");
                }
                else counterArea.UpdateCount(0);
            }
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
            Logger.Info($"[Admin Close] RpcSetDeviceUseStatus Called: Player={PlayerControl.LocalPlayer.PlayerId}, IsOpen=false");
            RpcSetDeviceUseStatus(DeviceType.Admin, PlayerControl.LocalPlayer.PlayerId, false);
        }
    }
    #endregion

    #region バイタル関連
    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    class VitalsMinigameBeginPatch
    {
        static bool Prefix(VitalsMinigame __instance)
        {
            if (MapSettingOptions.DeviceOptions && !DontCountBecausePortableVitals)
            {
                if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.OnOff && IsVitalDisabled)
                {
                    foreach (var vital in __instance.vitals)
                    {
                        vital.gameObject.SetActive(false);
                    }
                    __instance.Close();
                    return false;
                }
                if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsVitalRestrict)
                {
                    if (DeviceTimers[DeviceType.Vital.ToString()] <= 0)
                    {
                        foreach (var vital in __instance.vitals)
                        {
                            vital.gameObject.SetActive(false);
                        }
                        __instance.Close();
                        return false;
                    }
                    if (TimeRemaining == null)
                    {
                        TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                        TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                        TimeRemaining.transform.position = Vector3.zero;
                        TimeRemaining.transform.localPosition = new Vector3(1.7f, 3.95f);
                        TimeRemaining.transform.localScale *= 1.8f;
                        TimeRemaining.color = Palette.White;
                        TimeRemaining.text = "";
                    }
                    RpcSetDeviceUseStatus(DeviceType.Vital, PlayerControl.LocalPlayer.PlayerId, true);
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new Type[] { })]
    class VitalCloseOpen
    {
        static void Postfix(Minigame __instance)
        {
            if (__instance is VitalsMinigame)
            {
                // BlackHatHackerのバイタル状態をリセット
                SuperNewRoles.Roles.Ability.BlackHatHackerVitalsState.IsUsingVitals = false;
                if (DevicesPatch.DontCountBecausePortableVitals)
                {
                    DevicesPatch.DontCountBecausePortableVitals = false;
                    return;
                }
            }

            if (__instance is not VitalsMinigame || !(MapSettingOptions.DeviceOptions && MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsVitalRestrict))
                return;
            RpcSetDeviceUseStatus(DeviceType.Vital, PlayerControl.LocalPlayer.PlayerId, false);
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    class VitalsMinigameUpdatePatch
    {
        static void Postfix(VitalsMinigame __instance)
        {
            if (MapSettingOptions.DeviceOptions && !DontCountBecausePortableVitals)
            {
                if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.OnOff && IsVitalDisabled)
                {
                    __instance.Close();
                    return;
                }
                if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsVitalRestrict && DeviceTimers[DeviceType.Vital.ToString()] <= 0)
                {
                    __instance.Close();
                    return;
                }
            }

            // BlackHatHackerのバイタルフィルター
            var blackHatHacker = SuperNewRoles.Roles.Ability.BlackHatHackerAbility.LocalInstance;
            if (blackHatHacker != null && blackHatHacker.Data.CanInfectedVitals &&
                SuperNewRoles.Roles.Ability.BlackHatHackerVitalsState.IsUsingVitals)
            {
                foreach (VitalsPanel vital in __instance.vitals)
                {
                    if (vital.PlayerInfo == null) continue;
                    bool isInfected = blackHatHacker.InfectedPlayerId.Contains(vital.PlayerInfo.PlayerId) ||
                                     vital.PlayerInfo.Object.AmOwner;
                    vital.gameObject.SetActive(isInfected);
                }
            }

            if (!(MapSettingOptions.DeviceOptions && MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsVitalRestrict)) return;
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
        if (!(MapSettingOptions.DeviceOptions && MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsCameraRestrict))
            return;
        RpcSetDeviceUseStatus(DeviceType.Camera, PlayerControl.LocalPlayer.PlayerId, false);
    }

    static void CameraOpen(Transform instance)
    {
        if (MapSettingOptions.DeviceOptions)
        {
            if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.OnOff && IsCameraDisabled)
            {
                return;
            }

            IsCameraCloseNow = false;
            if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsCameraRestrict)
            {
                if (DeviceTimers[DeviceType.Camera.ToString()] <= 0)
                {
                    return;
                }
                if (TimeRemaining == null)
                {
                    TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, instance);
                    TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                    TimeRemaining.transform.position = Vector3.zero;
                    TimeRemaining.transform.localPosition =
                        GameManager.Instance.LogicOptions.currentGameOptions.MapId == 5 ?
                        new(2.3f, 4.2f, -10) :
                        new(-1.2609f, 0.6373f, -10f);
                    TimeRemaining.transform.localScale *= 1.8f;
                    TimeRemaining.color = Palette.White;
                    TimeRemaining.text = "";
                }
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
        if (MapSettingOptions.DeviceOptions)
        {
            if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.OnOff && IsCameraDisabled)
            {
                closeAction();
                return;
            }
            if (MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsCameraRestrict && DeviceTimers[DeviceType.Camera.ToString()] <= 0)
            {
                closeAction();
                return;
            }
        }

        if (!(MapSettingOptions.DeviceOptions && MapSettingOptions.RestrictionMode == DeviceRestrictionModeType.TimeLimit && IsCameraRestrict))
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
            UpdateCameraTimer(__instance.transform, () => __instance.Close(), new Vector3(0.95f, 3.95f, -10f));
        }
    }

    [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
    class PlanetSurveillanceMinigameUpdatePatch
    {
        public static void Postfix(PlanetSurveillanceMinigame __instance)
        {
            UpdateCameraTimer(__instance.transform, () => __instance.Close(), new Vector3(0.95f, 3.95f, -10f));
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
    [CustomRPC(onlyOtherPlayer: true)]
    public static void RpcSetDeviceTime(string deviceType, float time)
    {
        DeviceTimers[deviceType] = time;
        Logger.Info($"SET {deviceType}:{DeviceTimers[deviceType]}");
    }

    [CustomRPC]
    public static void RpcSetDeviceUseStatus(DeviceType deviceType, byte player, bool isOpen)
    {
        Logger.Info($"[RPC] RpcSetDeviceUseStatus Received: Device={deviceType}, Player={player}, IsOpen={isOpen}, IsHost={AmongUsClient.Instance.AmHost}");

        string deviceTypeStr = deviceType.ToString();
        if (!UsePlayers.ContainsKey(deviceTypeStr))
        {
            Logger.Warning($"[RPC] RpcSetDeviceUseStatus: Invalid device type string '{deviceTypeStr}' received.");
            return;
        }

        Logger.Info($"[RPC] RpcSetDeviceUseStatus Before: UsePlayers[{deviceTypeStr}].Count={UsePlayers[deviceTypeStr].Count}, Contains({player})={UsePlayers[deviceTypeStr].Contains(player)}");

        if (isOpen)
        {
            if (!UsePlayers[deviceTypeStr].Contains(player))
                UsePlayers[deviceTypeStr].Add(player);
        }
        else
        {
            UsePlayers[deviceTypeStr].Remove(player);
        }
        Logger.Info($"[RPC] RpcSetDeviceUseStatus After: UsePlayers[{deviceTypeStr}].Count={UsePlayers[deviceTypeStr].Count}");
    }
    #endregion
}