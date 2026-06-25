using AnimationEditor.Core.IO;
using Xunit;
using FilePath = AnimationEditor.Core.Paths.FilePath;

namespace AnimationEditor.Core.Tests;

public class AppSettingsLocationTests
{
    [Fact]
    public void ForApplicationDataRoot_PlacesFileUnderAnimationEditorSubfolder()
    {
        // Backslash literal proves the cross-platform FilePath handling (the editor may run on
        // Windows where ApplicationData is %APPDATA%\Roaming).
        var result = AppSettingsLocation.ForApplicationDataRoot(@"C:\Users\dev\AppData\Roaming");

        Assert.Equal(new FilePath(@"C:\Users\dev\AppData\Roaming\AnimationEditor\AESettings.json"), result);
    }

    [Fact]
    public void ForApplicationDataRoot_ResultIsNotInBuildOutput()
    {
        // Regression guard for #424: settings must live in the shared per-user root, never under
        // the build output (AppContext.BaseDirectory ends in bin/<Config>/<TFM>/).
        var result = AppSettingsLocation.ForApplicationDataRoot("/home/dev/.config");

        Assert.Contains("/AnimationEditor/", result.FullPath);
        Assert.EndsWith("/AESettings.json", result.FullPath);
        Assert.DoesNotContain("/bin/", result.FullPath);
    }
}
