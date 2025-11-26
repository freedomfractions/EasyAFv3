using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Data.Models;
using EasyAF.Data.Extensions;
using EasyAF.Modules.Project.Helpers;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// Partial class containing Import History recording functionality.
    /// </summary>
    public partial class ProjectSummaryViewModel
    {
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
                _document.Project.ImportHistory ??= new List<ImportFileRecord>();

                foreach (var filePath in filePaths)
                {
                    // Create temp dataset to analyze file contents
                    var tempDataSet = new DataSet();
                    var importManager = new EasyAF.Import.ImportManager();

                    try
                    {
                        importManager.Import(filePath, mappingConfig, tempDataSet);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Could not analyze file {File} for history recording - skipping", System.IO.Path.GetFileName(filePath));
                        continue;
                    }

                    // Detect data types present in this file
                    var dataTypes = GetDataTypesFromDataSet(tempDataSet);

                    // Get scenario mappings (either from composite plan or discover from dataset)
                    var mappings = scenarioMappings ?? GetScenarioMappingsFromDataSet(tempDataSet);

                    // Count total entries
                    int entryCount = CountEntriesInDataSet(tempDataSet);

                    // Create history record
                    var record = new ImportFileRecord(filePath, isNewData, dataTypes, mappings, entryCount);

                    _document.Project.ImportHistory.Add(record);

                    Log.Information("Recorded import history: {File} -> {DataTypes} ({Count} entries, {Scenarios} scenarios)",
                        System.IO.Path.GetFileName(filePath),
                        string.Join(", ", dataTypes),
                        entryCount,
                        mappings.Count);
                }

                _document.MarkDirty();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error recording import history");
                // Don't throw - this is non-critical functionality
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
