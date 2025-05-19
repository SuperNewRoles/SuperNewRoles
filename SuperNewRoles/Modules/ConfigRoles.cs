using BepInEx.Configuration;

namespace SuperNewRoles.Modules;

public static class ConfigRoles
{
    public static ConfigEntry<bool> CanUseDataConnection;

    public static void Init()
    {
        CanUseDataConnection = SuperNewRolesPlugin.Instance.Config.Bind("Default", "CanUseDataConnection", false, "サイズが大きいファイルをデータ通信でダウンロードするかどうか(Download large files over mobile data?)");
    }
}

