using System.IO;
using FilePath = AnimationEditor.Core.Paths.FilePath;

namespace AnimationEditor.Core.IO;

/// <summary>
/// Resolves where the app-global editor settings (recent files, restored tabs, theme,
/// default-handler suppression) are stored. The location is the shared per-user config directory —
/// deliberately NOT the build output — so settings survive rebuilds, <c>dotnet clean</c>, and
/// switching between git worktrees.
/// </summary>
public static class AppSettingsLocation
{
    public const string FolderName = "AnimationEditor";
    public const string FileName = "AESettings.json";

    /// <param name="applicationDataRoot">
    /// The platform's per-user application-data root — i.e.
    /// <c>Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)</c>
    /// (Windows <c>%APPDATA%\Roaming</c>, Linux/macOS <c>~/.config</c>).
    /// </param>
    public static FilePath ForApplicationDataRoot(string applicationDataRoot) =>
        new FilePath(Path.Combine(applicationDataRoot, FolderName, FileName));
}
