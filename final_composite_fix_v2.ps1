# Precise line-based insertion for Composite Import support

$file = "modules\EasyAF.Modules.Project\ViewModels\ProjectSummaryViewModel.cs"
$lines = Get-Content $file

# 1. Add using statements at top
$insertIndex = ($lines | Select-String -Pattern "using EasyAF\.Modules\.Project\.Models;" | Select-Object -First 1).LineNumber
$lines = $lines[0..($insertIndex-1)] + "using EasyAF.Modules.Project.Views;" + "using EasyAF.Modules.Project.Helpers;" + $lines[$insertIndex..($lines.Length-1)]

Write-Host "? Added using statements"

# 2. Make class partial (find "public class ProjectSummaryViewModel")
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -match 'public class ProjectSummaryViewModel') {
        $lines[$i] = $lines[$i] -replace 'public class', 'public partial class'
        Write-Host "? Made class partial"
        break
    }
}

# 3. Insert Composite check at exact line in ExecuteDropImport
# Find line with "// Step 2.5: Smart conflict detection" in ExecuteDropImport context (with filePaths)
$targetLine = ($lines | Select-String -Pattern "// Step 2\.5: Smart conflict detection.*filePaths" | Where-Object {$_.Line -match "filePaths"} | Select-Object -First 1).LineNumber

if ($targetLine) {
    $compositeBlock = @(
        "",
        "                // Step 2.5: COMPOSITE MODE - Show scenario selection dialog",
        "                if (ProjectType == ProjectType.Composite)",
        "                {",
        "                    var fileScenarios = CompositeImportHelper.PreScanFilesForScenarios(filePaths, mappingConfig);",
        "                    var existingDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;",
        "                    var existingScenarios = existingDataSet?.GetAvailableScenarios().ToList() ?? new System.Collections.Generic.List<string>();",
        "                    ",
        "                    var compositeDialog = new CompositeImportDialog(fileScenarios, existingScenarios)",
        "                    {",
        "                        Owner = System.Windows.Application.Current.MainWindow",
        "                    };",
        "",
        "                    if (compositeDialog.ShowDialog() != true)",
        "                    {",
        "                        Log.Information(`"User cancelled composite drop import`");",
        "                        return;",
        "                    }",
        "",
        "                    var importPlan = compositeDialog.ViewModel.GetImportPlan();",
        "                    ExecuteCompositeImport(importPlan, mappingConfig, isNewData);",
        "                    return;",
        "                }",
        "",
        "                // STANDARD MODE - Continue with existing logic"
    )
    
    # Insert before the "// Step 2.5: Smart conflict detection" line
    $lines = $lines[0..($targetLine-2)] + $compositeBlock + $lines[($targetLine-1)..($lines.Length-1)]
    Write-Host "? Inserted Composite mode check at line $targetLine"
} else {
    Write-Host "? ERROR: Could not find insertion point"
    exit 1
}

# Write back
Set-Content $file $lines
Write-Host "`n? ALL DONE! Composite Import Dialog support added to ExecuteDropImport"
