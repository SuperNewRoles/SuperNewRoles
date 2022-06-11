using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.HideAndSeek
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler(IntroCutscene __instance) {
            Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            int ImpostorNum = 0;
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (player.Data.Role.IsImpostor)
                {
                    ImpostorNum++;
                    ImpostorTeams.Add(player);
                }
            }
            return ImpostorTeams;
        }
        public static void IntroHandler(IntroCutscene __instance) {
            Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            int ImpostorNum = 0;
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (player.Data.Role.IsImpostor)
                {
                    ImpostorNum++;
                    ImpostorTeams.Add(player);
                }
            }
            __instance.BackgroundBar.material.color = Color.white;
            __instance.TeamTitle.text = ModTranslation.getString("HideAndSeekModeName");
            __instance.TeamTitle.color = Color.yellow;
            __instance.ImpostorText.text = string.Format("この{0}人が鬼だ。", ImpostorNum.ToString());
            __instance.ImpostorText.color = Color.yellow;
        }
    }
}
