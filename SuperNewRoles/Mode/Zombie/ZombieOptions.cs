using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomOption;
using UnityEngine;

namespace SuperNewRoles.Mode.Zombie
{
    public class ZombieOptions
    {
        public static CustomOption.CustomOption ZombieMode;
        public static CustomOption.CustomOption ZombieLight;
        public static void Load()
        {
            ZombieMode = CustomOption.CustomOption.Create(195, true, CustomOptionType.Generic, CustomOptions.cs(Color.white, "SettingZombieMode"), false, ModeHandler.ModeSetting);
            ZombieLight = CustomOption.CustomOption.Create(196, true, CustomOptionType.Generic, CustomOptions.cs(Color.white, "SettingZombieLight"), 0.25f, 0f, 5f, 0.25f, ZombieMode);
        }
    }
}
