# Install-Service.ps1
# This script installs the CMG Database Server as a Windows Service
# Must be run as Administrator

param(
    [string]$ServiceName = "CMGDBServer",
    [string]$DisplayName = "CMG Database Server",
    [string]$Description = "MySQL database server for CMG application",
    [string]$BinPath = "$PSScriptRoot\bin\Release\net10.0\CMGDatabaseServer.exe"
)

# Check if running as administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator!"
    exit 1
}

Write-Host "Installing $DisplayName as a Windows Service..." -ForegroundColor Green

# Check if service already exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existingService) {
    Write-Host "Service already exists. Stopping and removing..." -ForegroundColor Yellow

    # Stop the service if it's running
    if ($existingService.Status -eq 'Running') {
        Stop-Service -Name $ServiceName -Force
        Write-Host "Service stopped." -ForegroundColor Yellow
    }

    # Remove the service
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
    Write-Host "Existing service removed." -ForegroundColor Yellow
}

# Verify the executable exists
if (-not (Test-Path $BinPath)) {
    Write-Error "Executable not found at: $BinPath"
    Write-Host "Please build the project in Release mode first:" -ForegroundColor Yellow
    Write-Host "  dotnet publish -c Release" -ForegroundColor Cyan
    exit 1
}

# Create the service
Write-Host "Creating service..." -ForegroundColor Green
sc.exe create $ServiceName binPath= $BinPath start= auto DisplayName= $DisplayName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create service!"
    exit 1
}

# Set the description
sc.exe description $ServiceName $Description

# Configure service to restart on failure
sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000

Write-Host "Service created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To start the service now, run:" -ForegroundColor Cyan
Write-Host "  Start-Service -Name $ServiceName" -ForegroundColor White
Write-Host ""
Write-Host "To configure the service to start automatically:" -ForegroundColor Cyan
Write-Host "  Set-Service -Name $ServiceName -StartupType Automatic" -ForegroundColor White
Write-Host ""
Write-Host "To start the service:" -ForegroundColor Green
Start-Service -Name $ServiceName
Write-Host "Service started!" -ForegroundColor Green
