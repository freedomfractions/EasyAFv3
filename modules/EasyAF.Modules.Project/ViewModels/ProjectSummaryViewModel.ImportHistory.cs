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
        /// Gets the tree nodes for New Data import history.
        /// </summary>
        public ObservableCollection<ImportHistoryNodeViewModel> NewDataImportHistory { get; } = new();

        /// <summary>
        /// Gets the tree nodes for Old Data import history.
        /// </summary>
        public ObservableCollection<ImportHistoryNodeViewModel> OldDataImportHistory { get; } = new();

        /// <summary>
        /// Builds the import history tree from the project's import records.
        /// Creates separate trees for New Data and Old Data.
        /// </summary>
        public void BuildImportHistoryTree()
        {
            // Clear existing trees
            NewDataImportHistory.Clear();
            OldDataImportHistory.Clear();

            if (_document.Project.ImportHistory == null || _document.Project.ImportHistory.Count == 0)
            {
                Log.Debug("No import history to display");
                return;
            }

            // Group imports by timestamp and target (creates parent nodes)
            var importGroups = _document.Project.ImportHistory
                .GroupBy(record => new { record.ImportedAt, record.IsNewData })
                .OrderByDescending(g => g.Key.ImportedAt) // Most recent first
                .ToList();

            foreach (var group in importGroups)
            {
                var parentNode = CreateParentNode(group.Key.ImportedAt.LocalDateTime, group.Key.IsNewData, group.ToList());

                // Add to appropriate tree
                if (group.Key.IsNewData)
                    NewDataImportHistory.Add(parentNode);
                else
                    OldDataImportHistory.Add(parentNode);
            }

            Log.Debug("Built import history tree: {NewCount} New Data sessions, {OldCount} Old Data sessions",
                NewDataImportHistory.Count, OldDataImportHistory.Count);
        }

        /// <summary>
        /// Creates a parent node representing an import session.
        /// </summary>
        private ImportHistoryNodeViewModel CreateParentNode(DateTime timestamp, bool isNewData, List<ImportFileRecord> files)
        {
            var totalEntries = files.Sum(f => f.EntryCount);
            var fileCount = files.Count;
            var targetName = isNewData ? "New Data" : "Old Data";

            // Get mapping path from first file (all files in a session use the same mapping)
            var mappingPath = files.FirstOrDefault()?.MappingPath ?? "Unknown";
            var mappingName = System.IO.Path.GetFileName(mappingPath);

            var parentNode = new ImportHistoryNodeViewModel
            {
                DisplayText = $"{timestamp:MMM dd, yyyy h:mm tt} - {targetName} ({fileCount} file{(fileCount == 1 ? "" : "s")}, {totalEntries} entries)",
                Icon = isNewData ? "\uE8B7" : "\uE823", // Database or Archive icon
                Tooltip = $"Imported to {targetName}\nMapping: {mappingName}\n{totalEntries} total entries",
                IsExpanded = false, // Collapsed by default
                Timestamp = timestamp,
                IsNewData = isNewData,
                MappingPath = mappingPath,
                TotalEntries = totalEntries
            };

            // Add child nodes for each file
            foreach (var fileRecord in files.OrderBy(f => f.FilePath))
            {
                var childNode = CreateChildNode(fileRecord);
                parentNode.Children.Add(childNode);
            }

            return parentNode;
        }

        /// <summary>
        /// Creates a child node representing an imported file.
        /// </summary>
        private ImportHistoryNodeViewModel CreateChildNode(ImportFileRecord fileRecord)
        {
            var fileName = System.IO.Path.GetFileName(fileRecord.FilePath);
            var dataTypesSummary = string.Join(", ", fileRecord.DataTypes.OrderBy(dt => dt));

            // Build scenario mapping summary (if any)
            string? scenarioSummary = null;
            if (fileRecord.ScenarioMappings != null && fileRecord.ScenarioMappings.Count > 0)
            {
                var mappings = fileRecord.ScenarioMappings
                    .Where(kvp => kvp.Key != kvp.Value) // Only show renames
                    .Select(kvp => $"{kvp.Key} ? {kvp.Value}");
                
                if (mappings.Any())
                    scenarioSummary = string.Join(", ", mappings);
            }

            // Build display text
            var displayText = $"{fileName} - {dataTypesSummary}";
            if (fileRecord.EntryCount > 0)
                displayText += $" ({fileRecord.EntryCount} entries)";

            // Build tooltip
            var tooltipLines = new List<string>
            {
                $"File: {fileRecord.FilePath}",
                $"Data Types: {dataTypesSummary}",
                $"Entries: {fileRecord.EntryCount}"
            };

            if (scenarioSummary != null)
                tooltipLines.Add($"Scenario Mappings: {scenarioSummary}");

            var childNode = new ImportHistoryNodeViewModel
            {
                DisplayText = displayText,
                Icon = "\uE8A5", // Document icon
                Tooltip = string.Join("\n", tooltipLines),
                FilePath = fileRecord.FilePath,
                DataTypes = dataTypesSummary,
                ScenarioMappings = scenarioSummary,
                FileEntryCount = fileRecord.EntryCount
            };

            return childNode;
        }

        #endregion

        /// <summary>
        /// Records import operations in the project's import history.
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
                if (_document.Project.ImportHistory == null)
                    _document.Project.ImportHistory = new List<ImportFileRecord>();

                var timestamp = DateTimeOffset.Now;
                var targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;

                // Get mapping path (MappingConfig doesn't have FilePath, use MapPath from ViewModel)
                var mappingPath = MapPath ?? "Unknown";

                foreach (var filePath in filePaths)
                {
                    // Get data types present in this file
                    var dataTypes = GetDataTypesFromDataSet(targetDataSet);

                    // Get scenario mappings (if composite import or provided)
                    var fileScenarioMappings = scenarioMappings ?? GetScenarioMappingsFromDataSet(targetDataSet);

                    // Count entries for this file (best effort - counts all entries in target dataset)
                    var entryCount = CountEntriesInDataSet(targetDataSet);

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
                    Log.Debug("Recorded import: {File} ? {Target} ({Count} entries, {DataTypeCount} data types)",
                        System.IO.Path.GetFileName(filePath),
                        isNewData ? "New" : "Old",
                        entryCount,
                        dataTypes.Count);
                }

                // Mark project as dirty (changes need to be saved)
                _document.MarkDirty();

                // Rebuild import history tree to show new entries
                BuildImportHistoryTree();

                Log.Information("Import history updated: {Count} file(s) recorded", filePaths.Length);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error recording import in history");
                // Don't fail the import if history recording fails
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
