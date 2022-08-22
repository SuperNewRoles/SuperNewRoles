namespace SuperNewRoles.Mode.Detective
{
    class Intro
    {
        public static void YouAreHandle(IntroCutscene __instance)
        {
            if (Main.DetectivePlayer.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                __instance.YouAreText.color = Main.DetectiveColor;
                __instance.RoleText.text = ModTranslation.GetString("DetectiveName");
                __instance.RoleText.color = Main.DetectiveColor;
                __instance.RoleBlurbText.text = ModTranslation.GetString("DetectiveTitle1");
                __instance.RoleBlurbText.color = Main.DetectiveColor;
            }
        }
    }
}