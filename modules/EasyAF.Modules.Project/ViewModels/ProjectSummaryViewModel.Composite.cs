using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Data.Models;
using EasyAF.Modules.Project.Views;
using EasyAF.Modules.Project.Helpers;
using Prism.Commands;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// Partial class containing Composite Import functionality.
    /// </summary>
    public partial class ProjectSummaryViewModel
    {
        /// <summary>
        /// Executes composite import based on user's scenario selections from the Composite Import Dialog.
        /// </summary>
        private void ExecuteCompositeImport(
            List<ScenarioImportPlan> importPlan,
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
                var errors = new List<string>();

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

                // Record composite import in history (AFTER successful import)
                if (successCount > 0)
                {
                    // Build scenario mappings from import plan
                    var scenarioMappings = BuildScenarioMappingsFromPlan(importPlan);
                    
                    // Get unique file paths
                    var filePaths = importPlan.Select(p => p.FilePath).Distinct().ToArray();
                    
                    RecordImportInHistory(filePaths, isNewData, mappingConfig, scenarioMappings);
                }

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
    }
}
