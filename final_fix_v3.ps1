# Direct line-number insertion

$file = "modules\EasyAF.Modules.Project\ViewModels\ProjectSummaryViewModel.cs"
$lines = Get-Content $file

# 1. Add using statements at top
$insertIndex = ($lines | Select-String -Pattern "using EasyAF\.Modules\.Project\.Models;" | Select-Object -First 1).LineNumber
$lines = $lines[0..($insertIndex-1)] + "using EasyAF.Modules.Project.Views;" + "using EasyAF.Modules.Project.Helpers;" + $lines[$insertIndex..($lines.Length-1)]

# 2. Make class partial
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -match 'public class ProjectSummaryViewModel') {
        $lines[$i] = $lines[$i] -replace 'public class', 'public partial class'
        break
    }
}

# 3. Find the SECOND occurrence of "Step 2.5: Smart conflict" (in ExecuteDropImport)
$matches = $lines | Select-String -Pattern "Step 2\.5: Smart conflict"
if ($matches.Count -ge 2) {
    $targetLine = $matches[1].LineNumber  # Second match (index 1)
    
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
    
    # Insert before line $targetLine (array is 0-based, line numbers are 1-based)
    $lines = $lines[0..($targetLine-2)] + $compositeBlock + $lines[($targetLine-1)..($lines.Length-1)]
    Write-Host "? Inserted Composite mode check before line $targetLine"
} else {
    Write-Host "? ERROR: Could not find second occurrence"
    exit 1
}

Set-Content $file $lines
Write-Host "? ALL DONE!"
