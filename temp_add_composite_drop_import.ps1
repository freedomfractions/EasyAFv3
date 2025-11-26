# PowerShell script to add ExecuteCompositeImport and update ExecuteDropImport methods

$filePath = "modules\EasyAF.Modules.Project\ViewModels\ProjectSummaryViewModel.cs"
$content = Get-Content $filePath -Raw

# First, add the using statements if not present
if ($content -notmatch 'using EasyAF\.Modules\.Project\.Views;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Views;"
}

if ($content -notmatch 'using EasyAF\.Modules\.Project\.Helpers;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Helpers;"
}

# Define the ExecuteCompositeImport method
$executeCompositeImportMethod = @'

        /// <summary>
        /// Executes composite import based on user's scenario selections from the Composite Import Dialog.
        /// </summary>
        private void ExecuteCompositeImport(
            System.Collections.Generic.List<ScenarioImportPlan> importPlan,
            EasyAF.Import.MappingConfig mappingConfig,
            bool isNewData)
        {
            try
            {
                // Ensure target dataset exists
                var targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                if (targetDataSet == null)
                {
                    targetDataSet = new DataSet();
                    if (isNewData)
                        _document.Project.NewData = targetDataSet;
                    else
                        _document.Project.OldData = targetDataSet;
                }

                var importManager = new EasyAF.Import.ImportManager();
                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                // Group by file to avoid importing the same file multiple times
                var plansByFile = importPlan.GroupBy(p => p.FilePath);

                foreach (var fileGroup in plansByFile)
                {
                    var filePath = fileGroup.Key;
                    var fileName = System.IO.Path.GetFileName(filePath);

                    try
                    {
                        // Import file into temporary dataset
                        var tempDataSet = new DataSet();
                        importManager.Import(filePath, mappingConfig, tempDataSet);

                        // Process each scenario import plan for this file
                        foreach (var plan in fileGroup)
                        {
                            if (plan.OriginalScenario == null)
                            {
                                // Non-scenario file - merge all data directly
                                CompositeImportHelper.MergeDataSets(tempDataSet, targetDataSet);
                                Log.Information("Imported non-scenario file: {File}", fileName);
                            }
                            else
                            {
                                // Scenario-based file - filter and optionally rename scenario
                                var scenarioData = CompositeImportHelper.FilterDataSetByScenario(tempDataSet, plan.OriginalScenario);
                                
                                if (plan.TargetScenario != null && plan.TargetScenario != plan.OriginalScenario)
                                {
                                    // Rename scenario before merging
                                    CompositeImportHelper.RenameScenarioInDataSet(scenarioData, plan.OriginalScenario, plan.TargetScenario);
                                    Log.Information("Imported scenario '{Original}' from {File} as '{Target}'", 
                                        plan.OriginalScenario, fileName, plan.TargetScenario);
                                }
                                else
                                {
                                    Log.Information("Imported scenario '{Scenario}' from {File}", 
                                        plan.OriginalScenario, fileName);
                                }

                                // Merge scenario data into target
                                CompositeImportHelper.MergeDataSets(scenarioData, targetDataSet);
                            }
                        }

                        successCount++;
                    }
                    catch (Exception fileEx)
                    {
                        errors.Add($"{fileName}: {fileEx.Message}");
                        Log.Error(fileEx, "Error importing file: {File}", filePath);
                    }
                }

                // Report results
                _document.MarkDirty();
                RefreshStatistics(triggerHighlights: true);

                // Update command states
                (ClearNewDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (ClearOldDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();

                // Log scenario discovery
                LogScenarioDiscovery(targetDataSet, isNewData);

                if (errors.Count == 0)
                {
                    Log.Information("Composite import completed successfully: {Count} file(s) processed", successCount);
                }
                else
                {
                    var errorSummary = string.Join("\n", errors);
                    _dialogService.ShowWarning(
                        $"Composite import completed with errors:\n\n{errorSummary}",
                        "Partial Import");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during composite import execution");
                _dialogService.ShowError($"Composite import failed: {ex.Message}", "Import Failed");
            }
        }
'@

# Find the location to insert (after ExecuteDropImport method, before #endregion)
$pattern = '(\s+Log\.Information\("Drop import completed: \{Success\} of \{Total\} files imported successfully", successCount, filePaths\.Length\);\s+\}\s+catch.*?\}\s+\})\s+(#endregion)'

if ($content -match $pattern) {
    $content = $content -replace $pattern, "`$1`n$executeCompositeImportMethod`n`n        `$2"
    Write-Host "Added ExecuteCompositeImport method"
} else {
    Write-Host "ERROR: Could not find insertion point for ExecuteCompositeImport"
    exit 1
}

# Now update ExecuteDropImport to add Composite mode support
$dropImportCompositeSection = @'
                // Step 2.5: COMPOSITE MODE - Show scenario selection dialog
                if (ProjectType == ProjectType.Composite)
                {
                    // Pre-scan files to extract scenarios
                    var fileScenarios = CompositeImportHelper.PreScanFilesForScenarios(filePaths, mappingConfig);

                    // Get existing scenarios from target dataset
                    var existingDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                    var existingScenarios = existingDataSet?.GetAvailableScenarios().ToList() ?? new System.Collections.Generic.List<string>();

                    // Show composite import dialog
                    var compositeDialog = new CompositeImportDialog(fileScenarios, existingScenarios)
                    {
                        Owner = System.Windows.Application.Current.MainWindow
                    };

                    if (compositeDialog.ShowDialog() != true)
                    {
                        Log.Information("User cancelled composite drop import");
                        return;
                    }

                    // Get import plan from dialog
                    var importPlan = compositeDialog.ViewModel.GetImportPlan();

                    // Execute selective import based on plan
                    ExecuteCompositeImport(importPlan, mappingConfig, isNewData);
                    return;
                }

                // STANDARD MODE - Continue with existing logic
'@

# Replace the comment in ExecuteDropImport
$content = $content -replace '(\s+Log\.Warning\("Mapping validation warnings: \{Warnings\}", warnings\);\s+\}\s+)(\s+// Step 2\.5: Smart conflict detection)', "`$1`n$dropImportCompositeSection`$2"

# Write the modified content back
Set-Content -Path $filePath -Value $content -NoNewline

Write-Host "Successfully updated ProjectSummaryViewModel.cs with Composite Import support"
