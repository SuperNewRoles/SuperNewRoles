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
        if (DestroyableSingleton<HudManager>._instance) FastDestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage(text);
    }
    private static void SendToFile(string text, string callerMember = "", LogLevel level = LogLevel.Info, string tag = "", int lineNumber = 0, string fileName = "")
    {
        var logger = SuperNewRolesPlugin.Logger;
        string t = DateTime.Now.ToString("HH:mm:ss.fff");
        if (sendToGameList.Contains(tag) || isAlsoInGame) SendInGame($"[{tag}]{text}");
        text = text.Replace("\r", "\\r").Replace("\n", "\\n");
        string log_text = $"[{t}][{callerMember}][{tag}]{text}";
        if (isDetail && DebugModeManager.IsDebugMode)
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
}