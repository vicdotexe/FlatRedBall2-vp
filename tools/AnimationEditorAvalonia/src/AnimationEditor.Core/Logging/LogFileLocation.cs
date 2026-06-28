using System;
using System.IO;
using AnimationEditor.Core.IO;
using FilePath = AnimationEditor.Core.Paths.FilePath;

namespace AnimationEditor.Core.Logging;

/// <summary>
/// Resolves where the editor writes its <c>log.txt</c>. The default is next to the running
/// executable so the user can find and share it without hunting through <c>%APPDATA%</c>;
/// if that directory isn't writable (e.g. the editor is installed under <c>Program Files</c>)
/// the location falls back to the per-user config dir that <see cref="AppSettingsLocation"/> uses.
/// </summary>
public static class LogFileLocation
{
    public const string FileName = "log.txt";

    /// <param name="baseDirectory">
    /// The directory containing the running executable — i.e. <c>AppContext.BaseDirectory</c>.
    /// </param>
    public static FilePath NextToExecutable(string baseDirectory) =>
        new FilePath(Path.Combine(baseDirectory, FileName));

    /// <param name="applicationDataRoot">
    /// The platform's per-user application-data root — i.e.
    /// <c>Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)</c>.
    /// </param>
    public static FilePath ForApplicationDataRoot(string applicationDataRoot) =>
        new FilePath(Path.Combine(applicationDataRoot, AppSettingsLocation.FolderName, FileName));

    /// <summary>
    /// Picks <see cref="NextToExecutable"/> when its directory is writable, otherwise
    /// <see cref="ForApplicationDataRoot"/>. The writability probe is injected so callers
    /// (and tests) control the side effect.
    /// </summary>
    public static FilePath Resolve(string baseDirectory, string applicationDataRoot,
        Func<string, bool> isDirectoryWritable) =>
        isDirectoryWritable(baseDirectory)
            ? NextToExecutable(baseDirectory)
            : ForApplicationDataRoot(applicationDataRoot);
}
