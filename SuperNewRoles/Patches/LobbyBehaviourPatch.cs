using HarmonyLib;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(LobbyBehaviour))]
public class LobbyBehaviourPatch
{
    [HarmonyPatch(nameof(LobbyBehaviour.Update)), HarmonyPostfix]
    public static void UpdatePostfix(LobbyBehaviour __instance)
    {
        ISoundPlayer MapThemeSound = SoundManager.Instance.soundPlayers.Find(x => x.Name.Equals("MapTheme"));
        if (ConfigRoles.IsMuteLobbyBGM.Value)
        {
            if (MapThemeSound == null) return;
            SoundManager.Instance.StopNamedSound("MapTheme");
        }
        else
        {
            if (MapThemeSound != null) return;
            SoundManager.Instance.CrossFadeSound("MapTheme", __instance.MapTheme, 0.5f);
        }
    }
}
