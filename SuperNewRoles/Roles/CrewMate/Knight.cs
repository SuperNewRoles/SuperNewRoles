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
        public static CustomOption KnightCanAnnounceOfProtected;
        public static CustomOption KnightSetTheUpperLimitOfTheGuarding;
        public static CustomOption KnightMaximumNumberOfTimes;
        public static void SetupCustomOptions()
        {
            KnightOption = new(OptionId, false, CustomOptionType.Crewmate, "KnightName", color, 1);
            KnightPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], KnightOption);
            KnightCanAnnounceOfProtected = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "KnightCanAnnounceOfProtected", true, KnightOption);
            KnightSetTheUpperLimitOfTheGuarding = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "KnightSetTheUpperLimitOfTheGuarding", false, KnightOption);
            KnightMaximumNumberOfTimes = CustomOption.Create(OptionId + 4, false, CustomOptionType.Crewmate, "KnightMaximumNumberOfTimes", 5f, 0f, 30f, 1f, KnightSetTheUpperLimitOfTheGuarding);
        }

        // RoleClass
        public static List<PlayerControl> Player;
        public static Color32 color = new(229, 228, 230, byte.MaxValue);
        public static bool CanProtect;
        public static float Times;

        public static void ClearAndReload()
        {
            Player = new();
            Times = KnightMaximumNumberOfTimes.GetFloat(); //最大守護回数の取得
            CanProtect = true; //守護を使用可能か

            //CustomOptionからのGetBool()は判定が必要な場所でその都度行う為、ここに入れない。
        }
    }
}