using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(LobbyBehaviour))]
public static class LobbyBehaviourPatch
{
    private const string MapThemeName = "MapTheme";
    private const float MapThemeMaxVolume = 0.07f;
    private static int _frame;

    [HarmonyPatch(nameof(LobbyBehaviour.FixedUpdate))]
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(LobbyBehaviour __instance)
    {
        if (--_frame > 0)
            return;

        _frame = 15;
        ApplyLobbyBgmSetting(__instance);
    }

    public static void ApplyCurrentLobbyBgmSetting()
    {
        if (LobbyBehaviour.Instance == null)
            return;

        ApplyLobbyBgmSetting(LobbyBehaviour.Instance);
    }

    private static void ApplyLobbyBgmSetting(LobbyBehaviour lobby)
    {
        if (lobby == null || ConfigRoles.IsMuteLobbyBGM == null)
            return;

        if (ConfigRoles.IsMuteLobbyBGM.Value)
        {
            if (SoundManager.Instance.HasNamedSound(MapThemeName))
                SoundManager.Instance.StopNamedSound(MapThemeName);

            return;
        }

        if (!SoundManager.Instance.HasNamedSound(MapThemeName))
            SoundManager.Instance.CrossFadeSound(MapThemeName, lobby.MapTheme, MapThemeMaxVolume);
    }
}
