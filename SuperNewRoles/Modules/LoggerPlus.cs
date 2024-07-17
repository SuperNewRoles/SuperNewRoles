using System;
using System.Diagnostics;
using System.IO;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Modules;

class LoggerPlus
{
    /// <summary> ログ出力先(推定)のファイル名(起動時の既存起動数に依存) </summary>
    private static string LogName;
    /// <summary> ログ出力先(推定)のファイル名取得する </summary>
    public static void SetLogName()
    {
        // 呼び出し時のAmong Usの起動台数に従い、ログのファイル名を取得する。
        // "3台起動 => 2台目終了 => Among Us起動(*1)" 時 *1のログは"LogOutput.1.log"だが、記録されるファイル名は "LogOutput.2.log"になる

        int logCount = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length - 1;
        LogName = $"LogOutput{(logCount == 0 ? "" : $".{logCount}")}.log";
    }

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
        string userType = AmongUsClient.Instance.AmHost || AmongUsClient.Instance.GameState == AmongUsClient.GameStates.NotJoined ? "Host" : "Client";
        string splicingMemo = ReplaceUnusableStringsAsFileNames(memo);

        // ファイル名作成
        string fileName = $"{date}_SNR_v{version}_{userType}_{splicingBranch}_{splicingMemo}.log";

        // 出力先のパス作成
        string folderPath = Path.GetDirectoryName(UnityEngine.Application.dataPath) + @"\SuperNewRoles\SaveLogFolder\";
        Directory.CreateDirectory(folderPath);
        string filePath = @$"{folderPath}" + @$"{fileName}";


        // 出力
        string sourceLogFile = Path.GetDirectoryName(UnityEngine.Application.dataPath) + @$"\BepInEx\{LogName}";
        if (File.Exists(sourceLogFile))
        {
            // logを出力した旨のlogを印字 及びチャットが存在するときはチャットを表示
            var message = $"この時点までのログを [ {fileName} ] に保存しました。";
            Logger.Info($"[{LogName}] {message}", via);
            AddChatPatch.ChatInformation(PlayerControl.LocalPlayer, "システム", message, isSendFromGuest: true);

            FileInfo sourceLogPath = new(@sourceLogFile);
            sourceLogPath.CopyTo(@filePath, true);
        }
        else
        {
            var errorMessage = $"印字元のパスが正常に設定されていなかった為、保存の実行を中止しました。 [指定ログファイル] : {LogName}";
            Logger.Error(errorMessage, via);
            AddChatPatch.ChatInformation(PlayerControl.LocalPlayer, "システム", errorMessage, isSendFromGuest: true);
        }
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