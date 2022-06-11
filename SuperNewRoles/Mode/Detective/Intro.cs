using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.Detective
{
    class Intro
    {
        public static void YouAreHandle(IntroCutscene __instance)
        {
            if (main.DetectivePlayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                __instance.YouAreText.color = main.DetectiveColor;
                __instance.RoleText.text = ModTranslation.getString("DetectiveName");
                __instance.RoleText.color = main.DetectiveColor;
                __instance.RoleBlurbText.text = ModTranslation.getString("DetectiveTitle1");
                __instance.RoleBlurbText.color = main.DetectiveColor;
            }
        }
    }
}
