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
            BattleRoyalMode = CustomOption.CustomOption.Create(131, CustomOptions.cs(Color.white, "SettingBattleRoyalMode"), false, ModeHandler.ModeSetting);
           
        }
    }
}
