using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasyAF.Data.Models;
using EasyAF.Data.Extensions;
using EasyAF.Modules.Project.Helpers;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// Partial class containing Import History recording and tree building functionality.
    /// </summary>
    public partial class ProjectSummaryViewModel
    {
        #region Import History Tree

        /// <summary>
        /// Gets the tree nodes for import history (all imports, both New and Old Data).
        /// </summary>
        public ObservableCollection<ImportHistoryNodeViewModel> ImportHistoryNodes { get; } = new();

        /// <summary>
        /// Builds the import history tree from the project's import records.
        /// Each top-level node is a FILE, with children for DATA TYPES, and grandchildren for SCENARIOS.
        /// </summary>
        public void BuildImportHistoryTree()
        {
            // Clear existing tree
            ImportHistoryNodes.Clear();

            // Try building from source tracking first (new way)
            bool builtFromSourceTracking = TryBuildFromSourceTracking();
            
            // Fall back to legacy ImportHistory if needed
            if (!builtFromSourceTracking)
            {
                BuildFromLegacyImportHistory();
            }

            Log.Debug("Built import history tree: {Count} file(s)", ImportHistoryNodes.Count);
        }

        /// <summary>
        /// Creates a file node (top level) with data type children and scenario grandchildren.
        /// </summary>
        private ImportHistoryNodeViewModel CreateFileNode(ImportFileRecord record)
        {
            var targetIndicator = record.IsNewData ? "\uE72A New Data" : "\uE72A Old Data";
            
            var fileNode = new ImportHistoryNodeViewModel
            {
                DisplayText = $"{record.FileName} {targetIndicator}",
                Icon = "\uE8A5", // Document icon
                Tooltip = $"File: {record.FilePath}\nImported: {record.ImportedAt:MMM dd, yyyy h:mm tt}\nMapping: {System.IO.Path.GetFileName(record.MappingPath ?? "Unknown")}",
                FilePath = record.FilePath,
                IsExpanded = true // Expand by default to show data types
            };

            // Add data type children
            foreach (var dataType in record.DataTypes.OrderBy(dt => dt))
            {
                var dataTypeNode = CreateDataTypeNode(dataType, record);
                fileNode.Children.Add(dataTypeNode);
            }

            return fileNode;
        }

        /// <summary>
        /// Creates a data type node (second level) with scenario children.
        /// </summary>
        private ImportHistoryNodeViewModel CreateDataTypeNode(string dataType, ImportFileRecord record)
        {
            var dataTypeNode = new ImportHistoryNodeViewModel
            {
                DisplayText = dataType,
                Icon = "\uE8F1", // Database icon
                Tooltip = $"Data Type: {dataType}",
                DataTypes = dataType,
                IsExpanded = true // Expand by default to show scenarios
            };

            // Add scenario children (if any)
            if (record.ScenarioMappings != null && record.ScenarioMappings.Count > 0)
            {
                foreach (var mapping in record.ScenarioMappings.OrderBy(kvp => kvp.Key))
                {
                    var scenarioNode = CreateScenarioNode(mapping.Key, mapping.Value);
                    dataTypeNode.Children.Add(scenarioNode);
                }
            }

            return dataTypeNode;
        }

        /// <summary>
        /// Creates a scenario node (third level) showing original and target scenario names.
        /// </summary>
        private ImportHistoryNodeViewModel CreateScenarioNode(string originalScenario, string targetScenario)
        {
            // Determine display text and tooltip
            string displayText;
            string tooltip;
            
            if (originalScenario == targetScenario)
            {
                // No rename
                displayText = originalScenario;
                tooltip = $"Scenario: {originalScenario}";
            }
            else
            {
                // Renamed - use Segoe MDL2 Assets arrow icon
                displayText = $"{originalScenario} \uE72A {targetScenario}";
                tooltip = $"Scenario renamed:\nOriginal: {originalScenario}\nTarget: {targetScenario}";
            }

            var scenarioNode = new ImportHistoryNodeViewModel
            {
                DisplayText = displayText,
                Icon = "\uE8F4", // Tag icon
                Tooltip = tooltip,
                ScenarioMappings = displayText
            };

            return scenarioNode;
        }

        #endregion

        /// <summary>
        /// Records an import operation in the project's import history.
        /// Updates both ImportHistory (legacy) and DataSetSourceInfo (source of truth).
        /// IMPORTANT: Source tracking is per-entry (last-writer-wins per scenario/type), not per-project.
        /// </summary>
        /// <param name="filePaths">Array of files that were imported.</param>
        /// <param name="isNewData">Whether data was imported into New Data (true) or Old Data (false).</param>
        /// <param name="mappingConfig">The mapping configuration used for import.</param>
        /// <param name="scenarioMappings">Optional scenario renaming mappings (original ? target). If null, discovered from files.</param>
        private void RecordImportInHistory(
            string[] filePaths,
            bool isNewData,
            EasyAF.Import.MappingConfig mappingConfig,
            Dictionary<string, string>? scenarioMappings = null)
        {
            try
            {
                var targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                if (targetDataSet == null)
                    return;

                // Ensure source tracking exists
                if (isNewData)
                {
                    _document.Project.NewDataSources ??= new DataSetSourceInfo();
                }
                else
                {
                    _document.Project.OldDataSources ??= new DataSetSourceInfo();
                }

                var sourceInfo = isNewData ? _document.Project.NewDataSources : _document.Project.OldDataSources;

                // IMPORTANT: We need to track what THIS import contributed, not the entire dataset
                // For each file, we'll scan the file BEFORE merging to know what it contains
                // Then update source tracking with ONLY those entries
                
                // Since import has already happened, we do a best-effort reconstruction:
                // - For non-composite types: File imported this type ? mark as source
                // - For composite types: Extract scenarios from imported file's data and mark as source
                
                // NOTE: This is a simplified approach. For perfect accuracy, we'd need to:
                // 1. Scan file before import (know what it contains)
                // 2. Track during import (know what was added)
                // 3. Update sources after import (mark those entries)
                // For now, we use post-import scanning with best effort

                foreach (var filePath in filePaths)
                {
                    // Scan the actual file to determine what it contained
                    UpdateSourceTrackingFromFile(filePath, mappingConfig, sourceInfo!, targetDataSet, scenarioMappings);
                    
                    Log.Debug("Updated source tracking for: {File} ? {Target}", 
                        System.IO.Path.GetFileName(filePath), 
                        isNewData ? "New Data" : "Old Data");
                }

                // Legacy: Also update ImportHistory for backward compatibility
                UpdateLegacyImportHistory(filePaths, isNewData, mappingConfig, scenarioMappings, targetDataSet);

                // Mark project as dirty (changes need to be saved)
                _document.MarkDirty();

                // Rebuild import history tree from source tracking
                BuildImportHistoryTree();

                Log.Information("Source tracking updated: {Count} file(s) recorded", filePaths.Length);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error recording import in source tracking");
                // Don't fail the import if history recording fails
            }
        }

        /// <summary>
        /// Updates source tracking by scanning what a specific file contributed.
        /// Uses a temporary import to determine file contents, then applies scenario renaming.
        /// </summary>
        private void UpdateSourceTrackingFromFile(
            string filePath,
            EasyAF.Import.MappingConfig mappingConfig,
            DataSetSourceInfo sourceInfo,
            DataSet currentDataSet,
            Dictionary<string, string>? scenarioMappings = null)
        {
            try
            {
                // Import the file into a temporary dataset to see what it contains
                var tempDataSet = new DataSet();
                var importManager = new EasyAF.Import.ImportManager();
                
                importManager.Import(filePath, mappingConfig, tempDataSet);

                // Now scan the temp dataset to see what this file contributed
                foreach (var property in DataSetSourceHelper.GetDataSetEntryProperties())
                {
                    var collection = property.GetValue(tempDataSet);
                    if (collection == null)
                        continue;

                    var countProperty = collection.GetType().GetProperty("Count");
                    if (countProperty == null)
                        continue;

                    var count = (int?)countProperty.GetValue(collection);
                    if (count == null || count == 0)
                        continue;

                    var entryType = DataSetSourceHelper.GetEntryType(property);
                    if (entryType == null)
                        continue;

                    var propertyName = property.Name;

                    if (DataSetSourceHelper.IsCompositeType(entryType))
                    {
                        // Composite type - extract scenarios from THIS file's data
                        var originalScenarios = ExtractScenariosFromCollection(collection);
                        
                        if (!sourceInfo.CompositeDataTypeSources.ContainsKey(propertyName))
                            sourceInfo.CompositeDataTypeSources[propertyName] = new Dictionary<string, ScenarioSource>();

                        // IMPORTANT: Only track scenarios that were ACTUALLY imported (in scenarioMappings)
                        // If scenarioMappings is null, track all scenarios (standard import)
                        // If scenarioMappings exists, only track mapped scenarios (composite import with selection)
                        
                        if (scenarioMappings == null)
                        {
                            // Standard import - track all scenarios found in file
                            foreach (var scenario in originalScenarios)
                            {
                                sourceInfo.CompositeDataTypeSources[propertyName][scenario] = new ScenarioSource
                                {
                                    FilePath = filePath,
                                    OriginalScenario = null, // No renaming in standard import
                                    TargetScenario = scenario
                                };
                                Log.Debug("Source tracking: {PropertyName}[{Scenario}] ? {File}", 
                                    propertyName, scenario, System.IO.Path.GetFileName(filePath));
                            }
                        }
                        else
                        {
                            // Composite import - only track scenarios that user selected for import
                            foreach (var mapping in scenarioMappings)
                            {
                                var originalScenario = mapping.Key;
                                var targetScenario = mapping.Value;
                                
                                // Only track if this scenario was in the file
                                if (originalScenarios.Contains(originalScenario))
                                {
                                    // Track using TARGET scenario name (after renaming) as the key
                                    sourceInfo.CompositeDataTypeSources[propertyName][targetScenario] = new ScenarioSource
                                    {
                                        FilePath = filePath,
                                        OriginalScenario = originalScenario,
                                        TargetScenario = targetScenario
                                    };
                                    
                                    if (originalScenario != targetScenario)
                                    {
                                        Log.Debug("Source tracking (renamed): {PropertyName}[{Original} ? {Target}] ? {File}", 
                                            propertyName, originalScenario, targetScenario, System.IO.Path.GetFileName(filePath));
                                    }
                                    else
                                    {
                                        Log.Debug("Source tracking: {PropertyName}[{Scenario}] ? {File}", 
                                            propertyName, targetScenario, System.IO.Path.GetFileName(filePath));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Non-composite type - last-writer-wins per type
                        sourceInfo.DataTypeSources[propertyName] = filePath;
                        Log.Debug("Source tracking: {PropertyName} ? {File}", 
                            propertyName, System.IO.Path.GetFileName(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not scan file for source tracking: {File}", filePath);
                // Fall back to using current dataset (less accurate but better than nothing)
                DataSetSourceHelper.UpdateSourceTracking(currentDataSet, sourceInfo, filePath);
            }
        }

        /// <summary>
        /// Extracts unique scenario names from a collection of entries.
        /// </summary>
        private HashSet<string> ExtractScenariosFromCollection(object collection)
        {
            var scenarios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var valuesProperty = collection.GetType().GetProperty("Values");
            if (valuesProperty == null)
                return scenarios;

            var values = valuesProperty.GetValue(collection) as System.Collections.IEnumerable;
            if (values == null)
                return scenarios;

            foreach (var entry in values)
            {
                if (entry == null)
                    continue;

                var scenarioProperty = entry.GetType().GetProperty("Scenario");
                if (scenarioProperty == null)
                    continue;

                var scenario = scenarioProperty.GetValue(entry) as string;
                if (!string.IsNullOrWhiteSpace(scenario))
                    scenarios.Add(scenario);
            }

            return scenarios;
        }

        /// <summary>
        /// Updates legacy ImportHistory for backward compatibility.
        /// </summary>
        private void UpdateLegacyImportHistory(
            string[] filePaths,
            bool isNewData,
            EasyAF.Import.MappingConfig mappingConfig,
            Dictionary<string, string>? scenarioMappings,
            DataSet targetDataSet)
        {
            if (_document.Project.ImportHistory == null)
                _document.Project.ImportHistory = new List<ImportFileRecord>();

            var timestamp = DateTimeOffset.Now;
            var mappingPath = MapPath ?? "Unknown";

            foreach (var filePath in filePaths)
            {
                var existingRecord = _document.Project.ImportHistory
                    .FirstOrDefault(r => string.Equals(r.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

                var dataTypes = GetDataTypesFromDataSet(targetDataSet);
                var fileScenarioMappings = scenarioMappings ?? GetScenarioMappingsFromDataSet(targetDataSet);
                var entryCount = CountEntriesInDataSet(targetDataSet);

                if (existingRecord != null)
                {
                    existingRecord.ImportedAt = timestamp;
                    existingRecord.IsNewData = isNewData;
                    existingRecord.MappingPath = mappingPath;
                    existingRecord.EntryCount = entryCount;
                    existingRecord.DataTypes = existingRecord.DataTypes.Union(dataTypes).Distinct().ToList();
                    existingRecord.ScenarioMappings = fileScenarioMappings;
                }
                else
                {
                    var record = new ImportFileRecord
                    {
                        ImportedAt = timestamp,
                        FilePath = filePath,
                        IsNewData = isNewData,
                        MappingPath = mappingPath,
                        DataTypes = dataTypes,
                        ScenarioMappings = fileScenarioMappings,
                        EntryCount = entryCount
                    };
                    _document.Project.ImportHistory.Add(record);
                }
            }
        }

        /// <summary>
        /// Attempts to build tree from DataSetSourceInfo.
        /// Returns true if successful, false if no source info available.
        /// </summary>
        private bool TryBuildFromSourceTracking()
        {
            var newSources = _document.Project.NewDataSources;
            var oldSources = _document.Project.OldDataSources;

            if (newSources == null && oldSources == null)
                return false; // No source tracking data

            bool hasData = false;

            // Build tree from New Data sources
            if (newSources != null)
            {
                hasData |= BuildTreeFromSourceInfo(newSources, isNewData: true);
            }

            // Build tree from Old Data sources
            if (oldSources != null)
            {
                hasData |= BuildTreeFromSourceInfo(oldSources, isNewData: false);
            }

            return hasData;
        }

        /// <summary>
        /// Builds tree nodes from a DataSetSourceInfo object.
        /// </summary>
        private bool BuildTreeFromSourceInfo(DataSetSourceInfo sourceInfo, bool isNewData)
        {
            // Group all sources by file path
            var fileGroups = new Dictionary<string, List<(string propertyName, string? originalScenario, string targetScenario)>>(StringComparer.OrdinalIgnoreCase);

            // Add non-composite types
            foreach (var (propertyName, filePath) in sourceInfo.DataTypeSources)
            {
                if (!fileGroups.ContainsKey(filePath))
                    fileGroups[filePath] = new();
                
                fileGroups[filePath].Add((propertyName, null, propertyName)); // No scenario for non-composite
            }

            // Add composite types (group scenarios by file)
            foreach (var (propertyName, scenarioSources) in sourceInfo.CompositeDataTypeSources)
            {
                foreach (var (targetScenario, scenarioSource) in scenarioSources)
                {
                    var filePath = scenarioSource.FilePath;
                    
                    if (!fileGroups.ContainsKey(filePath))
                        fileGroups[filePath] = new();
                    
                    // Store with original and target scenario info
                    fileGroups[filePath].Add((propertyName, scenarioSource.OriginalScenario, targetScenario));
                }
            }

            // Create file nodes
            foreach (var (filePath, entries) in fileGroups.OrderByDescending(kvp => System.IO.File.Exists(kvp.Key) ? System.IO.File.GetLastWriteTime(kvp.Key) : DateTime.MinValue))
            {
                var fileNode = CreateFileNodeFromSourceInfo(filePath, entries, isNewData);
                ImportHistoryNodes.Add(fileNode);
            }

            return fileGroups.Count > 0;
        }

        /// <summary>
        /// Creates a file node from source tracking information.
        /// </summary>
        private ImportHistoryNodeViewModel CreateFileNodeFromSourceInfo(
            string filePath,
            List<(string propertyName, string? originalScenario, string targetScenario)> entries,
            bool isNewData)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            var targetIndicator = isNewData ? "\uE72A New Data" : "\uE72A Old Data";
            
            // Try to get last modified time
            DateTime? lastModified = System.IO.File.Exists(filePath) 
                ? System.IO.File.GetLastWriteTime(filePath) 
                : (DateTime?)null;

            var fileNode = new ImportHistoryNodeViewModel
            {
                DisplayText = $"{fileName} {targetIndicator}",
                Icon = "\uE8A5", // Document icon
                Tooltip = lastModified.HasValue
                    ? $"File: {filePath}\nLast Modified: {lastModified.Value:MMM dd, yyyy h:mm tt}"
                    : $"File: {filePath}",
                FilePath = filePath,
                IsExpanded = true
            };

            // Group by data type
            var dataTypeGroups = entries
                .GroupBy(x => x.propertyName)
                .OrderBy(g => g.Key);

            foreach (var group in dataTypeGroups)
            {
                var friendlyName = DataSetSourceHelper.GetFriendlyDataTypeName(group.Key);
                var dataTypeNode = new ImportHistoryNodeViewModel
                {
                    DisplayText = friendlyName,
                    Icon = "\uE8F1", // Database icon
                    Tooltip = $"Data Type: {friendlyName}",
                    DataTypes = friendlyName,
                    IsExpanded = true
                };

                // Add scenario children if any
                var scenarioEntries = group.Where(x => x.originalScenario != null || x.targetScenario != x.propertyName);
                foreach (var entry in scenarioEntries.OrderBy(x => x.targetScenario))
                {
                    // Create scenario node with rename arrow if applicable
                    string displayText;
                    string tooltip;
                    
                    if (entry.originalScenario != null && entry.originalScenario != entry.targetScenario)
                    {
                        // Renamed scenario - use Segoe MDL2 Assets arrow icon
                        displayText = $"{entry.originalScenario} \uE72A {entry.targetScenario}";
                        tooltip = $"Scenario renamed:\nOriginal: {entry.originalScenario}\nTarget: {entry.targetScenario}";
                    }
                    else
                    {
                        // Not renamed (or standard import)
                        displayText = entry.targetScenario;
                        tooltip = $"Scenario: {entry.targetScenario}";
                    }
                    
                    var scenarioNode = new ImportHistoryNodeViewModel
                    {
                        DisplayText = displayText,
                        Icon = "\uE8F4", // Tag icon
                        Tooltip = tooltip,
                        ScenarioMappings = displayText
                    };
                    dataTypeNode.Children.Add(scenarioNode);
                }

                fileNode.Children.Add(dataTypeNode);
            }

            return fileNode;
        }

        /// <summary>
        /// Builds tree from legacy ImportHistory (fallback).
        /// </summary>
        private void BuildFromLegacyImportHistory()
        {
            if (_document.Project.ImportHistory == null || _document.Project.ImportHistory.Count == 0)
            {
                Log.Debug("No import history to display");
                return;
            }

            foreach (var record in _document.Project.ImportHistory.OrderByDescending(r => r.ImportedAt))
            {
                var fileNode = CreateFileNode(record);
                ImportHistoryNodes.Add(fileNode);
            }
        }
        
        /// <summary>
        /// Extracts data type names from a DataSet.
        /// </summary>
        private List<string> GetDataTypesFromDataSet(DataSet ds)
        {
            var types = new List<string>();

            if ((ds.ArcFlashEntries?.Count ?? 0) > 0) types.Add("Arc Flash");
            if ((ds.ShortCircuitEntries?.Count ?? 0) > 0) types.Add("Short Circuit");
            if ((ds.BusEntries?.Count ?? 0) > 0) types.Add("Bus");
            if ((ds.LVBreakerEntries?.Count ?? 0) > 0) types.Add("LV Breaker");
            if ((ds.FuseEntries?.Count ?? 0) > 0) types.Add("Fuse");
            if ((ds.CableEntries?.Count ?? 0) > 0) types.Add("Cable");
            if ((ds.AFDEntries?.Count ?? 0) > 0) types.Add("AFD");
            if ((ds.ATSEntries?.Count ?? 0) > 0) types.Add("ATS");
            if ((ds.BatteryEntries?.Count ?? 0) > 0) types.Add("Battery");
            if ((ds.BuswayEntries?.Count ?? 0) > 0) types.Add("Busway");
            if ((ds.CapacitorEntries?.Count ?? 0) > 0) types.Add("Capacitor");
            if ((ds.CLReactorEntries?.Count ?? 0) > 0) types.Add("CL Reactor");
            if ((ds.CTEntries?.Count ?? 0) > 0) types.Add("CT");
            if ((ds.FilterEntries?.Count ?? 0) > 0) types.Add("Filter");
            if ((ds.GeneratorEntries?.Count ?? 0) > 0) types.Add("Generator");
            if ((ds.HVBreakerEntries?.Count ?? 0) > 0) types.Add("HV Breaker");
            if ((ds.InverterEntries?.Count ?? 0) > 0) types.Add("Inverter");
            if ((ds.LoadEntries?.Count ?? 0) > 0) types.Add("Load");
            if ((ds.MCCEntries?.Count ?? 0) > 0) types.Add("MCC");
            if ((ds.MeterEntries?.Count ?? 0) > 0) types.Add("Meter");
            if ((ds.MotorEntries?.Count ?? 0) > 0) types.Add("Motor");
            if ((ds.PanelEntries?.Count ?? 0) > 0) types.Add("Panel");
            if ((ds.PhotovoltaicEntries?.Count ?? 0) > 0) types.Add("Photovoltaic");
            if ((ds.POCEntries?.Count ?? 0) > 0) types.Add("POC");
            if ((ds.RectifierEntries?.Count ?? 0) > 0) types.Add("Rectifier");
            if ((ds.RelayEntries?.Count ?? 0) > 0) types.Add("Relay");
            if ((ds.ShuntEntries?.Count ?? 0) > 0) types.Add("Shunt");
            if ((ds.SwitchEntries?.Count ?? 0) > 0) types.Add("Switch");
            if ((ds.Transformer2WEntries?.Count ?? 0) > 0) types.Add("2-Winding Transformer");
            if ((ds.Transformer3WEntries?.Count ?? 0) > 0) types.Add("3-Winding Transformer");
            if ((ds.TransmissionLineEntries?.Count ?? 0) > 0) types.Add("Transmission Line");
            if ((ds.UPSEntries?.Count ?? 0) > 0) types.Add("UPS");
            if ((ds.UtilityEntries?.Count ?? 0) > 0) types.Add("Utility");
            if ((ds.ZigzagTransformerEntries?.Count ?? 0) > 0) types.Add("Zigzag Transformer");

            return types;
        }

        /// <summary>
        /// Discovers scenario mappings from a DataSet (default: no renaming).
        /// </summary>
        private Dictionary<string, string> GetScenarioMappingsFromDataSet(DataSet ds)
        {
            var mappings = new Dictionary<string, string>();

            // Get scenarios from dataset
            var scenarios = ds.GetAvailableScenarios();

            foreach (var scenario in scenarios)
            {
                mappings[scenario] = scenario; // Original = Target (no rename)
            }

            return mappings;
        }

        /// <summary>
        /// Counts total entries across all data types in a DataSet.
        /// </summary>
        private int CountEntriesInDataSet(DataSet ds)
        {
            int count = 0;

            if (ds.ArcFlashEntries != null) count += ds.ArcFlashEntries.Count;
            if (ds.ShortCircuitEntries != null) count += ds.ShortCircuitEntries.Count;
            if (ds.BusEntries != null) count += ds.BusEntries.Count;
            if (ds.LVBreakerEntries != null) count += ds.LVBreakerEntries.Count;
            if (ds.FuseEntries != null) count += ds.FuseEntries.Count;
            if (ds.CableEntries != null) count += ds.CableEntries.Count;
            if (ds.AFDEntries != null) count += ds.AFDEntries.Count;
            if (ds.ATSEntries != null) count += ds.ATSEntries.Count;
            if (ds.BatteryEntries != null) count += ds.BatteryEntries.Count;
            if (ds.BuswayEntries != null) count += ds.BuswayEntries.Count;
            if (ds.CapacitorEntries != null) count += ds.CapacitorEntries.Count;
            if (ds.CLReactorEntries != null) count += ds.CLReactorEntries.Count;
            if (ds.CTEntries != null) count += ds.CTEntries.Count;
            if (ds.FilterEntries != null) count += ds.FilterEntries.Count;
            if (ds.GeneratorEntries != null) count += ds.GeneratorEntries.Count;
            if (ds.HVBreakerEntries != null) count += ds.HVBreakerEntries.Count;
            if (ds.InverterEntries != null) count += ds.InverterEntries.Count;
            if (ds.LoadEntries != null) count += ds.LoadEntries.Count;
            if (ds.MCCEntries != null) count += ds.MCCEntries.Count;
            if (ds.MeterEntries != null) count += ds.MeterEntries.Count;
            if (ds.MotorEntries != null) count += ds.MotorEntries.Count;
            if (ds.PanelEntries != null) count += ds.PanelEntries.Count;
            if (ds.PhotovoltaicEntries != null) count += ds.PhotovoltaicEntries.Count;
            if (ds.POCEntries != null) count += ds.POCEntries.Count;
            if (ds.RectifierEntries != null) count += ds.RectifierEntries.Count;
            if (ds.RelayEntries != null) count += ds.RelayEntries.Count;
            if (ds.ShuntEntries != null) count += ds.ShuntEntries.Count;
            if (ds.SwitchEntries != null) count += ds.SwitchEntries.Count;
            if (ds.Transformer2WEntries != null) count += ds.Transformer2WEntries.Count;
            if (ds.Transformer3WEntries != null) count += ds.Transformer3WEntries.Count;
            if (ds.TransmissionLineEntries != null) count += ds.TransmissionLineEntries.Count;
            if (ds.UPSEntries != null) count += ds.UPSEntries.Count;
            if (ds.UtilityEntries != null) count += ds.UtilityEntries.Count;
            if (ds.ZigzagTransformerEntries != null) count += ds.ZigzagTransformerEntries.Count;

            return count;
        }

        /// <summary>
        /// Builds scenario mapping dictionary from composite import plan.
        /// </summary>
        private Dictionary<string, string> BuildScenarioMappingsFromPlan(List<ScenarioImportPlan> plan)
        {
            var mappings = new Dictionary<string, string>();

            foreach (var item in plan)
            {
                if (item.OriginalScenario != null && item.TargetScenario != null)
                {
                    // Store the mapping (handle duplicates by keeping first occurrence)
                    if (!mappings.ContainsKey(item.OriginalScenario))
                    {
                        mappings[item.OriginalScenario] = item.TargetScenario;
                    }
                }
            }

            return mappings;
        }
    }
}
