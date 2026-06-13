# PowerShell script to verify .cmg file association icon registration
# Run this after installing your application

Write-Host "Checking .cmg file association..." -ForegroundColor Cyan

# Check if .cmg extension is registered
$cmgKey = Get-ItemProperty -Path "Registry::HKEY_CLASSES_ROOT\.cmg" -ErrorAction SilentlyContinue
if ($cmgKey) {
	Write-Host "✓ .cmg extension is registered" -ForegroundColor Green
	$progId = $cmgKey.'(default)'
	Write-Host "  ProgID: $progId" -ForegroundColor Gray

	# Check the ProgID's default icon
	$iconKey = Get-ItemProperty -Path "Registry::HKEY_CLASSES_ROOT\$progId\DefaultIcon" -ErrorAction SilentlyContinue
	if ($iconKey) {
		$iconPath = $iconKey.'(default)'
		Write-Host "✓ Icon is registered: $iconPath" -ForegroundColor Green

		# Verify the icon file exists
		$iconFile = $iconPath -replace ',\d+$', ''  # Remove icon index
		if (Test-Path $iconFile) {
			Write-Host "✓ Icon file exists" -ForegroundColor Green
		} else {
			Write-Host "✗ Icon file NOT FOUND: $iconFile" -ForegroundColor Red
		}
	} else {
		Write-Host "✗ No DefaultIcon registered for $progId" -ForegroundColor Red
	}
} else {
	Write-Host "✗ .cmg extension is NOT registered" -ForegroundColor Red
}

Write-Host "`nTo refresh icons, run:" -ForegroundColor Yellow
Write-Host "  ie4uinit.exe -show" -ForegroundColor White
Write-Host "or restart Windows Explorer" -ForegroundColor White
