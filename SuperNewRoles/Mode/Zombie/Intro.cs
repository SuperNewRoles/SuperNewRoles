using UnityEngine;

namespace SuperNewRoles.Mode.Zombie
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler()
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new();

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
            __instance.BackgroundBar.material.color = Main.Zombiecolor;
            __instance.TeamTitle.text = ModTranslation.GetString("ZombieModeName");
            __instance.TeamTitle.color = Main.Zombiecolor;
            __instance.ImpostorText.text = "";
        }

        public static void YouAreHandle(IntroCutscene __instance)
        {
            Color backcolor = Main.Policecolor;
            string text = ModTranslation.GetString("ZombiePoliceName");
            string desc = ModTranslation.GetString("ZombiePoliceTitle1");
            if (PlayerControl.LocalPlayer.IsZombie())
            {
                text = ModTranslation.GetString("ZombieZombieName");
                desc = ModTranslation.GetString("ZombieZombieTitle1");
                backcolor = Main.Zombiecolor;
            }
            __instance.YouAreText.color = backcolor;
            __instance.RoleText.text = text;
            __instance.RoleText.color = backcolor;
            __instance.RoleBlurbText.text = desc;
            __instance.RoleBlurbText.color = backcolor;
            /**
            if (PlayerControl.LocalPlayer.IsQuarreled())
            {
                __instance.RoleBlurbText.text = __instance.RoleBlurbText.text + "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, String.Format(ModTranslation.GetString("QuarreledIntro"), SetNamesClass.AllNames[PlayerControl.LocalPlayer.GetOneSideQuarreled().PlayerId]));
            }
            */
        }
    }
}