using SuperNewRoles.CustomOption;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.Werewolf
{
    class WerewolfOptions
    {
        public static CustomOption.CustomOption WerewolfMode;
        public static CustomOption.CustomOption WerewolfHunterOption;
        public static void Load()
        {
            WerewolfMode = CustomOption.CustomOption.Create(229, true, CustomOptionType.Generic, "SettingWerewolfMode", false, ModeHandler.ModeSetting);
            WerewolfHunterOption = CustomOption.CustomOption.Create(230, true, CustomOptionType.Generic, "HunterName", CustomOptions.rates, WerewolfMode);
        }
    }
}