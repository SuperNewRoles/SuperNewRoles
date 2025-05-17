using HarmonyLib;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
public static class OverlayKillAnimationInitializePatch
{
    public static void Postfix(OverlayKillAnimation __instance)
    {
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
