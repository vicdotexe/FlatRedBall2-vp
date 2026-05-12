using Avalonia.Headless.XUnit;
using Xunit;

namespace AnimationEditor.App.Tests;

public class MainWindowChromeTests
{
    [AvaloniaFact]
    public void MainWindow_DoesNotExtendIntoSystemTitleBar()
    {
        var ctx = TestHelpers.BuildServices();
        var window = ctx.CreateMainWindow();
        window.Show();
        try
        {
            Assert.False(window.ExtendClientAreaToDecorationsHint);
        }
        finally
        {
            window.Close();
        }
    }
}
