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
        public static CustomOption.CustomOption IsViewAlivePlayer;
        public static CustomOption.CustomOption StartSeconds;
        public static void Load()
        {
            BattleRoyalMode = CustomOption.CustomOption.Create(131, true, CustomOptionType.Generic, "SettingBattleRoyalMode", false, ModeHandler.ModeSetting);
            IsViewAlivePlayer = CustomOption.CustomOption.Create(352, true, CustomOptionType.Generic, "BattleRoyalIsViewAlivePlayer", false, BattleRoyalMode);
            StartSeconds = CustomOption.CustomOption.Create(353, true, CustomOptionType.Generic, "BattleRoyalStartSeconds", 0f,0f,45f,2.5f, BattleRoyalMode);
        }
    }
}
