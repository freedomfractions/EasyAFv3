# EasyAF Model Deployment Script
# Deploys audited models from Generated/ to production Models/ folder
# Creates backup, validates, and reports

param(
    [switch]$DryRun,
    [switch]$SkipBackup,
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$scriptPath = $PSScriptRoot
$repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $scriptPath))
$generatedPath = Join-Path $repoRoot "lib\EasyAF.Data\Models\Generated"
$modelsPath = Join-Path $repoRoot "lib\EasyAF.Data\Models"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = Join-Path $modelsPath "Backup_$timestamp"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  EasyAF Model Deployment v1.0" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Validate generated models exist
Write-Host "[1/7] Validating generated models..." -ForegroundColor Yellow
if (-not (Test-Path $generatedPath)) {
    Write-Host "ERROR: Generated models not found at $generatedPath" -ForegroundColor Red
    exit 1
}

$generatedModels = Get-ChildItem "$generatedPath\*.cs" | Where-Object { $_.Name -ne "README.md" }
if ($generatedModels.Count -eq 0) {
    Write-Host "ERROR: No .cs files found in Generated folder" -ForegroundColor Red
    exit 1
}

Write-Host "  Found $($generatedModels.Count) generated models" -ForegroundColor Green

# Step 2: Check for compilation errors in generated models
Write-Host "`n[2/7] Checking generated models compile..." -ForegroundColor Yellow
Push-Location $repoRoot
$buildResult = dotnet build "lib\EasyAF.Data\EasyAF.Data.csproj" --no-restore 2>&1
Pop-Location

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Generated models have compilation errors!" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
    
    if (-not $Force) {
        Write-Host "`nUse -Force to deploy anyway (not recommended)" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "WARNING: Proceeding anyway due to -Force flag" -ForegroundColor Yellow
}
else {
    Write-Host "  ? All models compile successfully" -ForegroundColor Green
}

# Step 3: Create backup of existing models
if (-not $SkipBackup) {
    Write-Host "`n[3/7] Creating backup of existing models..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "  [DRY RUN] Would create backup at: $backupPath" -ForegroundColor Cyan
    }
    else {
        New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
        
        $existingModels = Get-ChildItem "$modelsPath\*.cs" | Where-Object { 
            $_.Name -ne "DataSet.cs" -and 
            $_.Name -ne "Project.cs" -and
            -not $_.FullName.Contains("\Backup_") -and
            -not $_.FullName.Contains("\Generated")
        }
        
        foreach ($model in $existingModels) {
            Copy-Item $model.FullName -Destination $backupPath
        }
        
        Write-Host "  ? Backed up $($existingModels.Count) models to: $backupPath" -ForegroundColor Green
    }
}
else {
    Write-Host "`n[3/7] Skipping backup (as requested)" -ForegroundColor Yellow
}

# Step 4: List models to be deployed
Write-Host "`n[4/7] Models to deploy:" -ForegroundColor Yellow

$deployList = @()
foreach ($model in $generatedModels) {
    $deployList += $model.Name
    $targetPath = Join-Path $modelsPath $model.Name
    $exists = Test-Path $targetPath
    
    $status = if ($exists) { "REPLACE" } else { "NEW" }
    $color = if ($exists) { "Yellow" } else { "Green" }
    
    Write-Host "  [$status] $($model.Name)" -ForegroundColor $color
}

# Step 5: Confirm deployment
if (-not $DryRun -and -not $Force) {
    Write-Host "`n[5/7] Ready to deploy $($generatedModels.Count) models." -ForegroundColor Yellow
    $confirmation = Read-Host "Continue? (y/N)"
    
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Host "Deployment cancelled by user." -ForegroundColor Red
        exit 0
    }
}
else {
    Write-Host "`n[5/7] Deployment confirmation:" -ForegroundColor Yellow
    if ($DryRun) {
        Write-Host "  [DRY RUN] Skipping confirmation" -ForegroundColor Cyan
    }
    else {
        Write-Host "  Automatic (-Force flag)" -ForegroundColor Yellow
    }
}

# Step 6: Deploy models
Write-Host "`n[6/7] Deploying models..." -ForegroundColor Yellow

$deployed = 0
$errors = @()

foreach ($model in $generatedModels) {
    $sourcePath = $model.FullName
    $targetPath = Join-Path $modelsPath $model.Name
    
    try {
        if ($DryRun) {
            Write-Host "  [DRY RUN] Would copy: $($model.Name)" -ForegroundColor Cyan
        }
        else {
            Copy-Item $sourcePath -Destination $targetPath -Force
            Write-Host "  ? Deployed: $($model.Name)" -ForegroundColor Green
            $deployed++
        }
    }
    catch {
        $errors += "  ? Failed: $($model.Name) - $_"
        Write-Host "  ? Failed: $($model.Name)" -ForegroundColor Red
    }
}

# Step 7: Final validation build
Write-Host "`n[7/7] Final validation..." -ForegroundColor Yellow

if (-not $DryRun) {
    Push-Location $repoRoot
    $finalBuild = dotnet build "lib\EasyAF.Data\EasyAF.Data.csproj" --no-restore 2>&1
    Pop-Location
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ? DEPLOYMENT FAILED - Models don't compile!" -ForegroundColor Red
        Write-Host $finalBuild -ForegroundColor Red
        
        Write-Host "`n  Attempting automatic rollback..." -ForegroundColor Yellow
        if (Test-Path $backupPath) {
            foreach ($backup in Get-ChildItem "$backupPath\*.cs") {
                Copy-Item $backup.FullName -Destination $modelsPath -Force
            }
            Write-Host "  ? Rolled back from backup" -ForegroundColor Green
        }
        else {
            Write-Host "  ? No backup found - manual intervention required!" -ForegroundColor Red
        }
        
        exit 1
    }
    else {
        Write-Host "  ? Final build successful!" -ForegroundColor Green
    }
}
else {
    Write-Host "  [DRY RUN] Skipping validation build" -ForegroundColor Cyan
}

# Summary Report
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  DEPLOYMENT SUMMARY" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "MODE: Dry Run (no changes made)" -ForegroundColor Cyan
}
else {
    Write-Host "MODE: Live Deployment" -ForegroundColor Green
}

Write-Host "Generated Models Found: $($generatedModels.Count)" -ForegroundColor White
Write-Host "Models Deployed: $deployed" -ForegroundColor $(if ($deployed -eq $generatedModels.Count) { "Green" } else { "Yellow" })

if ($errors.Count -gt 0) {
    Write-Host "Errors: $($errors.Count)" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host $error -ForegroundColor Red
    }
}
else {
    Write-Host "Errors: 0" -ForegroundColor Green
}

if (-not $SkipBackup -and -not $DryRun) {
    Write-Host "Backup Location: $backupPath" -ForegroundColor White
}

Write-Host "`n========================================`n" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "To perform actual deployment, run without -DryRun flag" -ForegroundColor Yellow
}
elseif ($deployed -eq $generatedModels.Count -and $errors.Count -eq 0) {
    Write-Host "? DEPLOYMENT SUCCESSFUL!" -ForegroundColor Green
    Write-Host "All $deployed models are now live in production." -ForegroundColor Green
}
else {
    Write-Host "? DEPLOYMENT COMPLETED WITH WARNINGS" -ForegroundColor Yellow
}

Write-Host ""
