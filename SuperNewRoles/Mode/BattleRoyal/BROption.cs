using SuperNewRoles.CustomOption;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class BROption
    {
        public static CustomOption.CustomOption BattleRoyalMode;
        public static void Load()
        {
            BattleRoyalMode = CustomOption.CustomOption.Create(131, true, CustomOptionType.Generic, "SettingBattleRoyalMode", false, ModeHandler.ModeSetting);
           
        }
    }
}
