using SuperNewRoles.MapCustoms;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public static class MapEditSettingsOptions
{
    // |:========== 配線タスクランダムの設定 ==========:|
    [CustomOptionBool("WireTaskIsRandom", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool WireTaskIsRandom;

    [CustomOptionInt("WireTaskNum", 1, 8, 1, 5, parentFieldName: nameof(WireTaskIsRandom))]
    public static int WireTaskNum;

    // |:========== (AirShip)ランダムスポーンの設定 ==========:|
    [CustomOptionBool("AirshipRandomSpawn", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool AirshipRandomSpawn;

    // |:========== (AirShip)アーカイブアドミン封印の設定 ==========:|
    [CustomOptionBool("RecordsAdminDestroy", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool RecordsAdminDestroy;

    // |:========== (AirShip)壁越しタスク禁止の設定 ==========:|
    [CustomOptionBool("AntiTaskOverWall", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool AntiTaskOverWall;

    // |:========== (AirShip)昇降機の影変更の設定 ==========:|
    [CustomOptionBool("ModifyGapRoomOneWayShadow", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool ModifyGapRoomOneWayShadow;

    [CustomOptionBool("GapRoomShadowIgnoresImpostors", true, parentFieldName: nameof(ModifyGapRoomOneWayShadow))]
    public static bool GapRoomShadowIgnoresImpostors;

    [CustomOptionBool("DisableGapRoomShadowForNonImpostor", true, parentFieldName: nameof(ModifyGapRoomOneWayShadow))]
    public static bool DisableGapRoomShadowForNonImpostor;

    // |:========== (TheFungle)スポーンタイプの設定 ==========:|
    [CustomOptionBool("TheFungleSetting", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool TheFungleSetting;

    [CustomOptionSelect("TheFungleSpawnType", typeof(FungleHandler.FungleSpawnType), "FungleHandler.FungleSpawnType.", parentFieldName: nameof(TheFungleSetting))]
    public static FungleHandler.FungleSpawnType TheFungleSpawnType;

    // |:========== (TheFungle)追加アドミンの設定 ==========:|
    [CustomOptionBool("TheFungleAdditionalAdmin", false, parentFieldName: nameof(TheFungleSetting))]
    public static bool TheFungleAdditionalAdmin;

    // |:========== (TheFungle)停電サボタージュの設定 ==========:|
    [CustomOptionBool("TheFunglePowerOutageSabotage", false, parentFieldName: nameof(TheFungleSetting))]
    public static bool TheFunglePowerOutageSabotage;

    // |:========== (TheFungle)胞子マスク非表示の設定 ==========:|
    [CustomOptionBool("TheFungleHideSporeMask", false, parentFieldName: nameof(TheFungleSetting))]
    public static bool TheFungleHideSporeMask;

    [CustomOptionBool("TheFungleHideSporeMaskOnlyImpostor", false, parentFieldName: nameof(TheFungleHideSporeMask))]
    public static bool TheFungleHideSporeMaskOnlyImpostor;
}