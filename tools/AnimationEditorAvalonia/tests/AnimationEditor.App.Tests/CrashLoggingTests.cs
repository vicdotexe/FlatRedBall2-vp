using System;
using System.IO;
using AnimationEditor.App.Services;
using AnimationEditor.Core.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Verifies the handler-to-logger wiring (issue #480): invoking the crash-log delegate
/// with a synthetic exception produces a log line. The actual event subscriptions and
/// <c>Process.Start</c> remain the thin untested boundary.
/// </summary>
public class CrashLoggingTests
{
    [Fact]
    public void Log_WithException_WritesSourceAndExceptionToSink()
    {
        var path = Path.Combine(Path.GetTempPath(), $"frb-crashlog-{Guid.NewGuid():N}.txt");
        try
        {
            ILogger logger = new FileLogger(path);

            CrashLogging.Log(logger, "Dispatcher.UnhandledException", new InvalidOperationException("ui-thread-boom"));

            var contents = File.ReadAllText(path);
            Assert.Contains("Dispatcher.UnhandledException", contents);
            Assert.Contains("ui-thread-boom", contents);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
