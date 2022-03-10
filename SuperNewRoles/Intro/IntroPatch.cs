using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Intro;
using SuperNewRoles.Roles;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patch;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.HideAndSeek;

namespace SuperNewRoles.Patches
{


    [HarmonyPatch]
    class IntroPatch
    {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor && PlayerControl.LocalPlayer.isNeutral())
            {
                var newTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                newTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = newTeam;
            }
            if (PlayerControl.LocalPlayer.isImpostor())
            {
                var newTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor())
                    {
                        newTeam.Add(p);
                    } else if (p.isRole(CustomRPC.RoleId.Egoist)) {
                        newTeam.Add(p);                    
                    }
                }
                yourTeam = newTeam;
            }
            if (ModeHandler.ModeSetting.getBool())
            {
                var a = ModeHandler.TeamHandler(__instance);
                if (a != new Il2CppSystem.Collections.Generic.List<PlayerControl>())
                {
                    yourTeam = a;
                }
                
            }
            if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && RoleClass.MadMate.IsImpostorCheck)
            {
                Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                int ImpostorNum = 0;
                ImpostorTeams.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.IsImpostor)
                    {
                        ImpostorNum++;
                        ImpostorTeams.Add(player);
                    }
                }
                yourTeam = ImpostorTeams;
            }
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.JackalFriends) && RoleClass.JackalFriends.IsJackalCheck)
            {
                Il2CppSystem.Collections.Generic.List<PlayerControl> JackalTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                int JackalNum = 0;
                JackalTeams.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.isRole(CustomRPC.RoleId.JackalFriends))
                    {
                        JackalNum++;
                        JackalTeams.Add(player);
                    }
                }
                yourTeam = JackalTeams;
            }
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Jackal))
            {
                Il2CppSystem.Collections.Generic.List<PlayerControl> JackalTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                int JackalNum = 0;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.isRole(CustomRPC.RoleId.Jackal))
                    {
                        JackalNum++;
                        JackalTeams.Add(player);
                    }
                }
                yourTeam = JackalTeams;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            Roles.IntroHandler.Handler();
            if (PlayerControl.LocalPlayer.isNeutral())
            {
                IntroDate Intro = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.getRole());
                __instance.BackgroundBar.material.color = Intro.color;
                __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey+"Name");
                __instance.TeamTitle.color = Intro.color;
                __instance.ImpostorText.text = "";
            }
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadMate) && CustomOption.CustomOptions.MadMateIsCheckImpostor.getBool())
            {
                IntroDate Intro = IntroDate.MadMateIntro;
                __instance.BackgroundBar.material.color = Intro.color;
                __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey + "Name");
                __instance.TeamTitle.color = Intro.color;
                __instance.ImpostorText.text = "";
            }
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.JackalFriends) && CustomOption.CustomOptions.JackalFriendsIsCheckJackal.getBool())
            {
                IntroDate Intro = IntroDate.JackalFriendsIntro;
                __instance.BackgroundBar.material.color = Intro.color;
                __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey + "Name");
                __instance.TeamTitle.color = Intro.color;
                __instance.ImpostorText.text = "";
            }
            if (ModeHandler.ModeSetting.getBool())
            {
                ModeHandler.IntroHandler(__instance);
            }
        }
        
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.SetUpRoleText))]
        class SetUpRoleTextPatch
        {
            private static byte ToByteIntro(float f)
            {
                f = Mathf.Clamp01(f);
                return (byte)(f * 255);
            }
            public static void Postfix(IntroCutscene __instance)
            {
                CustomButton.MeetingEndedUpdate();
                if (ModeHandler.isMode(ModeId.Default))
                {
                    var myrole = PlayerControl.LocalPlayer.getRole();
                    if (!(myrole == CustomRPC.RoleId.DefaultRole || myrole == CustomRPC.RoleId.Bestfalsecharge))
                    {
                        var date = Intro.IntroDate.GetIntroDate(myrole);
                        __instance.YouAreText.color = date.color;
                        __instance.RoleText.text = ModTranslation.getString(date.NameKey + "Name");
                        __instance.RoleText.color = date.color;
                        __instance.RoleBlurbText.text = date.TitleDesc;
                        __instance.RoleBlurbText.color = date.color;
                    }

                    if (PlayerControl.LocalPlayer.IsQuarreled())
                    {
                        __instance.RoleBlurbText.text = __instance.RoleBlurbText.text + "\n" + ModHelpers.cs(RoleClass.Quarreled.color, String.Format(ModTranslation.getString("QuarreledIntro"), SetNamesClass.AllNames[PlayerControl.LocalPlayer.GetOneSideQuarreled().PlayerId]));
                    }
                } else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    Mode.SuperHostRoles.Intro.RoleTextHandler(__instance);
                }
                ModeHandler.YouAreIntroHandler(__instance);
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
    }
}