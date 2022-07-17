using HarmonyLib;

//参考=>https://github.com/Koke1024/Town-Of-Moss/blob/main/TownOfMoss/Patches/MeltDownBoost.cs

namespace SuperNewRoles.Patch
{
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
        public static void Prefix(HeliSabotageSystem __instance, float deltaTime)
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