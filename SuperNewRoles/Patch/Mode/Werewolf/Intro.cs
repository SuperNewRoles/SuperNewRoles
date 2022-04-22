using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.Werewolf
{
    class Intro
    {
        public static void YouAreHandle(IntroCutscene __instance)
        {
            if (main.HunterPlayers.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
            {
                __instance.YouAreText.color = Color.green;
                __instance.RoleText.text = "ハンター";
                __instance.RoleText.color = Color.green;
                __instance.RoleBlurbText.text = "誰かを道連れにしよう";
                __instance.RoleBlurbText.color = Color.green;
            } else if (PlayerControl.LocalPlayer.isImpostor())
            {
                __instance.YouAreText.color = RoleClass.ImpostorRed;
                __instance.RoleText.text = "人狼";
                __instance.RoleText.color = RoleClass.ImpostorRed;
                __instance.RoleBlurbText.text = "市民を食べて勝利しよう";
                __instance.RoleBlurbText.color = RoleClass.ImpostorRed;
            }
            else if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadMate))
            {
                __instance.YouAreText.color = RoleClass.ImpostorRed;
                __instance.RoleText.text = "狂人";
                __instance.RoleText.color = RoleClass.ImpostorRed;
                __instance.RoleBlurbText.text = "人狼を助けて勝利しよう";
                __instance.RoleBlurbText.color = RoleClass.ImpostorRed;
            }
            else if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.DefaultRole))
            {
                __instance.RoleText.text = "市民";
                __instance.RoleBlurbText.text = "議論で人狼を追放して勝利しよう";
            }
            else if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.SoothSayer))
            {
                __instance.YouAreText.color = RoleClass.SoothSayer.color;
                __instance.RoleText.text = "占い師";
                __instance.RoleText.color = RoleClass.SoothSayer.color;
                __instance.RoleBlurbText.text = "占って人狼を発見して勝利しよう";
                __instance.RoleBlurbText.color = RoleClass.SoothSayer.color;
            }
            else if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.SoothSayer))
            {
                __instance.YouAreText.color = RoleClass.SpiritMedium.color;
                __instance.RoleText.text = "霊媒師";
                __instance.RoleText.color = RoleClass.SpiritMedium.color;
                __instance.RoleBlurbText.text = "死んだ人の役職を調べて勝利しよう";
                __instance.RoleBlurbText.color = RoleClass.SpiritMedium.color;
            }
        }
    }
}
