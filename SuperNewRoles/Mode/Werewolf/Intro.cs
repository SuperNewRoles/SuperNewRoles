using SuperNewRoles.Roles;
using UnityEngine;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Mode.Werewolf
{
    class Intro
    {
        public static void YouAreHandle(IntroCutscene __instance)
        {
            if (main.HunterPlayers.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
            {
                __instance.YouAreText.color = Color.green;
                __instance.RoleText.text = ModTranslation.getString("WareWolfHunter");
                __instance.RoleText.color = Color.green;
                __instance.RoleBlurbText.text = ModTranslation.getString("WereWolfHunterText");
                __instance.RoleBlurbText.color = Color.green;
            }
            else if (PlayerControl.LocalPlayer.isImpostor())
            {
                __instance.YouAreText.color = RoleClass.ImpostorRed;
                __instance.RoleText.text = ModTranslation.getString("WareWolfImpostor");
                __instance.RoleText.color = RoleClass.ImpostorRed;
                __instance.RoleBlurbText.text = ModTranslation.getString("WereWolfImpostorText");
                __instance.RoleBlurbText.color = RoleClass.ImpostorRed;
            }
            else if (PlayerControl.LocalPlayer.isRole(RoleId.MadMate))
            {
                __instance.YouAreText.color = RoleClass.ImpostorRed;
                __instance.RoleText.text = ModTranslation.getString("WareWolfMadmate");
                __instance.RoleText.color = RoleClass.ImpostorRed;
                __instance.RoleBlurbText.text = ModTranslation.getString("WereWolfMadmateText");
                __instance.RoleBlurbText.color = RoleClass.ImpostorRed;
            }
            else if (PlayerControl.LocalPlayer.isRole(RoleId.DefaultRole))
            {
                __instance.RoleText.text = ModTranslation.getString("WareWolfCrewmate");
                __instance.RoleBlurbText.text = ModTranslation.getString("WereWolfCrewmateText");
            }
            else if (PlayerControl.LocalPlayer.isRole(RoleId.SoothSayer))
            {
                __instance.YouAreText.color = RoleClass.SoothSayer.color;
                __instance.RoleText.text = ModTranslation.getString("WareWolfFortuneTeller");
                __instance.RoleText.color = RoleClass.SoothSayer.color;
                __instance.RoleBlurbText.text = ModTranslation.getString("WereWolfFortuneTellerText");
                __instance.RoleBlurbText.color = RoleClass.SoothSayer.color;
            }
            else if (PlayerControl.LocalPlayer.isRole(RoleId.SoothSayer))
            {
                __instance.YouAreText.color = RoleClass.SpiritMedium.color;
                __instance.RoleText.text = ModTranslation.getString("WareWolfMedium");
                __instance.RoleText.color = RoleClass.SpiritMedium.color;
                __instance.RoleBlurbText.text = ModTranslation.getString("WereWolfMediumText");
                __instance.RoleBlurbText.color = RoleClass.SpiritMedium.color;
            }
        }
    }
}
