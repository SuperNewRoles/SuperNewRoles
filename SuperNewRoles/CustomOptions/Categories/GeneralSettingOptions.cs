using SuperNewRoles.Modules;
using SuperNewRoles.Patches;

namespace SuperNewRoles.CustomOptions.Categories;

public static class GeneralSettingOptions
{
    [CustomOptionBool("KickPlatformPlayers", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool KickPlatformPlayers;
    /*[CustomOptionBool("KickPCPlayers", false, parentFieldName: nameof(KickPlatformPlayers))]
    public static bool KickPCPlayers;
    [CustomOptionBool("KickAndroidPlayers", false, parentFieldName: nameof(KickPlatformPlayers))]
    public static bool KickAndroidPlayers;
    */
    // todo:リリース時
    public static bool KickPCPlayers = false;
    public static bool KickAndroidPlayers = true;

    [CustomOptionBool("KickOtherPlayers", false, parentFieldName: nameof(KickPlatformPlayers))]
    public static bool KickOtherPlayers;

    [CustomOptionBool("BanNoFriendCodePlayers", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool BanNoFriendCodePlayers;

    // |:========== MODカラー禁止の設定 ==========:|
    [CustomOptionBool("DisableModColor", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool DisableModColor;

    [CustomOptionBool("PetOnlyMe", true, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool PetOnlyMe;

    [CustomOptionBool("AdvancedRandom", true, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool AdvancedRandom;

    [CustomOptionSelect("NetworkTransformType", typeof(NetworkTransformType), "NetworkTransformType.", parentFieldName: nameof(Categories.GeneralSettings))]
    public static NetworkTransformType NetworkTransformType;

    [CustomOptionSelect("NetworkTransformTypeLowLatencyLevel", typeof(NetworkTransformTypeLowLatencyLevel), "NetworkTransformTypeLowLatencyLevel.", parentFieldName: nameof(NetworkTransformType), parentActiveValue: NetworkTransformType.ModdedLowLatency, defaultValue: NetworkTransformTypeLowLatencyLevel.Medium)]
    public static NetworkTransformTypeLowLatencyLevel NetworkTransformTypeLowLatencyLevel;

    [CustomOptionBool("SumouMode", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool SumouMode;
}