using AnimationEditor.App.Theming;
using SkiaSharp;
using Xunit;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Verifies the neutral background/chrome colors the Skia-drawn editor canvases use
/// per theme variant. The dark background must stay on the BgCanvas design token
/// (#0e0f12); the light variant must be a genuinely light color so frames, grid,
/// and rulers read correctly.
/// </summary>
public class CanvasPaletteTests
{
    [Fact]
    public void For_Dark_BackgroundMatchesBgCanvasToken()
    {
        var expected = new SKColor(0x0e, 0x0f, 0x12);

        Assert.Equal(expected, CanvasPalette.For(isDark: true).Background);
    }

    [Fact]
    public void For_Light_BackgroundIsLight()
    {
        var expected = new SKColor(0xe8, 0xea, 0xed);

        Assert.Equal(expected, CanvasPalette.For(isDark: false).Background);
    }

    [Fact]
    public void For_LightVsDark_GridLineContrastsWithBackground()
    {
        // Dark grid lines are near-white (visible on a dark canvas); light grid lines
        // must be near-black (visible on a light canvas), otherwise the grid vanishes.
        var dark = CanvasPalette.For(isDark: true);
        var light = CanvasPalette.For(isDark: false);

        Assert.True(dark.GridLine.Red > 200, "dark-mode grid line should be near-white");
        Assert.True(light.GridLine.Red < 80, "light-mode grid line should be near-black");
    }
}
