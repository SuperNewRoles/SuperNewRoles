using HarmonyLib;

namespace SuperNewRoles.Patch.Harmony
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    class ShipStatusAwakePatch
    {
        private static void Postfix()
        {
            SuperNewRoles.Modules.ProctedMessager.Init();
        }
    }
}
