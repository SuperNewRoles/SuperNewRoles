using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    public class SetNamesClass
    {
        public static void SetPlayerNameColor(PlayerControl p, Color color)
        {
            p.nameText.color = color;
        }
        public static void SetPlayerName(PlayerControl p,string name)
        {
            p.nameText = (TMPro.TextMeshPro)name;
        }
        public static void SetPlayerNameColors(PlayerControl player)
        {
            var role = player.getRole();
            if (role == CustomRPC.RoleId.DefaultRole) return;
            SetPlayerNameColor(player, Intro.IntroDate.GetIntroDate(role).color);
        }
        public static void SetPlayerRoleNames(PlayerControl player)
        {
            var role = player.getRole();
            if (role == CustomRPC.RoleId.DefaultRole)
            {
                if (player.Data.Role.IsImpostor)
                {
                    SetPlayerName(player, ModTranslation.getString("ImpostorName") + "\n" + player.name);
                } else
                {
                    SetPlayerName(player, ModTranslation.getString("CrewMateName") + "\n" + player.name);
                }
            }
            else
            {
                SetPlayerName(player, ModTranslation.getString(Intro.IntroDate.GetIntroDate(role).NameKey + "Name") + "\n" + player.name);
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    SetNamesClass.SetPlayerRoleNames(player);
                    SetNamesClass.SetPlayerNameColors(player);
                }
            }
            else
            {
                SetNamesClass.SetPlayerRoleNames(PlayerControl.LocalPlayer);
                SetNamesClass.SetPlayerNameColors(PlayerControl.LocalPlayer);
            }
        }
    }
}
