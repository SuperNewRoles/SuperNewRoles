using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
class AmongUsClientStartPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        ExPlayerControl.SetUpExPlayers();
    }
}