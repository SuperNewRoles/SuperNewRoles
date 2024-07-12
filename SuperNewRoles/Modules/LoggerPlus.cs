using System;
using System.IO;

namespace SuperNewRoles.Modules;

class LoggerPlus
{
    /// <summary>
    /// SaveLogFolderにその時点までのlogを名前を付けて保存する
    /// </summary>
    /// <param name="memo">ファイル名につける為に取得したメモ(文字列)</param>
    /// <param name="via">どこからこのメソッドが呼び出されたかlogに表記する為の文字列</param>

    public static void SaveLog(string memo, string via)
    {
        // ファイル名に使用する変数作成
        string date = DateTime.Now.ToString("yyMMdd_HHmmss");
        string splicingBranch = ReplaceUnusableStringsAsFileNames(ThisAssembly.Git.Branch);
        string version = SuperNewRolesPlugin.VersionString.Replace(".", "");
        version = ReplaceUnusableStringsAsFileNames(version);
        string splicingMemo = ReplaceUnusableStringsAsFileNames(memo);

        // ファイル名作成
        string fileName = $"{date}_SNR_v{version}_{splicingBranch}_{splicingMemo}.log";

        // 出力先のパス作成
        string folderPath = Path.GetDirectoryName(UnityEngine.Application.dataPath) + @"\SuperNewRoles\SaveLogFolder\";
        Directory.CreateDirectory(folderPath);
        string filePath = @$"{folderPath}" + @$"{fileName}";

        // logを出力した旨のlogを印字 及びチャットが存在するときはチャットを表示
        Logger.Info($"この時点までのログを [ {fileName} ] に保存しました。", via);
        if (PlayerControl.LocalPlayer != null)
            FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, $"この時点までのログを [ {fileName} ] に保存しました。");

        // 出力
        string sourceLogFile = Path.GetDirectoryName(UnityEngine.Application.dataPath) + @"\BepInEx\LogOutput.log";
        FileInfo sourceLogPath = new(@sourceLogFile);
        sourceLogPath.CopyTo(@filePath, true);
    }

    /// <summary>
    /// stringsに含まれるファイル名に使用不可能な文字を"_"に置換する
    /// </summary>
    /// <param name="strings">ファイル名に使用したい未編集の文字列</param>
    /// <returns>ファイル名に使用できるように加工した文字列を返す</returns>
    private static string ReplaceUnusableStringsAsFileNames(string strings)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        string fileName = strings;
        foreach (var invalid in invalidChars)
            fileName = fileName.Replace($"{invalid}", "_");
        fileName = fileName.Replace($".", "_");
        return fileName;
    }
}