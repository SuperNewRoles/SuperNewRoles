using BepInEx.Configuration;

namespace SuperNewRoles.Modules;

public static class ConfigRoles
{
    public static ConfigEntry<bool> CanUseDataConnection;
    public static ConfigEntry<bool> IsSendAnalytics;
    public static ConfigEntry<bool> IsSendAnalyticsPopupViewd;
    public static ConfigEntry<bool> IsOnboardingViewd;
    public static ConfigEntry<bool> IsMuteLobbyBGM;
    public static ConfigEntry<bool> AutoCopyGameCode;
    public static ConfigEntry<bool> IsModCosmeticsAreNotLoaded;
    public static ConfigEntry<bool> IsNotUsingBlood;
    public static ConfigEntry<bool> IsLightAndDarker;
    public static ConfigEntry<bool> IsVersionErrorView;
    public static ConfigEntry<bool> _isCustomCosmeticsCacheResetRequested;

    public static ConfigEntry<bool> _isCPUProcessorAffinity;
    public static ConfigEntry<ulong> _ProcessorAffinityMask;

    private static ConfigEntry<bool> _isCompressCosmetics;

    public static bool IsCompressCosmetics { get; private set; }

    public static void SetIsCompressCosmetics(bool value)
    {
        IsCompressCosmetics = value;
        _isCompressCosmetics.Value = value;
    }

    public static void Init()
    {
        CanUseDataConnection = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CanUseDataConnection", false, "サイズが大きいファイルをデータ通信でダウンロードするかどうか(Download large files over mobile data?)");
        IsSendAnalytics = SuperNewRolesPlugin.Instance.Config.Bind("Default", "IsSendAnalyticsSNR2", true, "アナリティクスを送信するかどうか(Send analytics?)");
        IsSendAnalyticsPopupViewd = SuperNewRolesPlugin.Instance.Config.Bind("Default", "IsSendAnalyticsPopupViewd", false, "アナリティクスのポップアップが表示されたかどうか(Has the analytics popup been viewed?)");
        IsOnboardingViewd = SuperNewRolesPlugin.Instance.Config.Bind("Default", "IsOnboardingViewd", false, "初回起動時のオンボーディングが完了したかどうか(Has the first-launch onboarding been completed?)");
        IsMuteLobbyBGM = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsMutedLobbyBGM", false, "ロビーBGMをミュートするかどうか(Mute lobby BGM?)");
        AutoCopyGameCode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Auto Copy Game Code", true, "ゲームコードを自動でコピーするかどうか(Auto copy game code?)");
        IsModCosmeticsAreNotLoaded = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsModCosmeticsAreNotLoaded", false, "カスタムコスメティックを読み込まないかどうか(Do not load custom cosmetics?)");
        IsNotUsingBlood = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsNotUsingBlood", false, "血液表現を黒にするかどうか(Make blood expression black?)");
        IsLightAndDarker = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsLightAndDarker", true, "明暗を表示するかどうか(Show light and dark indicator?)");
        IsVersionErrorView = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsVersionErrorView", true, "同期エラーを表示するかどうか(Show sync errors?)");
        _isCustomCosmeticsCacheResetRequested = SuperNewRolesPlugin.Instance.Config.Bind("Default", "IsCustomCosmeticsCacheResetRequested", false, "カスタムコスメティックのキャッシュを次回起動時にリセットするかどうか(Reset custom cosmetics cache on next launch?)");
        _isCPUProcessorAffinity = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CPUProcessorAffinity", false, "CPUの割当を変更するかどうか(Change CPU affinity?)");
        _ProcessorAffinityMask = SuperNewRolesPlugin.Instance.Config.Bind("Default", "ProcessorAffinityMask", (ulong)3, "CPUの割当を変更するためのマスク(Mask for changing CPU affinity)");

        _isCompressCosmetics = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CompressCosmetics", true, "コスメティックを圧縮するかどうか(Compress cosmetics?)");
        IsCompressCosmetics = _isCompressCosmetics.Value;
    }
}

