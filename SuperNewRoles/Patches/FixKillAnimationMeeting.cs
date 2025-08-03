using HarmonyLib;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
public static class OverlayKillAnimationInitializePatch
{
    public static void Postfix(OverlayKillAnimation __instance)
    {
        // fix visor
        CustomCosmeticsLayer victimLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.victimParts.cosmetics);
        victimLayer.visor1.gameObject.SetActive(false);
        victimLayer.visor2.gameObject.SetActive(false);
        CustomCosmeticsLayer killerLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.killerParts.cosmetics);
        killerLayer.visor1.gameObject.SetActive(false);
        killerLayer.visor2.gameObject.SetActive(false);
        if (MeetingHud.Instance == null) return;
        ModHelpers.UpdateMeetingHudMaskAreas(false);
    }
}
[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.OnDestroy))]
public static class OverlayKillAnimationOnDestroyPatch
{
    public static void Postfix(OverlayKillAnimation __instance)
    {
        if (MeetingHud.Instance == null) return;
        ModHelpers.UpdateMeetingHudMaskAreas(true);
    }
}
