# PowerShell script to replace existing models with generated ones

$sourcePath = "C:\src\EasyAFv3\lib\EasyAF.Data\Models\Generated"
$destPath = "C:\src\EasyAFv3\lib\EasyAF.Data\Models"

# List of models to replace (ones we already implemented)
$modelsToReplace = @(
    "ArcFlash.cs",
    "ShortCircuit.cs",
    "Bus.cs",
    "LVCB.cs",
    "Fuse.cs",
    "Cable.cs",
    "Transformer.cs",
    "Motor.cs",
    "Generator.cs",
    "Utility.cs",
    "Capacitor.cs",
    "Load.cs"
)

# New models to add
$newModels = @(
    "Panel.cs",
    "MCC.cs",
    "Busway.cs",
    "TransmissionLine.cs",
    "CLReactor.cs",
    "Transformer3W.cs",
    "ZigzagTransformer.cs",
    "HVBreaker.cs",
    "Relay.cs",
    "CT.cs",
    "Switch.cs",
    "ATS.cs",
    "Shunt.cs",
    "Filter.cs",
    "AFD.cs",
    "UPS.cs",
    "Inverter.cs",
    "Rectifier.cs",
    "Photovoltaic.cs",
    "Battery.cs",
    "Meter.cs",
    "POC.cs"
)

Write-Host "=== EASYAF MODEL REPLACEMENT SCRIPT ===" -ForegroundColor Cyan
Write-Host "`nThis script will:" -ForegroundColor Yellow
Write-Host "1. Backup existing models to Models/Backup" -ForegroundColor Yellow
Write-Host "2. Replace 12 existing models with generated versions" -ForegroundColor Yellow
Write-Host "3. Copy 22 new models" -ForegroundColor Yellow
Write-Host "4. Generate summary report" -ForegroundColor Yellow

Write-Host "`n??  WARNING: This will overwrite existing model files!" -ForegroundColor Red
$confirm = Read-Host "Continue? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit
}

# Create backup directory
$backupPath = "$destPath\Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
New-Item -ItemType Directory -Path $backupPath -Force | Out-Null

$replaced = 0
$added = 0

# Backup and replace existing models
foreach ($model in $modelsToReplace) {
    $sourceFile = Join-Path $sourcePath $model
    $destFile = Join-Path $destPath $model
    
    if (Test-Path $sourceFile) {
        # Backup existing
        if (Test-Path $destFile) {
            Copy-Item $destFile -Destination $backupPath
        }
        
        # Replace with generated
        Copy-Item $sourceFile -Destination $destFile -Force
        Write-Host "? Replaced: $model" -ForegroundColor Green
        $replaced++
    } else {
        Write-Host "??  Source not found: $model" -ForegroundColor Yellow
    }
}

# Copy new models
foreach ($model in $newModels) {
    $sourceFile = Join-Path $sourcePath $model
    $destFile = Join-Path $destPath $model
    
    if (Test-Path $sourceFile) {
        Copy-Item $sourceFile -Destination $destFile -Force
        Write-Host "? Added: $model" -ForegroundColor Cyan
        $added++
    } else {
        Write-Host "??  Source not found: $model" -ForegroundColor Yellow
    }
}

Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Replaced: $replaced models" -ForegroundColor Green
Write-Host "Added: $added new models" -ForegroundColor Cyan
Write-Host "Backup location: $backupPath" -ForegroundColor Yellow
Write-Host "`n? Model replacement complete!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Run 'dotnet build' to check for errors" -ForegroundColor White
Write-Host "2. Review generated files in Models directory" -ForegroundColor White
Write-Host "3. Run audit script to verify 100% match" -ForegroundColor White
