namespace AnimationEditor.Core.Tests;

/// <summary>
/// Platform-agnostic fake absolute paths for tests that exercise path logic but never
/// touch the real filesystem.
/// </summary>
/// <remarks>
/// Use <see cref="Abs"/> instead of hardcoded <c>@"C:\..."</c> literals so tests pass on
/// both Windows (where <c>Path.IsPathRooted(@"C:\foo")</c> is true) and Linux/macOS
/// (where it is false and <c>Path.GetFullPath</c> would produce garbage).
/// <para>
/// Two roots are provided so tests can represent paths on distinct drives or volumes:
/// <list type="bullet">
///   <item><c>Abs()</c> — primary root, e.g. "project" files</item>
///   <item><c>AltAbs()</c> — secondary root, e.g. "external" files outside the project</item>
/// </list>
/// </para>
/// </remarks>
internal static class TestPaths
{
    private static readonly string Root    = OperatingSystem.IsWindows() ? @"C:\TestRoot" : "/TestRoot";
    private static readonly string AltRoot = OperatingSystem.IsWindows() ? @"D:\AltRoot"  : "/AltRoot";

    /// <summary>
    /// Returns a platform-valid absolute path under the primary test root.
    /// E.g. <c>Abs("Project", "Animations", "Hero.achx")</c> →
    ///   <c>"C:\TestRoot\Project\Animations\Hero.achx"</c> on Windows,
    ///   <c>"/TestRoot/Project/Animations/Hero.achx"</c> on Linux/macOS.
    /// </summary>
    public static string Abs(params string[] segments) =>
        Path.Combine(new[] { Root }.Concat(segments).ToArray());

    /// <summary>
    /// Like <see cref="Abs"/> but appends the platform directory separator.
    /// Use when a test needs a directory path (trailing slash).
    /// </summary>
    public static string AbsDir(params string[] segments) =>
        Abs(segments) + Path.DirectorySeparatorChar;

    /// <summary>
    /// Returns a platform-valid absolute path under a secondary test root (distinct from
    /// <see cref="Abs"/>). Use for paths that must be outside the primary root.
    /// </summary>
    public static string AltAbs(params string[] segments) =>
        Path.Combine(new[] { AltRoot }.Concat(segments).ToArray());

    /// <summary>
    /// Returns an absolute path that is almost certainly unwritable on the current OS.
    /// Use in tests that verify graceful failure when writing to an invalid location.
    /// </summary>
    public static string InvalidPath(string filename) =>
        OperatingSystem.IsWindows()
            ? Path.Combine(@"Z:\NonExistentDrive", filename)
            : $"/nonexistent-testroot-34f9e2/{filename}";
}
