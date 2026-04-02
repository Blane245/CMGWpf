using FlaUI.Core.AutomationElements;

namespace CMGWpf.UITests;

/// <summary>
/// Example UI tests for the CMGWpf main window.
/// This demonstrates how to write UI tests using FlaUI.
/// </summary>
[TestClass]
public class MainWindowTests : UITestBase
{
    [TestMethod]
    public void MainWindow_ShouldOpen_Successfully()
    {
        // Arrange & Act - handled by base class Setup

        // Assert
        Assert.IsNotNull(MainWindow, "Main window should not be null");
        Assert.IsTrue(MainWindow!.IsAvailable, "Main window should be available");
    }

    [TestMethod]
    public void MainWindow_ShouldHaveTitle()
    {
        // Arrange & Act - handled by base class Setup

        // Assert
        Assert.IsNotNull(MainWindow, "Main window should not be null");
        Assert.IsFalse(string.IsNullOrEmpty(MainWindow!.Title), "Main window should have a title");
    }

    // Example: Testing a button click
    // Uncomment and modify once you add AutomationProperties to your XAML
    /*
    [TestMethod]
    public void WhenSaveButtonClicked_ShouldSaveData()
    {
        // Arrange
        var saveButton = FindElementByAutomationId("SaveButton")?.AsButton();
        Assert.IsNotNull(saveButton, "Save button should be found");

        // Act
        saveButton!.Click();

        // Assert
        // Add assertions to verify the expected behavior
        // For example, check if a file was created, or if a message appeared
    }
    */

    // Example: Testing menu navigation
    // Uncomment and modify based on your actual menu structure
    /*
    [TestMethod]
    public void WhenFileMenuOpened_ShouldShowMenuItems()
    {
        // Arrange
        var fileMenu = FindElementByAutomationId("FileMenu");
        Assert.IsNotNull(fileMenu, "File menu should be found");

        // Act
        fileMenu!.Click();

        // Assert
        var newMenuItem = FindElementByAutomationId("NewMenuItem");
        Assert.IsNotNull(newMenuItem, "New menu item should be visible");
    }
    */
}
