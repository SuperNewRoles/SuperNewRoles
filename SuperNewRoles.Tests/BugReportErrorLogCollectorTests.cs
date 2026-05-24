using System;
using System.IO;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class BugReportErrorLogCollectorTests : IDisposable
{
    private readonly string _tempDirectory;

    public BugReportErrorLogCollectorTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SNR_BugReportErrorLogCollectorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    [Fact]
    public void ReadErrorLogText_Returns_Content_When_ErrorLog_Exists()
    {
        string expected = "sample error log";
        File.WriteAllText(Path.Combine(_tempDirectory, BugReportErrorLogCollector.ErrorLogFileName), expected);

        string actual = BugReportErrorLogCollector.ReadErrorLogText(_tempDirectory);

        actual.Should().Be(expected);
    }

    [Fact]
    public void ReadErrorLogText_Returns_Null_When_ErrorLog_Does_Not_Exist()
    {
        string actual = BugReportErrorLogCollector.ReadErrorLogText(_tempDirectory);

        actual.Should().BeNull();
    }

    [Fact]
    public void ReadErrorLogText_Returns_Null_When_ErrorLog_Is_Empty()
    {
        File.WriteAllText(Path.Combine(_tempDirectory, BugReportErrorLogCollector.ErrorLogFileName), string.Empty);

        string actual = BugReportErrorLogCollector.ReadErrorLogText(_tempDirectory);

        actual.Should().BeNull();
    }
}
