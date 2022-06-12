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
using System.Collections;
using TMPro;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch]
    class IntroPatch
    {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            if (ModeHandler.isMode(ModeId.Default))
            {
                var newTeam2 = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                newTeam2.Add(PlayerControl.LocalPlayer);
                yourTeam = newTeam2;
                if (PlayerControl.LocalPlayer.isCrew())
                {
                    var newTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    newTeam.Add(PlayerControl.LocalPlayer);
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                        {
                            newTeam.Add(p);
                        }
                    }
                    yourTeam = newTeam;
                }
                if (PlayerControl.LocalPlayer.isImpostor())
                {
                    var newTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.isImpostor())
                        {
                            newTeam.Add(p);
                        }
                        else if (p.isRole(CustomRPC.RoleId.Egoist))
                        {
                            newTeam.Add(p);
                        }
                    }
                    yourTeam = newTeam;
                }
                if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && Madmate.CheckImpostor(PlayerControl.LocalPlayer))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    int ImpostorNum = 0;
                    ImpostorTeams.Add(PlayerControl.LocalPlayer);
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.Data.Role.IsImpostor)
                        {
                            ImpostorNum++;
                            ImpostorTeams.Add(player);
                        }
                    }
                    yourTeam = ImpostorTeams;
                }
                if (RoleClass.MadMayor.MadMayorPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    int ImpostorNum = 0;
                    ImpostorTeams.Add(PlayerControl.LocalPlayer);
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.Data.Role.IsImpostor)
                        {
                            ImpostorNum++;
                            ImpostorTeams.Add(player);
                        }
                    }
                    yourTeam = ImpostorTeams;
                }
                if (RoleClass.MadJester.MadJesterPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && MadJester.CheckImpostor(PlayerControl.LocalPlayer))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    int ImpostorNum = 0;
                    ImpostorTeams.Add(PlayerControl.LocalPlayer);
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.Data.Role.IsImpostor)
                        {
                            ImpostorNum++;
                            ImpostorTeams.Add(player);
                        }
                    }
                    yourTeam = ImpostorTeams;
                }
                if (RoleClass.MadSeer.MadSeerPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && MadSeer.CheckImpostor(PlayerControl.LocalPlayer))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    int ImpostorNum = 0;
                    ImpostorTeams.Add(PlayerControl.LocalPlayer);
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.Data.Role.IsImpostor)
                        {
                            ImpostorNum++;
                            ImpostorTeams.Add(player);
                        }
                    }
                    yourTeam = ImpostorTeams;
                }
                if ((PlayerControl.LocalPlayer.isRole(RoleId.JackalFriends) ||
                    PlayerControl.LocalPlayer.isRole(RoleId.SeerFriends) || PlayerControl.LocalPlayer.isRole(RoleId.MayorFriends)) && JackalFriends.CheckJackal(PlayerControl.LocalPlayer))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerControl> JackalTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    int JackalNum = 0;
                    JackalTeams.Add(PlayerControl.LocalPlayer);
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.isRole(CustomRPC.RoleId.JackalFriends) || player.isRole(CustomRPC.RoleId.MayorFriends))
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
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.isRole(CustomRPC.RoleId.Jackal))
                        {
                            JackalNum++;
                            JackalTeams.Add(player);
                        }
                    }
                    yourTeam = JackalTeams;
                }
                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.JackalSeer))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerControl> JackalTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    int JackalNum = 0;
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.isRole(CustomRPC.RoleId.JackalSeer))
                        {
                            JackalNum++;
                            JackalTeams.Add(player);
                        }
                    }
                    yourTeam = JackalTeams;
                }
                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Fox))
                {
                    Il2CppSystem.Collections.Generic.List<PlayerControl> FoxTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    int FoxNum = 0;
                    foreach (PlayerControl player in CachedPlayer.AllPlayers)
                    {
                        if (player.isRole(CustomRPC.RoleId.Fox))
                        {
                            FoxNum++;
                            FoxTeams.Add(player);
                        }
                    }
                    yourTeam = FoxTeams;
                }
            } else
            {
                var a = ModeHandler.TeamHandler(__instance);
                if (a != new Il2CppSystem.Collections.Generic.List<PlayerControl>())
                {
                    yourTeam = a;
                } else
                {
                    var temp = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                    temp.Add(PlayerControl.LocalPlayer);
                    yourTeam = temp;
                }
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            Roles.IntroHandler.Handler();
            if (ModeHandler.isMode(ModeId.Default) || ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (PlayerControl.LocalPlayer.isNeutral())
                {
                    IntroDate Intro = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.getRole());
                    __instance.BackgroundBar.material.color = Intro.color;
                    __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey + "Name");
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
                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.SeerFriends) && CustomOption.CustomOptions.SeerFriendsIsCheckJackal.getBool())
                {
                    IntroDate Intro = IntroDate.SeerFriendsIntro;
                    __instance.BackgroundBar.material.color = Intro.color;
                    __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey + "Name");
                    __instance.TeamTitle.color = Intro.color;
                    __instance.ImpostorText.text = "";
                }
                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MayorFriends) && CustomOption.CustomOptions.MayorFriendsIsCheckJackal.getBool())
                {
                    IntroDate Intro = IntroDate.MayorFriendsIntro;
                    __instance.BackgroundBar.material.color = Intro.color;
                    __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey + "Name");
                    __instance.TeamTitle.color = Intro.color;
                    __instance.ImpostorText.text = "";
                }
            }
            else
            {
                ModeHandler.IntroHandler(__instance);
            }
            //SetUpRoleTextPatch.Postfix(__instance);
        }



        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        class IntroCutsceneDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {
                float SetTime = 0;
                bool Flag = true;
                switch (PlayerControl.LocalPlayer.getRole())
                {
                    case RoleId.DarkKiller:
                        SetTime = RoleClass.DarkKiller.KillCoolTime;
                        break;
                    case RoleId.Minimalist:
                        SetTime = RoleClass.Minimalist.KillCoolTime;
                        break;
                    case RoleId.Samurai:
                        SetTime = RoleClass.Samurai.KillCoolTime;
                        break;
                    case RoleId.HomeSecurityGuard:
                        foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        {
                            task.Complete();
                        }
                        Flag = false;
                        break;
                    default:
                        Flag = false;
                        break;
                }
                if (Flag)
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(SetTime);
                }
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene),nameof(IntroCutscene.ShowRole))]
        class SetUpRoleTextPatch
        {
            private static byte ToByteIntro(float f)
            {
                f = Mathf.Clamp01(f);
                return (byte)(f * 255);
            }
            public static void Prefix(IntroCutscene __instance)
            {
                new LateTask(() =>
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
                        if (PlayerControl.LocalPlayer.IsLovers())
                        {
                            __instance.RoleBlurbText.text += "\n" + ModHelpers.cs(RoleClass.Lovers.color, string.Format(ModTranslation.getString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? ""));
                        }
                        if (PlayerControl.LocalPlayer.IsQuarreled())
                        {
                            __instance.RoleBlurbText.text += "\n" + ModHelpers.cs(RoleClass.Quarreled.color, string.Format(ModTranslation.getString("QuarreledIntro"), PlayerControl.LocalPlayer.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));
                        }
                    }
                    else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        Mode.SuperHostRoles.Intro.RoleTextHandler(__instance);
                    }
                    ModeHandler.YouAreIntroHandler(__instance);
                }, 0f, "Override Role Text");
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeam(__instance, ref teamToDisplay);
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

        [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
        public static class ShouldAlwaysHorseAround
        {
            public static bool isHorseMode;
            public static bool Prefix(ref bool __result)
            {
                if (isHorseMode != HorseModeOption.enableHorseMode && LobbyBehaviour.Instance != null) __result = isHorseMode;
                else
                {
                    __result = HorseModeOption.enableHorseMode;
                    isHorseMode = HorseModeOption.enableHorseMode;
                }
                return false;
            }
        }
    }
}