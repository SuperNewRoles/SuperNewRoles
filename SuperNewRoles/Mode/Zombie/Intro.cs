using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.Zombie
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler(IntroCutscene __instance)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

            if (PlayerControl.LocalPlayer.IsZombie())
            {
                Teams.Add(PlayerControl.LocalPlayer);
            }
            else
            {
                Teams.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        Teams.Add(p);
                    }
                }
            }
            return Teams;
        }
        public static void IntroHandler(IntroCutscene __instance)
        {
            __instance.BackgroundBar.material.color = main.Zombiecolor;
            __instance.TeamTitle.text = ModTranslation.getString("ZombieModeName");
            __instance.TeamTitle.color = main.Zombiecolor;
            __instance.ImpostorText.text = "";
        }

        public static void YouAreHandle(IntroCutscene __instance)
        {
            Color backcolor = main.Policecolor;
            string text = ModTranslation.getString("ZombiePoliceName");
            string desc = ModTranslation.getString("ZombiePoliceTitle1");
            if (PlayerControl.LocalPlayer.IsZombie())
            {
                text = ModTranslation.getString("ZombieZombieName");
                desc = ModTranslation.getString("ZombieZombieTitle1");
                backcolor = main.Zombiecolor;
            }
            __instance.YouAreText.color = backcolor;
            __instance.RoleText.text = text;
            __instance.RoleText.color = backcolor;
            __instance.RoleBlurbText.text = desc;
            __instance.RoleBlurbText.color = backcolor;
            /**

            if (PlayerControl.LocalPlayer.IsQuarreled())
            {
                __instance.RoleBlurbText.text = __instance.RoleBlurbText.text + "\n" + ModHelpers.cs(RoleClass.Quarreled.color, String.Format(ModTranslation.getString("QuarreledIntro"), SetNamesClass.AllNames[PlayerControl.LocalPlayer.GetOneSideQuarreled().PlayerId]));
            }
            */
        }
    }
}
