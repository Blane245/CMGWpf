# Windows Jump List Setup for CMG Files

## Overview
The CMG application now supports Windows Jump Lists, allowing users to quickly access recent CMG files from the taskbar.

## How It Works

### Automatic Features
1. **Recent Files Tracking**: When you open or save a CMG file, it's automatically added to:
   - The in-app Recent Files menu
   - Windows Jump List (right-click taskbar icon)

2. **Command-Line Support**: You can open CMG files by:
   - Double-clicking a .cmg file in Windows Explorer (after file association)
   - Right-clicking the CMG taskbar icon and selecting a recent file
   - Passing a file path as a command-line argument

### File Association (Optional)

To enable double-clicking .cmg files in Windows Explorer:

#### Method 1: Manual Association (Windows 11/10)
1. Right-click any `.cmg` file
2. Select "Open with" → "Choose another app"
3. Click "More apps" → "Look for another app on this PC"
4. Navigate to the CMG application executable
5. Check "Always use this app to open .cmg files"
6. Click OK

#### Method 2: Registry Setup (Advanced)
You can create a `.reg` file with the following content:

```registry
Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Classes\.cmg]
@="CMGFile"

[HKEY_CURRENT_USER\Software\Classes\CMGFile]
@="CMG Music File"

[HKEY_CURRENT_USER\Software\Classes\CMGFile\DefaultIcon]
@="\"C:\\Path\\To\\CMGWpf.exe\",0"

[HKEY_CURRENT_USER\Software\Classes\CMGFile\shell\open\command]
@="\"C:\\Path\\To\\CMGWpf.exe\" \"%1\""
```

**Note**: Replace `C:\\Path\\To\\CMGWpf.exe` with the actual path to your CMG application.

#### Method 3: ClickOnce or Installer (Future)
For production deployment, consider using:
- Windows Installer (.msi)
- ClickOnce deployment
- MSIX package

These can automatically register file associations during installation.

## Features

### Jump List
- **Recent Files**: Up to 10 most recently opened files appear in the Jump List
- **File Locking**: Files already open in another instance show a warning
- **Automatic Updates**: Jump List updates every time you open or save a file

### Startup Behavior
- Launch CMG without arguments: Opens with a new empty file
- Launch with file path: Attempts to open the specified file
- Launch from Jump List: Opens the selected recent file

## Technical Details

### Implementation
- **JumpListService**: Manages Windows Jump List integration
- **FileLockService**: Prevents multiple instances from opening the same file
- **SHAddToRecentDocs**: Win32 API call to add files to Windows recent documents
- **Command-line args**: Handled in `App.xaml.cs` Application_Startup

### Files Modified
- `App.xaml.cs`: Initialize Jump List, handle command-line arguments
- `FileViewModel.cs`: Update Jump List when files are opened
- `JumpListService.cs`: New service for Jump List management
- `FileLockService.cs`: Cross-instance file locking

## Troubleshooting

### Jump List not showing recent files
1. Ensure Windows settings allow Jump Lists:
   - Settings → Personalization → Start → "Show recently opened items in Jump Lists"
2. Verify file paths are valid (files must exist)
3. Check Debug output for JumpListService messages

### File association not working
1. Verify the registry entries (if using Method 2)
2. Ensure the path to CMGWpf.exe is correct
3. Try logging out and back in to refresh Windows shell

### Multiple instances opening same file
- This should be prevented by FileLockService
- If it occurs, check for .lock files in the same directory as .cmg files
- Stale locks are automatically cleaned up when processes exit
