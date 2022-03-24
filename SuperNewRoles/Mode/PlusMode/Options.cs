using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.PlusMode
{
    class Options
    {
        public static CustomOption.CustomOption PlusModeSetting;
        public static CustomOption.CustomOption NoSabotageModeSetting;
        public static void Load()
        {
            PlusModeSetting = CustomOption.CustomOption.Create(235, ModTranslation.getString("PlusModeSetting"), false, null,isHeader:true);
            NoSabotageModeSetting = CustomOption.CustomOption.Create(236, ModTranslation.getString("SettingNoSabotageMode"), false, PlusModeSetting);
        }
    }
}
