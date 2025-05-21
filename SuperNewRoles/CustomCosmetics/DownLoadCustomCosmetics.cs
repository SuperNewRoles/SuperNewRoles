namespace SuperNewRoles.CustomCosmetics;

public static class DownLoadCustomCosmetics
{
    /// <summary>SuperNewCosmetics mainブランチのURL</summary>
    internal const string SNCmainURL = "https://raw.githubusercontent.com/SuperNewRoles/SuperNewCosmetics/main";

    /// <summary>CustomCosmeticの読み込みを行うか</summary>
    public static bool IsLoad => !(DebugModeManager.IsDebugMode || ConfigRoles.IsModCosmeticsAreNotLoaded.Value) || forceLoad;

    public static void CosmeticsLoad()
    {
        return;
        DownLoadCustomhat.Load();
        DownLoadClassPlate.Load();
        DownLoadClassVisor.Load();
    }

    // |:====== デバッグ関連の変数 & 定数 =====:|

    /// <summary>
    /// デバッグモードの状態及びクライアントオプションの状態に影響せず, 強制的にCustomCosmeticsを読み込むか
    /// ( プルリク時, trueなら指摘 )
    /// </summary>
    private const bool forceLoad = false;

    /// <summary>
    /// テスト用のリポジトリ 及び ブランチから, CustomCosmeticsをダウンロードするか
    /// </summary>
    internal static bool IsTestLoad => TestRepoURL != null;

    /// <summary>
    /// テスト時にダウンロードする, リポジトリ 及び ブランチのURL
    /// ( プルリク時, null以外なら指摘 )
    /// </summary>
    internal const string TestRepoURL = null;

    /// <summary>
    /// [ IsTestLoad ]有効時に, [ SNCmainURL ] からのダウンロードを, ブロックするか。
    /// SNCの別のブランチからダウンロードを実行する場合, 同時にSNC mainブランチからダウンロードを行っていると正常に反映されない為, ブロックが必要。
    /// </summary>
    internal const bool IsBlocLoadSNCmain = true;
}
