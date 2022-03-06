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
            HideAndSeekMode = CustomOption.CustomOption.Create(101, CustomOptions.cs(Color.white, "SettingHideAndSeekMode"), false, ModeHandler.ModeSetting);
            HASDeathTask = CustomOption.CustomOption.Create(128, CustomOptions.cs(Color.white, "HASDeathTaskSetting"), false, HideAndSeekMode);
            HASUseSabo = CustomOption.CustomOption.Create(129, CustomOptions.cs(Color.white, "HASUseSaboSetting"), false, HideAndSeekMode);
            HASUseVent = CustomOption.CustomOption.Create(130, CustomOptions.cs(Color.white, "HASUseVentSetting"), false, HideAndSeekMode);
        }
    }
}
