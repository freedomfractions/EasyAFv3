# PowerShell script to add Composite Import support to ExecuteDropImport

$filePath = "modules\EasyAF.Modules.Project\ViewModels\ProjectSummaryViewModel.cs"
$content = Get-Content $filePath -Raw

# 1. Add using statements if not present
if ($content -notmatch 'using EasyAF\.Modules\.Project\.Views;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Views;"
    Write-Host "Added 'using EasyAF.Modules.Project.Views;'"
}

if ($content -notmatch 'using EasyAF\.Modules\.Project\.Helpers;') {
    $content = $content -replace '(using EasyAF\.Modules\.Project\.Models;)', "`$1`nusing EasyAF.Modules.Project.Helpers;"
    Write-Host "Added 'using EasyAF.Modules.Project.Helpers;'"
}

# 2. Insert Composite mode check in ExecuteDropImport (after mapping validation, before conflict detection)
$compositeSection = @'
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

# Find the location in ExecuteDropImport right after the mapping warnings
$pattern = '(public void ExecuteDropImport.*?Log\.Warning\("Mapping validation warnings: \{Warnings\}", warnings\);\s+\}\s+)\s+(// Step 2\.5: Smart conflict detection)'

if ($content -match $pattern) {
    $content = $content -replace $pattern, "`$1`n$compositeSection`$2"
    Write-Host "Added Composite mode support to ExecuteDropImport"
} else {
    Write-Host "ERROR: Could not find insertion point in ExecuteDropImport"
    Write-Host "Pattern not found. Checking if already added..."
    
    if ($content -match 'CompositeImportHelper\.PreScanFilesForScenarios') {
        Write-Host "Composite support already added!"
    } else {
        exit 1
    }
}

# 3. Add ExecuteCompositeImport method before #endregion in Commands section
$executeCompositeMethod = @'

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

# Find location to add ExecuteCompositeImport (after ExecuteDropImport, before #endregion)
$methodPattern = '(public void ExecuteDropImport.*?Log\.Information\("Drop import completed:.*?\}\s+\}\s+)\s+(#endregion)'

if ($content -match $methodPattern) {
    $content = $content -replace $methodPattern, "`$1$executeCompositeMethod`n        `$2"
    Write-Host "Added ExecuteCompositeImport method"
} else {
    Write-Host "Checking if ExecuteCompositeImport already exists..."
    if ($content -match 'private void ExecuteCompositeImport') {
        Write-Host "ExecuteCompositeImport method already exists!"
    } else {
        Write-Host "ERROR: Could not find insertion point for ExecuteCompositeImport"
        exit 1
    }
}

# Write back
Set-Content -Path $filePath -Value $content -NoNewline

Write-Host "`n? Successfully updated ProjectSummaryViewModel.cs with Composite Import Dialog support!"
Write-Host "Changes made:"
Write-Host "  1. Added using statements"
Write-Host "  2. Added Composite mode detection in ExecuteDropImport"
Write-Host "  3. Added ExecuteCompositeImport helper method"
