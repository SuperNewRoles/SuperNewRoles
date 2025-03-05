using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Deteriorate))]
public static class ReactorDurationPatch
{
    public static void Prefix(ReactorSystemType __instance, float deltaTime)
    {
        if (!MapSettingOptions.ReactorDurationOption) return;

        if (!__instance.IsActive) return;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship when __instance.Countdown >= MapSettingOptions.SkeldReactorTimeLimit:
                __instance.Countdown = MapSettingOptions.SkeldReactorTimeLimit;
                return;
            case ShipStatus.MapType.Hq when __instance.Countdown >= MapSettingOptions.MiraReactorTimeLimit:
                __instance.Countdown = MapSettingOptions.MiraReactorTimeLimit;
                return;
            case ShipStatus.MapType.Pb when __instance.Countdown >= MapSettingOptions.PolusReactorTimeLimit:
                __instance.Countdown = MapSettingOptions.PolusReactorTimeLimit;
                return;
            case ShipStatus.MapType.Fungle when __instance.Countdown >= MapSettingOptions.FungleReactorTimeLimit:
                __instance.Countdown = MapSettingOptions.FungleReactorTimeLimit;
                return;
            default:
                return;
        }
    }
}

[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.Deteriorate))]
public static class LifeSuppDurationPatch
{
    public static void Prefix(LifeSuppSystemType __instance, float deltaTime)
    {
        if (!MapSettingOptions.ReactorDurationOption) return;

        if (!__instance.IsActive) return;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship when __instance.Countdown >= MapSettingOptions.SkeldLifeSuppTimeLimit:
                __instance.Countdown = MapSettingOptions.SkeldLifeSuppTimeLimit;
                return;
            case ShipStatus.MapType.Hq when __instance.Countdown >= MapSettingOptions.MiraLifeSuppTimeLimit:
                __instance.Countdown = MapSettingOptions.MiraLifeSuppTimeLimit;
                return;
            default:
                return;
        }
    }
}

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Deteriorate))]
public static class HeliSabotageDurationPatch
{
    public static void Prefix(HeliSabotageSystem __instance, float deltaTime)
    {
        if (!MapSettingOptions.ReactorDurationOption) return;

        if (!__instance.IsActive) return;

        if (ShipStatus.Instance.Type == ShipStatus.MapType.Ship && __instance.Countdown >= MapSettingOptions.AirshipReactorTimeLimit)
        {
            __instance.Countdown = MapSettingOptions.AirshipReactorTimeLimit;
        }
    }
}