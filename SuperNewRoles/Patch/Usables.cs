using HarmonyLib;
using InnerNet;

namespace SuperNewRoles.Patch
{
    class Usables
    {
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
        class OnPlayerLeftPatch
        {
            public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                {
                }
            }
        }

    }
}