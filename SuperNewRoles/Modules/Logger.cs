namespace SuperNewRoles;

public static class Logger
{
    public static void Info(string message, string source = "SuperNewRoles")
    {
        SuperNewRolesPlugin.Instance.Log.LogInfo($"[{source}] {message}");
    }
    public static void Error(string message, string source = "SuperNewRoles")
    {
        SuperNewRolesPlugin.Instance.Log.LogError($"[{source}] {message}");
    }
    public static void Warning(string message, string source = "SuperNewRoles")
    {
        SuperNewRolesPlugin.Instance.Log.LogWarning($"[{source}] {message}");
    }
}
