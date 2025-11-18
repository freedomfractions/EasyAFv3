# PowerShell Script to Audit ALL EasyPower Models vs CSV

$csvFile = "C:\src\EasyAFv3\easypower fields.csv"
$lines = Get-Content $csvFile

# Parse CSV into hashtable of class -> columns
$classData = @{}
foreach ($line in $lines) {
    $cols = $line -split ','
    $className = $cols[0].Trim()
    if ($className -and $className -ne '') {
        $columns = $cols[1..($cols.Length-1)] | Where-Object { $_ -and $_ -ne '' } | ForEach-Object { $_.Trim() }
        $classData[$className] = $columns
    }
}

Write-Host "=== COMPLETE EASYPOWER MODEL AUDIT ===" -ForegroundColor Cyan
Write-Host "Parsed $($classData.Count) classes from CSV`n" -ForegroundColor Green

# Map of CSV class names to C# file names
$classMap = @{
    "Arc Flash Scenario Report" = "ArcFlash.cs"
    "Equipment Duty Scenario Report" = "ShortCircuit.cs"
    "Buses" = "Bus.cs"
    "LV Breakers" = "LVCB.cs"
    "Fuses" = "Fuse.cs"
    "Cables" = "Cable.cs"
    "2W Transformers" = "Transformer.cs"
    "Motors" = "Motor.cs"
    "Generators" = "Generator.cs"
    "Utilities" = "Utility.cs"
    "Capacitors" = "Capacitor.cs"
    "Loads" = "Load.cs"
}

$report = @()

foreach ($csvClass in $classData.Keys | Sort-Object) {
    $csvColumns = $classData[$csvClass]
    $modelFile = $classMap[$csvClass]
    
    $status = if ($modelFile) {
        $filePath = "lib/EasyAF.Data/Models/$modelFile"
        if (Test-Path $filePath) {
            $fileContent = Get-Content $filePath -Raw
            $codeColumns = [regex]::Matches($fileContent, 'Column: ([^)]+)\)') | ForEach-Object { $_.Groups[1].Value }
            
            $missing = $csvColumns | Where-Object { $_ -notin $codeColumns }
            $extra = $codeColumns | Where-Object { $_ -notin $csvColumns }
            
            if ($missing.Count -eq 0 -and $extra.Count -eq 0) {
                "? PERFECT"
            } else {
                "? MISMATCH (Missing: $($missing.Count), Extra: $($extra.Count))"
            }
        } else {
            "??  FILE NOT FOUND"
        }
    } else {
        "? NOT IMPLEMENTED"
    }
    
    $report += [PSCustomObject]@{
        CSVClass = $csvClass
        Columns = $csvColumns.Count
        ModelFile = if ($modelFile) { $modelFile } else { "N/A" }
        Status = $status
    }
}

$report | Format-Table -AutoSize
Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Perfect: $(($report | Where-Object { $_.Status -like '*PERFECT*' }).Count)" -ForegroundColor Green
Write-Host "Mismatches: $(($report | Where-Object { $_.Status -like '*MISMATCH*' }).Count)" -ForegroundColor Red
Write-Host "Not Implemented: $(($report | Where-Object { $_.Status -like '*NOT IMPLEMENTED*' }).Count)" -ForegroundColor Yellow
