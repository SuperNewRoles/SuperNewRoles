using HarmonyLib;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Sabotage
{
    class Patch
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
        class OpenMeetingPatch
        {
            public static void Prefix(HudManager __instance)
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
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
        [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
        class EmergencyUpdatePatch
        {
            public static void Postfix(EmergencyMinigame __instance)
            {
                if (!SabotageManager.IsOKMeeting())
                {
                    __instance.state = 2;
                    __instance.ButtonActive = false;
                    __instance.StatusText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyDuringCrisis);
                    __instance.NumberText.text = string.Empty;
                    __instance.ClosedLid.gameObject.SetActive(true);
                    __instance.OpenLid.gameObject.SetActive(false);
                }
            }
        }
    }
}