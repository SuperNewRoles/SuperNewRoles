using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor.MadRole;

public static class MadRaccoon
{
    internal static class CustomOptionData
    {
        private static int optionId = 1321;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.MadRaccoon); optionId++;
            PlayerCount = CustomOption.Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option); optionId++;
        }
    }

    internal static class RoleClass
    {
        public static List<PlayerControl> Player;
        public static Color32 color = Roles.RoleClass.ImpostorRed;
        public static void ClearAndReload()
        {
            Player = new();
        }
    }
}