using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public static class MapSettingOptions
{
    // |:========== 配線タスクランダムの設定 ==========:|
    [CustomOptionBool("WireTaskIsRandom", false, parentFieldName: nameof(Categories.MapSettings))]
    public static bool WireTaskIsRandom;

    [CustomOptionInt("WireTaskNum", 1, 8, 1, 5, parentFieldName: nameof(WireTaskIsRandom))]
    public static int WireTaskNum;

    // |:========== 緊急タスク継続時間の設定 ==========:|
    [CustomOptionBool("ReactorDurationSetting", false, parentFieldName: nameof(Categories.MapSettings))]
    public static bool ReactorDurationOption;

    [CustomOptionFloat("SkeldReactorTime", 0f, 30f, 2.5f, 20f, parentFieldName: nameof(ReactorDurationOption))]
    public static float SkeldReactorTimeLimit;

    [CustomOptionFloat("SkeldLifeSuppTime", 0f, 30f, 2.5f, 20f, parentFieldName: nameof(ReactorDurationOption))]
    public static float SkeldLifeSuppTimeLimit;

    [CustomOptionFloat("MiraReactorTime", 0f, 45f, 2.5f, 30f, parentFieldName: nameof(ReactorDurationOption))]
    public static float MiraReactorTimeLimit;

    [CustomOptionFloat("MiraLifeSuppTime", 0f, 45f, 2.5f, 30f, parentFieldName: nameof(ReactorDurationOption))]
    public static float MiraLifeSuppTimeLimit;

    [CustomOptionFloat("PolusReactorTime", 0f, 60f, 2.5f, 40f, parentFieldName: nameof(ReactorDurationOption))]
    public static float PolusReactorTimeLimit;

    [CustomOptionFloat("AirshipReactorTime", 0f, 90f, 2.5f, 60f, parentFieldName: nameof(ReactorDurationOption))]
    public static float AirshipReactorTimeLimit;

    [CustomOptionFloat("FungleReactorTime", 0f, 60f, 2.5f, 40f, parentFieldName: nameof(ReactorDurationOption))]
    public static float FungleReactorTimeLimit;


    // |:========== 情報機器制限の設定 ==========:|
    [CustomOptionBool("DeviceOptions", false, parentFieldName: nameof(Categories.MapSettings))]
    public static bool DeviceOptions;

    // アドミン設定
    [CustomOptionSelect("DeviceAdminOption", typeof(DeviceOptionType), "DeviceOptionType.", parentFieldName: nameof(DeviceOptions))]
    public static DeviceOptionType DeviceAdminOption;

    [CustomOptionFloat("DeviceTimeSettingAdmin", 0f, 120f, 2.5f, 10f, parentFieldName: nameof(DeviceAdminOption), parentActiveValue: DeviceOptionType.Restrict)]
    public static float DeviceUseAdminTime;

    // バイタル/ドアログ設定
    [CustomOptionSelect("DeviceVitalOrDoorLogOption", typeof(DeviceOptionType), "DeviceOptionType.", parentFieldName: nameof(DeviceOptions))]
    public static DeviceOptionType DeviceVitalOrDoorLogOption;

    [CustomOptionFloat("DeviceTimeSettingVitalOrDoorLog", 0f, 120f, 2.5f, 10f, parentFieldName: nameof(DeviceVitalOrDoorLogOption), parentActiveValue: DeviceOptionType.Restrict)]
    public static float DeviceUseVitalOrDoorLogTime;

    // カメラ設定
    [CustomOptionSelect("DeviceCameraOption", typeof(DeviceOptionType), "DeviceOptionType.", parentFieldName: nameof(DeviceOptions))]
    public static DeviceOptionType DeviceCameraOption;

    [CustomOptionFloat("DeviceTimeSettingCamera", 0f, 120f, 2.5f, 10f, parentFieldName: nameof(DeviceCameraOption), parentActiveValue: DeviceOptionType.Restrict)]
    public static float DeviceUseCameraTime;
    // |:========== 梯子クールダウンの設定 ==========:|
    [CustomOptionBool("LadderCoolChangeSetting", false, parentFieldName: nameof(Categories.MapSettings))]
    public static bool LadderCoolChangeOption;

    [CustomOptionFloat("LadderCoolTimeSetting", 0f, 60f, 2.5f, 2.5f, parentFieldName: nameof(LadderCoolChangeOption))]
    public static float LadderCoolTimeOption;

    [CustomOptionBool("LadderImpostorCoolChangeSetting", false, parentFieldName: nameof(LadderCoolChangeOption))]
    public static bool LadderImpostorCoolChangeOption;

    [CustomOptionFloat("LadderImpostorCoolTimeSetting", 0f, 60f, 2.5f, 2.5f, parentFieldName: nameof(LadderImpostorCoolChangeOption))]
    public static float LadderImpostorCoolTimeOption;

    // |:========== ジップラインクールダウンの設定 ==========:|
    [CustomOptionBool("ZiplineCoolChangeSetting", false, parentFieldName: nameof(Categories.MapSettings))]
    public static bool ZiplineCoolChangeOption;

    [CustomOptionFloat("ZiplineCoolTimeSetting", 0f, 60f, 2.5f, 7.5f, parentFieldName: nameof(ZiplineCoolChangeOption))]
    public static float ZiplineCoolTimeOption;

    [CustomOptionBool("ZiplineImpostorCoolChangeSetting", false, parentFieldName: nameof(ZiplineCoolChangeOption))]
    public static bool ZiplineImpostorCoolChangeOption;

    [CustomOptionFloat("ZiplineImpostorCoolTimeSetting", 0f, 60f, 2.5f, 7.5f, parentFieldName: nameof(ZiplineImpostorCoolChangeOption))]
    public static float ZiplineImpostorCoolTimeOption;
}