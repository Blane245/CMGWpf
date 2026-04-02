# FlaUI Setup Complete! ✅

## What Was Installed

1. **New Test Project**: `tests/CMGWpf.UITests`
2. **FlaUI Packages**: 
   - FlaUI.Core (v4.0.0)
   - FlaUI.UIA3 (v4.0.0)
3. **Test Framework**: MSTest (matching your existing unit tests)

## Project Structure

```
tests/CMGWpf.UITests/
├── CMGWpf.UITests.csproj          # Test project file
├── UITestBase.cs                   # Base class with app launch/cleanup
├── Usings.cs                       # Global usings
├── MainWindowTests.cs              # Example tests for main window
├── README.md                       # Comprehensive guide
└── Examples/
    ├── MoveCopyGeneratorDialogTests.cs              # Example dialog tests (commented)
    └── MoveCopyGeneratorDialog_AutomationProperties.md  # How to add AutomationProperties
```

## Current Test Status

✅ **2 tests discovered:**
- `MainWindow_ShouldOpen_Successfully`
- `MainWindow_ShouldHaveTitle`

These tests will launch your application and verify the main window opens correctly.

## Next Steps

### 1. Build Your Application First
Before running UI tests, always build the main app:
```powershell
dotnet build src\CMGWpf\CMGWpf.csproj -c Debug
```

### 2. Run the Tests

**From Visual Studio:**
- Open Test Explorer (Test → Test Explorer)
- Click "Run All Tests"

**From Command Line:**
```powershell
dotnet test tests\CMGWpf.UITests\CMGWpf.UITests.csproj
```

### 3. Add AutomationProperties to Your XAML

To test specific controls, add AutomationProperties:

```xml
<Button x:Name="MyButton"
        Content="Click Me"
        AutomationProperties.AutomationId="MyButton"
        AutomationProperties.Name="Click Me Button"/>
```

See `Examples/MoveCopyGeneratorDialog_AutomationProperties.md` for a complete example.

### 4. Write More Tests

Create new test classes that inherit from `UITestBase`:

```csharp
[TestClass]
public class MyFeatureTests : UITestBase
{
    [TestMethod]
    public void MyTest()
    {
        var button = FindElementByAutomationId("MyButton")?.AsButton();
        button!.Click();
        // Assert expected behavior
    }
}
```

## Useful Tools

### Inspect.exe (Windows SDK)
Use this tool to explore your UI and see AutomationId values:
1. Launch your app
2. Run Inspect.exe (comes with Windows SDK)
3. Hover over controls to see their automation properties

## Tips

- **Always build first**: UI tests launch the compiled executable
- **Use AutomationProperties**: Makes tests reliable and readable
- **Add waits**: Use the timeout parameter in Find methods for slow-loading elements
- **Keep tests independent**: Each test should work on its own

## Documentation

- Full guide: `tests/CMGWpf.UITests/README.md`
- Example XAML: `tests/CMGWpf.UITests/Examples/MoveCopyGeneratorDialog_AutomationProperties.md`
- FlaUI docs: https://github.com/FlaUI/FlaUI

## Common Issues

**Tests can't find main window?**
- Ensure app builds successfully
- Check path in `UITestBase.Setup()` matches your build output

**Elements not found?**
- Add AutomationProperties to XAML
- Use Inspect.exe to verify AutomationId values
- Increase timeout in Find methods

Happy Testing! 🚀
