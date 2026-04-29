# Build-And-Deploy.ps1
# Builds the project and optionally updates the service

param(
    [switch]$UpdateService,
    [string]$ServiceName = "CMGDBServer"
)

Write-Host "Building CMG Database Server..." -ForegroundColor Green

# Build in Release mode
dotnet publish -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green

if ($UpdateService) {
    # Check if running as administrator
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Error "Updating the service requires Administrator privileges!"
        exit 1
    }

    # Check if service exists
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

    if ($service) {
        Write-Host "Stopping service..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 2

        Write-Host "Service stopped. Files are ready to be updated." -ForegroundColor Green
        Write-Host "Starting service..." -ForegroundColor Green
        Start-Service -Name $ServiceName

        Write-Host "Service updated and restarted successfully!" -ForegroundColor Green
    } else {
        Write-Host "Service not installed. Run Install-Service.ps1 to install." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Build output location:" -ForegroundColor Cyan
Write-Host "  $PSScriptRoot\bin\Release\net10.0\publish\" -ForegroundColor White
