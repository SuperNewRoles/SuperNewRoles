using HarmonyLib;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Sabotage;

class Patch
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
    class OpenMeetingPatch
    {
        public static void Prefix(HudManager __instance)
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
            {
                p.resetChange();
            }
        }
    }
    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.Update))]
    class SetUpCustomButton
    {
        public static void Postfix(InfectedOverlay __instance)
        {
            SabotageManager.InfectedOverlayInstance = __instance;
        }
    }
    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.Start))]
    class SetUpCustomSabotageButton
    {
        public static void Postfix(InfectedOverlay __instance)
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                CognitiveDeficit.Main.Create(__instance);
            }
        }
    }
}