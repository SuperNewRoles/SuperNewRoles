using HarmonyLib;

namespace SuperNewRoles.MapCustoms
{
    class Patch
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        static class ShipStatus_AwakePatch
        {
            static void Postfix(ShipStatus __instance)
            {
                Airship.SecretRoom.ShipStatusAwake(__instance);
            }
        }
    }
}