using System;

namespace AnimationEditor.Core.CommandsAndState.Commands
{
    /// <summary>
    /// Unlimited undo/redo history for the animation editor.
    /// Call <see cref="Record"/> after every mutating operation.
    /// </summary>
    public interface IUndoManager
    {
        bool CanUndo { get; }
        bool CanRedo { get; }

        /// <summary>
        /// Current save state of the open document.
        /// Updated by <see cref="MarkSaved"/>, <see cref="MarkSaveFailed"/>, and <see cref="Clear"/>.
        /// </summary>
        SaveState SaveState { get; }

        /// <summary>Raised after <see cref="Record"/>, <see cref="Undo"/>, <see cref="Redo"/>, <see cref="Clear"/>, <see cref="MarkSaved"/>, or <see cref="MarkSaveFailed"/>.</summary>
        event Action? StackChanged;

        /// <summary>
        /// Records a command in the undo history and clears the redo stack.
        /// Call this immediately after a mutating operation completes.
        /// </summary>
        void Record(IUndoableCommand cmd);

        /// <summary>No-op when <see cref="CanUndo"/> is false.</summary>
        void Undo();

        /// <summary>No-op when <see cref="CanRedo"/> is false.</summary>
        void Redo();

        void Clear();

        /// <summary>Marks the last auto-save as successful, setting <see cref="SaveState"/> to <see cref="SaveState.AutoSaveOn"/>.</summary>
        void MarkSaved();

        /// <summary>Marks the last auto-save as failed, setting <see cref="SaveState"/> to <see cref="SaveState.Failed"/>.</summary>
        void MarkSaveFailed();
    }
}
