# CMG Database Server - Windows Service Setup Complete! ✅

Your CMG Database Server has been configured to run as a Windows Service named **CMGDBServer**.

## What's Been Set Up

### ✅ Windows Service Support
- Service name: **CMGDBServer**
- Automatic startup on computer boot
- Automatic restart on failure
- Integrated with Windows Service Manager

### ✅ Keep-Alive Feature
- Pings MySQL every 60 minutes with `SELECT 1 + 1`
- Prevents connection timeouts
- Configurable interval

### ✅ Management Scripts
- `Install-Service.ps1` - Install the service
- `Uninstall-Service.ps1` - Remove the service
- `Manage-Service.ps1` - Start/Stop/Restart/Status
- `Build-And-Deploy.ps1` - Build and update running service

### ✅ Documentation
- `SERVICE-INSTALLATION.md` - Complete installation guide
- `QUICK-REFERENCE.md` - Quick command reference
- `README.md` - General usage guide

## Next Steps

### 1. Configure MySQL Connection

Edit `CMGDatabaseServer/appsettings.json`:

```json
{
  "DatabaseSettings": {
    "Server": "localhost",
    "Port": 3306,
    "Database": "cmg",
    "UserId": "root",
    "Password": "YOUR_MYSQL_PASSWORD"
  }
}
```

### 2. Build the Project

Run this command:
```powershell
dotnet publish -c Release
```

Or use the build script:
```powershell
cd CMGDatabaseServer
.\Build-And-Deploy.ps1
```

### 3. Install as Windows Service

**Open PowerShell as Administrator** and run:
```powershell
cd CMGDatabaseServer
.\Install-Service.ps1
```

The service will:
- ✅ Install as "CMGDBServer"
- ✅ Set to start automatically on boot
- ✅ Start immediately
- ✅ Configure auto-restart on failure

### 4. Verify Installation

```powershell
.\Manage-Service.ps1 -Action Status
```

You should see:
```
Service Name:    CMGDBServer
Display Name:    CMG Database Server
Status:          Running
Startup Type:    Automatic
```

## Daily Usage

After installation, the service runs automatically. You can manage it using:

```powershell
# Check if running
.\Manage-Service.ps1 -Action Status

# Restart after config changes
.\Manage-Service.ps1 -Action Restart

# Stop temporarily
.\Manage-Service.ps1 -Action Stop

# Start again
.\Manage-Service.ps1 -Action Start
```

## Viewing Logs

1. **Windows Event Viewer**:
   - Press `Win + R`, type `eventvwr.msc`
   - Navigate to: Windows Logs → Application
   - Filter by source: "CMGDBServer"

2. **PowerShell**:
   ```powershell
   Get-EventLog -LogName Application -Source ".NET Runtime" -Newest 10
   ```

## Testing the Service

After installation, test the connection:

```csharp
using var client = new TcpClient();
await client.ConnectAsync("localhost", 5555);
Console.WriteLine("Connected to CMGDBServer!");
```

## Troubleshooting

### Service Won't Start?

1. **Check MySQL is running**:
   ```powershell
   Get-Service -Name MySQL*
   ```

2. **Verify port 5555 is available**:
   ```powershell
   netstat -ano | findstr :5555
   ```

3. **Check Event Viewer** for error details

4. **Test MySQL connection** manually using the credentials in `appsettings.json`

### Need to Update?

```powershell
# Build new version
.\Build-And-Deploy.ps1 -UpdateService
```

This will:
- Build the latest code
- Stop the service
- Update files
- Restart the service

## Files Created

```
CMGDatabaseServer/
├── Program.cs                    (Service entry point)
├── DatabaseServer.cs             (Server with keep-alive)
├── DatabaseServerService.cs      (Windows Service wrapper)
├── ExampleClient.cs              (Test client)
├── appsettings.json              (Configuration)
├── Install-Service.ps1           (Installation script)
├── Uninstall-Service.ps1         (Removal script)
├── Manage-Service.ps1            (Management script)
├── Build-And-Deploy.ps1          (Build & deploy script)
├── README.md                     (General documentation)
├── SERVICE-INSTALLATION.md       (Installation guide)
└── QUICK-REFERENCE.md            (Command reference)
```

## Quick Reference

| Task | Command |
|------|---------|
| Install Service | `.\Install-Service.ps1` |
| Check Status | `.\Manage-Service.ps1 -Action Status` |
| Start Service | `.\Manage-Service.ps1 -Action Start` |
| Stop Service | `.\Manage-Service.ps1 -Action Stop` |
| Restart Service | `.\Manage-Service.ps1 -Action Restart` |
| Uninstall Service | `.\Uninstall-Service.ps1` |
| Build & Update | `.\Build-And-Deploy.ps1 -UpdateService` |

## Support

For detailed information, see:
- 📖 [SERVICE-INSTALLATION.md](SERVICE-INSTALLATION.md) - Complete installation guide
- 📋 [QUICK-REFERENCE.md](QUICK-REFERENCE.md) - Quick command reference
- 📚 [README.md](README.md) - API and usage documentation

---

**Your CMG Database Server is ready to run as a Windows Service!** 🎉
