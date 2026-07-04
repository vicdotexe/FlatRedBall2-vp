using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace AnimationEditor.App.Services;

public sealed record AvailableAppUpdate(
    string Version,
    UpdateInfo UpdateInfo);

public sealed class AppUpdateService
{
    private const string RepositoryUrl =
        "https://github.com/vicdotexe/FlatRedBall2-vp";

    private readonly UpdateManager _manager;

    public AppUpdateService()
    {
        _manager = new UpdateManager(
            new GithubSource(
                repoUrl: RepositoryUrl,
                accessToken: null,
                prerelease: true));
    }

    public async Task<AvailableAppUpdate?> CheckForUpdatesAsync()
    {
        var updateInfo = await _manager.CheckForUpdatesAsync();

        if (updateInfo?.TargetFullRelease is null)
        {
            return null;
        }

        return new AvailableAppUpdate(
            Version: updateInfo.TargetFullRelease.Version.ToString(),
            UpdateInfo: updateInfo);
    }

    public async Task DownloadAndRestartAsync(AvailableAppUpdate update)
    {
        await _manager.DownloadUpdatesAsync(update.UpdateInfo);

        _manager.ApplyUpdatesAndRestart(
            update.UpdateInfo.TargetFullRelease);
    }

    public string AppVersion =>
        Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split("+")
            .FirstOrDefault()
        ?? "Unknown";
}