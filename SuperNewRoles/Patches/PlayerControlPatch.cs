using HarmonyLib;

namespace SuperNewRoles.Patches;


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckShapeshift))]
class CheckShapeshiftPatch
{
    public static bool Prefix(PlayerControl __instance, PlayerControl target, bool shouldAnimate)
    {
        __instance.logger.Debug($"Checking if {__instance.PlayerId} can shapeshift into {(target == null ? "null player" : target.PlayerId.ToString())}");
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
            return false;
        if (target == null ||
            target.Data == null ||
            __instance.Data.IsDead || __instance.Data.Disconnected)
        {
            int num = target != null ? target.PlayerId : -1;
            __instance.logger.Warning($"Bad shapeshift from {__instance.PlayerId} to {num}");
            __instance.RpcRejectShapeshift();
            return false;
        }
        if (target.IsMushroomMixupActive() && shouldAnimate)
        {
            __instance.logger.Warning("Tried to shapeshift while mushroom mixup was active");
            __instance.RpcRejectShapeshift();
            return false;
        }
        if (MeetingHud.Instance && shouldAnimate)
        {
            __instance.logger.Warning("Tried to shapeshift while a meeting was starting");
            __instance.RpcRejectShapeshift();
            return false;
        }
        __instance.RpcShapeshift(target, shouldAnimate);
        return false;
    }
}
