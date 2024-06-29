using HarmonyLib;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(LobbyBehaviour))]
public class LobbyBehaviourPatch
{
    public static int Frame = 0;

    [HarmonyPatch(nameof(LobbyBehaviour.FixedUpdate)), HarmonyPostfix]
    public static void UpdatePostfix(LobbyBehaviour __instance)
    {
        if (--Frame <= 0)
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
            Frame = 15;
        }
    }
}
