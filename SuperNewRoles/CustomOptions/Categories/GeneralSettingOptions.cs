using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public static class GeneralSettingOptions
{
    [CustomOptionBool("KickNonPCPlayers", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool KickNonPCPlayers;

    [CustomOptionBool("BanNoFriendCodePlayers", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool BanNoFriendCodePlayers;

    // |:========== MODカラー禁止の設定 ==========:|
    [CustomOptionBool("DisableModColor", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool DisableModColor;

    // |:========== 情報機器制限の設定 ==========:|
    [CustomOptionBool("DeviceOptions", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool DeviceOptions;

    // ON/OFFの設定
    [CustomOptionBool("CanUseDeviceSetting", true, parentFieldName: nameof(DeviceOptions))]
    public static bool CanUseDeviceSetting;

    [CustomOptionBool("DeviceUseAdminSetting", true, parentFieldName: nameof(CanUseDeviceSetting))]
    public static bool DeviceUseAdmin;

    [CustomOptionBool("DeviceUseVitalOrDoorLogSetting", true, parentFieldName: nameof(CanUseDeviceSetting))]
    public static bool DeviceUseVitalOrDoorLog;

    [CustomOptionBool("DeviceUseCameraSetting", true, parentFieldName: nameof(CanUseDeviceSetting))]
    public static bool DeviceUseCamera;

    // 時間制限の設定
    [CustomOptionBool("RestrictDevicesTimeOption", false, parentFieldName: nameof(DeviceOptions))]
    public static bool RestrictDevicesTimeOption;

    [CustomOptionBool("RestrictAdmin", false, parentFieldName: nameof(RestrictDevicesTimeOption))]
    public static bool RestrictAdmin;

    [CustomOptionFloat("DeviceTimeSettingAdmin", 0f, 120f, 1f, 10f, parentFieldName: nameof(RestrictAdmin))]
    public static float DeviceUseAdminTime;

    [CustomOptionBool("RestrictVital", false, parentFieldName: nameof(RestrictDevicesTimeOption))]
    public static bool RestrictVital;

    [CustomOptionFloat("DeviceTimeSettingVitalOrDoorLog", 0f, 120f, 1f, 10f, parentFieldName: nameof(RestrictVital))]
    public static float DeviceUseVitalOrDoorLogTime;

    [CustomOptionBool("RestrictCamera", false, parentFieldName: nameof(RestrictDevicesTimeOption))]
    public static bool RestrictCamera;

    [CustomOptionFloat("DeviceTimeSettingCamera", 0f, 120f, 1f, 10f, parentFieldName: nameof(RestrictCamera))]
    public static float DeviceUseCameraTime;
}
