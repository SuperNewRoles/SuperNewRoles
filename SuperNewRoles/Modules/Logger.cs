namespace SuperNewRoles;

public static class Logger
{
    public static void Info(string message, string source = "SuperNewRoles")
    {
        SuperNewRolesPlugin.Instance.Log.LogInfo(Encryption.Encrypt($"[{source}] {message}"));
    }
    public static void Error(string message, string source = "SuperNewRoles")
    {
        SuperNewRolesPlugin.Instance.Log.LogError(Encryption.Encrypt($"[{source}] {message}"));
    }
    public static void Warning(string message, string source = "SuperNewRoles")
    {
        SuperNewRolesPlugin.Instance.Log.LogWarning(Encryption.Encrypt($"[{source}] {message}"));
    }
}
