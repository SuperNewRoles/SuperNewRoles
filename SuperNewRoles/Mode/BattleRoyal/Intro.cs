using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler(IntroCutscene __instance)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            Teams.Add(PlayerControl.LocalPlayer);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isImpostor() && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                {
                    Teams.Add(p);
                }
            }
            return Teams;
        }
        public static void IntroHandler(IntroCutscene __instance)
        {
            __instance.BackgroundBar.material.color = Color.white;
            __instance.TeamTitle.text = ModTranslation.getString("BattleRoyalModeName");
            __instance.TeamTitle.color = new Color32(116,80,48,byte.MaxValue);
            __instance.ImpostorText.text = "";
        }
    }
}
