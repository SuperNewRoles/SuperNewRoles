using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class BROption
    {
        public static CustomOption.CustomOption BattleRoyalMode;
        public static CustomOption.CustomOption IsViewAlivePlayer;
        public static CustomOption.CustomOption StartSeconds;
        public static CustomOption.CustomOption IsTeamBattle;
        public static CustomOption.CustomOption TeamAmount;
        public static void Load()
        {
            BattleRoyalMode = CustomOption.CustomOption.Create(479, true, CustomOptionType.Generic, "SettingBattleRoyalMode", false, ModeHandler.ModeSetting);
            IsViewAlivePlayer = CustomOption.CustomOption.Create(480, true, CustomOptionType.Generic, "BattleRoyalIsViewAlivePlayer", false, BattleRoyalMode);
            StartSeconds = CustomOption.CustomOption.Create(481, true, CustomOptionType.Generic, "BattleRoyalStartSeconds", 0f, 0f, 45f, 2.5f, BattleRoyalMode);
            IsTeamBattle = CustomOption.CustomOption.Create(482, true, CustomOptionType.Generic, "BattleRoyalIsTeamBattle", false, BattleRoyalMode);
            TeamAmount = CustomOption.CustomOption.Create(483, true, CustomOptionType.Generic, "BattleRoyalTeamAmount", 2f, 2f, 8f, 1f, IsTeamBattle);
        }
    }
}