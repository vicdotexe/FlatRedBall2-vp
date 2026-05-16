using AnimationEditor.App.Controls;
using Avalonia.Headless.XUnit;
using Xunit;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Tests for <see cref="PreviewControl.SetGuides"/>, which bulk-restores
/// guide positions from world coordinates (used when loading .aeproperties).
/// </summary>
public class PreviewControlSetGuidesTests
{
    private static PreviewControl BuildCtrl() =>
        TestHelpers.BuildServices().CreatePreviewControl();

    [AvaloniaFact]
    public void SetGuides_RestoresHorizontalAndVerticalGuides()
    {
        var ctrl = BuildCtrl();

        ctrl.SetGuides(
            hGuides: new[] { 10f, 20f },
            vGuides: new[] { 30f });

        Assert.Equal(2, ctrl.HGuides.Count);
        Assert.Single(ctrl.VGuides);
        Assert.Equal(10f, ctrl.HGuides[0]);
        Assert.Equal(20f, ctrl.HGuides[1]);
        Assert.Equal(30f, ctrl.VGuides[0]);
    }

    [AvaloniaFact]
    public void SetGuides_ReplacesExistingGuides()
    {
        var ctrl = BuildCtrl();

        // Plant some guides first using SetGuides itself
        ctrl.SetGuides(hGuides: new[] { 99f }, vGuides: new[] { 88f });

        // A second call should replace, not append
        ctrl.SetGuides(
            hGuides: new[] { 5f },
            vGuides: new[] { 7f, 9f });

        Assert.Single(ctrl.HGuides);
        Assert.Equal(5f, ctrl.HGuides[0]);
        Assert.Equal(2, ctrl.VGuides.Count);
        Assert.Equal(7f, ctrl.VGuides[0]);
        Assert.Equal(9f, ctrl.VGuides[1]);
    }

    [AvaloniaFact]
    public void SetGuides_WithEmptyLists_ClearsAllGuides()
    {
        var ctrl = BuildCtrl();

        ctrl.SetGuides(hGuides: new[] { 5f }, vGuides: new[] { 6f });

        ctrl.SetGuides(hGuides: System.Array.Empty<float>(), vGuides: System.Array.Empty<float>());

        Assert.Empty(ctrl.HGuides);
        Assert.Empty(ctrl.VGuides);
    }
}
