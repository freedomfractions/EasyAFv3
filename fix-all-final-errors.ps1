# Final Comprehensive Fix
# Fixes all remaining build errors:
# 1. DataSet.cs - Transformer ? Transformer2W
# 2. Property name collisions (CT, Filter, Capacitor, CLReactor)
# 3. @ and % characters

$ErrorActionPreference = "Stop"

Write-Host "`n??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?         FINAL COMPREHENSIVE FIX - ALL ISSUES               ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

# FIX 1: DataSet.cs - Transformer ? Transformer2W
Write-Host "[1/3] Fixing DataSet.cs (Transformer ? Transformer2W)..." -ForegroundColor Yellow
$dataSetFile = "lib\EasyAF.Data\Models\DataSet.cs"
$content = Get-Content $dataSetFile -Raw
$content = $content -replace 'Dictionary<string, Transformer>', 'Dictionary<string, Transformer2W>'
$content = $content -replace '<summary>Gets or sets the dictionary of transformer', '<summary>Gets or sets the dictionary of 2-winding transformer'
Set-Content $dataSetFile $content
Write-Host "  ? Fixed DataSet.cs" -ForegroundColor Green

# FIX 2: Property name collisions in Generated folder
Write-Host "`n[2/3] Fixing property name collisions (plural ? singular)..." -ForegroundColor Yellow

# CT.cs: CT ? CTs
$ctFile = "lib\EasyAF.Data\Models\Generated\CT.cs"
$content = Get-Content $ctFile -Raw
$content = $content -replace 'public string\? CT \{', 'public string? CTs {'
$content = $content -replace 'get => CT;', 'get => CTs;'
$content = $content -replace 'set => CT = value;', 'set => CTs = value;'
$content = $content -replace '\$"CT: \{CT\}"', '$"CT: {CTs}"'
Set-Content $ctFile $content
Write-Host "  ? Fixed CT.cs (CT ? CTs)" -ForegroundColor Green

# Filter.cs: Filter ? Filters
$filterFile = "lib\EasyAF.Data\Models\Generated\Filter.cs"
$content = Get-Content $filterFile -Raw
$content = $content -replace 'public string\? Filter \{', 'public string? Filters {'
$content = $content -replace 'get => Filter;', 'get => Filters;'
$content = $content -replace 'set => Filter = value;', 'set => Filters = value;'
$content = $content -replace '\$"Filter: \{Filter\}"', '$"Filter: {Filters}"'
Set-Content $filterFile $content
Write-Host "  ? Fixed Filter.cs (Filter ? Filters)" -ForegroundColor Green

# Capacitor.cs: Capacitor ? Capacitors
$capFile = "lib\EasyAF.Data\Models\Generated\Capacitor.cs"
$content = Get-Content $capFile -Raw
$content = $content -replace 'public string\? Capacitor \{', 'public string? Capacitors {'
$content = $content -replace 'get => Capacitor;', 'get => Capacitors;'
$content = $content -replace 'set => Capacitor = value;', 'set => Capacitors = value;'
$content = $content -replace '\$"Capacitor: \{Capacitor\}"', '$"Capacitor: {Capacitors}"'
Set-Content $capFile $content
Write-Host "  ? Fixed Capacitor.cs (Capacitor ? Capacitors)" -ForegroundColor Green

# CLReactor.cs: CLReactor ? CLReactors
$clFile = "lib\EasyAF.Data\Models\Generated\CLReactor.cs"
$content = Get-Content $clFile -Raw
$content = $content -replace 'public string\? CLReactor \{', 'public string? CLReactors {'
$content = $content -replace 'get => CLReactor;', 'get => CLReactors;'
$content = $content -replace 'set => CLReactor = value;', 'set => CLReactors = value;'
$content = $content -replace '\$"CLReactor: \{CLReactor\}"', '$"CLReactor: {CLReactors}"'
Set-Content $clFile $content
Write-Host "  ? Fixed CLReactor.cs (CLReactor ? CLReactors)" -ForegroundColor Green

# FIX 3: Special characters (@ and %)
Write-Host "`n[3/3] Fixing special characters (@ and %)..." -ForegroundColor Yellow

# HVBreaker.cs: @ ? At
$hvBreakerFile = "lib\EasyAF.Data\Models\Generated\HVBreaker.cs"
$content = Get-Content $hvBreakerFile -Raw
$content = $content -replace 'RatedKA@MaxKV', 'RatedKAAtMaxKV'
Set-Content $hvBreakerFile $content
Write-Host "  ? Fixed HVBreaker.cs (@ ? At)" -ForegroundColor Green

# ShortCircuit.cs: % ? Percent
$scFile = "lib\EasyAF.Data\Models\Generated\ShortCircuit.cs"
$content = Get-Content $scFile -Raw
$content = $content -replace 'OneTwoCycleDuty%', 'OneTwoCycleDutyPercent'
$content = $content -replace '1/2 Cycle Duty %', '1/2 Cycle Duty Percent'
Set-Content $scFile $content
Write-Host "  ? Fixed ShortCircuit.cs (% ? Percent)" -ForegroundColor Green

# DEPLOY: Copy all fixed files to Models folder
Write-Host "`n[DEPLOY] Copying all fixed models to production..." -ForegroundColor Yellow
Copy-Item "lib\EasyAF.Data\Models\Generated\*.cs" -Destination "lib\EasyAF.Data\Models\" -Force
Write-Host "  ? Deployed 34 models" -ForegroundColor Green

# BUILD TEST
Write-Host "`n[BUILD] Testing compilation..." -ForegroundColor Yellow
$buildResult = dotnet build lib\EasyAF.Data\EasyAF.Data.csproj 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n??????????????????????????????????????????????????????????????" -ForegroundColor Green -BackgroundColor Black
    Write-Host "?                  ? BUILD SUCCESSFUL! ?                   ?" -ForegroundColor Green -BackgroundColor Black
    Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Green -BackgroundColor Black
    Write-Host "`n?? ALL 34 EASYPOWER MODELS NOW COMPILE PERFECTLY! ??`n" -ForegroundColor Green -BackgroundColor Black
} else {
    Write-Host "`n? BUILD FAILED" -ForegroundColor Red
    Write-Host "First 10 errors:" -ForegroundColor Yellow
    $buildResult | Select-String "error CS" | Select-Object -First 10
}
