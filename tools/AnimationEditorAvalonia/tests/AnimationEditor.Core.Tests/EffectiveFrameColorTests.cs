using System.Collections.Generic;
using AnimationEditor.Core.Rendering;
using FlatRedBall2.Animation;
using FlatRedBall2.Animation.Content;
using Xunit;

namespace AnimationEditor.Core.Tests;

/// <summary>
/// Tests for <see cref="EffectiveFrameColor"/> — the sticky "unset = keep the last value"
/// resolution the editor uses so its preview and inspector match runtime behavior.
/// </summary>
public class EffectiveFrameColorTests
{
    private static List<AnimationFrameSave> Chain(params AnimationFrameSave[] frames)
        => new(frames);

    [Fact]
    public void Resolve_ChannelSetOnEarlierFrame_CarriesForward()
    {
        // Frame 0 sets Alpha=250; frames 1 and 2 omit it → should stay 250 (sticky), not reset.
        var frames = Chain(
            new AnimationFrameSave { Alpha = 250 },
            new AnimationFrameSave(),
            new AnimationFrameSave());

        Assert.Equal(250, EffectiveFrameColor.Resolve(frames, 2).Alpha);
    }

    [Fact]
    public void Resolve_ChannelNeverSet_ReturnsNull()
    {
        var frames = Chain(new AnimationFrameSave(), new AnimationFrameSave());

        Assert.Null(EffectiveFrameColor.Resolve(frames, 1).Alpha);
    }

    [Fact]
    public void Resolve_ChannelResetOnLaterFrame_ReturnsMostRecentValue()
    {
        // Alpha changes across frames; resolving at frame 2 returns the closest preceding set (100).
        var frames = Chain(
            new AnimationFrameSave { Alpha = 250 },
            new AnimationFrameSave { Alpha = 100 },
            new AnimationFrameSave());

        Assert.Equal(100, EffectiveFrameColor.Resolve(frames, 2).Alpha);
    }

    [Fact]
    public void Resolve_ChannelsSetOnDifferentFrames_ResolveIndependently()
    {
        // Red set on frame 0, Blue set on frame 1; frame 2 inherits both.
        var frames = Chain(
            new AnimationFrameSave { Red = 200 },
            new AnimationFrameSave { Blue = 50 },
            new AnimationFrameSave());

        var resolved = EffectiveFrameColor.Resolve(frames, 2);
        Assert.Equal(200, resolved.Red);
        Assert.Equal(50, resolved.Blue);
        Assert.Null(resolved.Green);
    }

    [Fact]
    public void Resolve_OperationSetOnEarlierFrame_CarriesForward()
    {
        var frames = Chain(
            new AnimationFrameSave { ColorOperation = ColorOperation.Add },
            new AnimationFrameSave());

        Assert.Equal(ColorOperation.Add, EffectiveFrameColor.Resolve(frames, 1).Operation);
    }

    [Fact]
    public void ChannelDefault_Add_ReturnsZero()
        => Assert.Equal(0, EffectiveFrameColor.ChannelDefault(ColorOperation.Add));

    [Fact]
    public void ChannelDefault_MultiplyOrNone_Returns255()
    {
        Assert.Equal(255, EffectiveFrameColor.ChannelDefault(ColorOperation.Multiply));
        Assert.Equal(255, EffectiveFrameColor.ChannelDefault(null));
    }
}
