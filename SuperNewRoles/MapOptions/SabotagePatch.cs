using HarmonyLib;

//参考=>https://github.com/Koke1024/Town-Of-Moss/blob/main/TownOfMoss/Patches/MeltDownBoost.cs

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Detoriorate))]
    public static class MeltdownBooster
    {
        public static void Prefix(ReactorSystemType __instance, float deltaTime)
        {
            if (!__instance.IsActive)
            {
                return;
            }
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Pb)
            {
                if (__instance.Countdown >= MapOptions.MapOption.PolusReactorTimeLimit.getFloat())
                {
                    __instance.Countdown = MapOptions.MapOption.PolusReactorTimeLimit.getFloat();
                }
                return;
            }
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq)
            {
                if (__instance.Countdown >= MapOptions.MapOption.MiraReactorTimeLimit.getFloat())
                {
                    __instance.Countdown = MapOptions.MapOption.MiraReactorTimeLimit.getFloat();
                }
                return;
            }
            return;
        }
    }

    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Detoriorate))]
    public static class HeliMeltdownBooster
    {
        public static void Prefix(HeliSabotageSystem __instance, float deltaTime)
        {
            if (!__instance.IsActive)
            {
                return;
            }

            if (AirshipStatus.Instance != null)
            {
                if (__instance.Countdown >= MapOptions.MapOption.AirshipReactorTimeLimit.getFloat())
                {
                    __instance.Countdown = MapOptions.MapOption.AirshipReactorTimeLimit.getFloat();
                }
            }
        }
    }
}