using System;
using System.IO;
using AnimationEditor.Core.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AnimationEditor.Core.Tests.Logging;

public class FileLoggerTests
{
    [Fact]
    public void FormatEntry_WithoutException_WritesTimestampLevelAndMessage()
    {
        var timestamp = new DateTimeOffset(2026, 6, 28, 9, 30, 15, 250, TimeSpan.Zero);

        var line = FileLogger.FormatEntry(timestamp, LogLevel.Information, "App started", null);

        Assert.Equal("[2026-06-28 09:30:15.250] [Information] App started" + Environment.NewLine, line);
    }

    [Fact]
    public void FormatEntry_WithException_IncludesExceptionDetails()
    {
        var timestamp = new DateTimeOffset(2026, 6, 28, 9, 30, 15, 0, TimeSpan.Zero);
        var exception = new InvalidOperationException("boom");

        var line = FileLogger.FormatEntry(timestamp, LogLevel.Error, "Unhandled exception", exception);

        Assert.Contains("[Error] Unhandled exception", line);
        Assert.Contains("InvalidOperationException", line);
        Assert.Contains("boom", line);
    }

    [Fact]
    public void Log_AppendsFormattedEntryToFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"frb-logtest-{Guid.NewGuid():N}.txt");
        try
        {
            ILogger logger = new FileLogger(path);

            logger.LogError(new InvalidOperationException("kaboom"), "Crash in {Source}", "Main");

            var contents = File.ReadAllText(path);
            Assert.Contains("[Error]", contents);
            Assert.Contains("Crash in Main", contents);
            Assert.Contains("kaboom", contents);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Log_AppendsAcrossMultipleCalls()
    {
        var path = Path.Combine(Path.GetTempPath(), $"frb-logtest-{Guid.NewGuid():N}.txt");
        try
        {
            ILogger logger = new FileLogger(path);

            logger.LogInformation("first");
            logger.LogInformation("second");

            var contents = File.ReadAllText(path);
            Assert.Contains("first", contents);
            Assert.Contains("second", contents);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
