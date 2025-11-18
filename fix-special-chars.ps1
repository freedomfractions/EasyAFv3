# Fix Special Characters in Generated Models
# Handles %, +, /, ^, and other illegal C# property name characters

$ErrorActionPreference = "Stop"
$generatedPath = "lib\EasyAF.Data\Models\Generated"
$modelsPath = "lib\EasyAF.Data\Models"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Fix Special Characters in Models" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Get all .cs files in Generated folder (excluding README)
$files = Get-ChildItem "$generatedPath\*.cs" | Where-Object { $_.Name -ne "README.md" }

Write-Host "Processing $($files.Count) model files..." -ForegroundColor Yellow

$fixCount = 0
$filesFixes = @{}

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix % in property names (percent sign)
    $content = $content -replace '([a-zA-Z0-9])% \{', '$1Percent {'
    $content = $content -replace 'public string\? %', 'public string? Percent'
    $content = $content -replace 'SCFailureMode%', 'SCFailureModePercent'
    $content = $content -replace 'Scaling%', 'ScalingPercent'
    $content = $content -replace '100%', '100Percent'
    $content = $content -replace 'Pec-r%', 'PecrPercent'
    
    # Fix + in property names (plus sign)
    $content = $content -replace 'Ohm\+X0', 'OhmPlusX0'
    $content = $content -replace '0\+3X', '0Plus3X'
    $content = $content -replace '\+3X', 'Plus3X'
    
    # Fix ^ in property names (caret/exponent)
    $content = $content -replace 'I\^2t', 'ISquaredT'
    $content = $content -replace '\^2', 'Squared'
    
    # Fix / in property names (slash) - but be careful not to break // comments
    $content = $content -replace '([a-zA-Z])\/([a-zA-Z])', '$1$2'
    $content = $content -replace 'X/R', 'XR'
    
    # Fix - in property names that break (hyphen)
    $content = $content -replace 'Pec-r', 'Pecr'
    
    # Fix numeric prefixes (property names can't start with numbers)
    $content = $content -replace 'public string\? 2X', 'public string? TwoX'
    $content = $content -replace 'public string\? 3', 'public string? Three'
    $content = $content -replace 'public string\? 1/', 'public string? OneSlash'
    
    # Fix malformed Description attributes with embedded quotes
    $content = $content -replace '\[Description\(""([^""]*)""([^""]*)""([^""]*)""([^""]*)""([^""]*)""([^""]*)""\)\]', '[Description("$1$2$3$4$5$6")]'
    $content = $content -replace '""', '"'
    
    # Check if any changes were made
    if ($content -ne $originalContent) {
        Set-Content $file.FullName $content
        $fixCount++
        $filesFixes[$file.Name] = $true
        Write-Host "  ? Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Files Processed: $($files.Count)" -ForegroundColor White
Write-Host "Files Fixed: $fixCount" -ForegroundColor Green

if ($fixCount -gt 0) {
    Write-Host "`nFixed Files:" -ForegroundColor Yellow
    foreach ($fileName in $filesFixes.Keys | Sort-Object) {
        Write-Host "  - $fileName" -ForegroundColor White
    }
}

# Now copy all fixed files to Models folder
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Deploying to Models Folder" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Copying all models from Generated to Models..." -ForegroundColor Yellow

# Copy all .cs files except README.md
$files | ForEach-Object {
    Copy-Item $_.FullName -Destination $modelsPath -Force
}

Write-Host "? Copied $($files.Count) models" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Building Solution" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Running build..." -ForegroundColor Yellow
$buildOutput = dotnet build 2>&1

# Check if build succeeded
if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "All models deployed and building correctly!" -ForegroundColor Green
} else {
    Write-Host "`n? BUILD FAILED" -ForegroundColor Red
    Write-Host "Showing first 20 errors:" -ForegroundColor Yellow
    $buildOutput | Select-String "error CS" | Select-Object -First 20 | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Red
    }
    exit 1
}

Write-Host "`n========================================`n" -ForegroundColor Cyan
