using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches.Harmony;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
class ExilerController_WrapUp
{
    private static void Postfix(PlayerControl __instance)
    {
        var MyRole = PlayerControl.LocalPlayer.GetRole();
        JackalSeer.JackalSeerFixedPatch.Postfix(__instance, MyRole);
    }
}