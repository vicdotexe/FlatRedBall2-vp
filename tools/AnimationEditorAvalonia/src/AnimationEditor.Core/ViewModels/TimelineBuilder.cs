using System;
using System.Collections.Generic;
using FlatRedBall2.Animation.Content;

namespace AnimationEditor.Core.ViewModels;

public static class TimelineBuilder
{
    public const double BaseCellWidth = 24.0;
    public const double PixelsPerSecond = 120.0;

    public static List<TimelineFrameVm> BuildFrameItems(AnimationChainSave? chain)
    {
        if (chain is null)
            return [];

        var result = new List<TimelineFrameVm>(chain.Frames.Count);
        for (int i = 0; i < chain.Frames.Count; i++)
        {
            var length = Math.Max(0f, chain.Frames[i].FrameLength);
            var width = BaseCellWidth + (length * PixelsPerSecond);
            result.Add(new TimelineFrameVm(i, width));
        }

        return result;
    }
}
