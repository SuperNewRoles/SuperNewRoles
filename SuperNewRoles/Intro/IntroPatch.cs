using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.HideAndSeek;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

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
                else
                {
                    switch (PlayerControl.LocalPlayer.getRole())
                    {
                        case RoleId.MadMate:
                        case RoleId.MadMayor:
                        case RoleId.MadJester:
                        case RoleId.MadSeer:
                        case RoleId.BlackCat:
                            if (Madmate.CheckImpostor(PlayerControl.LocalPlayer)) break;
                            ImpostorIntroTeam:
                            Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new();
                            ImpostorTeams.Add(PlayerControl.LocalPlayer);
                            foreach (PlayerControl player in CachedPlayer.AllPlayers)
                            {
                                if ((player.isImpostor() || player.isRole(RoleId.Spy)) && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                                {
                                    ImpostorTeams.Add(player);
                                }
                            }
                            yourTeam = ImpostorTeams;
                            break;
                        case RoleId.SeerFriends:
                        case RoleId.MayorFriends:
                        case RoleId.JackalFriends:
                            if (JackalFriends.CheckJackal(PlayerControl.LocalPlayer)) break;
                            JackalIntroTeam:
                            Il2CppSystem.Collections.Generic.List<PlayerControl> JackalTeams = new();
                            JackalTeams.Add(PlayerControl.LocalPlayer);
                            foreach (PlayerControl player in CachedPlayer.AllPlayers)
                            {
                                if (player.IsJackalTeamJackal() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                                {
                                    JackalTeams.Add(player);
                                }
                            }
                            yourTeam = JackalTeams;
                            break;
                        case RoleId.Fox:
                            Il2CppSystem.Collections.Generic.List<PlayerControl> FoxTeams = new();
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
                            break;
                        default:
                            if (PlayerControl.LocalPlayer.isImpostor())
                            {
                                goto ImpostorIntroTeam;
                            }
                            else if (PlayerControl.LocalPlayer.IsJackalTeamJackal())
                            {
                                goto JackalIntroTeam;
                            }
                            break;
                    }
                }
            }
            else
            {
                var temp = ModeHandler.TeamHandler(__instance);
                if (temp != new Il2CppSystem.Collections.Generic.List<PlayerControl>())
                {
                    yourTeam = temp;
                }
                else
                {
                    temp = new();
                    temp.Add(PlayerControl.LocalPlayer);
                    yourTeam = temp;
                }
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            IntroHandler.Handler();
            if (ModeHandler.isMode(ModeId.Default, ModeId.SuperHostRoles))
            {
                if (PlayerControl.LocalPlayer.isNeutral())
                {
                    IntroDate Intro = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.getRole());
                    __instance.BackgroundBar.material.color = Intro.color;
                    __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey + "Name");
                    __instance.TeamTitle.color = Intro.color;
                    __instance.ImpostorText.text = "";
                }
                else
                {
                    switch (PlayerControl.LocalPlayer.getRole())
                    {
                        case RoleId.MadMate:
                        case RoleId.MadJester:
                        case RoleId.MadStuntMan:
                        case RoleId.MadMayor:
                        case RoleId.SeerFriends:
                        case RoleId.MayorFriends:
                            IntroDate Intro = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.getRole());
                            __instance.BackgroundBar.material.color = Intro.color;
                            __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey + "Name");
                            __instance.TeamTitle.color = Intro.color;
                            __instance.ImpostorText.text = "";
                            break;
                    }
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

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
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
                        if (myrole is not (CustomRPC.RoleId.DefaultRole or CustomRPC.RoleId.Bestfalsecharge))
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
