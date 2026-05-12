using AnimationEditor.Core;
using AnimationEditor.Core.Data;
using AnimationEditor.Core.ViewModels;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using FlatRedBall2.Animation.Content;
using System;
using System.Collections.ObjectModel;
using Xunit;

namespace AnimationEditor.App.Tests;

public class TimelineScrubberTests
{
    [AvaloniaFact]
    public void TimelineScrubber_FollowsPlaybackFrame_WhenChainSelected()
    {
        var ctx = TestHelpers.BuildServices();
        var chain = new AnimationChainSave { Name = "Run" };
        chain.Frames.Add(new AnimationFrameSave { TextureName = "a.png", FrameLength = 0.1f, ShapesSave = new ShapesSave() });
        chain.Frames.Add(new AnimationFrameSave { TextureName = "b.png", FrameLength = 0.1f, ShapesSave = new ShapesSave() });
        var acls = new AnimationChainListSave();
        acls.AnimationChains.Add(chain);
        ctx.ProjectManager.AnimationChainListSave = acls;

        var window = ctx.CreateMainWindow();
        window.Show();

        try
        {
            ctx.SelectedState.SelectedChain = chain;
            Dispatcher.UIThread.RunJobs();

            var timeline = window.FindControl<ItemsControl>("TimelineStrip")
                ?? throw new InvalidOperationException("TimelineStrip not found");
            var preview = window.FindControl<AnimationEditor.App.Controls.PreviewControl>("PreviewCtrl")
                ?? throw new InvalidOperationException("PreviewCtrl not found");

            var items = Assert.IsType<ObservableCollection<TimelineFrameVm>>(timeline.ItemsSource);
            Assert.Equal(2, items.Count);

            preview.PauseAutoPlayback();
            preview.Playback.SetChain(chain);
            preview.Playback.Play();
            Dispatcher.UIThread.RunJobs();

            Assert.True(items[0].IsCurrent);
            Assert.False(items[1].IsCurrent);

            preview.Playback.Advance(0.15);
            Dispatcher.UIThread.RunJobs();

            Assert.False(items[0].IsCurrent);
            Assert.True(items[1].IsCurrent);
        }
        finally
        {
            window.Close();
        }
    }
}
