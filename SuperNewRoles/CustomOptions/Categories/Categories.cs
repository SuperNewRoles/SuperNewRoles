using SuperNewRoles.Modules;
using SuperNewRoles.WaveCannonObj;

namespace SuperNewRoles.CustomOptions.Categories;

public static class Categories
{
    public static CustomOptionCategory PresetSettings;
    public static CustomOptionCategory GeneralSettings;
    public static CustomOptionCategory ModeSettings;
    public static CustomOptionCategory GameSettings;
    public static CustomOptionCategory MapSettings;
    public static CustomOptionCategory MapEditSettings;

    [CustomOptionSelect("ModeOption", typeof(ModeId), "ModeId.", parentFieldName: nameof(ModeSettings))]
    public static ModeId ModeOption;

    // バトルロワイヤルモード設定
    [CustomOptionBool("WaveCannonBattleRoyalTeamMode", false, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(ModeOption), parentActiveValue: ModeId.BattleRoyal)]
    public static bool WaveCannonBattleRoyalTeamMode;

    [CustomOptionInt("WaveCannonBattleRoyalTeamCount", 2, 10, 1, 2, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(WaveCannonBattleRoyalTeamMode), parentActiveValue: true)]
    public static int WaveCannonBattleRoyalTeamCount;

    [CustomOptionBool("WaveCannonBattleRoyalFriendlyFire", true, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(WaveCannonBattleRoyalTeamMode), parentActiveValue: true)]
    public static bool WaveCannonBattleRoyalFriendlyFire;

    [CustomOptionFloat("WaveCannonBattleRoyalCooldown", 2.5f, 60f, 2.5f, 15f, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(ModeOption), parentActiveValue: ModeId.BattleRoyal)]
    public static float WaveCannonBattleRoyalCooldown;

    [CustomOptionFloat("WaveCannonBattleRoyalDuration", 0f, 10f, 0.5f, 3f, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(ModeOption), parentActiveValue: ModeId.BattleRoyal)]
    public static float WaveCannonBattleRoyalDuration;
}
