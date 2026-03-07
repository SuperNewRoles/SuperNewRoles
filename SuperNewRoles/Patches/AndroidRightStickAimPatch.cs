using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AdjustLighting))]
public static class AndroidRightStickAimAdjustLightingPatch
{
    public static bool Prefix()
    {
        return !AndroidRightStickAim.ShouldSuppressAdjustLighting();
    }
}
