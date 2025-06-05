using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public static class ConsoleCanUsePatch
{
    public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;
        __result = 0f;
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return true;
        if (!GameSettingOptions.CannotTaskTrigger) return true;
        if (__instance.AllowImpostor) return true;
        ExPlayerControl exPlayer = (ExPlayerControl)pc;
        if (exPlayer.IsTaskTriggerRole()) return true;
        __result = float.MaxValue;
        return false;
    }
}