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
        public static void Load()
        {
            PlusModeSetting = CustomOption.CustomOption.Create(235, ModTranslation.getString("PlusModeSetting"), false, null,isHeader:true);
            NoSabotageModeSetting = CustomOption.CustomOption.Create(238, ModTranslation.getString("SettingNoSabotageMode"), false, PlusModeSetting);
            NoTaskWinModeSetting = CustomOption.CustomOption.Create(241, ModTranslation.getString("SettingNoTaskWinMode"), false, PlusModeSetting);
        }
    }
}
