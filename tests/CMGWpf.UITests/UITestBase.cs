using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace CMGWpf.UITests;

/// <summary>
/// Base class for UI tests providing common setup and teardown logic.
/// </summary>
public abstract class UITestBase
{
    protected Application? App { get; private set; }
    protected UIA3Automation? Automation { get; private set; }
    protected Window? MainWindow { get; private set; }

    [TestInitialize]
    public virtual void Setup()
    {
        // Get the path to the application executable
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\.."));
        var appPath = Path.Combine(projectDir, @"src\CMGWpf\bin\Debug\net10.0-windows\CMGWpf.exe");

        // Launch the application
        App = Application.Launch(appPath);
        Automation = new UIA3Automation();

        // Wait for and get the main window
        MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(10));
        Assert.IsNotNull(MainWindow, "Main window could not be found");
    }

    [TestCleanup]
    public virtual void Cleanup()
    {
        // Close the application
        App?.Close();
        App?.Dispose();
        Automation?.Dispose();
    }

    /// <summary>
    /// Helper method to find an element by AutomationId with retry logic.
    /// </summary>
    protected AutomationElement? FindElementByAutomationId(string automationId, int timeoutSeconds = 5)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        while (DateTime.Now < endTime)
        {
            var element = MainWindow?.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element != null)
                return element;

            Thread.Sleep(100);
        }
        return null;
    }

    /// <summary>
    /// Helper method to find an element by Name with retry logic.
    /// </summary>
    protected AutomationElement? FindElementByName(string name, int timeoutSeconds = 5)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        while (DateTime.Now < endTime)
        {
            var element = MainWindow?.FindFirstDescendant(cf => cf.ByName(name));
            if (element != null)
                return element;

            Thread.Sleep(100);
        }
        return null;
    }
}
