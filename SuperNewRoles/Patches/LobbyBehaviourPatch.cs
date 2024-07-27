using System;
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
            bool MapThemeSound = SoundManager.Instance.soundPlayers.Any(x => x.Name.Equals("MapTheme"));
            if (ConfigRoles.IsMuteLobbyBGM.Value)
            {
                if (!MapThemeSound) return;
                SoundManager.Instance.StopNamedSound("MapTheme");
            }
            else
            {
                if (MapThemeSound) return;
                SoundManager.Instance.CrossFadeSound("MapTheme", __instance.MapTheme, 0.5f);
            }
            Frame = 15;
        }
    }
}
