using AnimationEditor.Core.ViewModels;
using Xunit;

namespace AnimationEditor.Core.Tests;

// The ANIMATIONS search box's inner ✕ is two-stage: clear text while there is text,
// collapse the box when it's already empty (issue #517).
public class TreeSearchBoxLogicTests
{
    [Fact]
    public void ClearShouldCollapse_EmptyText_ReturnsTrue()
    {
        Assert.True(TreeSearchBoxLogic.ClearShouldCollapse(""));
    }

    [Fact]
    public void ClearShouldCollapse_NullText_ReturnsTrue()
    {
        Assert.True(TreeSearchBoxLogic.ClearShouldCollapse(null));
    }

    [Fact]
    public void ClearShouldCollapse_WhitespaceText_ReturnsFalse()
    {
        // Whitespace is still text the user can see and clear, so ✕ clears it first.
        Assert.False(TreeSearchBoxLogic.ClearShouldCollapse("   "));
    }

    [Fact]
    public void ClearShouldCollapse_WithText_ReturnsFalse()
    {
        Assert.False(TreeSearchBoxLogic.ClearShouldCollapse("walk"));
    }
}
