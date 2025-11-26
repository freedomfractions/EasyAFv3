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

            if (_document.Project.ImportHistory == null || _document.Project.ImportHistory.Count == 0)
            {
                Log.Debug("No import history to display");
                return;
            }

            // Build tree: File ? Data Type ? Scenario
            foreach (var record in _document.Project.ImportHistory.OrderByDescending(r => r.ImportedAt))
            {
                var fileNode = CreateFileNode(record);
                ImportHistoryNodes.Add(fileNode);
            }

            Log.Debug("Built import history tree: {Count} file(s)", ImportHistoryNodes.Count);
        }

        /// <summary>
        /// Creates a file node (top level) with data type children and scenario grandchildren.
        /// </summary>
        private ImportHistoryNodeViewModel CreateFileNode(ImportFileRecord record)
        {
            var targetIndicator = record.IsNewData ? "? New Data" : "? Old Data";
            
            var fileNode = new ImportHistoryNodeViewModel
            {
                DisplayText = $"{record.FileName} {targetIndicator}",
                Icon = "\uE8A5", // Document icon
                Tooltip = $"File: {record.FilePath}\nImported: {record.ImportedAt:MMM dd, yyyy h:mm tt}\nMapping: {System.IO.Path.GetFileName(record.MappingPath ?? "Unknown")}",
                FilePath = record.FilePath,
                IsExpanded = false // Collapsed by default
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
                DataTypes = dataType
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
                // Renamed
                displayText = $"{originalScenario} ? {targetScenario}";
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
