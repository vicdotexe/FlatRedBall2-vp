namespace AnimationEditor.Core.ViewModels;

/// <summary>
/// Pure decision logic for the ANIMATIONS search box's two-stage clear (✕) button.
/// </summary>
public static class TreeSearchBoxLogic
{
    /// <summary>
    /// Decides what the in-field ✕ does: when the box already has no text it
    /// <b>collapses</b> the box (returns <c>true</c>); otherwise it <b>clears</b> the
    /// text and stays open (returns <c>false</c>). Whitespace counts as text — the user
    /// can see it, so ✕ clears it first.
    /// </summary>
    public static bool ClearShouldCollapse(string? currentText) => string.IsNullOrEmpty(currentText);
}
