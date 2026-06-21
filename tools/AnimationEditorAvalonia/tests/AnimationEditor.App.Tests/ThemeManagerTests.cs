using AnimationEditor.App.Theming;
using AnimationEditor.Core.Models;
using Avalonia.Styling;
using Xunit;

namespace AnimationEditor.App.Tests;

public class ThemeManagerTests
{
    [Fact]
    public void ToVariant_Dark_ReturnsDarkVariant()
    {
        Assert.Equal(ThemeVariant.Dark, ThemeManager.ToVariant(AppTheme.Dark));
    }

    [Fact]
    public void ToVariant_Light_ReturnsLightVariant()
    {
        Assert.Equal(ThemeVariant.Light, ThemeManager.ToVariant(AppTheme.Light));
    }

    [Fact]
    public void ToVariant_System_ReturnsDefaultVariant()
    {
        // ThemeVariant.Default makes Avalonia follow the OS light/dark setting.
        Assert.Equal(ThemeVariant.Default, ThemeManager.ToVariant(AppTheme.System));
    }
}
