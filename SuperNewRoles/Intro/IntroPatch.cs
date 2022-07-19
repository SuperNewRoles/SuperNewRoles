using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch]
    class IntroPatch
    {
        public static void SetupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                var newTeam2 = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                newTeam2.Add(PlayerControl.LocalPlayer);
                yourTeam = newTeam2;
                if (PlayerControl.LocalPlayer.IsCrew())
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
                    switch (PlayerControl.LocalPlayer.GetRole())
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
                                if ((player.IsImpostor() || player.IsRole(RoleId.Spy)) && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
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
                                if (player.IsRole(RoleId.Fox))
                                {
                                    FoxNum++;
                                    FoxTeams.Add(player);
                                }
                            }
                            yourTeam = FoxTeams;
                            break;
                        default:
                            if (PlayerControl.LocalPlayer.IsImpostor())
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

        public static void SetupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            IntroHandler.Handler();
            Color32 color = new(127, 127, 127, byte.MaxValue);
            if (ModeHandler.IsMode(ModeId.Default, ModeId.SuperHostRoles))
            {
                if (PlayerControl.LocalPlayer.IsNeutral())
                {
                    IntroDate Intro = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.GetRole());
                    __instance.BackgroundBar.material.color = color;
                    __instance.TeamTitle.text = ModTranslation.GetString("Neutral");
                    __instance.TeamTitle.color = color;
                    __instance.ImpostorText.text = ModTranslation.GetString("NeutralSubIntro");
                }
                else
                {
                    switch (PlayerControl.LocalPlayer.GetRole())
                    {
                        case RoleId.MadMate:
                        case RoleId.MadJester:
                        case RoleId.MadStuntMan:
                        case RoleId.MadMayor:
                        case RoleId.MadHawk:
                        case RoleId.MadSeer:
                        case RoleId.MadMaker:
                        case RoleId.BlackCat:
                        case RoleId.JackalFriends:
                        case RoleId.SeerFriends:
                        case RoleId.MayorFriends:
                            IntroDate Intro = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.GetRole());
                            __instance.BackgroundBar.material.color = Intro.color;
                            __instance.TeamTitle.text = ModTranslation.GetString(Intro.NameKey + "Name");
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
            public static void Prefix()
            {
                float SetTime = 0;
                bool Flag = true;
                switch (PlayerControl.LocalPlayer.GetRole())
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
                    if (ModeHandler.IsMode(ModeId.Default))
                    {
                        var myrole = PlayerControl.LocalPlayer.GetRole();
                        if (myrole is not (RoleId.DefaultRole or RoleId.Bestfalsecharge))
                        {
                            var date = IntroDate.GetIntroDate(myrole);
                            __instance.YouAreText.color = date.color;
                            __instance.RoleText.text = ModTranslation.GetString(date.NameKey + "Name");
                            __instance.RoleText.color = date.color;
                            __instance.RoleBlurbText.text = date.TitleDesc;
                            __instance.RoleBlurbText.color = date.color;
                        }
                        if (PlayerControl.LocalPlayer.IsLovers())
                        {
                            __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Lovers.color, string.Format(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? ""));
                        }
                        if (PlayerControl.LocalPlayer.IsQuarreled())
                        {
                            __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, string.Format(ModTranslation.GetString("QuarreledIntro"), PlayerControl.LocalPlayer.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));
                        }
                    }
                    else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
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
                SetupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                SetupIntroTeam(__instance, ref teamToDisplay);
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                SetupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                SetupIntroTeam(__instance, ref yourTeam);
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