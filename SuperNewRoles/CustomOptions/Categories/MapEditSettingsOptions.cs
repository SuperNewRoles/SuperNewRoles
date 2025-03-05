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
}