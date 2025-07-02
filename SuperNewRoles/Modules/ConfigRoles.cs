using BepInEx.Configuration;

namespace SuperNewRoles.Modules;

public static class ConfigRoles
{
    public static ConfigEntry<bool> CanUseDataConnection;
    public static ConfigEntry<bool> IsSendAnalytics;
    public static ConfigEntry<bool> IsSendAnalyticsPopupViewd;

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
        IsSendAnalytics = SuperNewRolesPlugin.Instance.Config.Bind("Default", "IsSendAnalytics", false, "アナリティクスを送信するかどうか(Send analytics?)");
        IsSendAnalyticsPopupViewd = SuperNewRolesPlugin.Instance.Config.Bind("Default", "IsSendAnalyticsPopupViewd", false, "アナリティクスのポップアップが表示されたかどうか(Has the analytics popup been viewed?)");

        _isCPUProcessorAffinity = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CPUProcessorAffinity", false, "CPUの割当を変更するかどうか(Change CPU affinity?)");
        _ProcessorAffinityMask = SuperNewRolesPlugin.Instance.Config.Bind("Default", "ProcessorAffinityMask", (ulong)3, "CPUの割当を変更するためのマスク(Mask for changing CPU affinity)");

        _isCompressCosmetics = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CompressCosmetics", true, "コスメティックを圧縮するかどうか(Compress cosmetics?)");
        IsCompressCosmetics = _isCompressCosmetics.Value;
    }
}

