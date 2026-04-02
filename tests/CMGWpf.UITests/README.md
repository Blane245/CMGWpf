# CMGWpf UI Tests

This project contains automated UI tests for the CMGWpf application using FlaUI.

## Prerequisites

- The CMGWpf application must be built before running UI tests
- Tests will launch the application from: `src\CMGWpf\bin\Debug\net10.0-windows\CMGWpf.exe`

## Running the Tests

### From Visual Studio
1. Build the solution (Ctrl+Shift+B)
2. Open Test Explorer (Test → Test Explorer)
3. Run All Tests or select specific tests

### From Command Line
```powershell
dotnet test tests\CMGWpf.UITests\CMGWpf.UITests.csproj
```

## Writing UI Tests

### 1. Add AutomationProperties to Your XAML

To make elements easily testable, add AutomationProperties to your controls:

```xml
<Button x:Name="SaveButton"
        Content="Save"
        AutomationProperties.AutomationId="SaveButton"
        AutomationProperties.Name="Save Changes" />

<TextBox x:Name="NameTextBox"
         AutomationProperties.AutomationId="NameTextBox"
         AutomationProperties.Name="Name Input" />

<Menu x:Name="MainMenu">
    <MenuItem Header="File" AutomationProperties.AutomationId="FileMenu">
        <MenuItem Header="New" AutomationProperties.AutomationId="NewMenuItem" />
        <MenuItem Header="Open" AutomationProperties.AutomationId="OpenMenuItem" />
    </MenuItem>
</Menu>
```

### 2. Create a Test Class

Inherit from `UITestBase` to get automatic app launch/cleanup:

```csharp
[TestClass]
public class MyFeatureTests : UITestBase
{
    [TestMethod]
    public void MyTest()
    {
        // Your test code here
    }
}
```

### 3. Common Test Patterns

#### Finding and Clicking a Button
```csharp
var button = FindElementByAutomationId("SaveButton")?.AsButton();
Assert.IsNotNull(button, "Button should be found");
button!.Click();
```

#### Entering Text
```csharp
var textBox = FindElementByAutomationId("NameTextBox")?.AsTextBox();
Assert.IsNotNull(textBox);
textBox!.Text = "New Value";
```

#### Working with ComboBoxes
```csharp
var comboBox = FindElementByAutomationId("MyComboBox")?.AsComboBox();
comboBox!.Select(1); // Select by index
// or
comboBox.Select("Option Name"); // Select by text
```

#### Working with CheckBoxes
```csharp
var checkBox = FindElementByAutomationId("MyCheckBox")?.AsCheckBox();
checkBox!.IsChecked = true;
```

#### Waiting for Elements
```csharp
// FindElementByAutomationId has built-in retry logic (5 seconds default)
var element = FindElementByAutomationId("SlowLoadingElement", timeoutSeconds: 10);
Assert.IsNotNull(element, "Element should appear within 10 seconds");
```

#### Working with Windows/Dialogs
```csharp
// Click button that opens dialog
var openDialogButton = FindElementByAutomationId("OpenDialogButton")?.AsButton();
openDialogButton!.Click();

// Find the dialog window
var dialog = MainWindow!.ModalWindows.FirstOrDefault();
Assert.IsNotNull(dialog, "Dialog should open");

// Interact with dialog elements
var okButton = dialog.FindFirstDescendant(cf => cf.ByAutomationId("OkButton"))?.AsButton();
okButton!.Click();
```

## Best Practices

1. **Always build the application before running tests** - Tests launch the compiled executable
2. **Use AutomationProperties** - Makes element selection reliable and test code readable
3. **Use descriptive test names** - Follow pattern: `WhenAction_ShouldExpectedResult`
4. **Add waits for async operations** - Use the timeout parameter in Find methods
5. **Clean up resources** - UITestBase handles this, but be aware of it
6. **Test one thing per test** - Keep tests focused and independent
7. **Use the helper methods** - `FindElementByAutomationId` and `FindElementByName` have retry logic built-in

## Troubleshooting

### Tests Fail to Find Main Window
- Ensure the application builds successfully
- Check the path in `UITestBase.Setup()` matches your build output
- Increase the timeout in `GetMainWindow()`

### Elements Not Found
- Verify AutomationProperties are set in XAML
- Use Inspect.exe (Windows SDK tool) to verify AutomationId values
- Increase timeout in Find methods for slow-loading elements

### Tests Are Flaky
- Add appropriate waits for async operations
- Ensure tests are independent (don't rely on order)
- Check for timing issues in UI updates

## Additional Resources

- [FlaUI Documentation](https://github.com/FlaUI/FlaUI)
- [UI Automation Overview](https://docs.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-overview)
- [Inspect.exe Tool](https://docs.microsoft.com/en-us/windows/win32/winauto/inspect-objects) - For exploring UI automation properties
