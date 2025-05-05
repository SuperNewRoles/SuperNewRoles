using SuperNewRoles.Modules;
using SuperNewRoles.Patches;

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

    [CustomOptionBool("PetOnlyMe", true, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool PetOnlyMe;

    [CustomOptionBool("AdvancedRandom", true, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool AdvancedRandom;

    [CustomOptionSelect("NetworkTransformType", typeof(NetworkTransformType), "NetworkTransformType.", parentFieldName: nameof(Categories.GeneralSettings))]
    public static NetworkTransformType NetworkTransformType;

    [CustomOptionSelect("NetworkTransformTypeLowLatencyLevel", typeof(NetworkTransformTypeLowLatencyLevel), "NetworkTransformTypeLowLatencyLevel.", parentFieldName: nameof(NetworkTransformType), parentActiveValue: NetworkTransformType.ModdedLowLatency, defaultValue: NetworkTransformTypeLowLatencyLevel.Medium)]
    public static NetworkTransformTypeLowLatencyLevel NetworkTransformTypeLowLatencyLevel;
}