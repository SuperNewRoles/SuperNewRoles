using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class Intro
    {
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler(IntroCutscene __instance)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            Teams.Add(PlayerControl.LocalPlayer);
            try
            {
                if (PlayerControl.LocalPlayer.isCrew())
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.PlayerId != CachedPlayer.LocalPlayer.PlayerId && p.IsPlayer())
                        {
                            Teams.Add(p);
                        }
                    }
                }
                else if (PlayerControl.LocalPlayer.isImpostor())
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.isImpostor() && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId && p.IsPlayer())
                        {
                            Teams.Add(p);
                        }
                    }
                }
            } catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("イントロエラー:"+e);
            }
            return Teams;
        }
        public static void IntroHandler(IntroCutscene __instance)
        {
            /*
            __instance.BackgroundBar.material.color = Color.white;
            __instance.TeamTitle.text = ModTranslation.getString("BattleRoyalModeName");
            __instance.TeamTitle.color = new Color32(116,80,48,byte.MaxValue);
            __instance.ImpostorText.text = "";
            */
        }
        public static void RoleTextHandler(IntroCutscene __instance)
        {
            var myrole = PlayerControl.LocalPlayer.getRole();
            if (!(myrole == CustomRPC.RoleId.DefaultRole || myrole == CustomRPC.RoleId.Bestfalsecharge))
            {
                var date = SuperNewRoles.Intro.IntroDate.GetIntroDate(myrole);
                __instance.YouAreText.color = date.color;
                __instance.RoleText.text = ModTranslation.getString(date.NameKey + "Name");
                __instance.RoleText.color = date.color;
                __instance.RoleBlurbText.text = date.TitleDesc;
                __instance.RoleBlurbText.color = date.color;
            }
            if (PlayerControl.LocalPlayer.IsLovers())
            {
                __instance.RoleBlurbText.text += "\n" + ModHelpers.cs(RoleClass.Lovers.color, string.Format(ModTranslation.getString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.getDefaultName() ?? ""));
            }
            if (PlayerControl.LocalPlayer.IsQuarreled())
            {
                __instance.RoleBlurbText.text += "\n" + ModHelpers.cs(RoleClass.Quarreled.color, string.Format(ModTranslation.getString("QuarreledIntro"), PlayerControl.LocalPlayer.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));
            }
            /**

            if (PlayerControl.LocalPlayer.IsQuarreled())
            {
                __instance.RoleBlurbText.text = __instance.RoleBlurbText.text + "\n" + ModHelpers.cs(RoleClass.Quarreled.color, String.Format(ModTranslation.getString("QuarreledIntro"), SetNamesClass.AllNames[PlayerControl.LocalPlayer.GetOneSideQuarreled().PlayerId]));
            }
            */
        }
    }
}
