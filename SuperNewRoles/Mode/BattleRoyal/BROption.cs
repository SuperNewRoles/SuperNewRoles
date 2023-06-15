namespace SuperNewRoles.Mode.BattleRoyal;

class BROption
{
    public static CustomOption BattleRoyalMode;
    public static CustomOption IsViewAlivePlayer;
    public static CustomOption StartSeconds;
    public static CustomOption IsTeamBattle;
    public static CustomOption TeamAmount;
    public static CustomOption IsKillCountView;
    public static CustomOption IsKillCountViewSelfOnly;
    public static void Load()
    {
        BattleRoyalMode = CustomOption.Create(101400, true, CustomOptionType.Generic, "SettingBattleRoyalMode", false, ModeHandler.ModeSetting);
        IsViewAlivePlayer = CustomOption.Create(101401, true, CustomOptionType.Generic, "BattleRoyalIsViewAlivePlayer", false, BattleRoyalMode);
        StartSeconds = CustomOption.Create(101402, true, CustomOptionType.Generic, "BattleRoyalStartSeconds", 0f, 0f, 45f, 2.5f, BattleRoyalMode);
        IsTeamBattle = CustomOption.Create(101403, true, CustomOptionType.Generic, "BattleRoyalIsTeamBattle", false, BattleRoyalMode);
        TeamAmount = CustomOption.Create(101404, true, CustomOptionType.Generic, "BattleRoyalTeamAmount", 2f, 2f, 8f, 1f, IsTeamBattle);
        IsKillCountView = CustomOption.Create(101405, true, CustomOptionType.Generic, "BattleRoyalIsKillCountView", true, BattleRoyalMode);
        IsKillCountViewSelfOnly = CustomOption.Create(101406, true, CustomOptionType.Generic, "BattleRoyalIsKillCountViewSelfOnly", false, IsKillCountView);
    }
}