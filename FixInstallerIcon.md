# Fix CMGWpf Installer Icon Issue

## Problem
The taskbar icon works in debug mode but not when installed via the MSI. This is because:
1. The project now uses `RuntimeIdentifier=win-x64`
2. The installer (.vdproj) is configured to use the old output path without the RID folder
3. The installer may be packaging an outdated executable without the embedded icon

## Solution Options

### Option 1: Remove RuntimeIdentifier (Recommended for Installer Compatibility)

Since you're using `SelfContained=false`, the RuntimeIdentifier isn't strictly necessary. Remove it to restore the original output path structure that the installer expects.

**Edit CMGWpf.csproj:**
```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <UseWPF>true</UseWPF>
  <ApplicationIcon>Assets\CMG-Logo.ico</ApplicationIcon>
  <Platforms>AnyCPU;x64</Platforms>
  <!-- REMOVE these two lines: -->
  <!-- <RuntimeIdentifier>win-x64</RuntimeIdentifier> -->
  <!-- <SelfContained>false</SelfContained> -->
</PropertyGroup>
```

**Then:**
1. Clean and rebuild CMGWpf project
2. Rebuild the installer project
3. Uninstall the old version
4. Install the new MSI

### Option 2: Update Installer to Use Publish Output

Use the published output instead of the build output:

1. **In Visual Studio**, open the CMGWpfSetup project
2. Right-click on the setup project → **View** → **File System**
3. Right-click on **Primary output from CMGWpf** → **Properties**
4. Change the configuration to use **Release** configuration
5. Rebuild the setup project

### Option 3: Manually Update .vdproj (Advanced)

**Only if Options 1 & 2 don't work:**

1. Close Visual Studio
2. Open `CMGWpfSetup\CMGWpfSetup.vdproj` in a text editor
3. Find the line (around line 802):
   ```
   "SourcePath" = "8:..\\src\\CMGWpf\\obj\\Debug\\net10.0-windows\\apphost.exe"
   ```
4. Change it to:
   ```
   "SourcePath" = "8:..\\src\\CMGWpf\\obj\\Debug\\net10.0-windows\\win-x64\\apphost.exe"
   ```
5. Save and reopen in Visual Studio
6. Rebuild the installer

## Verification Steps

After rebuilding and reinstalling:

1. Check installed location (typically `C:\Program Files\{Manufacturer}\CMGWpf\`)
2. Verify `CMGWpf.exe` has the icon embedded:
   - Right-click → Properties → Details tab
3. Run the application and verify taskbar icon appears

## Why This Happened

The RuntimeIdentifier change for the SQLite native library fix altered the output folder structure. Visual Studio Setup Projects (.vdproj) use hardcoded paths that don't automatically update when project properties change.
