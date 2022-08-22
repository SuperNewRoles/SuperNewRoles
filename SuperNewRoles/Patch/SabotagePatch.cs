using System;
using HarmonyLib;

//参考=>https://github.com/Koke1024/Town-Of-Moss/blob/main/TownOfMoss/Patches/MeltDownBoost.cs

namespace SuperNewRoles.Patch
{
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
                ElectricPatch.done = false;
            }
        }
        [HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.FixedUpdate))]
        class SwitchMinigameClosePatch
        {
            static void Postfix(SwitchMinigame __instance)
            {
                ElectricPatch.lastUpdate = DateTime.UtcNow;
                DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
                {
                    if (p == 1f)
                    {
                        float diff = (float)((DateTime.UtcNow - lastUpdate).TotalMilliseconds);
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
    [HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Detoriorate))]
    public static class MeltdownBooster
    {
        public static void Prefix(ReactorSystemType __instance, float deltaTime)
        {
            if (MapOptions.MapOption.ReactorDurationOption.GetBool())
            {
                if (!__instance.IsActive)
                {
                    return;
                }
                if (MapUtilities.CachedShipStatus.Type == ShipStatus.MapType.Pb)
                {
                    if (__instance.Countdown >= MapOptions.MapOption.PolusReactorTimeLimit.GetFloat())
                    {
                        __instance.Countdown = MapOptions.MapOption.PolusReactorTimeLimit.GetFloat();
                    }
                    return;
                }
                if (MapUtilities.CachedShipStatus.Type == ShipStatus.MapType.Hq)
                {
                    if (__instance.Countdown >= MapOptions.MapOption.MiraReactorTimeLimit.GetFloat())
                    {
                        __instance.Countdown = MapOptions.MapOption.MiraReactorTimeLimit.GetFloat();
                    }
                    return;
                }
                return;
            }
        }
    }

    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Detoriorate))]
    public static class HeliMeltdownBooster
    {
        public static void Prefix(HeliSabotageSystem __instance)
        {
            if (MapOptions.MapOption.ReactorDurationOption.GetBool())
            {
                if (!__instance.IsActive)
                {
                    return;
                }

                if (MapUtilities.CachedShipStatus != null)
                {
                    if (__instance.Countdown >= MapOptions.MapOption.AirshipReactorTimeLimit.GetFloat())
                    {
                        __instance.Countdown = MapOptions.MapOption.AirshipReactorTimeLimit.GetFloat();
                    }
                }
            }
        }
    }
}