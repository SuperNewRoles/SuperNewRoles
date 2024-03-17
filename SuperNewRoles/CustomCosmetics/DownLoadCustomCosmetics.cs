namespace SuperNewRoles.CustomCosmetics;

public static class DownLoadCustomCosmetics
{
    /// <summary>CustomCosmeticの読み込みを行うか</summary>
    public static bool IsLoad => !(ConfigRoles.DebugMode.Value || ConfigRoles.IsModCosmeticsAreNotLoaded.Value) || forceLoad;

    /// <summary>デバッグモードの状態及びクライアントオプションの状態に影響せず, 強制的にCustomCosmeticsを読み込むか</summary>
    private static bool forceLoad = false; // プルリク時, trueなら指摘

    public static void CosmeticsLoad()
    {
        DownLoadCustomhat.Load();
        DownLoadClassPlate.Load();
        DownLoadClassVisor.Load();
    }
}