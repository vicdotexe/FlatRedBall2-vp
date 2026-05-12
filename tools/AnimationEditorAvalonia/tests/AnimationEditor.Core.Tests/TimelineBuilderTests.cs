using AnimationEditor.Core.ViewModels;
using FlatRedBall2.Animation.Content;
using Xunit;

namespace AnimationEditor.Core.Tests;

public class TimelineBuilderTests
{
    [Fact]
    public void BuildFrameItems_NullChain_ReturnsEmptyList()
    {
        var result = TimelineBuilder.BuildFrameItems(null);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildFrameItems_UsesFrameLengthForRelativeWidths()
    {
        var chain = new AnimationChainSave { Name = "Run" };
        chain.Frames.Add(new AnimationFrameSave { FrameLength = 0.1f });
        chain.Frames.Add(new AnimationFrameSave { FrameLength = 0.35f });

        var result = TimelineBuilder.BuildFrameItems(chain);

        Assert.Equal(2, result.Count);
        Assert.True(result[1].Width > result[0].Width);
        Assert.Equal("1", result[0].IndexLabel);
        Assert.Equal("2", result[1].IndexLabel);
    }

    [Fact]
    public void BuildFrameItems_ClampsNegativeFrameLengthToBaseWidth()
    {
        var chain = new AnimationChainSave { Name = "Run" };
        chain.Frames.Add(new AnimationFrameSave { FrameLength = -1f });

        var result = TimelineBuilder.BuildFrameItems(chain);

        Assert.Single(result);
        Assert.Equal(TimelineBuilder.BaseCellWidth, result[0].Width);
    }
}
