using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Xunit;

namespace AnimationEditor.App.Tests;

/// <summary>
/// Regression tests for issue #239: the yes/no confirmation dialog (delete
/// confirmation) must be keyboard-accessible — ENTER confirms, ESC cancels.
/// </summary>
public class ConfirmDialogKeyboardTests
{
    private static void RaiseKey(Window dialog, Key key) =>
        dialog.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Source = dialog,
            Key = key,
        });

    [AvaloniaFact]
    public void BuildConfirmDialog_EnterKey_ResolvesTrue()
    {
        var tcs = new TaskCompletionSource<bool>();
        var dialog = MainWindow.BuildConfirmDialog("Delete this frame?", "Delete?", tcs);
        dialog.Show();
        Dispatcher.UIThread.RunJobs();

        RaiseKey(dialog, Key.Enter);
        Dispatcher.UIThread.RunJobs();

        Assert.True(tcs.Task.IsCompletedSuccessfully, "ENTER should dismiss the dialog");
        Assert.True(tcs.Task.Result, "ENTER should confirm (Yes)");
    }

    [AvaloniaFact]
    public void BuildConfirmDialog_EscapeKey_ResolvesFalse()
    {
        var tcs = new TaskCompletionSource<bool>();
        var dialog = MainWindow.BuildConfirmDialog("Delete this frame?", "Delete?", tcs);
        dialog.Show();
        Dispatcher.UIThread.RunJobs();

        RaiseKey(dialog, Key.Escape);
        Dispatcher.UIThread.RunJobs();

        Assert.True(tcs.Task.IsCompletedSuccessfully, "ESC should dismiss the dialog");
        Assert.False(tcs.Task.Result, "ESC should cancel (No)");
    }

    [AvaloniaFact]
    public void BuildConfirmDialog_ClosedWithoutChoice_ResolvesFalse()
    {
        var tcs = new TaskCompletionSource<bool>();
        var dialog = MainWindow.BuildConfirmDialog("Delete this frame?", "Delete?", tcs);
        dialog.Show();
        Dispatcher.UIThread.RunJobs();

        dialog.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.True(tcs.Task.IsCompletedSuccessfully);
        Assert.False(tcs.Task.Result, "Closing the dialog without choosing must not delete");
    }
}
