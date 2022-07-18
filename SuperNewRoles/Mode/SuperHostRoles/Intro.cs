using System;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler(IntroCutscene __instance)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new();
            Teams.Add(PlayerControl.LocalPlayer);
            try
            {
                if (PlayerControl.LocalPlayer.IsCrew())
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.PlayerId != CachedPlayer.LocalPlayer.PlayerId && p.IsPlayer())
                        {
                            Teams.Add(p);
                        }
                    }
                }
                else if (PlayerControl.LocalPlayer.IsImpostor())
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
        public static void IntroHandler()
        {
            /*
            __instance.BackgroundBar.material.color = Color.white;
            __instance.TeamTitle.text = ModTranslation.GetString("BattleRoyalModeName");
            __instance.TeamTitle.color = new Color32(116,80,48,byte.MaxValue);
            __instance.ImpostorText.text = "";
            */
        }
        public static void RoleTextHandler(IntroCutscene __instance)
        {
            var myrole = PlayerControl.LocalPlayer.GetRole();
            if (myrole is not (RoleId.DefaultRole or RoleId.Bestfalsecharge))
            {
                var date = SuperNewRoles.Intro.IntroDate.GetIntroDate(myrole);
                __instance.YouAreText.color = date.color;
                __instance.RoleText.text = ModTranslation.GetString(date.NameKey + "Name");
                __instance.RoleText.color = date.color;
                __instance.RoleBlurbText.text = date.TitleDesc;
                __instance.RoleBlurbText.color = date.color;
            }
            if (PlayerControl.LocalPlayer.IsLovers()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Lovers.color, string.Format(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.GetDefaultName() ?? ""));
            if (PlayerControl.LocalPlayer.IsQuarreled()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, string.Format(ModTranslation.GetString("QuarreledIntro"), PlayerControl.LocalPlayer.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));
        }
    }
}