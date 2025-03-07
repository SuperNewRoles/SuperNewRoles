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
    // |:========== 配線タスクランダムの設定 ==========:|
    [CustomOptionBool("WireTaskIsRandom", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool WireTaskIsRandom;

    [CustomOptionInt("WireTaskNum", 1, 8, 1, 5, parentFieldName: nameof(WireTaskIsRandom))]
    public static int WireTaskNum;


    // |:========== 情報機器制限の設定 ==========:|
    [CustomOptionBool("DeviceOptions", false, parentFieldName: nameof(Categories.MapEditSettings))]
    public static bool DeviceOptions;

    // アドミン設定
    [CustomOptionSelect("DeviceAdminOption", typeof(DeviceOptionType), "DeviceOptionType.", parentFieldName: nameof(DeviceOptions))]
    public static DeviceOptionType DeviceAdminOption;

    [CustomOptionFloat("DeviceTimeSettingAdmin", 0f, 120f, 1f, 10f, parentFieldName: nameof(DeviceAdminOption), parentActiveValue: DeviceOptionType.Restrict)]
    public static float DeviceUseAdminTime;

    // バイタル/ドアログ設定
    [CustomOptionSelect("DeviceVitalOrDoorLogOption", typeof(DeviceOptionType), "DeviceOptionType.", parentFieldName: nameof(DeviceOptions))]
    public static DeviceOptionType DeviceVitalOrDoorLogOption;

    [CustomOptionFloat("DeviceTimeSettingVitalOrDoorLog", 0f, 120f, 1f, 10f, parentFieldName: nameof(DeviceVitalOrDoorLogOption), parentActiveValue: DeviceOptionType.Restrict)]
    public static float DeviceUseVitalOrDoorLogTime;

    // カメラ設定
    [CustomOptionSelect("DeviceCameraOption", typeof(DeviceOptionType), "DeviceOptionType.", parentFieldName: nameof(DeviceOptions))]
    public static DeviceOptionType DeviceCameraOption;

    [CustomOptionFloat("DeviceTimeSettingCamera", 0f, 120f, 1f, 10f, parentFieldName: nameof(DeviceCameraOption), parentActiveValue: DeviceOptionType.Restrict)]
    public static float DeviceUseCameraTime;

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