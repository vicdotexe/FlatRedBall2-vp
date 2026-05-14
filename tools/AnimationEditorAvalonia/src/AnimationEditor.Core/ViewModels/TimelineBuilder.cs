using System;
using System.Collections.Generic;
using FlatRedBall2.Animation.Content;

namespace AnimationEditor.Core.ViewModels;

public static class TimelineBuilder
{
    /// <summary>
    /// Minimum cell width in pixels applied only to zero-length and negative-length frames.
    /// Keeps degenerate frames visible and clickable. Normal frames use purely proportional widths
    /// so the playhead moves at a constant <see cref="PixelsPerSecond"/> px/s across all cells.
    /// </summary>
    public const double MinCellWidth = 24.0;
    public const double PixelsPerSecond = 120.0;

    public static List<TimelineFrameVm> BuildFrameItems(AnimationChainSave? chain)
    {
        if (chain is null)
            return [];

        var result = new List<TimelineFrameVm>(chain.Frames.Count);
        for (int i = 0; i < chain.Frames.Count; i++)
        {
            var length = Math.Max(0f, chain.Frames[i].FrameLength);
            var width = length > 0 ? length * PixelsPerSecond : MinCellWidth;
            result.Add(new TimelineFrameVm(i, width));
        }

        return result;
    }
}
