using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using LogLevel = BepInEx.Logging.LogLevel;

namespace SuperNewRoles;

class Logger
{
    public static bool isEnable;
    public static List<string> disableList = new();
    public static List<string> sendToGameList = new();
    public static bool isDetail = false;
    public static bool isAlsoInGame = false;
    public static void Enable() => isEnable = true;
    public static void Disable() => isEnable = false;
    public static void Enable(string tag, bool toGame = false)
    {
        disableList.Remove(tag);
        if (toGame && !sendToGameList.Contains(tag)) sendToGameList.Add(tag);
        else sendToGameList.Remove(tag);
    }
    public static void Disable(string tag) { if (!disableList.Contains(tag)) disableList.Add(tag); }
    public static void SendInGame(string text, bool isAlways = false)
    {
        if (!isEnable) return;
        if (DestroyableSingleton<HudManager>._instance) FastDestroyableSingleton<HudManager>.Instance.Notifier.AddItem(text);
    }
    private static void SendToFile(string text, string callerMember = "", LogLevel level = LogLevel.Info, string tag = "", int lineNumber = 0, string fileName = "")
    {
        var logger = SuperNewRolesPlugin.Logger;
        string t = DateTime.Now.ToString("HH:mm:ss.fff");
        if (sendToGameList.Contains(tag) || isAlsoInGame) SendInGame($"[{tag}]{text}");
        text = text.Replace("\r", "\\r").Replace("\n", "\\n");
        string log_text = $"[{t}][{callerMember}][{tag}]{text}";
        if (isDetail && ConfigRoles.DebugMode.Value)
        {
            StackFrame stack = new(2);
            string className = stack.GetMethod().ReflectedType.Name;
            string memberName = stack.GetMethod().Name;
            log_text = $"[{t}][{className}.{memberName}({Path.GetFileName(fileName)}:{lineNumber})][{tag}]{text}";
        }

        switch (level)
        {
            case LogLevel.Info:
                logger.LogInfo(log_text);
                break;
            case LogLevel.Warning:
                logger.LogWarning(log_text);
                break;
            case LogLevel.Error:
                logger.LogError(log_text);
                break;
            case LogLevel.Fatal:
                logger.LogFatal(log_text);
                break;
            case LogLevel.Message:
                logger.LogMessage(log_text);
                break;
            default:
                logger.LogWarning("Error:Invalid LogLevel");
                logger.LogInfo(log_text);
                break;
        }
    }
    public static void Info(string text, string tag = "", [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string callerMember = "") =>
        SendToFile(text, callerMember, LogLevel.Info, tag, lineNumber, fileName);
    public static void Warn(string text, string tag, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string callerMember = "") =>
        SendToFile(text, callerMember, LogLevel.Warning, tag, lineNumber, fileName);
    public static void Error(string text, string tag, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string callerMember = "") =>
        SendToFile(text, callerMember, LogLevel.Error, tag, lineNumber, fileName);
    public static void Fatal(string text, string tag, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string callerMember = "") =>
        SendToFile(text, callerMember, LogLevel.Fatal, tag, lineNumber, fileName);
    public static void Msg(string text, string tag, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string callerMember = "") =>
        SendToFile(text, callerMember, LogLevel.Message, tag, lineNumber, fileName);
    public static void CurrentMethod([CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "", [CallerMemberName] string callerMember = "")
    {
        StackFrame stack = new(1);
        Msg($"\"{stack.GetMethod().ReflectedType.Name}.{stack.GetMethod().Name}\" Called in \"{Path.GetFileName(fileName)}({lineNumber})\"", "Method");
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
        string splicingMemo = ReplaceUnusableStringsAsFileNames(memo);

        // ファイル名作成
        string fileName = $"{date}_SNR_v{version}_{splicingBranch}_{splicingMemo}.log";

        // 出力先のパス作成
        string folderPath = Path.GetDirectoryName(UnityEngine.Application.dataPath) + @"\SuperNewRoles\SaveLogFolder\";
        Directory.CreateDirectory(folderPath);
        string filePath = @$"{folderPath}" + @$"{fileName}";

        // logを出力した旨のlogを印字 及びチャットが存在するときはチャットを表示
        Info($"この時点までのログを [ {fileName} ] に保存しました。", via);
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