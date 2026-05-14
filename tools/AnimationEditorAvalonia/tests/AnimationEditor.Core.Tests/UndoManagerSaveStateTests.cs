using AnimationEditor.Core.CommandsAndState.Commands;
using Xunit;

namespace AnimationEditor.Core.Tests;

public class UndoManagerSaveStateTests
{
    private readonly UndoManager _undo = new();

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void HasUnsavedChanges_TrueInitially()
    {
        Assert.True(_undo.HasUnsavedChanges);
    }

    // ── MarkSaved ─────────────────────────────────────────────────────────────

    [Fact]
    public void MarkSaved_SetsHasUnsavedChangesToFalse()
    {
        _undo.MarkSaved();

        Assert.False(_undo.HasUnsavedChanges);
    }

    [Fact]
    public void MarkSaved_FiresStackChanged()
    {
        int count = 0;
        _undo.StackChanged += () => count++;

        _undo.MarkSaved();

        Assert.Equal(1, count);
    }

    [Fact]
    public void MarkSaved_IsIdempotent()
    {
        _undo.MarkSaved();
        _undo.MarkSaved();

        Assert.False(_undo.HasUnsavedChanges);
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Clear_ResetsHasUnsavedChangesToTrue()
    {
        _undo.MarkSaved();

        _undo.Clear();

        Assert.True(_undo.HasUnsavedChanges);
    }

    // ── Record / Undo / Redo do not affect save state ─────────────────────────

    [Fact]
    public void Record_DoesNotChangeHasUnsavedChanges_WhenAlreadySaved()
    {
        _undo.MarkSaved();

        _undo.Record(new StubCommand());

        Assert.False(_undo.HasUnsavedChanges);
    }

    [Fact]
    public void Undo_DoesNotChangeHasUnsavedChanges_WhenAlreadySaved()
    {
        _undo.Record(new StubCommand());
        _undo.MarkSaved();

        _undo.Undo();

        Assert.False(_undo.HasUnsavedChanges);
    }

    [Fact]
    public void Redo_DoesNotChangeHasUnsavedChanges_WhenAlreadySaved()
    {
        _undo.Record(new StubCommand());
        _undo.Undo();
        _undo.MarkSaved();

        _undo.Redo();

        Assert.False(_undo.HasUnsavedChanges);
    }

    // ── Stub ──────────────────────────────────────────────────────────────────

    private sealed class StubCommand : IUndoableCommand
    {
        public void Undo() { }
        public void Redo() { }
    }
}
