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
        public static CustomOption.CustomOption ZombieWasImpostor;
        public static void Load()
        {
            ZombieMode = CustomOption.CustomOption.Create(195, CustomOptions.cs(Color.white, "SettingZombieMode"), false, ModeHandler.ModeSetting);
            ZombieWasImpostor = CustomOption.CustomOption.Create(196, CustomOptions.cs(Color.white, "HASDeathTaskSetting"), true, ZombieMode);
        }
    }
}
