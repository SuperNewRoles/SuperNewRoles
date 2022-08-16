using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Mode.PlusMode
{
    class Options
    {
        public static CustomOption.CustomOption PlusModeSetting;
        public static CustomOption.CustomOption NoSabotageModeSetting;
        public static CustomOption.CustomOption NoTaskWinModeSetting;
        //public static CustomOption.CustomOption FixedSpawnSetting;
        public static void Load()
        {
            PlusModeSetting = CustomOption.CustomOption.Create(508, true, CustomOptionType.Generic, "PlusModeSetting", false, null, isHeader: true);
            NoSabotageModeSetting = CustomOption.CustomOption.Create(509, true, CustomOptionType.Generic, "SettingNoSabotageMode", false, PlusModeSetting);
            NoTaskWinModeSetting = CustomOption.CustomOption.Create(510, true, CustomOptionType.Generic, "SettingNoTaskWinMode", false, PlusModeSetting);
            //FixedSpawnSetting = CustomOption.CustomOption.Create(511, ModTranslation.GetString("SettingFixedSpawnMode"), false, PlusModeSetting);
        }
    }
}