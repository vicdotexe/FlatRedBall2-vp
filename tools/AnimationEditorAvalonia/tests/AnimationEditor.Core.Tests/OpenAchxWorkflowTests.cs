using AnimationEditor.Core;
using AnimationEditor.Core.CommandsAndState;
using AnimationEditor.Core.Paths;
using FlatRedBall2.Animation.Content;
using System.IO;
using Xunit;

namespace AnimationEditor.Core.Tests;

/// <summary>
/// Verifies that <see cref="IAppCommands.OpenAchxWorkflow"/> correctly sequences
/// the open flow (load data, notify WireframeCtrl/PreviewCtrl via AchxLoaded,
/// then raise CurrentFileChanged and AvailableTexturesChanged for the UI layer)
/// — all without any Avalonia dependency.
/// </summary>
[Collection("SequentialSingletons")]
public class OpenAchxWorkflowTests : IDisposable
{
    private readonly TestHelpers.TempDir _dir;
    private readonly TestServices _ctx;

    public OpenAchxWorkflowTests()
    {
        _dir = new TestHelpers.TempDir();
        _ctx = TestHelpers.SetupFreshAcls();
    }

    public void Dispose() => _dir.Dispose();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string WriteMinimalAchx(string chainName = "Idle")
    {
        var path = Path.Combine(_dir.Path, "test.achx");
        var acls = new AnimationChainListSave();
        acls.AnimationChains.Add(new AnimationChainSave { Name = chainName });
        acls.Save(path);
        return path;
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    [Fact]
    public void OpenAchxWorkflow_WithValidFile_LoadsChainIntoProjectManager()
    {
        var path = WriteMinimalAchx("Walk");

        _ctx.AppCommands.OpenAchxWorkflow(path);

        Assert.NotNull(_ctx.ProjectManager.AnimationChainListSave);
        Assert.Single(_ctx.ProjectManager.AnimationChainListSave!.AnimationChains);
        Assert.Equal("Walk", _ctx.ProjectManager.AnimationChainListSave.AnimationChains[0].Name);
    }

    [Fact]
    public void OpenAchxWorkflow_WithValidFile_SetsProjectManagerFileName()
    {
        var path = WriteMinimalAchx();

        _ctx.AppCommands.OpenAchxWorkflow(path);

        Assert.Equal(new FilePath(path), new FilePath(_ctx.ProjectManager.FileName!));
    }

    // ── Event ordering ────────────────────────────────────────────────────────

    [Fact]
    public void OpenAchxWorkflow_AchxLoadedFiresAfterDataIsLoaded()
    {
        var path = WriteMinimalAchx("Run");
        FilePath? fileNameAtNotification = null;
        _ctx.ApplicationEvents.AchxLoaded += _ =>
            fileNameAtNotification = _ctx.ProjectManager.FileName;

        _ctx.AppCommands.OpenAchxWorkflow(path);

        // AchxLoaded fires after LoadAnimationChain sets the FileName
        Assert.Equal(new FilePath(path), fileNameAtNotification);
    }

    [Fact]
    public void OpenAchxWorkflow_WithValidFile_FiresAchxLoadedWithPath()
    {
        var path = WriteMinimalAchx();
        string? received = null;
        _ctx.ApplicationEvents.AchxLoaded += p => received = p;

        _ctx.AppCommands.OpenAchxWorkflow(path);

        Assert.Equal(new FilePath(path), new FilePath(received!));
    }

    // ── CurrentFileChanged ────────────────────────────────────────────────────

    [Fact]
    public void OpenAchxWorkflow_WithValidFile_FiresCurrentFileChangedWithPath()
    {
        var path = WriteMinimalAchx();
        string? received = null;
        _ctx.ApplicationEvents.CurrentFileChanged += p => received = p;

        _ctx.AppCommands.OpenAchxWorkflow(path);

        Assert.Equal(new FilePath(path), new FilePath(received!));
    }

    [Fact]
    public void OpenAchxWorkflow_CurrentFileChangedFiresAfterDataIsLoaded()
    {
        var path = WriteMinimalAchx("Jump");
        FilePath? fileNameAtNotification = null;
        _ctx.ApplicationEvents.CurrentFileChanged += _ =>
            fileNameAtNotification = _ctx.ProjectManager.FileName;

        _ctx.AppCommands.OpenAchxWorkflow(path);

        Assert.Equal(new FilePath(path), fileNameAtNotification);
    }

    // ── AvailableTexturesChanged ───────────────────────────────────────────────

    [Fact]
    public void OpenAchxWorkflow_WithValidFile_FiresAvailableTexturesChanged()
    {
        var path = WriteMinimalAchx();
        bool fired = false;
        _ctx.ApplicationEvents.AvailableTexturesChanged += () => fired = true;

        _ctx.AppCommands.OpenAchxWorkflow(path);

        Assert.True(fired);
    }

    // ── Non-existent file (graceful no-op) ────────────────────────────────────

    [Fact]
    public void OpenAchxWorkflow_WithNonExistentFile_StillFiresCurrentFileChangedAndAvailableTextures()
    {
        // Pre-existing behaviour: LoadAnimationChain silently skips when file is
        // absent, but the UI still receives the events (keeps parity with old HandleAchxLoaded).
        var path = Path.Combine(_dir.Path, "ghost.achx");
        bool currentFileFired = false;
        bool texturesFired = false;
        _ctx.ApplicationEvents.CurrentFileChanged += _ => currentFileFired = true;
        _ctx.ApplicationEvents.AvailableTexturesChanged += () => texturesFired = true;

        _ctx.AppCommands.OpenAchxWorkflow(path);

        Assert.True(currentFileFired);
        Assert.True(texturesFired);
    }
}
