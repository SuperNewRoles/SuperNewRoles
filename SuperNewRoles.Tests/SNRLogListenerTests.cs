using System;
using BepInEx.Logging;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class SNRLogListenerTests
{
    [Fact]
    public void GetLogText_Returns_Empty_String_When_No_Events_Were_Logged()
    {
        SNRLogListener previous = SNRLogListener.Instance;
        using var listener = new SNRLogListener();
        try
        {
            listener.GetLogText().Should().BeEmpty();
        }
        finally
        {
            RestoreListener(previous);
        }
    }

    [Fact]
    public void GetLogText_Returns_InMemory_Lines_From_This_Process()
    {
        SNRLogListener previous = SNRLogListener.Instance;
        using var listener = new SNRLogListener();
        try
        {
            var source = new ManualLogSource("SNRLogListenerTests");
            listener.LogEvent(source, new LogEventArgs("first-in-process-line", LogLevel.Info, source));
            listener.LogEvent(source, new LogEventArgs("second-in-process-line", LogLevel.Warning, source));

            string text = listener.GetLogText();
            text.Should().Contain("first-in-process-line");
            text.Should().Contain("second-in-process-line");
            text.IndexOf("first-in-process-line", StringComparison.Ordinal)
                .Should().BeLessThan(text.IndexOf("second-in-process-line", StringComparison.Ordinal));
        }
        finally
        {
            RestoreListener(previous);
        }
    }

    [Fact]
    public void GetLogText_Retains_Every_Logged_Line()
    {
        SNRLogListener previous = SNRLogListener.Instance;
        using var listener = new SNRLogListener();
        try
        {
            var source = new ManualLogSource("SNRLogListenerTests");
            const int lineCount = 5000;
            for (int i = 0; i < lineCount; i++)
                listener.LogEvent(source, new LogEventArgs($"LINE_{i}", LogLevel.Info, source));

            string text = listener.GetLogText();
            text.Should().Contain("LINE_0");
            text.Should().Contain($"LINE_{lineCount / 2}");
            text.Should().Contain($"LINE_{lineCount - 1}");
            CountOccurrences(text, "LINE_").Should().Be(lineCount);
        }
        finally
        {
            RestoreListener(previous);
        }
    }

    [Fact]
    public void GetLogText_Retains_Lines_Beyond_Three_Megabytes()
    {
        SNRLogListener previous = SNRLogListener.Instance;
        using var listener = new SNRLogListener();
        try
        {
            var source = new ManualLogSource("SNRLogListenerTests");
            const int overThreeMegabytes = (3 * 1024 * 1024) + 4096;
            listener.LogEvent(source, new LogEventArgs("UNIQUE_START_MARKER", LogLevel.Info, source));

            int remaining = overThreeMegabytes;
            const int chunkSize = 1024;
            while (remaining > 0)
            {
                int size = Math.Min(chunkSize, remaining);
                listener.LogEvent(source, new LogEventArgs(new string('x', size), LogLevel.Info, source));
                remaining -= size;
            }

            listener.LogEvent(source, new LogEventArgs("UNIQUE_END_MARKER", LogLevel.Info, source));

            string text = listener.GetLogText();
            text.Should().Contain("UNIQUE_START_MARKER");
            text.Should().Contain("UNIQUE_END_MARKER");
            text.IndexOf("UNIQUE_START_MARKER", StringComparison.Ordinal)
                .Should().BeLessThan(text.IndexOf("UNIQUE_END_MARKER", StringComparison.Ordinal));
            text.Length.Should().BeGreaterThan(overThreeMegabytes);
        }
        finally
        {
            RestoreListener(previous);
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    private static void RestoreListener(SNRLogListener previous)
    {
        if (previous != null)
            typeof(SNRLogListener).GetProperty(nameof(SNRLogListener.Instance))!
                .SetValue(null, previous);
        else if (SNRLogListener.Instance != null)
            SNRLogListener.Instance.Dispose();
    }
}
