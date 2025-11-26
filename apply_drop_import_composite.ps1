# CORRECTED script - only modify ExecuteDropImport, NOT ExecuteImport

$file = "modules\EasyAF.Modules.Project\ViewModels\ProjectSummaryViewModel.cs"
$content = Get-Content $file -Raw

# 1. Add using statements
if ($content -notmatch 'using EasyAF\.Modules\.Project\.Views;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Views;"
}

if ($content -notmatch 'using EasyAF\.Modules\.Project\.Helpers;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Helpers;"
}

# 2. Make class partial
$content = $content -replace 'public class ProjectSummaryViewModel', 'public partial class ProjectSummaryViewModel'

# 3. Insert Composite check in ExecuteDropImport ONLY
# Use unique marker that appears ONLY in ExecuteDropImport (not ExecuteImport)
$dropImportMarker = 'public void ExecuteDropImport\(string\[\] filePaths, bool isNewData\)'

# Find ExecuteDropImport and insert Composite check after mapping validation
$composite = @'
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

# Match ONLY in ExecuteDropImport - look for the DROP import context
$pattern = '(public void ExecuteDropImport.*?Log\.Warning\("Mapping validation warnings: \{Warnings\}", warnings\);\s+\}\s+)\s+(// Step 2\.5: Smart conflict detection.*?filePaths)'

if ($content -match $pattern) {
    $content = $content -replace $pattern, "`$1`n$composite`$2"
    Write-Host "? Added Composite support to ExecuteDropImport"
} else {
    Write-Host "? ERROR: Could not find ExecuteDropImport insertion point"
    exit 1
}

Set-Content $file $content -NoNewline
Write-Host "? All changes applied!"
