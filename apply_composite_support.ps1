# Minimal surgical edit to add Composite Import support

$file = "modules\EasyAF.Modules.Project\ViewModels\ProjectSummaryViewModel.cs"
$content = Get-Content $file -Raw

# 1. Add using statements ONLY if not present
if ($content -notmatch 'using EasyAF\.Modules\.Project\.Views;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Views;"
    Write-Host "Added Views using"
}

if ($content -notmatch 'using EasyAF\.Modules\.Project\.Helpers;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Helpers;"
    Write-Host "Added Helpers using"
}

# 2. Make class partial
$content = $content -replace 'public class ProjectSummaryViewModel', 'public partial class ProjectSummaryViewModel'

# 3. Insert Composite check in ExecuteDropImport - find exact location after validation warnings
$insertPoint = 'Log\.Warning\("Mapping validation warnings: \{Warnings\}", warnings\);\s+\}\s+'
$compositeCheck = @'
                // Step 2.5: COMPOSITE MODE - Show scenario selection dialog
                if (ProjectType == ProjectType.Composite)
                {
                    var fileScenarios = CompositeImportHelper.PreScanFilesForScenarios(filePaths, mappingConfig);
                    var existingDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                    var existingScenarios = existingDataSet?.GetAvailableScenarios().ToList() ?? new System.Collections.Generic.List<string>();
                    
                    var compositeDialog = new CompositeImportDialog(fileScenarios, existingScenarios)
                    {
                        Owner = System.Windows.Application.Current.MainWindow
                    };

                    if (compositeDialog.ShowDialog() != true)
                    {
                        Log.Information("User cancelled composite drop import");
                        return;
                    }

                    var importPlan = compositeDialog.ViewModel.GetImportPlan();
                    ExecuteCompositeImport(importPlan, mappingConfig, isNewData);
                    return;
                }

                // STANDARD MODE - Continue with existing logic
'@

# Find and replace in ExecuteDropImport only
if ($content -match $insertPoint) {
    $content = $content -replace "($insertPoint)\s+(// Step 2\.5: Smart conflict detection)", "`$1`n$compositeCheck`n`n                `$2"
    Write-Host "Added Composite mode check to ExecuteDropImport"
} else {
    Write-Host "ERROR: Could not find insertion point"
    exit 1
}

Set-Content $file $content -NoNewline
Write-Host "? Done!"
