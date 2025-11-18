# Fix Malformed Properties
# Targets specific broken properties that need manual fixing

$ErrorActionPreference = "Stop"

Write-Host "`nFixing malformed properties..." -ForegroundColor Cyan

# Fix Motor.cs - line with embedded quotes
$motorFile = "lib\EasyAF.Data\Models\Generated\Motor.cs"
$content = Get-Content $motorFile -Raw

# Replace the malformed IEC property
$content = $content -replace '/// <summary>"IEC RMf / X"d " \(Column: "IEC RMf / X"d "\)</summary>', '/// <summary>IEC RMf / Xd (Column: IEC RMf / Xd)</summary>'
$content = $content -replace '\[Description\("IEC RMf / X"d "\)\]', '[Description("IEC RMf / Xd")]'
$content = $content -replace 'public string\? "IECRMfX"d"', 'public string? IECRMfXd'

Set-Content $motorFile $content
Write-Host "? Fixed Motor.cs" -ForegroundColor Green

# Fix Relay.cs - properties starting with numbers
$relayFile = "lib\EasyAF.Data\Models\Generated\Relay.cs"
$content = Get-Content $relayFile -Raw

$content = $content -replace 'public string\? 2XPickup', 'public string? TwoXPickup'
$content = $content -replace 'public string\? 5XPickup', 'public string? FiveXPickup'

Set-Content $relayFile $content
Write-Host "? Fixed Relay.cs" -ForegroundColor Green

# Fix Transformer.cs - property with slash
$transformerFile = "lib\EasyAF.Data\Models\Generated\Transformer.cs"
$content = Get-Content $transformerFile -Raw

$content = $content -replace 'public string\? ([a-zA-Z0-9]+)/([a-zA-Z0-9]+) \{', 'public string? $1$2 {'

Set-Content $transformerFile $content
Write-Host "? Fixed Transformer.cs" -ForegroundColor Green

# Fix Transformer3W.cs - property with + sign
$transformer3WFile = "lib\EasyAF.Data\Models\Generated\Transformer3W.cs"
$content = Get-Content $transformer3WFile -Raw

$content = $content -replace 'Rsn0\+3Rsg', 'Rsn0Plus3Rsg'
$content = $content -replace 'Xsn0\+3Xsg', 'Xsn0Plus3Xsg'

Set-Content $transformer3WFile $content
Write-Host "? Fixed Transformer3W.cs" -ForegroundColor Green

# Fix Utility.cs - property starting with number
$utilityFile = "lib\EasyAF.Data\Models\Generated\Utility.cs"
$content = Get-Content $utilityFile -Raw

$content = $content -replace 'public string\? 1PHSC2', 'public string? OnePHSC2'

Set-Content $utilityFile $content
Write-Host "? Fixed Utility.cs" -ForegroundColor Green

# Fix all pu capitalization issues
$files = Get-ChildItem "lib\EasyAF.Data\Models\Generated\*.cs" | Where-Object { $_.Name -ne "README.md" }
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $original = $content
    
    # Fix lowercase pu to Pu
    $content = $content -replace 'pu \{', 'Pu {'
    
    if ($content -ne $original) {
        Set-Content $file.FullName $content
    }
}
Write-Host "? Fixed pu capitalization in all files" -ForegroundColor Green

Write-Host "`nCopying fixed files to Models folder..." -ForegroundColor Yellow
Copy-Item "lib\EasyAF.Data\Models\Generated\*.cs" -Destination "lib\EasyAF.Data\Models\" -Force -Exclude "README.md"
Write-Host "? Copied all files" -ForegroundColor Green

Write-Host "`nDone!" -ForegroundColor Cyan
