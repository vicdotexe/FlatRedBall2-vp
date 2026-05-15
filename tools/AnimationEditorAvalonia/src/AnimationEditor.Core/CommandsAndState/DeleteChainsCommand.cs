using FlatRedBall2.Animation.Content;
using System;
using System.Collections.Generic;

namespace AnimationEditor.Core.CommandsAndState.Commands
{
    internal sealed class DeleteChainsCommand : IUndoableCommand
    {
        private readonly IReadOnlyList<AnimationChainSave> _chains;
        private readonly AnimationChainListSave _acls;
        private readonly IAppCommands _commands;
        private readonly IApplicationEvents _events;
        private readonly ISelectedState _selectedState;

        // Captured by Do(): the chains actually removed, paired with where they were.
        private (AnimationChainSave Chain, int OriginalIndex)[] _removed = [];

        public string Description { get; }

        public DeleteChainsCommand(
            IReadOnlyList<AnimationChainSave> chains,
            AnimationChainListSave acls,
            IAppCommands commands,
            IApplicationEvents events,
            ISelectedState selectedState)
        {
            _chains = chains;
            _acls = acls;
            _commands = commands;
            _events = events;
            _selectedState = selectedState;
            Description = chains.Count == 1
                ? $"Delete '{chains[0].Name}'"
                : $"Delete {chains.Count} Animations";
        }

        public bool Do()
        {
            // Capture every original index BEFORE removing anything — removing one
            // chain would shift the indices of those still to be captured.
            var removed = new List<(AnimationChainSave, int)>();
            foreach (var chain in _chains)
            {
                int idx = _acls.AnimationChains.IndexOf(chain);
                if (idx >= 0) removed.Add((chain, idx));
            }
            _removed = removed.ToArray();

            if (_removed.Length == 0) return false;

            foreach (var (chain, _) in _removed)
                _acls.AnimationChains.Remove(chain);

            _commands.RefreshTreeView();
            _commands.RefreshAnimationFrameDisplay();
            _events.RaiseAnimationChainsChanged();
            _commands.RefreshWireframe();
            _commands.SaveCurrentAnimationChainList();
            _selectedState.SelectedChain = null;
            return true;
        }

        public void Undo()
        {
            // Re-insert in original order so indices remain correct.
            foreach (var (chain, idx) in _removed)
            {
                int safeIdx = Math.Min(idx, _acls.AnimationChains.Count);
                _acls.AnimationChains.Insert(safeIdx, chain);
            }
            _commands.RefreshTreeView();
            _commands.RefreshAnimationFrameDisplay();
            _events.RaiseAnimationChainsChanged();
            _commands.RefreshWireframe();
            _commands.SaveCurrentAnimationChainList();
            _selectedState.SelectedChain = _removed[0].Chain;
        }

        public void Redo()
        {
            foreach (var (chain, _) in _removed)
                _acls.AnimationChains.Remove(chain);
            _commands.RefreshTreeView();
            _commands.RefreshAnimationFrameDisplay();
            _events.RaiseAnimationChainsChanged();
            _commands.RefreshWireframe();
            _commands.SaveCurrentAnimationChainList();
            _selectedState.SelectedChain = null;
        }
    }
}
