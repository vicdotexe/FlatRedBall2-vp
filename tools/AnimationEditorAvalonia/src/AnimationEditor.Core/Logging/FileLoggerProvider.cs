using Microsoft.Extensions.Logging;

namespace AnimationEditor.Core.Logging;

/// <summary>
/// <see cref="ILoggerProvider"/> backing all categories with a single <see cref="FileLogger"/>.
/// Register it via <c>ILoggingBuilder.AddProvider</c> so future code can inject
/// <see cref="ILogger{T}"/> and have it flow to <c>log.txt</c> with no call-site changes.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLogger _logger;

    public FileLoggerProvider(string filePath) => _logger = new FileLogger(filePath);

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose() { }
}
