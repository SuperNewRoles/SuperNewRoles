using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Crook
{
    public static class CustomOptionData
    {
        private static int optionId = 303600;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.Crook); optionId++;
            PlayerCount = CustomOption.Create(optionId, true, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option); optionId++;
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = new(96, 161, 189, byte.MaxValue);

        public static void ClearAndReload()
        {
            Player = new();
        }
    }


    // ここにコードを書きこんでください
}