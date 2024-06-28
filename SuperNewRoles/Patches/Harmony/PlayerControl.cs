using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches.Harmony;

//キルされたとき実行！
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
class PlayerControl_MurderPlayer
{
    private static void Postfix(PlayerControl __instance)
    {
        var MyRole = PlayerControl.LocalPlayer.GetRole();
        JackalSeer.JackalSeerFixedPatch.Postfix(__instance, MyRole);
    }
}