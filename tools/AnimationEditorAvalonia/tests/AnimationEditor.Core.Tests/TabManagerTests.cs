using AnimationEditor.Core.Models;
using AnimationEditor.Core.Paths;
using System.Collections.Generic;
using Xunit;

namespace AnimationEditor.Core.Tests;

/// <summary>
/// Tests for <see cref="TabManager"/> — open, focus, close, dedup, and restore logic.
/// </summary>
public class TabManagerTests
{
    private static FilePath P(string path) => new FilePath(path);

    // ── OpenOrFocus ───────────────────────────────────────────────────────────

    [Fact]
    public void OpenOrFocus_FirstFile_AddsTab()
    {
        var tm = new TabManager();

        var result = tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        Assert.Equal(TabOpenResult.Opened, result);
        Assert.Single(tm.Tabs);
    }

    [Fact]
    public void OpenOrFocus_FirstFile_SetsActiveTab()
    {
        var tm = new TabManager();

        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        Assert.NotNull(tm.ActiveTab);
        Assert.Equal(P(@"C:\Games\hero.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void OpenOrFocus_SecondFile_AddsBothTabs()
    {
        var tm = new TabManager();

        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));
        tm.OpenOrFocus(P(@"C:\Games\enemy.achx"));

        Assert.Equal(2, tm.Tabs.Count);
    }

    [Fact]
    public void OpenOrFocus_SecondFile_SetsSecondAsActive()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        tm.OpenOrFocus(P(@"C:\Games\enemy.achx"));

        Assert.Equal(P(@"C:\Games\enemy.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void OpenOrFocus_DuplicatePath_ReturnsFocused()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));
        tm.OpenOrFocus(P(@"C:\Games\enemy.achx"));

        var result = tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        Assert.Equal(TabOpenResult.Focused, result);
    }

    [Fact]
    public void OpenOrFocus_DuplicatePath_DoesNotAddNewTab()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        Assert.Single(tm.Tabs);
    }

    [Fact]
    public void OpenOrFocus_DuplicatePath_ActivatesExistingTab()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));
        tm.OpenOrFocus(P(@"C:\Games\enemy.achx"));

        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        Assert.Equal(P(@"C:\Games\hero.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void OpenOrFocus_DuplicatePathDifferentCase_Deduplicates()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\Hero.achx"));

        var result = tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        Assert.Equal(TabOpenResult.Focused, result);
        Assert.Single(tm.Tabs);
    }

    // ── Close ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Close_OnlyTab_ClearsAll()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));

        tm.Close(P(@"C:\Games\hero.achx"));

        Assert.Empty(tm.Tabs);
        Assert.Null(tm.ActiveTab);
    }

    [Fact]
    public void Close_ActiveTabWithNext_ActivatesNextTab()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\a.achx"));
        tm.OpenOrFocus(P(@"C:\Games\b.achx"));
        tm.OpenOrFocus(P(@"C:\Games\c.achx"));
        tm.Activate(P(@"C:\Games\b.achx"));

        tm.Close(P(@"C:\Games\b.achx"));

        Assert.Equal(P(@"C:\Games\c.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void Close_ActiveLastTab_ActivatesPreviousTab()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\a.achx"));
        tm.OpenOrFocus(P(@"C:\Games\b.achx"));

        tm.Close(P(@"C:\Games\b.achx"));

        Assert.Equal(P(@"C:\Games\a.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void Close_NonActiveTab_DoesNotChangeActiveTab()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\a.achx"));
        tm.OpenOrFocus(P(@"C:\Games\b.achx"));
        // b is active
        tm.Close(P(@"C:\Games\a.achx"));

        Assert.Equal(P(@"C:\Games\b.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void Close_UnknownPath_IsNoOp()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\hero.achx"));
        int before = tm.Tabs.Count;

        tm.Close(P(@"C:\Games\unknown.achx"));

        Assert.Equal(before, tm.Tabs.Count);
    }

    // ── Activate ─────────────────────────────────────────────────────────────

    [Fact]
    public void Activate_KnownPath_SetsActiveTab()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\a.achx"));
        tm.OpenOrFocus(P(@"C:\Games\b.achx"));

        tm.Activate(P(@"C:\Games\a.achx"));

        Assert.Equal(P(@"C:\Games\a.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void Activate_UnknownPath_IsNoOp()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\a.achx"));

        tm.Activate(P(@"C:\Games\unknown.achx"));

        Assert.Equal(P(@"C:\Games\a.achx"), tm.ActiveTab!.Path);
    }

    // ── RestoreFrom ───────────────────────────────────────────────────────────

    [Fact]
    public void RestoreFrom_EmptyList_TabsStayEmpty()
    {
        var tm = new TabManager();

        tm.RestoreFrom(new List<string>(), activePath: null);

        Assert.Empty(tm.Tabs);
        Assert.Null(tm.ActiveTab);
    }

    [Fact]
    public void RestoreFrom_ListWithPaths_CreatesTabs()
    {
        var tm = new TabManager();

        tm.RestoreFrom(
            new List<string> { @"C:\Games\a.achx", @"C:\Games\b.achx" },
            activePath: null);

        Assert.Equal(2, tm.Tabs.Count);
    }

    [Fact]
    public void RestoreFrom_WithActivePath_SetsCorrectActiveTab()
    {
        var tm = new TabManager();

        tm.RestoreFrom(
            new List<string> { @"C:\Games\a.achx", @"C:\Games\b.achx" },
            activePath: @"C:\Games\b.achx");

        Assert.Equal(P(@"C:\Games\b.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void RestoreFrom_ActivePathNotInList_FirstTabBecomesActive()
    {
        var tm = new TabManager();

        tm.RestoreFrom(
            new List<string> { @"C:\Games\a.achx" },
            activePath: @"C:\Games\gone.achx");

        Assert.Equal(P(@"C:\Games\a.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void RestoreFrom_ClearsExistingTabs()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\old.achx"));

        tm.RestoreFrom(new List<string> { @"C:\Games\new.achx" }, activePath: null);

        Assert.Single(tm.Tabs);
        Assert.Equal(P(@"C:\Games\new.achx"), tm.Tabs[0].Path);
    }

    // ── OpenTabPaths helper ───────────────────────────────────────────────────

    [Fact]
    public void OpenTabPaths_ReturnsFullPathsForAllTabs()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\a.achx"));
        tm.OpenOrFocus(P(@"C:\Games\b.achx"));

        var paths = tm.OpenTabPaths;

        // FilePath normalises separators to '/'; compare via FilePath equality.
        Assert.Equal(2, paths.Count);
        Assert.Equal(P(@"C:\Games\a.achx"), new FilePath(paths[0]));
        Assert.Equal(P(@"C:\Games\b.achx"), new FilePath(paths[1]));
    }

    // ── ActiveChanged event ───────────────────────────────────────────────────

    [Fact]
    public void OpenOrFocus_FirstFile_RaisesActiveChanged()
    {
        var tm = new TabManager();
        bool raised = false;
        tm.ActiveChanged += _ => raised = true;

        tm.OpenOrFocus(P(@"C:\Games\a.achx"));

        Assert.True(raised);
    }

    [Fact]
    public void Close_RaisesActiveChanged()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\a.achx"));
        bool raised = false;
        tm.ActiveChanged += _ => raised = true;

        tm.Close(P(@"C:\Games\a.achx"));

        Assert.True(raised);
    }

    // ── RegisterBackground ────────────────────────────────────────────────────

    [Fact]
    public void RegisterBackground_AddsTabAtPositionZero()
    {
        var tm = new TabManager();

        tm.RegisterBackground(P(@"C:\Games\existing.achx"));

        Assert.Single(tm.Tabs);
        Assert.Equal(P(@"C:\Games\existing.achx"), tm.Tabs[0].Path);
    }

    [Fact]
    public void RegisterBackground_DoesNotActivateTab()
    {
        var tm = new TabManager();

        tm.RegisterBackground(P(@"C:\Games\existing.achx"));

        Assert.Null(tm.ActiveTab);
    }

    [Fact]
    public void RegisterBackground_DoesNotRaiseActiveChanged()
    {
        var tm = new TabManager();
        bool raised = false;
        tm.ActiveChanged += _ => raised = true;

        tm.RegisterBackground(P(@"C:\Games\existing.achx"));

        Assert.False(raised);
    }

    [Fact]
    public void RegisterBackground_IsNoOpIfPathAlreadyPresent()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\existing.achx"));

        tm.RegisterBackground(P(@"C:\Games\existing.achx"));

        Assert.Single(tm.Tabs);
    }

    [Fact]
    public void RegisterBackground_InsertsBeforeExistingTabs()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\Games\second.achx"));

        tm.RegisterBackground(P(@"C:\Games\first.achx"));

        Assert.Equal(2, tm.Tabs.Count);
        Assert.Equal(P(@"C:\Games\first.achx"), tm.Tabs[0].Path);
        Assert.Equal(P(@"C:\Games\second.achx"), tm.Tabs[1].Path);
    }

    [Fact]
    public void RegisterBackground_AfterRegister_OpenOrFocusActivatesNewFileButKeepsBoth()
    {
        var tm = new TabManager();
        tm.RegisterBackground(P(@"C:\Games\existing.achx"));

        tm.OpenOrFocus(P(@"C:\Games\new.achx"));

        Assert.Equal(2, tm.Tabs.Count);
        Assert.Equal(P(@"C:\Games\existing.achx"), tm.Tabs[0].Path);
        Assert.Equal(P(@"C:\Games\new.achx"), tm.Tabs[1].Path);
        Assert.Equal(P(@"C:\Games\new.achx"), tm.ActiveTab!.Path);
    }

    [Fact]
    public void RegisterBackground_WithDisplayNameOverride_UsesOverrideInDisplayName()
    {
        var tm = new TabManager();

        tm.RegisterBackground(P(@"C:\Games\existing.achx"), "My Custom Label");

        Assert.Single(tm.Tabs);
        Assert.Equal("My Custom Label", tm.Tabs[0].DisplayName);
    }

    // ── TabEntry.DisplayName ──────────────────────────────────────────────────

    [Fact]
    public void TabEntry_DisplayName_UsesOverrideWhenProvided()
    {
        var entry = new TabEntry(P(@"C:\Games\foo.achx"), "My Override");

        Assert.Equal("My Override", entry.DisplayName);
    }

    [Fact]
    public void TabEntry_DisplayName_FallsBackToNoPathWhenNoOverride()
    {
        var entry = new TabEntry(P(@"C:\Games\foo.achx"));

        Assert.Equal("foo.achx", entry.DisplayName);
    }

    [Fact]
    public void TabEntry_DisplayName_ReturnsUntitledWhenPathOriginalIsNullOrEmpty()
    {
        // Construct via RegisterBackground with an empty-original FilePath sentinel.
        var entry = new TabEntry(new FilePath(null!));

        Assert.Equal("Untitled", entry.DisplayName);
    }

    // ── Move ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Move_FirstToLast_ReordersCorrectly()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));
        tm.OpenOrFocus(P(@"C:\c.achx"));

        tm.Move(P(@"C:\a.achx"), 2);

        Assert.Equal("a.achx", tm.Tabs[2].Path.NoPath);
        Assert.Equal("b.achx", tm.Tabs[0].Path.NoPath);
        Assert.Equal("c.achx", tm.Tabs[1].Path.NoPath);
    }

    [Fact]
    public void Move_LastToFirst_ReordersCorrectly()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));
        tm.OpenOrFocus(P(@"C:\c.achx"));

        tm.Move(P(@"C:\c.achx"), 0);

        Assert.Equal("c.achx", tm.Tabs[0].Path.NoPath);
        Assert.Equal("a.achx", tm.Tabs[1].Path.NoPath);
        Assert.Equal("b.achx", tm.Tabs[2].Path.NoPath);
    }

    [Fact]
    public void Move_MiddleToEnd_ReordersCorrectly()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));
        tm.OpenOrFocus(P(@"C:\c.achx"));

        tm.Move(P(@"C:\b.achx"), 2);

        Assert.Equal("a.achx", tm.Tabs[0].Path.NoPath);
        Assert.Equal("c.achx", tm.Tabs[1].Path.NoPath);
        Assert.Equal("b.achx", tm.Tabs[2].Path.NoPath);
    }

    [Fact]
    public void Move_SameIndex_IsNoOp()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));

        tm.Move(P(@"C:\a.achx"), 0);

        Assert.Equal("a.achx", tm.Tabs[0].Path.NoPath);
        Assert.Equal("b.achx", tm.Tabs[1].Path.NoPath);
    }

    [Fact]
    public void Move_NegativeIndex_ClampsToZero()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));
        tm.OpenOrFocus(P(@"C:\c.achx"));

        tm.Move(P(@"C:\c.achx"), -5);

        Assert.Equal("c.achx", tm.Tabs[0].Path.NoPath);
    }

    [Fact]
    public void Move_IndexBeyondEnd_ClampsToLast()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));
        tm.OpenOrFocus(P(@"C:\c.achx"));

        tm.Move(P(@"C:\a.achx"), 99);

        Assert.Equal("a.achx", tm.Tabs[2].Path.NoPath);
    }

    [Fact]
    public void Move_UnknownPath_IsNoOp()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));

        tm.Move(P(@"C:\nonexistent.achx"), 0); // must not throw

        Assert.Equal(2, tm.Tabs.Count);
        Assert.Equal("a.achx", tm.Tabs[0].Path.NoPath);
    }

    [Fact]
    public void Move_DoesNotChangeActiveTab()
    {
        var tm = new TabManager();
        tm.OpenOrFocus(P(@"C:\a.achx"));
        tm.OpenOrFocus(P(@"C:\b.achx"));
        tm.OpenOrFocus(P(@"C:\c.achx")); // c is active

        tm.Move(P(@"C:\a.achx"), 2);

        Assert.Equal(P(@"C:\c.achx"), tm.ActiveTab!.Path);
    }
}
