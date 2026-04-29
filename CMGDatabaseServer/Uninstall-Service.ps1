# Uninstall-Service.ps1
# This script uninstalls the CMG Database Server Windows Service
# Must be run as Administrator

param(
    [string]$ServiceName = "CMGDBServer"
)

# Check if running as administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator!"
    exit 1
}

Write-Host "Uninstalling $ServiceName Windows Service..." -ForegroundColor Yellow

# Check if service exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $existingService) {
    Write-Host "Service '$ServiceName' does not exist." -ForegroundColor Yellow
    exit 0
}

# Stop the service if it's running
if ($existingService.Status -eq 'Running') {
    Write-Host "Stopping service..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force
    Start-Sleep -Seconds 2
    Write-Host "Service stopped." -ForegroundColor Green
}

# Remove the service
Write-Host "Removing service..." -ForegroundColor Yellow
sc.exe delete $ServiceName

if ($LASTEXITCODE -eq 0) {
    Write-Host "Service uninstalled successfully!" -ForegroundColor Green
} else {
    Write-Error "Failed to uninstall service!"
    exit 1
}
