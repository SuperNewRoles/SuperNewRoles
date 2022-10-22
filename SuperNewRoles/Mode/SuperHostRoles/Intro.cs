using System;

using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler(IntroCutscene __instance)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new();
            Teams.Add(CachedPlayer.LocalPlayer.PlayerControl);
            try
            {
                if (CachedPlayer.LocalPlayer.PlayerControl.IsCrew())
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.PlayerId != CachedPlayer.LocalPlayer.PlayerId && p.IsPlayer())
                        {
                            Teams.Add(p);
                        }
                    }
                }
                else if (CachedPlayer.LocalPlayer.PlayerControl.IsImpostor())
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if ((p.IsImpostor() || p.IsRole(RoleId.Spy)) && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId && p.IsPlayer())
                        {
                            Teams.Add(p);
                        }
                    }
                }
            }
            catch (Exception e) { SuperNewRolesPlugin.Logger.LogInfo("[SHR:Intro] Intro Error:" + e); }
            return Teams;
        }

        public static void RoleTextHandler(IntroCutscene __instance)
        {
            var myrole = CachedPlayer.LocalPlayer.PlayerControl.GetRole();
            if (myrole is not (RoleId.DefaultRole or RoleId.Bestfalsecharge))
            {
                var date = IntroDate.GetIntroDate(myrole);
                __instance.YouAreText.color = date.color;
                __instance.RoleText.text = ModTranslation.GetString(date.NameKey + "Name");
                __instance.RoleText.color = date.color;
                __instance.RoleBlurbText.text = date.TitleDesc;
                __instance.RoleBlurbText.color = date.color;
            }
            if (CachedPlayer.LocalPlayer.PlayerControl.IsLovers()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Lovers.color, string.Format(ModTranslation.GetString("LoversIntro"), CachedPlayer.LocalPlayer.PlayerControl.GetOneSideLovers()?.GetDefaultName() ?? ""));
            if (CachedPlayer.LocalPlayer.PlayerControl.IsQuarreled()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, string.Format(ModTranslation.GetString("QuarreledIntro"), CachedPlayer.LocalPlayer.PlayerControl.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));
        }
    }
}