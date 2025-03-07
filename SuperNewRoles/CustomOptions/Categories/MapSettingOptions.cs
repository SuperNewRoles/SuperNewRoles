using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public static class MapSettingOptions
{

    // |:========== 緊急タスク継続時間の設定 ==========:|
    [CustomOptionBool("ReactorDurationSetting", false, parentFieldName: nameof(Categories.MapSettings))]
    public static bool ReactorDurationOption;

    [CustomOptionFloat("SkeldReactorTime", 0f, 30f, 1f, 20f, parentFieldName: nameof(ReactorDurationOption))]
    public static float SkeldReactorTimeLimit;

    [CustomOptionFloat("SkeldLifeSuppTime", 0f, 30f, 1f, 20f, parentFieldName: nameof(ReactorDurationOption))]
    public static float SkeldLifeSuppTimeLimit;

    [CustomOptionFloat("MiraReactorTime", 0f, 45f, 1f, 30f, parentFieldName: nameof(ReactorDurationOption))]
    public static float MiraReactorTimeLimit;

    [CustomOptionFloat("MiraLifeSuppTime", 0f, 45f, 1f, 30f, parentFieldName: nameof(ReactorDurationOption))]
    public static float MiraLifeSuppTimeLimit;

    [CustomOptionFloat("PolusReactorTime", 0f, 60f, 1f, 40f, parentFieldName: nameof(ReactorDurationOption))]
    public static float PolusReactorTimeLimit;

    [CustomOptionFloat("AirshipReactorTime", 0f, 90f, 1f, 60f, parentFieldName: nameof(ReactorDurationOption))]
    public static float AirshipReactorTimeLimit;

    [CustomOptionFloat("FungleReactorTime", 0f, 60f, 1f, 40f, parentFieldName: nameof(ReactorDurationOption))]
    public static float FungleReactorTimeLimit;

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