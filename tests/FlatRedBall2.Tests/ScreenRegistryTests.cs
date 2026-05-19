using Shouldly;
using Xunit;

namespace FlatRedBall2.Tests;

public class ScreenRegistryTests
{
    [Fact]
    public void Count_ReflectsNumberOfRegisteredItems()
    {
        var registry = new ScreenRegistry<string>();
        registry.Register("a");
        registry.Register("b");

        registry.Count.ShouldBe(2);
    }

    [Fact]
    public void Register_AddsItemToItems()
    {
        var registry = new ScreenRegistry<string>();

        registry.Register("hello");

        registry.Items.ShouldContain("hello");
    }

    [Fact]
    public void Unregister_RemovesItemFromItems()
    {
        var registry = new ScreenRegistry<string>();
        registry.Register("hello");

        registry.Unregister("hello");

        registry.Items.ShouldNotContain("hello");
    }
}
