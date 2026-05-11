using AnimationEditor.Core;
using AnimationEditor.Core.CommandsAndState;
using AnimationEditor.Core.CommandsAndState.Commands;
using AnimationEditor.Core.IO;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Shared helpers for headless App tests that exercise singleton/Self-bridge state.
/// Call <see cref="ResetServices"/> at the start of every <c>ResetSingletons()</c>
/// or <c>CreateWindow()</c> helper so that <c>ProjectManager.Self</c> etc. are
/// non-null valid instances before any per-test overrides run.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Creates fresh service instances and wires all static Self bridges.
    /// After this call, <c>ProjectManager.Self</c>, <c>SelectedState.Self</c>,
    /// <c>AppCommands.Self</c>, <c>AppState.Self</c>, <c>ApplicationEvents.Self</c>,
    /// <c>IoManager.Self</c>, and <c>ObjectFinder.Self</c> are all non-null.
    /// </summary>
    internal static void ResetServices()
    {
        var pm            = new ProjectManager();
        var events        = new ApplicationEvents();
        var selectedState = new SelectedState(pm);
        var appState      = new AppState(events, selectedState);
        var ioManager     = new IoManager(appState);
        var objectFinder  = new ObjectFinder(pm);
        var appCommands   = new AppCommands(pm, selectedState, events, ioManager, objectFinder);

        ProjectManager.Self    = pm;
        ApplicationEvents.Self = events;
        SelectedState.Self     = selectedState;
        AppState.Self          = appState;
        IoManager.Self         = ioManager;
        ObjectFinder.Self      = objectFinder;
        AppCommands.Self       = appCommands;
        UndoManager.Self       = new UndoManager();
    }
}
