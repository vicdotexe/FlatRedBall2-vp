using System.Collections.ObjectModel;
using System.Reflection;
using AnimationEditor.Core;
using AnimationEditor.Core.IO;
using AnimationEditor.Core.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using FlatRedBall2.Animation.Content;
using Xunit;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Regression coverage for App-layer mutation paths that used to bypass the undo
/// stack entirely: the flip toggle buttons and multi-select delete. All project
/// mutation must flow through <c>IUndoManager.Execute</c>.
///
/// <para>The paste path (<c>HandlePasteAsync</c>) is not covered here: its only
/// untested residue is the system-clipboard read (the headless <c>IClipboard</c>
/// has no text-write API to seed it). The mutating core it delegates to —
/// <c>IAppCommands.PasteChains/PasteFrames/PasteRectangle/PasteCircle</c> — is
/// covered by <c>AnimationEditor.Core.Tests.AppCommandsPasteTests</c>.</para>
/// </summary>
public class UndoBypassRegressionTests
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
        return (window, ctx);
    }

    private static TreeView GetTree(MainWindow w)
        => w.FindControl<TreeView>("AnimTree")!;

    private static ObservableCollection<TreeNodeVm> GetRoots(TreeView tree)
        => (ObservableCollection<TreeNodeVm>)tree.ItemsSource!;

    private static void Invoke(MainWindow window, string method)
        => typeof(MainWindow)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(window, null);

    /// <summary>Adds a chain with <paramref name="frameCount"/> shaped frames to the project.</summary>
    private static AnimationChainSave MakeChain(TestServices ctx, string name, int frameCount)
    {
        var chain = new AnimationChainSave { Name = name };
        for (int i = 0; i < frameCount; i++)
            chain.Frames.Add(new AnimationFrameSave { TextureName = $"f{i}.png", ShapesSave = new ShapesSave() });
        ctx.ProjectManager.AnimationChainListSave!.AnimationChains.Add(chain);
        return chain;
    }

    // ── Flip toggle buttons ───────────────────────────────────────────────────

    [AvaloniaFact]
    public void FlipHorizontalToggle_Checked_IsUndoable()
    {
        var (window, ctx) = CreateWindow();
        try
        {
            var chain = MakeChain(ctx, "Walk", 1);
            var frame = chain.Frames[0];
            ctx.SelectedState.SelectedFrame = frame;

            var flipH = window.FindControl<ToggleButton>("PropFlipH")!;
            flipH.IsChecked = true; // user clicks the flip button

            Assert.True(frame.FlipHorizontal);
            Assert.True(ctx.UndoManager.CanUndo);

            ctx.UndoManager.Undo();
            Assert.False(frame.FlipHorizontal);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Undo_AfterFlip_ResyncsFlipToggleButton()
    {
        var (window, ctx) = CreateWindow();
        try
        {
            var chain = MakeChain(ctx, "Walk", 1);
            var frame = chain.Frames[0];
            ctx.SelectedState.SelectedFrame = frame;

            var flipH = window.FindControl<ToggleButton>("PropFlipH")!;
            flipH.IsChecked = true;

            ctx.UndoManager.Undo();

            // The model is unflipped again, so the button must not stay lit.
            Assert.False(frame.FlipHorizontal);
            Assert.False(flipH.IsChecked == true);
        }
        finally { window.Close(); }
    }

    // ── Multi-select delete ───────────────────────────────────────────────────

    [AvaloniaFact]
    public void HandleDelete_MultipleChainsSelected_DeletesAllInOneUndoStep()
    {
        var (window, ctx) = CreateWindow();
        try
        {
            var a = MakeChain(ctx, "A", 1);
            var b = MakeChain(ctx, "B", 1);
            var tree = GetTree(window);
            var roots = GetRoots(tree);
            var vmA = new TreeNodeVm { Header = "A", Data = a };
            roots.Add(vmA);
            roots.Add(new TreeNodeVm { Header = "B", Data = b });
            tree.SelectedItem = vmA;
            ctx.SelectedState.SelectedNodes = new List<object> { a, b };

            Invoke(window, "HandleDelete");

            Assert.Empty(ctx.ProjectManager.AnimationChainListSave!.AnimationChains);
            Assert.True(ctx.UndoManager.CanUndo);

            ctx.UndoManager.Undo();
            Assert.Equal(2, ctx.ProjectManager.AnimationChainListSave!.AnimationChains.Count);
            Assert.False(ctx.UndoManager.CanUndo); // a single composite step
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void HandleDelete_MultipleRectanglesSelected_DeletesAllInOneUndoStep()
    {
        var (window, ctx) = CreateWindow();
        try
        {
            var chain = MakeChain(ctx, "Walk", 1);
            var frame = chain.Frames[0];
            var r1 = new AARectSave { Name = "R1" };
            var r2 = new AARectSave { Name = "R2" };
            frame.ShapesSave!.AARectSaves.Add(r1);
            frame.ShapesSave!.AARectSaves.Add(r2);

            var tree = GetTree(window);
            var roots = GetRoots(tree);
            var vmR1 = new TreeNodeVm { Header = "R1", Data = r1 };
            roots.Add(vmR1);
            tree.SelectedItem = vmR1;
            ctx.SelectedState.SelectedNodes = new List<object> { r1, r2 };

            Invoke(window, "HandleDelete");

            Assert.Empty(frame.ShapesSave!.AARectSaves);
            Assert.True(ctx.UndoManager.CanUndo);

            ctx.UndoManager.Undo();
            Assert.Equal(2, frame.ShapesSave!.AARectSaves.Count);
            Assert.False(ctx.UndoManager.CanUndo);
        }
        finally { window.Close(); }
    }
}
