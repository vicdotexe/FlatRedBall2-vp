using System;
using System.Collections.Generic;

namespace AnimationEditor.Core.CommandsAndState.Commands
{
    /// <summary>
    /// Manages unlimited undo/redo history for the animation editor.
    /// Call <see cref="Record"/> after every mutating operation.
    /// </summary>
    public class UndoManager : IUndoManager
    {
        private readonly Stack<IUndoableCommand> _undoStack = new();
        private readonly Stack<IUndoableCommand> _redoStack = new();
        private SaveState _saveState = SaveState.Unsaved;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public SaveState SaveState => _saveState;

        /// <summary>Raised after <see cref="Record"/>, <see cref="Undo"/>, <see cref="Redo"/>, <see cref="Clear"/>, <see cref="MarkSaved"/>, or <see cref="MarkSaveFailed"/>.</summary>
        public event Action? StackChanged;

        /// <summary>
        /// Records a command in the undo history and clears the redo stack.
        /// Call this immediately after a mutating operation completes.
        /// </summary>
        public void Record(IUndoableCommand cmd)
        {
            _undoStack.Push(cmd);
            _redoStack.Clear();
            StackChanged?.Invoke();
        }

        /// <summary>No-op when <see cref="CanUndo"/> is false.</summary>
        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
            StackChanged?.Invoke();
        }

        /// <summary>No-op when <see cref="CanRedo"/> is false.</summary>
        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = _redoStack.Pop();
            cmd.Redo();
            _undoStack.Push(cmd);
            StackChanged?.Invoke();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _saveState = SaveState.Unsaved;
            StackChanged?.Invoke();
        }

        /// <summary>Marks the last auto-save as successful, setting <see cref="SaveState"/> to <see cref="SaveState.AutoSaveOn"/>.</summary>
        public void MarkSaved()
        {
            _saveState = SaveState.AutoSaveOn;
            StackChanged?.Invoke();
        }

        /// <summary>Marks the last auto-save as failed, setting <see cref="SaveState"/> to <see cref="SaveState.Failed"/>.</summary>
        public void MarkSaveFailed()
        {
            _saveState = SaveState.Failed;
            StackChanged?.Invoke();
        }
    }
}
