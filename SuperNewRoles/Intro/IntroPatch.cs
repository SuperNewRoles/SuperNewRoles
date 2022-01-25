using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SuperNewRoles.Patches
{


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
            PlayerControl.LocalPlayer.setRole(CustomRPC.RoleId.MeetingSheriff);
            var RoleDate = PlayerControl.LocalPlayer.getRole();
            if (RoleDate == CustomRPC.RoleId.DefaultRole) return;
            var date= Intro.IntroDate.GetIntroDate(RoleDate);
            __instance.RoleText.text = ModTranslation.getString(date.NameKey+"Name");
            __instance.RoleText.color = date.color;
            __instance.RoleBlurbText.text = Intro.IntroDate.GetTitle(date.NameKey,date.TitleNum);
            __instance.RoleBlurbText.color = date.color;
        }
    }

}