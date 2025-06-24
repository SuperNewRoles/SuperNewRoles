using System;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

[Flags]
public enum DeviceTypeFlag
{
    None = 0,
    Admin = 1 << 1,
    Camera = 1 << 2,
    Vital = 1 << 3,
    All = Admin | Camera | Vital,
}
public class DeviceCanUseAbility : AbilityBase
{
    private Func<DeviceTypeFlag> _cannotUseDeviceType;
    public DeviceCanUseAbility(Func<DeviceTypeFlag> cannotUseDeviceType)
    {
        _cannotUseDeviceType = cannotUseDeviceType;
    }
    public bool TryUse(DevicesPatch.DeviceType deviceType)
    {
        DeviceTypeFlag type = DeviceTypeFlag.None;
        switch (deviceType)
        {
            case DevicesPatch.DeviceType.Admin:
                type = DeviceTypeFlag.Admin;
                break;
            case DevicesPatch.DeviceType.Camera:
                type = DeviceTypeFlag.Camera;
                break;
            case DevicesPatch.DeviceType.Vital:
                type = DeviceTypeFlag.Vital;
                break;
            default:
                return true;
        }
        return !((_cannotUseDeviceType?.Invoke() ?? DeviceTypeFlag.None).HasFlag(type));
    }
}

public static class DeviceAbilityHelper
{
    public static bool CheckDeviceUse(DevicesPatch.DeviceType deviceType, Action onFailure = null)
    {
        if (ExPlayerControl.LocalPlayer.TryGetAbility<DeviceCanUseAbility>(out var ability))
        {
            if (!ability.TryUse(deviceType))
            {
                onFailure?.Invoke();
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
public static class MapConsoleUsePatch_DeviceAbility
{
    public static bool Prefix()
    {
        return DeviceAbilityHelper.CheckDeviceUse(DevicesPatch.DeviceType.Admin);
    }
}
[HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
public static class VitalsMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(VitalsMinigame __instance)
    {
        return DeviceAbilityHelper.CheckDeviceUse(DevicesPatch.DeviceType.Vital, __instance.Close);
    }
}
[HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
public static class SurveillanceMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(SurveillanceMinigame __instance)
    {
        return DeviceAbilityHelper.CheckDeviceUse(DevicesPatch.DeviceType.Camera, __instance.Close);
    }
}
[HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
public static class PlanetSurveillanceMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(PlanetSurveillanceMinigame __instance)
    {
        return DeviceAbilityHelper.CheckDeviceUse(DevicesPatch.DeviceType.Camera, __instance.Close);
    }
}
[HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
public static class FungleSurveillanceMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(FungleSurveillanceMinigame __instance)
    {
        return DeviceAbilityHelper.CheckDeviceUse(DevicesPatch.DeviceType.Camera, __instance.Close);
    }
}