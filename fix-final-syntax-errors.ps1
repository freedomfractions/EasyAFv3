# Quick Fix for Remaining Syntax Errors
# Fixes @ and % characters in property names

$ErrorActionPreference = "Stop"

Write-Host "`n=== Fixing Remaining Syntax Errors ===" -ForegroundColor Cyan

# Fix HVBreaker.cs - @ character
$hvBreakerFile = "lib\EasyAF.Data\Models\Generated\HVBreaker.cs"
$content = Get-Content $hvBreakerFile -Raw
$content = $content -replace 'RatedKA@MaxKV', 'RatedKAAtMaxKV'
Set-Content $hvBreakerFile $content
Write-Host "? Fixed HVBreaker.cs (@ ? At)" -ForegroundColor Green

# Fix ShortCircuit.cs - % character
$scFile = "lib\EasyAF.Data\Models\Generated\ShortCircuit.cs"
$content = Get-Content $scFile -Raw
$content = $content -replace 'OneTwoCycleDuty%', 'OneTwoCycleDutyPercent'
$content = $content -replace '1/2 Cycle Duty %', '1/2 Cycle Duty Percent'
Set-Content $scFile $content
Write-Host "? Fixed ShortCircuit.cs (% ? Percent)" -ForegroundColor Green

# Re-deploy
Write-Host "`nRe-deploying fixed files..." -ForegroundColor Yellow
Copy-Item "lib\EasyAF.Data\Models\Generated\HVBreaker.cs" -Destination "lib\EasyAF.Data\Models\" -Force
Copy-Item "lib\EasyAF.Data\Models\Generated\ShortCircuit.cs" -Destination "lib\EasyAF.Data\Models\" -Force
Write-Host "? Re-deployed fixed models" -ForegroundColor Green

# Build test
Write-Host "`nBuilding..." -ForegroundColor Yellow
$buildResult = dotnet build lib\EasyAF.Data\EasyAF.Data.csproj 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n? BUILD SUCCESSFUL!" -ForegroundColor Green -BackgroundColor Black
    Write-Host "?? ALL 34 MODELS NOW COMPILE!" -ForegroundColor Green -BackgroundColor Black
} else {
    Write-Host "`n? BUILD STILL FAILED" -ForegroundColor Red
    $buildResult | Select-String "error CS" | Select-Object -First 5
}
