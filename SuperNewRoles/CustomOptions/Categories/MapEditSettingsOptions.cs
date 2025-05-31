using SuperNewRoles.MapCustoms;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public enum DeviceOptionType
{
    None,
    CantUse,
    Restrict,
}
public static class MapEditSettingsOptions
{
    // |:========== Polus設定 ==========:|
    [CustomOptionBool("PolusSetting", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool PolusSetting;

    [CustomOptionSelect("PolusSpawnType", typeof(SpawnTypeOptions), "SpawnType.", parentFieldName: nameof(PolusSetting))]
    public static SpawnTypeOptions PolusSpawnType;

    // |:========== Airship設定 ==========:|
    [CustomOptionBool("AirshipSetting", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool AirshipSetting;

    // |:========== (AirShip)ランダムスポーンの設定 ==========:|
    [CustomOptionBool("AirshipRandomSpawn", false, parentFieldName: nameof(AirshipSetting))]
    public static bool AirshipRandomSpawn;

    // |:========== (AirShip)アーカイブアドミン封印の設定 ==========:|
    [CustomOptionBool("RecordsAdminDestroy", false, parentFieldName: nameof(AirshipSetting))]
    public static bool RecordsAdminDestroy;
    // |:========== (AirShip)壁越しタスク禁止の設定 ==========:|
    [CustomOptionBool("AntiTaskOverWall", false, parentFieldName: nameof(AirshipSetting))]
    public static bool AntiTaskOverWall;

    // |:========== (AirShip)昇降機の影変更の設定 ==========:|
    [CustomOptionBool("ModifyGapRoomOneWayShadow", false, parentFieldName: nameof(AirshipSetting))]
    public static bool ModifyGapRoomOneWayShadow;

    [CustomOptionBool("GapRoomShadowIgnoresImpostors", true, parentFieldName: nameof(ModifyGapRoomOneWayShadow))]
    public static bool GapRoomShadowIgnoresImpostors;

    [CustomOptionBool("DisableGapRoomShadowForNonImpostor", true, parentFieldName: nameof(ModifyGapRoomOneWayShadow))]
    public static bool DisableGapRoomShadowForNonImpostor;

    // |:========== (TheFungle)スポーンタイプの設定 ==========:|
    [CustomOptionBool("TheFungleSetting", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool TheFungleSetting;

    [CustomOptionSelect("TheFungleSpawnType", typeof(SpawnTypeOptions), "SpawnType.", parentFieldName: nameof(TheFungleSetting))]
    public static SpawnTypeOptions TheFungleSpawnType;

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

    [CustomOptionBool("TheFungleZiplineOption", false, parentFieldName: nameof(TheFungleSetting))]
    public static bool TheFungleZiplineOption;

    [CustomOptionBool("TheFungleCanUseZiplineOption", true, parentFieldName: nameof(TheFungleZiplineOption))]
    public static bool TheFungleCanUseZiplineOption;

    [CustomOptionFloat("TheFungleZiplineUpTime", 0.5f, 12f, 0.5f, 4f, parentFieldName: nameof(TheFungleCanUseZiplineOption))]
    public static float TheFungleZiplineUpTime;

    [CustomOptionFloat("TheFungleZiplineDownTime", 0.5f, 12f, 0.5f, 1.75f, parentFieldName: nameof(TheFungleCanUseZiplineOption))]
    public static float TheFungleZiplineDownTime;

    [CustomOptionSelect("TheFungleZiplineUpOrDown", typeof(FungleZiplineDirectionOptions), "FungleZiplineDirectionOptions.", parentFieldName: nameof(TheFungleCanUseZiplineOption))]
    public static FungleZiplineDirectionOptions TheFungleZiplineUpOrDown;

    /* TODO
    [CustomOptionBool("TheFungleCameraOption", false, parentFieldName: nameof(TheFungleSetting))]
    public static bool TheFungleCameraOption;

    [CustomOptionFloat("TheFungleCameraChangeRange", 0.5f, 15f, 0.5f, 7.5f, parentFieldName: nameof(TheFungleCameraOption))]
    public static float TheFungleCameraChangeRange;

    [CustomOptionFloat("TheFungleCameraSpeed", 0f, 10f, 0.25f, 1f, parentFieldName: nameof(TheFungleCameraOption))]
    public static float TheFungleCameraSpeed;*/

    [CustomOptionBool("TheFungleMushroomMixupOption", false, parentFieldName: nameof(TheFungleSetting))]
    public static bool TheFungleMushroomMixupOption;

    [CustomOptionBool("TheFungleMushroomMixupCantOpenMeeting", false, parentFieldName: nameof(TheFungleMushroomMixupOption))]
    public static bool TheFungleMushroomMixupCantOpenMeeting;

    [CustomOptionFloat("TheFungleMushroomMixupTime", 1f, 30f, 0.5f, 10f, parentFieldName: nameof(TheFungleMushroomMixupOption))]
    public static float TheFungleMushroomMixupTime;

}

public enum SpawnTypeOptions
{
    Normal,
    Random,
    Select
}

public enum FungleZiplineDirectionOptions
{
    TheFungleZiplineAlways,
    TheFungleZiplineOnlyUp,
    TheFungleZiplineOnlyDown
}