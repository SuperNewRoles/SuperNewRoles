using System;
using HarmonyLib;

//参考=>https://github.com/Koke1024/Town-Of-Moss/blob/main/TownOfMoss/Patches/MeltDownBoost.cs

namespace SuperNewRoles.Patches;

public static class ElectricPatch
{
    public static void Reset()
    {
        onTask = false;
    }
    public static bool onTask = false;
    public static bool done = false;
    public static DateTime lastUpdate;

    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
    class VitalsMinigameStartPatch
    {
        static void Postfix(VitalsMinigame __instance)
        {
            onTask = true;
            done = false;
        }
    }
    [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.FixedUpdate))]
    class SwitchMinigameClosePatch
    {
        static void Postfix(SwitchMinigame __instance)
        {
            lastUpdate = DateTime.UtcNow;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
            {
                if (p == 1f)
                {
                    float diff = (float)(DateTime.UtcNow - lastUpdate).TotalMilliseconds;
                    if (diff > 100 && !done)
                    {
                        done = true;
                        onTask = false;
                    }
                }
            })));
        }
    }
}
[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.Deteriorate))]
public static class LifeSuppBooster
{
    public static void Prefix(LifeSuppSystemType __instance, float deltaTime)
    {
        if (MapOption.MapOption.IsReactorDurationSetting)
        {
            if (!__instance.IsActive)
            {
                return;
            }
            switch (MapUtilities.CachedShipStatus.Type)
            {
                case ShipStatus.MapType.Ship when __instance.Countdown >= MapOption.MapOption.SkeldLifeSuppTimeLimit.GetFloat():
                    __instance.Countdown = MapOption.MapOption.SkeldLifeSuppTimeLimit.GetFloat();
                    return;
                case ShipStatus.MapType.Hq when __instance.Countdown >= MapOption.MapOption.MiraLifeSuppTimeLimit.GetFloat():
                    __instance.Countdown = MapOption.MapOption.MiraLifeSuppTimeLimit.GetFloat();
                    return;
                default:
                    return;
            }
        }
    }
}
[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Deteriorate))]
public static class MeltdownBooster
{
    public static void Prefix(ReactorSystemType __instance, float deltaTime)
    {
        if (MapOption.MapOption.IsReactorDurationSetting)
        {
            if (!__instance.IsActive)
            {
                return;
            }
            switch (MapUtilities.CachedShipStatus.Type)
            {
                case ShipStatus.MapType.Ship when __instance.Countdown >= MapOption.MapOption.SkeldReactorTimeLimit.GetFloat():
                    __instance.Countdown = MapOption.MapOption.SkeldReactorTimeLimit.GetFloat();
                    return;
                case ShipStatus.MapType.Hq when __instance.Countdown >= MapOption.MapOption.MiraReactorTimeLimit.GetFloat():
                    __instance.Countdown = MapOption.MapOption.MiraReactorTimeLimit.GetFloat();
                    return;
                case ShipStatus.MapType.Pb when __instance.Countdown >= MapOption.MapOption.PolusReactorTimeLimit.GetFloat():
                    __instance.Countdown = MapOption.MapOption.PolusReactorTimeLimit.GetFloat();
                    return;
                case ShipStatus.MapType.Fungle when __instance.Countdown >= MapOption.MapOption.FungleReactorTimeLimit.GetFloat():
                    __instance.Countdown = MapOption.MapOption.FungleReactorTimeLimit.GetFloat();
                    return;
                default:
                    return;
            }
        }
    }
}

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Deteriorate))]
public static class HeliMeltdownBooster
{
    public static void Prefix(HeliSabotageSystem __instance)
    {
        if (MapOption.MapOption.IsReactorDurationSetting)
        {
            if (!__instance.IsActive)
            {
                return;
            }

            if (MapUtilities.CachedShipStatus != null)
            {
                if (__instance.Countdown >= MapOption.MapOption.AirshipReactorTimeLimit.GetFloat())
                {
                    __instance.Countdown = MapOption.MapOption.AirshipReactorTimeLimit.GetFloat();
                }
            }
        }
    }
}