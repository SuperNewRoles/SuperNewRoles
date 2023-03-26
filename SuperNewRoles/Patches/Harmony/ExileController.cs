using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches.Harmony;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
class ExilerController_WrapUp
{
    private static void Postfix(PlayerControl __instance)
    {
        var MyRole = PlayerControl.LocalPlayer.GetRole();
        Jackal.JackalFixedPatch.Postfix(__instance, MyRole);
        JackalSeer.JackalSeerFixedPatch.Postfix(__instance, MyRole);
        Roles.Neutral.WaveCannonJackal.WaveCannonJackalFixedPatch.Postfix(__instance, MyRole);
    }
}