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
        public static (string, string, Color) IntroHandler(IntroCutscene __instance)
        {
            return (ModTranslation.GetString("ZombieModeName"), "", Main.Zombiecolor);
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
        }
    }
}