using System.Text;
using BepInEx.Logging;

namespace SuperNewRoles.Modules;

public class SNRLogListener : ILogListener
{
    public static SNRLogListener Instance { get; private set; }
    public LogLevel LogLevelFilter => LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info;
    public StringBuilder logBuilder { get; private set; } = new();

    public SNRLogListener()
    {
        Instance = this;
    }

    public void Dispose()
    {
        logBuilder.Clear();
        logBuilder = null;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        logBuilder.AppendLine(eventArgs.ToStringLine());
    }
}
