using System.Collections.Generic;
using AnimationEditor.Core.ViewModels;
using Xunit;

namespace AnimationEditor.Core.Tests;

// Pure tests for the ANIMATIONS tree search/filter predicate (issue #517).
public class TreeFilterTests
{
    private static readonly string[] Chains = { "walkLeft", "slowWalk", "Idle", "RunRight" };

    [Fact]
    public void FilterChainNames_EmptyQuery_ReturnsAll()
    {
        var result = TreeBuilder.FilterChainNames(Chains, "");
        Assert.Equal(Chains, result);
    }

    [Fact]
    public void FilterChainNames_WhitespaceQuery_ReturnsAll()
    {
        var result = TreeBuilder.FilterChainNames(Chains, "   ");
        Assert.Equal(Chains, result);
    }

    [Fact]
    public void FilterChainNames_CaseInsensitive_MatchesRegardlessOfCase()
    {
        var result = TreeBuilder.FilterChainNames(Chains, "WALK");
        Assert.Equal(new List<string> { "walkLeft", "slowWalk" }, result);
    }

    [Fact]
    public void FilterChainNames_MidStringSubstring_Matches()
    {
        // "walk" appears mid-string in "slowWalk", not just as a prefix.
        var result = TreeBuilder.FilterChainNames(Chains, "walk");
        Assert.Equal(new List<string> { "walkLeft", "slowWalk" }, result);
    }

    [Fact]
    public void FilterChainNames_NoMatch_ReturnsEmpty()
    {
        var result = TreeBuilder.FilterChainNames(Chains, "zzz");
        Assert.Empty(result);
    }
}
