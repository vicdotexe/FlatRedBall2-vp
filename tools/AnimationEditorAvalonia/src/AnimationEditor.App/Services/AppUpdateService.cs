using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace AnimationEditor.App.Services;

public enum AppUpdateChannel
{
    Release,
    PreRelease,
    Test,
}

public static class AppUpdateChannelExtensions
{
    public static AppUpdateChannel[] All { get; } =
    {
        AppUpdateChannel.Release,
        AppUpdateChannel.PreRelease,
        AppUpdateChannel.Test,
    };

    public static string GetDisplayName(this AppUpdateChannel channel) =>
        channel switch
        {
            AppUpdateChannel.Release => "Release",
            AppUpdateChannel.PreRelease => "PreRelease",
            AppUpdateChannel.Test => "Test",
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null),
        };

    internal static string GetFeedSuffix(this AppUpdateChannel channel) =>
        channel switch
        {
            AppUpdateChannel.Release => "release",
            AppUpdateChannel.PreRelease => "prerelease",
            AppUpdateChannel.Test => "test",
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null),
        };
}

public sealed record AvailableAppUpdate(
    string Version,
    AppUpdateChannel TargetChannel,
    bool IsChannelSwitch,
    bool IsDowngrade,
    UpdateInfo UpdateInfo)
{
    // Keep the exact manager that checked this update. For a channel switch,
    // it contains the explicit target channel and downgrade settings needed
    // when downloading/applying the package.
    internal UpdateManager Manager { get; init; } = null!;
}

public sealed class AppUpdateService
{
    private const string RepositoryUrl =
        "https://github.com/vicdotexe/FlatRedBall2-vp";

    private readonly IVelopackLocator _locator;
    private readonly UpdateManager _defaultManager;

    public AppUpdateService()
    {
        _locator = VelopackLocator.Current;
        _defaultManager = CreateManager();
    }

    /// <summary>
    /// True when the app is running from a Velopack installer or portable
    /// package capable of receiving updates.
    /// </summary>
    public bool SupportsAutomaticUpdates =>
        _defaultManager.IsInstalled;

    /// <summary>
    /// True when the current package supports updates and has one of this
    /// application's expected channels: release, prerelease, or test.
    /// </summary>
    public bool CanCheckForUpdates =>
        SupportsAutomaticUpdates &&
        CurrentChannel is not null;

    /// <summary>
    /// The internal Velopack channel, such as "win-x64-release".
    /// This remains an implementation detail; display CurrentChannel instead.
    /// </summary>
    public string CurrentVelopackChannel =>
        _locator.Channel ?? "Missing";

    /// <summary>
    /// The user-facing channel embedded in the currently running package.
    /// </summary>
    public AppUpdateChannel? CurrentChannel =>
        TryParseChannel(CurrentVelopackChannel);

    public string CurrentChannelDisplayName =>
        CurrentChannel?.GetDisplayName() ?? "Unknown";

    public string AppVersion =>
        _defaultManager.CurrentVersion?.ToString()
        ?? Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion?
            .Split('+')
            .FirstOrDefault()
        ?? "Unknown";

    /// <summary>
    /// Checks the selected channel.
    ///
    /// Same-channel checks use ordinary update behavior: only newer packages
    /// are offered.
    ///
    /// Cross-channel checks explicitly target the selected channel and permit
    /// a downgrade or same-version lateral switch.
    /// </summary>
    public async Task<AvailableAppUpdate?> CheckForUpdatesAsync(
        AppUpdateChannel targetChannel)
    {
        var currentChannel = GetCurrentChannelOrThrow();
        var isChannelSwitch = currentChannel != targetChannel;

        var manager = isChannelSwitch
            ? CreateManager(
                new UpdateOptions
                {
                    ExplicitChannel = GetVelopackChannel(targetChannel),
                    AllowVersionDowngrade = true,
                })
            : _defaultManager;

        var updateInfo = await manager.CheckForUpdatesAsync();

        if (updateInfo is null)
        {
            return null;
        }

        return new AvailableAppUpdate(
            Version: updateInfo.TargetFullRelease.Version.ToString(),
            TargetChannel: targetChannel,
            IsChannelSwitch: isChannelSwitch,
            IsDowngrade: updateInfo.IsDowngrade,
            UpdateInfo: updateInfo)
        {
            Manager = manager,
        };
    }

    public async Task DownloadAndRestartAsync(
        AvailableAppUpdate update)
    {
        await update.Manager.DownloadUpdatesAsync(update.UpdateInfo);

        update.Manager.ApplyUpdatesAndRestart(
            update.UpdateInfo.TargetFullRelease);
    }

    private static UpdateManager CreateManager(
        UpdateOptions? options = null)
    {
        return new UpdateManager(
            new GithubSource(
                repoUrl: RepositoryUrl,
                accessToken: null,
                prerelease: true),
            options);
    }

    private AppUpdateChannel GetCurrentChannelOrThrow()
    {
        if (!SupportsAutomaticUpdates)
        {
            throw new InvalidOperationException(
                "Automatic updates are available only when running a Velopack-managed release or portable package.");
        }

        return CurrentChannel
            ?? throw new InvalidOperationException(
                $"The running package has an unsupported update channel: '{CurrentVelopackChannel}'.");
    }

    private string GetVelopackChannel(
        AppUpdateChannel targetChannel)
    {
        if (!TryGetPlatformPrefix(
                CurrentVelopackChannel,
                out var platformPrefix))
        {
            throw new InvalidOperationException(
                $"The running package channel '{CurrentVelopackChannel}' does not match the expected format.");
        }

        return $"{platformPrefix}-{targetChannel.GetFeedSuffix()}";
    }

    private static AppUpdateChannel? TryParseChannel(
        string velopackChannel)
    {
        foreach (var channel in AppUpdateChannelExtensions.All)
        {
            var suffix = $"-{channel.GetFeedSuffix()}";

            if (velopackChannel.EndsWith(
                    suffix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return channel;
            }
        }

        return null;
    }

    private static bool TryGetPlatformPrefix(
        string velopackChannel,
        out string platformPrefix)
    {
        foreach (var channel in AppUpdateChannelExtensions.All)
        {
            var suffix = $"-{channel.GetFeedSuffix()}";

            if (!velopackChannel.EndsWith(
                    suffix,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            platformPrefix = velopackChannel[..^suffix.Length];

            return !string.IsNullOrWhiteSpace(platformPrefix);
        }

        platformPrefix = string.Empty;
        return false;
    }
}