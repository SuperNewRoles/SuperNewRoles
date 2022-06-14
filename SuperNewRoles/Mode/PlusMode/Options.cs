using SuperNewRoles.CustomOption;
using System;
using System.Collections.Generic;
using System.Text;

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
            PlusModeSetting = CustomOption.CustomOption.Create(235, true, CustomOptionType.Generic, "PlusModeSetting", false, null, isHeader: true);
            NoSabotageModeSetting = CustomOption.CustomOption.Create(238, true, CustomOptionType.Generic, "SettingNoSabotageMode", false, PlusModeSetting);
            NoTaskWinModeSetting = CustomOption.CustomOption.Create(241, true, CustomOptionType.Generic, "SettingNoTaskWinMode", false, PlusModeSetting);
            //FixedSpawnSetting = CustomOption.CustomOption.Create(243, ModTranslation.getString("SettingFixedSpawnMode"), false, PlusModeSetting);
        }
    }
}