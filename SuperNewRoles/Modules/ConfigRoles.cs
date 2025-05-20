using BepInEx.Configuration;

namespace SuperNewRoles.Modules;

public static class ConfigRoles
{
    public static ConfigEntry<bool> CanUseDataConnection;

    public static ConfigEntry<bool> _isCPUProcessorAffinity;
    public static ConfigEntry<ulong> _ProcessorAffinityMask;


    public static void Init()
    {
        CanUseDataConnection = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CanUseDataConnection", false, "サイズが大きいファイルをデータ通信でダウンロードするかどうか(Download large files over mobile data?)");

        _isCPUProcessorAffinity = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CPUProcessorAffinity", true, "CPUの割当を変更するかどうか(Change CPU affinity?)");
        _ProcessorAffinityMask = SuperNewRolesPlugin.Instance.Config.Bind("Default", "ProcessorAffinityMask", (ulong)3, "CPUの割当を変更するためのマスク(Mask for changing CPU affinity)");
    }
}

