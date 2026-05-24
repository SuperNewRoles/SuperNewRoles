using System;
using System.IO;

namespace SuperNewRoles.Modules;

public static class BugReportErrorLogCollector
{
    public const string ErrorLogFileName = "ErrorLog.log";
    public const string PayloadKey = "error_log_compressed";

    public static string CollectAndCompress()
    {
        string errorLogText = ReadErrorLogText();
        if (string.IsNullOrEmpty(errorLogText))
            return null;

        return LogCompression.CompressAndEncryptLog(errorLogText);
    }

    public static string ReadErrorLogText()
    {
        string baseDirectory = string.IsNullOrEmpty(BepInEx.Paths.BepInExRootPath)
            ? AppContext.BaseDirectory
            : BepInEx.Paths.BepInExRootPath;

        return ReadErrorLogText(baseDirectory);
    }

    public static string ReadErrorLogText(string baseDirectory)
    {
        if (string.IsNullOrEmpty(baseDirectory))
        {
            Logger.Info("BugReportErrorLogCollector: base directory is empty.");
            return null;
        }

        string filePath = Path.Combine(baseDirectory, ErrorLogFileName);
        if (!File.Exists(filePath))
        {
            Logger.Info($"BugReportErrorLogCollector: {ErrorLogFileName} was not found at {filePath}.");
            return null;
        }

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (stream.Length == 0)
            {
                Logger.Info($"BugReportErrorLogCollector: {ErrorLogFileName} is empty.");
                return null;
            }

            using var reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            if (string.IsNullOrEmpty(text))
            {
                Logger.Info($"BugReportErrorLogCollector: {ErrorLogFileName} had no readable content.");
                return null;
            }

            return text;
        }
        catch (Exception ex)
        {
            Logger.Warning($"BugReportErrorLogCollector: failed to read {ErrorLogFileName}: {ex.Message}");
            return null;
        }
    }
}
