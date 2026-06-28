using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AnimationEditor.Core.Logging;

/// <summary>
/// Minimal <see cref="ILogger"/> that appends formatted entries to a text file. Built for
/// crash logging first, but accepts any level so future warning/info/debug calls flow through
/// the same sink. Write failures are swallowed — logging must never itself crash the app.
/// </summary>
public sealed class FileLogger : ILogger
{
    private readonly string _filePath;
    private readonly Func<DateTimeOffset> _clock;
    private static readonly object _gate = new();

    /// <param name="clock">Injected for deterministic timestamps in tests; defaults to local now.</param>
    public FileLogger(string filePath, Func<DateTimeOffset>? clock = null)
    {
        _filePath = filePath;
        _clock = clock ?? (() => DateTimeOffset.Now);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        Append(FormatEntry(_clock(), logLevel, formatter(state, exception), exception));
    }

    /// <summary>
    /// Formats one entry as <c>[yyyy-MM-dd HH:mm:ss.fff] [Level] message</c>, followed by the
    /// full exception (type, message, stack) on the next lines when present.
    /// </summary>
    public static string FormatEntry(DateTimeOffset timestamp, LogLevel level, string message, Exception? exception)
    {
        var sb = new StringBuilder();
        sb.Append('[').Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] [")
          .Append(level).Append("] ").Append(message).AppendLine();
        if (exception != null)
            sb.AppendLine(exception.ToString());
        return sb.ToString();
    }

    private void Append(string text)
    {
        try
        {
            lock (_gate)
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.AppendAllText(_filePath, text);
            }
        }
        catch
        {
            // Logging must never crash the app — swallow IO failures.
        }
    }
}
