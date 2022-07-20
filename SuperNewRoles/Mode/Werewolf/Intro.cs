using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.Werewolf
{
    class Intro
    {
        public static void YouAreHandle(IntroCutscene __instance)
        {
            if (Main.HunterPlayers.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
            {
                __instance.YouAreText.color = Color.green;
                __instance.RoleText.text = ModTranslation.GetString("WareWolfHunter");
                __instance.RoleText.color = Color.green;
                __instance.RoleBlurbText.text = ModTranslation.GetString("WereWolfHunterText");
                __instance.RoleBlurbText.color = Color.green;
            }
            else if (PlayerControl.LocalPlayer.IsImpostor())
            {
                __instance.YouAreText.color = RoleClass.ImpostorRed;
                __instance.RoleText.text = ModTranslation.GetString("WareWolfImpostor");
                __instance.RoleText.color = RoleClass.ImpostorRed;
                __instance.RoleBlurbText.text = ModTranslation.GetString("WereWolfImpostorText");
                __instance.RoleBlurbText.color = RoleClass.ImpostorRed;
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.MadMate))
            {
                __instance.YouAreText.color = RoleClass.ImpostorRed;
                __instance.RoleText.text = ModTranslation.GetString("WareWolfMadmate");
                __instance.RoleText.color = RoleClass.ImpostorRed;
                __instance.RoleBlurbText.text = ModTranslation.GetString("WereWolfMadmateText");
                __instance.RoleBlurbText.color = RoleClass.ImpostorRed;
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.DefaultRole))
            {
                __instance.RoleText.text = ModTranslation.GetString("WareWolfCrewmate");
                __instance.RoleBlurbText.text = ModTranslation.GetString("WereWolfCrewmateText");
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.SoothSayer))
            {
                __instance.YouAreText.color = RoleClass.SoothSayer.color;
                __instance.RoleText.text = ModTranslation.GetString("WareWolfFortuneTeller");
                __instance.RoleText.color = RoleClass.SoothSayer.color;
                __instance.RoleBlurbText.text = ModTranslation.GetString("WereWolfFortuneTellerText");
                __instance.RoleBlurbText.color = RoleClass.SoothSayer.color;
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.SoothSayer))
            {
                __instance.YouAreText.color = RoleClass.SpiritMedium.color;
                __instance.RoleText.text = ModTranslation.GetString("WareWolfMedium");
                __instance.RoleText.color = RoleClass.SpiritMedium.color;
                __instance.RoleBlurbText.text = ModTranslation.GetString("WereWolfMediumText");
                __instance.RoleBlurbText.color = RoleClass.SpiritMedium.color;
            }
        }
    }
}