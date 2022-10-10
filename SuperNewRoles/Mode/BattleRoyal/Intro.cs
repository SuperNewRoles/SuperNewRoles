using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler()
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new();
            Teams.Add(PlayerControl.LocalPlayer);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsImpostor() && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                {
                    Teams.Add(p);
                }
            }
            return Teams;
        }
        public static (string, string, Color) IntroHandler(IntroCutscene __instance)
        {
            return (ModTranslation.GetString("BattleRoyalModeName"), "", new Color32(116, 80, 48, byte.MaxValue));
        }
    }
}