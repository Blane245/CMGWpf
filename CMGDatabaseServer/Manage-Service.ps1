# Manage-Service.ps1
# Helper script to manage the CMG Database Server service

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Start", "Stop", "Restart", "Status")]
    [string]$Action,

    [string]$ServiceName = "CMGDBServer"
)

# Check if running as administrator for Start/Stop/Restart
if ($Action -ne "Status") {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Error "This script must be run as Administrator for $Action operation!"
        exit 1
    }
}

# Check if service exists
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $service) {
    Write-Error "Service '$ServiceName' does not exist. Please install it first using Install-Service.ps1"
    exit 1
}

switch ($Action) {
    "Start" {
        if ($service.Status -eq 'Running') {
            Write-Host "Service is already running." -ForegroundColor Yellow
        } else {
            Write-Host "Starting service..." -ForegroundColor Green
            Start-Service -Name $ServiceName
            Write-Host "Service started successfully!" -ForegroundColor Green
        }
    }

    "Stop" {
        if ($service.Status -eq 'Stopped') {
            Write-Host "Service is already stopped." -ForegroundColor Yellow
        } else {
            Write-Host "Stopping service..." -ForegroundColor Yellow
            Stop-Service -Name $ServiceName -Force
            Write-Host "Service stopped successfully!" -ForegroundColor Green
        }
    }

    "Restart" {
        Write-Host "Restarting service..." -ForegroundColor Green
        Restart-Service -Name $ServiceName -Force
        Write-Host "Service restarted successfully!" -ForegroundColor Green
    }

    "Status" {
        $service = Get-Service -Name $ServiceName
        Write-Host ""
        Write-Host "Service Name:    $($service.Name)" -ForegroundColor Cyan
        Write-Host "Display Name:    $($service.DisplayName)" -ForegroundColor Cyan
        Write-Host "Status:          $($service.Status)" -ForegroundColor $(if ($service.Status -eq 'Running') { 'Green' } else { 'Yellow' })
        Write-Host "Startup Type:    $($service.StartType)" -ForegroundColor Cyan
        Write-Host ""
    }
}
