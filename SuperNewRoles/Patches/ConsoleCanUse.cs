using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public static class ConsoleCanUsePatch
{
    public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;
        __result = 0f;
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return true;
        if (__instance.AllowImpostor) return true;
        ExPlayerControl exPlayer = (ExPlayerControl)pc;

        if (ShouldBypassRoleCanUse(__instance, pc, exPlayer))
        {
            __result = GetTaskConsoleUseDistance(__instance, pc.Object, out canUse, out couldUse);
            return false;
        }

        if (!GameSettingOptions.CannotTaskTrigger) return true;
        if (exPlayer.IsTaskTriggerRole()) return true;
        __result = float.MaxValue;
        return false;
    }

    private static bool ShouldBypassRoleCanUse(Console console, NetworkedPlayerInfo pc, ExPlayerControl exPlayer)
    {
        if (console == null || pc?.Object == null || exPlayer == null) return false;
        if (!exPlayer.IsTaskTriggerRole()) return false;

        return !pc.Object.Data.Role.CanUse(console.TryCast<IUsable>());
    }

    private static float GetTaskConsoleUseDistance(Console console, PlayerControl player, out bool canUse, out bool couldUse)
    {
        canUse = couldUse = false;
        if (console == null || player == null) return float.MaxValue;

        Vector2 truePosition = player.GetTruePosition();
        Vector3 consolePosition = console.transform.position;
        PlayerTask task = console.FindTask(player);

        couldUse = task != null
            && (!console.onlySameRoom || console.InRoom(truePosition))
            && (!console.onlyFromBelow || truePosition.y < consolePosition.y);

        if (!couldUse) return float.MaxValue;

        float distance = Vector2.Distance(truePosition, consolePosition);
        canUse = distance <= console.UsableDistance;
        if (canUse && console.checkWalls)
            canUse = !PhysicsHelpers.AnythingBetween(truePosition, consolePosition, Constants.ShadowMask, false);

        return distance;
    }
}
