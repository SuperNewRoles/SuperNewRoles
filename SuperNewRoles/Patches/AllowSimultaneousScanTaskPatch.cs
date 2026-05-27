using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

public static class AllowSimultaneousScanTaskPatch
{
    private static bool IsEnabled()
    {
        if (!MapSettingOptions.AllowSimultaneousScanTask) return false;

        var options = GameOptionsManager.Instance?.CurrentGameOptions;
        if (options == null) return false;

        var map = (MapNames)options.MapId;
        return map is MapNames.Skeld or MapNames.Polus;
    }

    private static void SetLocalPlayerAsCurrentUser(MedScanMinigame minigame)
    {
        if (minigame?.medscan == null || PlayerControl.LocalPlayer == null) return;

        minigame.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
    }

    private static void ClearLocalPlayerAsCurrentUser(MedScanMinigame minigame)
    {
        if (minigame?.medscan == null || PlayerControl.LocalPlayer == null) return;

        if (minigame.medscan.CurrentUser == PlayerControl.LocalPlayer.PlayerId)
            minigame.medscan.CurrentUser = MedScanSystem.NoPlayer;
    }

    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.Begin))]
    private static class MedScanMinigameBeginPatch
    {
        public static void Postfix(MedScanMinigame __instance)
        {
            if (!IsEnabled() || __instance?.medscan == null || PlayerControl.LocalPlayer == null) return;

            if (__instance.walking != null)
            {
                __instance.StopCoroutine(__instance.walking);
                __instance.walking = null;
            }

            __instance.state = MedScanMinigame.PositionState.WalkingToPad;
            __instance.walking = __instance.StartCoroutine(__instance.WalkToPad());
        }
    }

    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
    private static class MedScanMinigameFixedUpdatePatch
    {
        public static void Prefix(MedScanMinigame __instance)
        {
            if (!IsEnabled() || __instance?.medscan == null || PlayerControl.LocalPlayer == null) return;
            if (__instance.MyNormTask != null && __instance.MyNormTask.IsComplete) return;
            if (__instance.walking != null || __instance.state != MedScanMinigame.PositionState.WalkingToPad) return;

            SetLocalPlayerAsCurrentUser(__instance);
        }
    }

    [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.Close))]
    private static class MedScanMinigameClosePatch
    {
        public static void Postfix(MedScanMinigame __instance)
        {
            if (!IsEnabled() || __instance?.medscan == null) return;

            ClearLocalPlayerAsCurrentUser(__instance);
        }
    }
}
