using SuperNewRoles.Patch;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class BROption
    {
        public static CustomOption BattleRoyalMode;
        public static CustomOption IsViewAlivePlayer;
        public static CustomOption StartSeconds;
        public static CustomOption IsTeamBattle;
        public static CustomOption TeamAmount;
        public static void Load()
        {
            BattleRoyalMode = CustomOption.Create(479, true, CustomOptionType.Generic, "SettingBattleRoyalMode", false, ModeHandler.ModeSetting);
            IsViewAlivePlayer = CustomOption.Create(480, true, CustomOptionType.Generic, "BattleRoyalIsViewAlivePlayer", false, BattleRoyalMode);
            StartSeconds = CustomOption.Create(481, true, CustomOptionType.Generic, "BattleRoyalStartSeconds", 0f, 0f, 45f, 2.5f, BattleRoyalMode);
            IsTeamBattle = CustomOption.Create(482, true, CustomOptionType.Generic, "BattleRoyalIsTeamBattle", false, BattleRoyalMode);
            TeamAmount = CustomOption.Create(483, true, CustomOptionType.Generic, "BattleRoyalTeamAmount", 2f, 2f, 8f, 1f, IsTeamBattle);
        }
    }
}