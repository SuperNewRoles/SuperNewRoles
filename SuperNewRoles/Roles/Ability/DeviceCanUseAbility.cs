using System;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

[Flags]
public enum DeviceTypeFlag
{
    None = 1 << 0,
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

[HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
public static class MapConsoleUsePatch_DeviceAbility
{
    public static bool Prefix()
    {
        if (ExPlayerControl.LocalPlayer.TryGetAbility<DeviceCanUseAbility>(out var ability))
        {
            if (!ability.TryUse(DevicesPatch.DeviceType.Admin))
            {
                return false;
            }
        }
        return true;
    }
}
[HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
public static class VitalsMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(VitalsMinigame __instance)
    {
        if (ExPlayerControl.LocalPlayer.TryGetAbility<DeviceCanUseAbility>(out var ability))
        {
            if (!ability.TryUse(DevicesPatch.DeviceType.Vital))
            {
                __instance.Close();
                return false;
            }
        }
        return true;
    }
}
[HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
public static class SurveillanceMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(SurveillanceMinigame __instance)
    {
        if (ExPlayerControl.LocalPlayer.TryGetAbility<DeviceCanUseAbility>(out var ability))
        {
            if (!ability.TryUse(DevicesPatch.DeviceType.Camera))
            {
                __instance.Close();
                return false;
            }
        }
        return true;
    }
}
[HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
public static class PlanetSurveillanceMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(PlanetSurveillanceMinigame __instance)
    {
        if (ExPlayerControl.LocalPlayer.TryGetAbility<DeviceCanUseAbility>(out var ability))
        {
            if (!ability.TryUse(DevicesPatch.DeviceType.Camera))
            {
                __instance.Close();
                return false;
            }
        }
        return true;
    }
}
[HarmonyPatch(typeof(FungleSurveillanceMinigame), nameof(FungleSurveillanceMinigame.Begin))]
public static class FungleSurveillanceMinigameBeginPatch_DeviceAbility
{
    public static bool Prefix(FungleSurveillanceMinigame __instance)
    {
        if (ExPlayerControl.LocalPlayer.TryGetAbility<DeviceCanUseAbility>(out var ability))
        {
            if (!ability.TryUse(DevicesPatch.DeviceType.Camera))
            {
                __instance.Close();
                return false;
            }
        }
        return true;
    }
}