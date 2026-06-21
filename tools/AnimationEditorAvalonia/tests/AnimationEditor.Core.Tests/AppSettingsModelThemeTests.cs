using System.Text.Json;
using AnimationEditor.Core.Models;
using Xunit;

namespace AnimationEditor.Core.Tests;

public class AppSettingsModelThemeTests
{
    [Fact]
    public void Theme_Default_IsDark()
    {
        // Dark preserves the editor's historical look for users upgrading from a
        // build that had no theme setting (and for settings files predating Theme).
        var settings = new AppSettingsModel();

        Assert.Equal(AppTheme.Dark, settings.Theme);
    }

    [Fact]
    public void Theme_MissingFromJson_DefaultsToDark()
    {
        // A settings file written before the Theme field existed must deserialize to Dark.
        var json = "{\"RecentFiles\":[],\"OpenTabPaths\":[]}";

        var settings = JsonSerializer.Deserialize<AppSettingsModel>(json);

        Assert.NotNull(settings);
        Assert.Equal(AppTheme.Dark, settings!.Theme);
    }

    [Fact]
    public void Theme_RoundTripsThroughJson()
    {
        var settings = new AppSettingsModel { Theme = AppTheme.Light };

        var json = JsonSerializer.Serialize(settings);
        var restored = JsonSerializer.Deserialize<AppSettingsModel>(json);

        Assert.NotNull(restored);
        Assert.Equal(AppTheme.Light, restored!.Theme);
    }
}
