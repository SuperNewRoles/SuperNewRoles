using HarmonyLib;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Jackal
    {
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
        class SpawnBot
        {
            public static void Prefix(AmongUsClient __instance)
            {
                if (!ModeHandler.isMode(ModeId.SuperHostRoles)) return;
            }
        }
    }
}
