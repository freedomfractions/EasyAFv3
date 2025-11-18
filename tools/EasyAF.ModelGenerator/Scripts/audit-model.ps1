# EasyAF Model Audit Helper
# Assists in auditing individual generated models against CSV source

param(
    [Parameter(Mandatory=$true)]
    [string]$ModelName,
    
    [string]$CsvPath = "easypower fields.csv"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$csvFullPath = Join-Path $repoRoot $CsvPath
$generatedPath = Join-Path $repoRoot "lib\EasyAF.Data\Models\Generated\$ModelName.cs"

# CSV row index mapping (add more as needed)
$csvIndexMap = @{
    "ArcFlash" = 0
    "ShortCircuit" = 1
    "Bus" = 2
    "Panel" = 3
    "MCC" = 4
    "Cable" = 7
    "Busway" = 8
    "TransmissionLine" = 9
    "CLReactor" = 10
    "Transformer" = 11
    "Transformer3W" = 12
    "ZigzagTransformer" = 13
    "LVCB" = 14
    "HVBreaker" = 15
    "Relay" = 16
    "CT" = 17
    "Fuse" = 18
    "Switch" = 19
    "ATS" = 20
    "Load" = 22
    "Shunt" = 23
    "Capacitor" = 24
    "Filter" = 25
    "AFD" = 26
    "UPS" = 27
    "Inverter" = 28
    "Rectifier" = 29
    "Photovoltaic" = 30
    "Battery" = 31
    "Meter" = 32
    "POC" = 33
    "Motor" = 5
    "Generator" = 6
    "Utility" = 21
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Model Audit Helper: $ModelName" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Validate inputs
if (-not (Test-Path $csvFullPath)) {
    Write-Host "ERROR: CSV file not found: $csvFullPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $generatedPath)) {
    Write-Host "ERROR: Generated model not found: $generatedPath" -ForegroundColor Red
    exit 1
}

if (-not $csvIndexMap.ContainsKey($ModelName)) {
    Write-Host "ERROR: Unknown model '$ModelName'. Add to csvIndexMap." -ForegroundColor Red
    exit 1
}

# Get CSV columns
$csvIndex = $csvIndexMap[$ModelName]
$lines = Get-Content $csvFullPath
$csvLine = $lines[$csvIndex]
$csvCols = ($csvLine -split ',')[1..200] | Where-Object { $_ -ne '' } | ForEach-Object { $_.Trim() }

Write-Host "[CSV] Found $($csvCols.Count) columns in source" -ForegroundColor Green
Write-Host "`nCSV Columns:" -ForegroundColor Yellow
for ($i = 0; $i -lt $csvCols.Count; $i++) {
    Write-Host "  $(($i+1).ToString().PadLeft(3)). $($csvCols[$i])" -ForegroundColor White
}

# Get generated properties
Write-Host "`n----------------------------------------`n" -ForegroundColor Cyan
$content = Get-Content $generatedPath
$properties = $content | Select-String "public string\?" | ForEach-Object {
    if ($_ -match 'public string\? (\w+) \{') {
        $matches[1]
    }
} | Where-Object { $_ -ne "Id" }  # Exclude Id alias

Write-Host "[GENERATED] Found $($properties.Count) properties in model" -ForegroundColor Green
Write-Host "`nGenerated Properties:" -ForegroundColor Yellow
for ($i = 0; $i -lt $properties.Count; $i++) {
    Write-Host "  $(($i+1).ToString().PadLeft(3)). $($properties[$i])" -ForegroundColor White
}

# Compare counts
Write-Host "`n----------------------------------------`n" -ForegroundColor Cyan
Write-Host "COMPARISON:" -ForegroundColor Yellow

if ($csvCols.Count -eq $properties.Count) {
    Write-Host "  ? Property count MATCHES ($($csvCols.Count) == $($properties.Count))" -ForegroundColor Green
}
else {
    Write-Host "  ? Property count MISMATCH!" -ForegroundColor Red
    Write-Host "    CSV: $($csvCols.Count) columns" -ForegroundColor Red
    Write-Host "    Generated: $($properties.Count) properties" -ForegroundColor Red
    
    if ($csvCols.Count -gt $properties.Count) {
        Write-Host "    Missing $($csvCols.Count - $properties.Count) properties!" -ForegroundColor Red
    }
    else {
        Write-Host "    Extra $($properties.Count - $csvCols.Count) properties!" -ForegroundColor Red
    }
}

# Check for duplicates
$duplicates = $properties | Group-Object | Where-Object { $_.Count -gt 1 }
if ($duplicates) {
    Write-Host "`n  ? DUPLICATE PROPERTIES FOUND:" -ForegroundColor Red
    foreach ($dup in $duplicates) {
        Write-Host "    '$($dup.Name)' appears $($dup.Count) times" -ForegroundColor Red
    }
}
else {
    Write-Host "`n  ? No duplicate properties" -ForegroundColor Green
}

# Check for common naming issues
Write-Host "`nCOMMON ISSUES CHECK:" -ForegroundColor Yellow

$issues = @()

# Check for lowercase kV/kA
$lowercaseKV = $properties | Where-Object { $_ -match 'kV' }
if ($lowercaseKV) {
    $issues += "Lowercase kV found (should be KV): $($lowercaseKV -join ', ')"
}

$lowercaseKA = $properties | Where-Object { $_ -match 'kA' }
if ($lowercaseKA) {
    $issues += "Lowercase kA found (should be KA): $($lowercaseKA -join ', ')"
}

# Check for lowercase pu
$lowercasePu = $properties | Where-Object { $_ -match 'pu$' -or $_ -match 'pu[A-Z]' }
if ($lowercasePu) {
    $issues += "Lowercase pu found (should be Pu): $($lowercasePu -join ', ')"
}

# Check for "of" not capitalized
$lowercaseOf = $properties | Where-Object { $_ -match 'Noof|[a-z]of[A-Z]' }
if ($lowercaseOf) {
    $issues += "Lowercase 'of' found (should be 'Of'): $($lowercaseOf -join ', ')"
}

# Check for ACDC
$badAcDc = $properties | Where-Object { $_ -eq 'ACDC' }
if ($badAcDc) {
    $issues += "ACDC found (should be AcDc)"
}

# Check for numeric prefixes
$numericPrefix = $properties | Where-Object { $_ -match '^\d' }
if ($numericPrefix) {
    $issues += "Properties starting with numbers (invalid in C#): $($numericPrefix -join ', ')"
}

if ($issues.Count -gt 0) {
    Write-Host "  ? Found $($issues.Count) potential issues:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "    - $issue" -ForegroundColor Red
    }
}
else {
    Write-Host "  ? No common naming issues detected" -ForegroundColor Green
}

# Suggest fixes for known patterns
Write-Host "`nQUICK FIX SUGGESTIONS:" -ForegroundColor Yellow
Write-Host @"
# Common PowerShell fix patterns:

`$content = Get-Content "$generatedPath" -Raw

# Fix AC/DC
`$content = `$content -replace 'public string\? ACDC \{', 'public string? AcDc {'

# Fix No of Phases
`$content = `$content -replace 'public string\? Noofphases \{', 'public string? NoOfPhases {'

# Fix kV capitalization
`$content = `$content -replace 'BasekV', 'BaseKV'
`$content = `$content -replace 'FromBasekV', 'FromBaseKV'
`$content = `$content -replace 'ToBasekV', 'ToBaseKV'

# Fix kA capitalization (use full property names to avoid partial matches)
`$content = `$content -replace 'SCIntkA', 'SCIntKA'
`$content = `$content -replace 'TCCMomkA', 'TCCMomKA'

# Fix pu capitalization
`$content = `$content -replace 'R1pu', 'R1Pu'
`$content = `$content -replace 'MVARpu', 'MVARPu'

# Save changes
Set-Content "$generatedPath" `$content
"@

Write-Host "`n========================================`n" -ForegroundColor Cyan
