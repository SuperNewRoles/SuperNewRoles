using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Intro;
using SuperNewRoles.Roles;
using SuperNewRoles.Buttons;

namespace SuperNewRoles.Patches
{


    [HarmonyPatch]
    class IntroPatch
    {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            SuperNewRolesPlugin.Logger.LogInfo("Seted");
            if (PlayerControl.LocalPlayer.isNeutral())
            {
                IntroDate Intro = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.getRole());
                __instance.BackgroundBar.material.color = Intro.color;
                __instance.TeamTitle.text = ModTranslation.getString(Intro.NameKey+"Name");
                __instance.TeamTitle.color = Intro.color;
            }
            if (CustomOption.CustomOptions.HideAndSeekMode.getBool())
            {
                Il2CppSystem.Collections.Generic.List<PlayerControl> ImpostorTeams = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                int ImpostorNum = 0;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.IsImpostor)
                    {
                        ImpostorNum++;
                        ImpostorTeams.Add(player);
                    }
                }
                yourTeam = ImpostorTeams;
                __instance.BackgroundBar.material.color = Color.white;
                __instance.TeamTitle.text = ModTranslation.getString("HideAndSeekModeName");
                __instance.TeamTitle.color = Color.yellow;
                __instance.ImpostorText.text = string.Format("この{0}人が鬼だ。",ImpostorNum.ToString());
                __instance.ImpostorText.color = Color.yellow;
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
                var RoleDate = PlayerControl.LocalPlayer.getRole();
                SuperNewRolesPlugin.Logger.LogInfo(RoleDate);
                CustomButton.MeetingEndedUpdate();
                if (RoleDate == CustomRPC.RoleId.DefaultRole) return;
                var date = Intro.IntroDate.GetIntroDate(RoleDate);
                __instance.YouAreText.color = date.color;
                __instance.RoleText.text = ModTranslation.getString(date.NameKey + "Name");
                __instance.RoleText.color = date.color;
                __instance.RoleBlurbText.text = Intro.IntroDate.GetTitle(date.NameKey, date.TitleNum);
                __instance.RoleBlurbText.color = date.color;
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
        [HarmonyPatch(typeof(GameStartManager),nameof(GameStartManager.BeginGame))]
        class RoleResetClass
        {
            public static void Postfix(GameStartManager __instance)
            {
                RoleClass.clearAndReloadRoles();
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