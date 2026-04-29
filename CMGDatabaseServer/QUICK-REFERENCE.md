# CMGDBServer - Quick Reference

## Service Commands (Run as Administrator)

### Installation
```powershell
# 1. Build the project
dotnet publish -c Release

# 2. Install as Windows Service
.\Install-Service.ps1
```

### Management
```powershell
# Check status
.\Manage-Service.ps1 -Action Status

# Start service
.\Manage-Service.ps1 -Action Start

# Stop service
.\Manage-Service.ps1 -Action Stop

# Restart service
.\Manage-Service.ps1 -Action Restart
```

### Uninstallation
```powershell
.\Uninstall-Service.ps1
```

### Update & Redeploy
```powershell
.\Build-And-Deploy.ps1 -UpdateService
```

## Service Information

| Property | Value |
|----------|-------|
| Service Name | CMGDBServer |
| Display Name | CMG Database Server |
| Default Port | 5555 |
| Startup Type | Automatic |
| Keep-Alive | Every 60 minutes |

## Configuration File

Location: `appsettings.json`

```json
{
  "ServerSettings": {
    "Port": 5555,
    "MaxConnections": 100,
    "KeepAliveIntervalMinutes": 60
  },
  "DatabaseSettings": {
    "Server": "localhost",
    "Port": 3306,
    "Database": "cmg",
    "UserId": "root",
    "Password": ""
  }
}
```

**After changing settings**: Restart the service

## Troubleshooting

### Service won't start?
1. Check Event Viewer (Windows Logs → Application)
2. Verify MySQL is running
3. Check port 5555 is available: `netstat -ano | findstr :5555`
4. Verify `appsettings.json` MySQL credentials

### Can't connect to service?
1. Check service status: `.\Manage-Service.ps1 -Action Status`
2. Verify firewall allows port 5555
3. Test MySQL connection manually

### Need to change database password?
1. Edit `appsettings.json`
2. Restart service: `.\Manage-Service.ps1 -Action Restart`

## Log Locations

- **Event Viewer**: Windows Logs → Application (Source: CMGDBServer)
- **Service Status**: `Get-Service CMGDBServer`

## Client Connection Example

```csharp
var client = new ExampleClient("localhost", 5555);
var response = await client.ExecuteQueryAsync(
    "SELECT * FROM users WHERE id = @id",
    new Dictionary<string, object?> { { "@id", 1 } }
);
```

## Useful Windows Commands

```powershell
# View service details
Get-Service CMGDBServer | Format-List *

# View service in Services Manager
services.msc

# View Event Viewer
eventvwr.msc

# Check if port is in use
netstat -ano | findstr :5555
```
