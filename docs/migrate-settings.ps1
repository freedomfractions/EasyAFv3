# EasyAF Data Type Settings Migration Script
# This script migrates your data type settings from the backup to the new global format

$settingsPath = "$env:APPDATA\EasyAF\settings.json"
$migratedDataPath = "C:\src\EasyAFv3\docs\migrated-datatype-settings.json"

Write-Host "===== EasyAF Settings Migration =====" -ForegroundColor Cyan
Write-Host ""

# 1. Backup current settings
$backupPath = "$settingsPath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Write-Host "[1/4] Creating backup..." -ForegroundColor Yellow
Copy-Item $settingsPath $backupPath -Force
Write-Host "      Backup saved to: $backupPath" -ForegroundColor Green
Write-Host ""

# 2. Load current settings
Write-Host "[2/4] Loading current settings..." -ForegroundColor Yellow
$current = Get-Content $settingsPath -Raw | ConvertFrom-Json
Write-Host "      Current settings loaded" -ForegroundColor Green
Write-Host ""

# 3. Load migrated data type settings
Write-Host "[3/4] Loading your data type configuration..." -ForegroundColor Yellow
$migrated = Get-Content $migratedDataPath -Raw | ConvertFrom-Json
$dataTypeSettings = $migrated.'Global.DataTypeVisibility'
Write-Host "      Found settings for $($dataTypeSettings.DataTypes.PSObject.Properties.Count) data types" -ForegroundColor Green
Write-Host ""

# 4. Merge into current settings
Write-Host "[4/4] Merging settings..." -ForegroundColor Yellow

# Ensure Global object exists
if (-not $current.PSObject.Properties['Global']) {
    $current | Add-Member -NotePropertyName 'Global' -NotePropertyValue ([PSCustomObject]@{}) -Force
}

# Add the data type settings
$current.Global | Add-Member -NotePropertyName 'Global.DataTypeVisibility' -NotePropertyValue $dataTypeSettings -Force

# Save
$current | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8

Write-Host "      Settings merged and saved!" -ForegroundColor Green
Write-Host ""
Write-Host "===== Migration Complete! =====" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  - Enabled data types: ArcFlash, Bus, Cable, Fuse, LVCB, MVCB, ShortCircuit" -ForegroundColor White
Write-Host "  - Each enabled type has custom property selections" -ForegroundColor White
Write-Host "  - Disabled types: 25 (will not appear in Map/Spec editors)" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Restart EasyAF to load the new settings" -ForegroundColor White
Write-Host "  2. Go to Options > Data Types to verify" -ForegroundColor White
Write-Host "  3. If needed, restore from: $backupPath" -ForegroundColor Gray
Write-Host ""
