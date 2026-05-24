using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using System;
using Xunit;

namespace AnimationEditor.App.Tests;

public class MainWindowChromeTests
{
    // ── Non-macOS: custom chrome ──────────────────────────────────────────────

    [AvaloniaFact]
    public void OnNonMacOS_WindowDecorations_IsNone()
    {
        if (OperatingSystem.IsMacOS()) return; // macOS uses system decorations — tested separately

        var ctx = TestHelpers.BuildServices();
        var window = ctx.CreateMainWindow();
        window.Show();
        try
        {
            Assert.Equal(WindowDecorations.None, window.WindowDecorations);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void TitleBar_ContainsMenuAndAppIdentity()
    {
        var ctx = TestHelpers.BuildServices();
        var window = ctx.CreateMainWindow();
        window.Show();
        try
        {
            Assert.NotNull(window.FindControl<Border>("TitleBarBorder"));
            Assert.NotNull(window.FindControl<Menu>("MainMenu"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void TitleBar_HasWindowControlButtons()
    {
        var ctx = TestHelpers.BuildServices();
        var window = ctx.CreateMainWindow();
        window.Show();
        try
        {
            Assert.NotNull(window.FindControl<Button>("MinimizeBtn"));
            Assert.NotNull(window.FindControl<Button>("MaximizeBtn"));
            Assert.NotNull(window.FindControl<Button>("CloseBtn"));
        }
        finally
        {
            window.Close();
        }
    }

    // ── macOS: native traffic-light chrome ────────────────────────────────────

    [AvaloniaFact]
    public void OnMacOS_WindowDecorations_IsFull()
    {
        if (!OperatingSystem.IsMacOS()) return; // Windows/Linux use custom chrome — tested separately

        var ctx = TestHelpers.BuildServices();
        var window = ctx.CreateMainWindow();
        window.Show();
        try
        {
            Assert.Equal(WindowDecorations.Full, window.WindowDecorations);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void OnMacOS_TitleBarBorder_IsHidden()
    {
        if (!OperatingSystem.IsMacOS()) return;

        var ctx = TestHelpers.BuildServices();
        var window = ctx.CreateMainWindow();
        window.Show();
        try
        {
            var titleBar = window.FindControl<Border>("TitleBarBorder");
            Assert.NotNull(titleBar);
            Assert.False(titleBar!.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void OnMacOS_ResizeGrips_AreHidden()
    {
        if (!OperatingSystem.IsMacOS()) return;

        var ctx = TestHelpers.BuildServices();
        var window = ctx.CreateMainWindow();
        window.Show();
        try
        {
            string[] gripNames = ["GripN", "GripS", "GripW", "GripE", "GripNW", "GripNE", "GripSW", "GripSE"];
            foreach (var name in gripNames)
            {
                var grip = window.FindControl<Border>(name);
                Assert.NotNull(grip);
                Assert.False(grip!.IsVisible, $"{name} should be hidden on macOS");
            }
        }
        finally
        {
            window.Close();
        }
    }
}
