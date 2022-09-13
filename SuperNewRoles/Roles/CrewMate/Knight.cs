using SuperNewRoles.Patch;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Roles.RoleClass;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate
{
    public static class Knight
    {
        public const int OptionId = 992;// 設定のId
        // CustomOptionDate
        public static CustomRoleOption KnightOption;
        public static CustomOption KnightPlayerCount;

        public static void SetupCustomOptions()
        {
            KnightOption = new(OptionId, false, CustomOptionType.Crewmate, "KnightName", color, 1);
            KnightPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], KnightOption);
        }

        // RoleClass
        public static List<PlayerControl> Player;
        public static Color32 color = new(229, 228, 230, byte.MaxValue);

        public static void ClearAndReload()
        {
            Player = new();
        }
    }
}