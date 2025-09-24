namespace SuperNewRoles;

public static class Logger
{
    public static void Info(string message, string source = "SuperNewRoles")
    {
        if (SuperNewRolesPlugin.Instance?.Log != null)
            SuperNewRolesPlugin.Instance.Log.LogInfo($"[{source}] {message}");
        else
            System.Console.WriteLine($"[INFO] [{source}] {message}");
    }
    public static void Error(string message, string source = "SuperNewRoles")
    {
        if (SuperNewRolesPlugin.Instance?.Log != null)
            SuperNewRolesPlugin.Instance.Log.LogError($"[{source}] {message}");
        else
            System.Console.Error.WriteLine($"[ERROR] [{source}] {message}");
    }
    public static void Warning(string message, string source = "SuperNewRoles")
    {
        if (SuperNewRolesPlugin.Instance?.Log != null)
            SuperNewRolesPlugin.Instance.Log.LogWarning($"[{source}] {message}");
        else
            System.Console.WriteLine($"[WARN] [{source}] {message}");
    }
    public static void Debug(string message, string source = "SuperNewRoles")
    {
#if DEBUG
        if (SuperNewRolesPlugin.Instance?.Log != null)
            SuperNewRolesPlugin.Instance.Log.LogInfo($"[{source}] {message}");
        else
            System.Console.WriteLine($"[DEBUG] [{source}] {message}");
#endif
    }
}
