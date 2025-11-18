# Global Refactor: LVCB ? LVBreaker
# Performs workspace-wide rename with safety checks

param(
    [switch]$DryRun,
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$repoRoot = "C:\src\EasyAFv3"

Write-Host "`n??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?       GLOBAL REFACTOR: LVCB ? LVBreaker                    ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

# STEP 1: Find all files containing LVCB
Write-Host "[1/6] Scanning workspace for LVCB references..." -ForegroundColor Yellow

$searchPaths = @(
    "lib\*.cs",
    "modules\*.cs",
    "app\*.cs",
    "*.csproj",
    "docs\*.md"
)

$filesToUpdate = @()
foreach ($pattern in $searchPaths) {
    $files = Get-ChildItem -Path $repoRoot -Recurse -Include $pattern -Exclude *\bin\*,*\obj\*,*\Generated\*
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match '\bLVCB\b') {
            $matches = ([regex]::Matches($content, '\bLVCB\b')).Count
            $filesToUpdate += @{
                Path = $file.FullName
                RelPath = $file.FullName.Replace("$repoRoot\", "")
                Count = $matches
            }
        }
    }
}

Write-Host "  Found $($filesToUpdate.Count) files with LVCB references" -ForegroundColor Green
foreach ($file in $filesToUpdate | Sort-Object { $_.RelPath }) {
    Write-Host "    $($file.RelPath) ($($file.Count) occurrences)" -ForegroundColor White
}

if ($filesToUpdate.Count -eq 0) {
    Write-Host "`n? No LVCB references found. Nothing to refactor." -ForegroundColor Green
    exit 0
}

# STEP 2: Confirmation
if (-not $DryRun -and -not $Force) {
    Write-Host "`n[2/6] Confirm refactoring..." -ForegroundColor Yellow
    Write-Host "  This will replace ALL occurrences of 'LVCB' with 'LVBreaker'" -ForegroundColor Yellow
    Write-Host "  in $($filesToUpdate.Count) files." -ForegroundColor Yellow
    $confirm = Read-Host "`nProceed? (y/N)"
    
    if ($confirm -ne 'y' -and $confirm -ne 'Y') {
        Write-Host "`n? Refactoring cancelled by user." -ForegroundColor Red
        exit 0
    }
}
else {
    Write-Host "`n[2/6] Confirmation:" -ForegroundColor Yellow
    if ($DryRun) {
        Write-Host "  [DRY RUN MODE] - No files will be modified" -ForegroundColor Cyan
    }
    else {
        Write-Host "  Automatic confirmation (-Force)" -ForegroundColor Yellow
    }
}

# STEP 3: Create backup
if (-not $DryRun) {
    Write-Host "`n[3/6] Creating backup..." -ForegroundColor Yellow
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupPath = Join-Path $repoRoot "Backups\Refactor_LVCB_$timestamp"
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    
    foreach ($file in $filesToUpdate) {
        $relativePath = $file.RelPath
        $backupFile = Join-Path $backupPath $relativePath
        $backupDir = Split-Path $backupFile -Parent
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        Copy-Item $file.Path -Destination $backupFile
    }
    
    Write-Host "  ? Backed up $($filesToUpdate.Count) files to:" -ForegroundColor Green
    Write-Host "    $backupPath" -ForegroundColor White
}
else {
    Write-Host "`n[3/6] Backup skipped (dry run)" -ForegroundColor Cyan
}

# STEP 4: Perform replacements
Write-Host "`n[4/6] Performing replacements..." -ForegroundColor Yellow

$updatedCount = 0
$totalReplacements = 0

foreach ($file in $filesToUpdate) {
    $content = Get-Content $file.Path -Raw
    $originalContent = $content
    
    # Replace LVCB with LVBreaker (preserving word boundaries)
    $content = $content -replace '\bLVCB\b', 'LVBreaker'
    $content = $content -replace '\bLVCBs\b', 'LVBreakers'
    
    # Count replacements
    $replacements = ([regex]::Matches($originalContent, '\bLVCB\b')).Count
    $totalReplacements += $replacements
    
    if ($content -ne $originalContent) {
        if (-not $DryRun) {
            Set-Content $file.Path $content
            Write-Host "  ? Updated: $($file.RelPath) ($replacements replacements)" -ForegroundColor Green
        }
        else {
            Write-Host "  [DRY RUN] Would update: $($file.RelPath) ($replacements replacements)" -ForegroundColor Cyan
        }
        $updatedCount++
    }
}

Write-Host "`n  Summary: $totalReplacements total replacements in $updatedCount files" -ForegroundColor White

# STEP 5: Verify no LVCB remains
Write-Host "`n[5/6] Verifying refactoring..." -ForegroundColor Yellow

if (-not $DryRun) {
    $remaining = @()
    foreach ($pattern in $searchPaths) {
        $files = Get-ChildItem -Path $repoRoot -Recurse -Include $pattern -Exclude *\bin\*,*\obj\*,*\Backups\*
        foreach ($file in $files) {
            $matches = Select-String -Path $file.FullName -Pattern '\bLVCB\b' -SimpleMatch
            if ($matches) {
                $remaining += $file.FullName.Replace("$repoRoot\", "")
            }
        }
    }
    
    if ($remaining.Count -gt 0) {
        Write-Host "  ?? WARNING: LVCB still found in:" -ForegroundColor Yellow
        $remaining | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
    }
    else {
        Write-Host "  ? No LVCB references remain" -ForegroundColor Green
    }
}
else {
    Write-Host "  [DRY RUN] Verification skipped" -ForegroundColor Cyan
}

# STEP 6: Build verification
Write-Host "`n[6/6] Building solution..." -ForegroundColor Yellow

if (-not $DryRun) {
    Push-Location $repoRoot
    $buildResult = dotnet build 2>&1
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? Build successful!" -ForegroundColor Green
    }
    else {
        Write-Host "  ? Build failed!" -ForegroundColor Red
        Write-Host "`nShowing first 10 errors:" -ForegroundColor Yellow
        $buildResult | Select-String "error" | Select-Object -First 10 | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Red
        }
        
        Write-Host "`nTo rollback, restore from:" -ForegroundColor Yellow
        Write-Host "  $backupPath" -ForegroundColor White
        exit 1
    }
}
else {
    Write-Host "  [DRY RUN] Build skipped" -ForegroundColor Cyan
}

# FINAL SUMMARY
Write-Host "`n??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?                 REFACTORING COMPLETE                       ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "MODE: Dry Run (no changes made)" -ForegroundColor Cyan
    Write-Host "`nTo perform actual refactoring, run:" -ForegroundColor White
    Write-Host "  .\refactor-lvcb-to-lvbreaker.ps1" -ForegroundColor Cyan
}
else {
    Write-Host "? Successfully refactored LVCB ? LVBreaker" -ForegroundColor Green
    Write-Host "  Files updated: $updatedCount" -ForegroundColor White
    Write-Host "  Total replacements: $totalReplacements" -ForegroundColor White
    Write-Host "  Build status: PASSED" -ForegroundColor Green
    
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "  1. Review changes with: git diff" -ForegroundColor White
    Write-Host "  2. Run tests: dotnet test" -ForegroundColor White
    Write-Host "  3. Commit: git add . && git commit -m 'refactor: Rename LVCB to LVBreaker across workspace'" -ForegroundColor White
}

Write-Host ""
