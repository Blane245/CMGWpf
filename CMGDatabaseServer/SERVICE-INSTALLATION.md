# Installing CMGDBServer as a Windows Service

This guide explains how to install, configure, and manage the CMG Database Server as a Windows Service that starts automatically when your computer boots.

## Prerequisites

- Windows operating system
- .NET 10.0 Runtime installed
- MySQL Server installed and running
- Administrator privileges

## Installation Steps

### 1. Build the Project

First, build the project in Release mode:

```powershell
dotnet publish -c Release
```

This creates the executable at: `bin\Release\net10.0\CMGDatabaseServer.exe`

### 2. Configure Database Settings

Before installing, edit `appsettings.json` to configure your MySQL connection:

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
    "Password": "your_password_here"
  }
}
```

### 3. Install the Service

Run PowerShell **as Administrator** and execute:

```powershell
.\Install-Service.ps1
```

This script will:
- ✅ Create the Windows Service named "CMGDBServer"
- ✅ Set it to start automatically on boot
- ✅ Configure automatic restart on failure
- ✅ Start the service immediately

## Managing the Service

### Using PowerShell Scripts

#### Check Service Status
```powershell
.\Manage-Service.ps1 -Action Status
```

#### Start the Service
```powershell
.\Manage-Service.ps1 -Action Start
```

#### Stop the Service
```powershell
.\Manage-Service.ps1 -Action Stop
```

#### Restart the Service
```powershell
.\Manage-Service.ps1 -Action Restart
```

### Using Windows Services Manager

1. Press `Win + R`
2. Type `services.msc` and press Enter
3. Find "CMG Database Server" in the list
4. Right-click to Start, Stop, or Restart

### Using PowerShell Commands

```powershell
# Start the service
Start-Service -Name CMGDBServer

# Stop the service
Stop-Service -Name CMGDBServer

# Restart the service
Restart-Service -Name CMGDBServer

# Check service status
Get-Service -Name CMGDBServer
```

## Uninstalling the Service

Run PowerShell **as Administrator** and execute:

```powershell
.\Uninstall-Service.ps1
```

This will:
- Stop the service if running
- Remove the service from Windows

## Service Details

- **Service Name**: CMGDBServer
- **Display Name**: CMG Database Server
- **Description**: MySQL database server for CMG application
- **Startup Type**: Automatic
- **Recovery**: Restarts automatically on failure (up to 3 times)

## Viewing Service Logs

Since this is a Windows Service, logs are written to:

1. **Windows Event Viewer**:
   - Press `Win + R`, type `eventvwr.msc`
   - Navigate to: Windows Logs → Application
   - Look for events from source "CMGDBServer"

2. **Custom Log File** (if configured):
   - Check the application directory for log files

## Troubleshooting

### Service Won't Start

1. **Check MySQL Connection**:
   - Ensure MySQL is running
   - Verify connection settings in `appsettings.json`
   - Test MySQL connection manually

2. **Check Port Availability**:
   - Ensure port 5555 (or your configured port) is not in use
   - Run: `netstat -ano | findstr :5555`

3. **Check Permissions**:
   - Service runs under LocalSystem account by default
   - Ensure it has access to MySQL

4. **View Event Logs**:
   - Check Windows Event Viewer for error details

### Change Service Account

To run the service under a different account:

```powershell
sc.exe config CMGDBServer obj= "DOMAIN\Username" password= "password"
```

### Update Configuration

After changing `appsettings.json`:

```powershell
.\Manage-Service.ps1 -Action Restart
```

## Testing the Service

After installation, test the connection using the example client:

```csharp
var client = new ExampleClient();
var response = await client.ExecuteQueryAsync("SELECT 1 + 1 AS result");
Console.WriteLine($"Success: {response?.Success}");
```

## Advanced Configuration

### Custom Installation Path

```powershell
.\Install-Service.ps1 -BinPath "C:\Custom\Path\CMGDatabaseServer.exe"
```

### Change Service Name

```powershell
.\Install-Service.ps1 -ServiceName "MyCustomServiceName" -DisplayName "My Custom Display Name"
```

## Security Recommendations

⚠️ **Important for Production**:

1. **Change Default Password**: Update MySQL password in `appsettings.json`
2. **Use Encryption**: Consider encrypting sensitive configuration values
3. **Firewall Rules**: Configure Windows Firewall to allow only necessary connections
4. **Service Account**: Run under a dedicated service account with minimal privileges
5. **SSL/TLS**: Enable encrypted connections to MySQL

## Automatic Updates

After updating the application:

1. Build the new version: `dotnet publish -c Release`
2. Stop the service: `.\Manage-Service.ps1 -Action Stop`
3. Replace the executable files
4. Start the service: `.\Manage-Service.ps1 -Action Start`

Or use the provided script:

```powershell
# Stop service
.\Manage-Service.ps1 -Action Stop

# Build and publish
dotnet publish -c Release

# Start service
.\Manage-Service.ps1 -Action Start
```
