using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches.Harmony
{
    //キルされたとき実行！
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    class PlayerControl_MurderPlayer
    {
        private static void Postfix(PlayerControl __instance)
        {
            RoleId MyRole = PlayerControl.LocalPlayer.GetRole();
            Jackal.JackalFixedPatch.Postfix(__instance, MyRole);
            JackalSeer.JackalSeerFixedPatch.Postfix(__instance, MyRole);
        }
    }
}