using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace SuperNewRoles.Modules;

public class SNRLogListener : ILogListener
{
    public static SNRLogListener Instance { get; private set; }
    public LogLevel LogLevelFilter => LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info;

    private readonly List<string> _logLines = new();
    private readonly object _logLinesLock = new();

    public SNRLogListener()
    {
        Instance = this;
    }

    /// <summary>
    /// バグ報告用。このプロセスが受信したログ全文を返す。
    /// LogOutput.log は未フラッシュや複数起動時の別ファイルになり得るため使わない。
    /// </summary>
    public string GetLogText()
    {
        List<string> snapshot;
        lock (_logLinesLock)
            snapshot = new List<string>(_logLines);

        if (snapshot.Count == 0)
            return string.Empty;

        return string.Join(Environment.NewLine, snapshot) + Environment.NewLine;
    }

    public void Dispose()
    {
        lock (_logLinesLock)
            _logLines.Clear();
        if (Instance == this)
            Instance = null;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        string line = eventArgs.ToString();
        lock (_logLinesLock)
            _logLines.Add(line);
    }
}
