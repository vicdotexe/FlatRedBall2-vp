using AnimationEditor.App.Services;
using Avalonia.Headless.XUnit;
using FlatRedBall2.Animation.Content;
using SkiaSharp;
using Xunit;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Issue #261: <see cref="ThumbnailService.GetFrameThumbnail"/> cropped a frame region by
/// scaling a sub-rect of the full sprite sheet with linear filtering. The sampler reached
/// past the sub-rect edges and pulled in neighbouring frames — visible colour bleed / thin
/// seam lines on the tree preview icon. The fix isolates the region first and samples it
/// with nearest-neighbour ("point") filtering.
/// </summary>
public class ThumbnailServiceTests
{
    /// <summary>
    /// Writes a sprite sheet whose left half is one colour and right half another, so a
    /// crop of one half can be checked for bleed from the other.
    /// </summary>
    private static string WriteSplitSheet(string dir, int width, int height,
                                          SKColor left, SKColor right)
    {
        var path = Path.Combine(dir, "sheet.png");
        using var bm = new SKBitmap(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                bm.SetPixel(x, y, x < width / 2 ? left : right);
        using var data = bm.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
        return path;
    }

    /// <summary>Decodes an Avalonia bitmap back into an <see cref="SKBitmap"/> for pixel inspection.</summary>
    private static SKBitmap ToSkBitmap(Avalonia.Media.Imaging.Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms);
        ms.Position = 0;
        return SKBitmap.Decode(ms);
    }

    [AvaloniaFact]
    public void GetFrameThumbnail_CroppingOneRegion_DoesNotBleedTheNeighbouringRegion()
    {
        var ctx = TestHelpers.BuildServices();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            // Left half pure red, right half pure blue.
            var sheet = WriteSplitSheet(dir, 16, 8, SKColors.Red, SKColors.Blue);

            // Crop exactly the left (red) half. An absolute TextureName needs no saved .achx.
            var frame = new AnimationFrameSave
            {
                TextureName     = sheet,
                LeftCoordinate  = 0f, TopCoordinate    = 0f,
                RightCoordinate = 0.5f, BottomCoordinate = 1f,
            };

            var thumb = ctx.ThumbnailService.GetFrameThumbnail(frame, 56, 56);
            Assert.NotNull(thumb);

            using var sk = ToSkBitmap(thumb!);
            for (int y = 0; y < sk.Height; y++)
            {
                for (int x = 0; x < sk.Width; x++)
                {
                    var p = sk.GetPixel(x, y);
                    if (p.Alpha == 0) continue;   // transparent padding, no colour to bleed
                    Assert.True(p.Red >= p.Blue,
                        $"Pixel ({x},{y}) = {p} — blue from the neighbouring region bled into the red crop.");
                }
            }
        }
        finally { Directory.Delete(dir, true); }
    }

    [AvaloniaFact]
    public void GetFrameThumbnail_UsesPointSampling_SoUpscaledArtStaysCrisp()
    {
        // A 2×1 source upscaled hugely must stay a hard red|blue split with point sampling —
        // linear filtering would smear a band of purple across the seam.
        var ctx = TestHelpers.BuildServices();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var sheet = WriteSplitSheet(dir, 2, 1, SKColors.Red, SKColors.Blue);
            var frame = new AnimationFrameSave
            {
                TextureName     = sheet,
                LeftCoordinate  = 0f, TopCoordinate    = 0f,
                RightCoordinate = 1f, BottomCoordinate = 1f,
            };

            var thumb = ctx.ThumbnailService.GetFrameThumbnail(frame, 40, 40);
            Assert.NotNull(thumb);

            using var sk = ToSkBitmap(thumb!);
            // Every pixel must be (close to) pure red or pure blue — never a blended purple.
            for (int y = 0; y < sk.Height; y++)
            {
                for (int x = 0; x < sk.Width; x++)
                {
                    var p = sk.GetPixel(x, y);
                    if (p.Alpha == 0) continue;
                    bool pureRed  = p is { Red: > 200, Blue: < 55 };
                    bool pureBlue = p is { Blue: > 200, Red: < 55 };
                    Assert.True(pureRed || pureBlue,
                        $"Pixel ({x},{y}) = {p} — point sampling should leave a hard seam, not a blended pixel.");
                }
            }
        }
        finally { Directory.Delete(dir, true); }
    }
}
