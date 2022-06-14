using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomOption;
using UnityEngine;

namespace SuperNewRoles.Mode.HideAndSeek
{
    public class ZombieOptions
    {
        public static CustomOption.CustomOption HideAndSeekMode;
        public static CustomOption.CustomOption HASDeathTask;
        public static CustomOption.CustomOption HASUseSabo;
        public static CustomOption.CustomOption HASUseVent;
        public static void Load()
        {
            HideAndSeekMode = CustomOption.CustomOption.Create(101, true, CustomOptionType.Generic, "SettingHideAndSeekMode", false, ModeHandler.ModeSetting);
            HASDeathTask = CustomOption.CustomOption.Create(128, true, CustomOptionType.Generic, "HASDeathTaskSetting", false, HideAndSeekMode);
            HASUseSabo = CustomOption.CustomOption.Create(129, true, CustomOptionType.Generic, "HASUseSaboSetting", false, HideAndSeekMode);
            HASUseVent = CustomOption.CustomOption.Create(130, true, CustomOptionType.Generic, "HASUseVentSetting", false, HideAndSeekMode);
        }
    }
}