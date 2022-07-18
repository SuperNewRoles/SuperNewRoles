using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patch.Harmony
{
    //追放されたとき実行！
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    class AirShipExileController_WrapUpAndSpawn
    {
        private static void Postfix()//PlayerControl __instance)
        {
            //
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class ExilerController_WrapUp
    {
        private static void Postfix(PlayerControl __instance)
        {
            var MyRole = PlayerControl.LocalPlayer.GetRole();
            Jackal.JackalFixedPatch.Postfix(__instance, MyRole);
            JackalSeer.JackalSeerFixedPatch.Postfix(__instance, MyRole);
        }
    }
}