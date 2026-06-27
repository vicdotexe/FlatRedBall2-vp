using AnimationEditor.Core;
using AnimationEditor.Core.IO;
using AnimationEditor.Core.ViewModels;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using FlatRedBall2.Animation.Content;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Structure tests for the tree right-click menu (issue #454 follow-up):
/// Duplicate is a single item grouped with Copy/Paste, and for chains it is a
/// submenu (Original / Flip Horizontal / Flip Vertical) rather than three
/// top-level items.
/// </summary>
public class TreeContextMenuTests
{
    private static (MainWindow Window, TestServices Ctx) CreateWindow()
    {
        var ctx = TestHelpers.BuildServices();
        ctx.ProjectManager.AnimationChainListSave = new AnimationChainListSave();
        ctx.ProjectManager.FileName = null;
        ctx.SelectedState.SelectedChain = null;
        ctx.AppCommands.ConfirmAsync = (_, _) => Task.FromResult(true);
        ctx.AppCommands.FileDialogService = NullFileDialogService.Instance;

        var window = ctx.CreateMainWindow();
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, ctx);
    }

    // Selects a tree node and rebuilds the context menu by invoking the real
    // Opening handler (the popup itself doesn't open under the headless backend,
    // so we drive the builder directly). Returns its top-level items.
    private static List<object> OpenMenuFor(MainWindow window, object data, string header)
    {
        var tree = window.FindControl<TreeView>("AnimTree")!;
        var roots = (ObservableCollection<TreeNodeVm>)tree.ItemsSource!;
        var vm = new TreeNodeVm { Header = header, Data = data };
        roots.Add(vm);
        tree.SelectedItems!.Add(vm);
        Dispatcher.UIThread.RunJobs();

        typeof(MainWindow)
            .GetMethod("OnTreeContextMenuOpening", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(window, new object?[] { null, new CancelEventArgs() });
        return tree.ContextMenu!.Items.Cast<object>().ToList();
    }

    private static int IndexOfItem(List<object> items, string header) =>
        items.FindIndex(o => o is MenuItem m && (string?)m.Header == header);

    [AvaloniaFact]
    public void ChainMenu_DuplicateIsSubmenuWithThreeVariants()
    {
        var (window, ctx) = CreateWindow();
        try
        {
            var chain = new AnimationChainSave { Name = "Run" };
            ctx.ProjectManager.AnimationChainListSave!.AnimationChains.Add(chain);
            var items = OpenMenuFor(window, chain, "Run");

            // Exactly one "Duplicate" entry (no "Duplicate (original)" etc. at top level).
            var duplicate = items.OfType<MenuItem>().Single(m => (string?)m.Header == "Duplicate");
            var children = duplicate.Items.OfType<MenuItem>().Select(m => (string?)m.Header).ToList();
            Assert.Equal(new[] { "Original", "Flip Horizontal", "Flip Vertical" }, children);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ChainMenu_CopyPasteDuplicateAreConsecutive()
    {
        var (window, ctx) = CreateWindow();
        try
        {
            var chain = new AnimationChainSave { Name = "Run" };
            ctx.ProjectManager.AnimationChainListSave!.AnimationChains.Add(chain);
            var items = OpenMenuFor(window, chain, "Run");

            int copy = IndexOfItem(items, "Copy");
            Assert.True(copy >= 0);
            Assert.Equal(copy + 1, IndexOfItem(items, "Paste"));
            Assert.Equal(copy + 2, IndexOfItem(items, "Duplicate"));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void FrameMenu_CopyPasteDuplicateAreConsecutive()
    {
        var (window, ctx) = CreateWindow();
        try
        {
            var chain = new AnimationChainSave { Name = "Walk" };
            var frame = new AnimationFrameSave { TextureName = "run.png" };
            chain.Frames.Add(frame);
            ctx.ProjectManager.AnimationChainListSave!.AnimationChains.Add(chain);
            var items = OpenMenuFor(window, frame, "run.png");

            int copy = IndexOfItem(items, "Copy");
            Assert.True(copy >= 0);
            Assert.Equal(copy + 1, IndexOfItem(items, "Paste"));
            Assert.Equal(copy + 2, IndexOfItem(items, "Duplicate"));
        }
        finally { window.Close(); }
    }
}
