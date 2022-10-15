using SuperNewRoles.Patches;

namespace SuperNewRoles.Mode.PlusMode
{
    class Options
    {
        public static CustomOption PlusModeSetting;
        public static CustomOption NoSabotageModeSetting;
        public static CustomOption NoTaskWinModeSetting;
        //public static CustomOption FixedSpawnSetting;
        public static void Load()
        {
            PlusModeSetting = CustomOption.Create(508, true, CustomOptionType.Generic, "PlusModeSetting", false, null, isHeader: true);
            NoSabotageModeSetting = CustomOption.Create(509, true, CustomOptionType.Generic, "SettingNoSabotageMode", false, PlusModeSetting);
            NoTaskWinModeSetting = CustomOption.Create(510, true, CustomOptionType.Generic, "SettingNoTaskWinMode", false, PlusModeSetting);
            //FixedSpawnSetting = CustomOption.Create(511, ModTranslation.GetString("SettingFixedSpawnMode"), false, PlusModeSetting);
        }
    }
}