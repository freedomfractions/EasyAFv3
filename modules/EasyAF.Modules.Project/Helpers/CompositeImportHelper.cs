using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Data.Models;
using EasyAF.Data.Extensions;
using Serilog;

namespace EasyAF.Modules.Project.Helpers
{
    /// <summary>
    /// Helper class for Composite mode import operations.
    /// Handles scenario extraction, filtering, renaming, and merging of DataSets.
    /// </summary>
    public static class CompositeImportHelper
    {
        /// <summary>
        /// Pre-scans files to extract scenarios and data types for the Composite import dialog.
        /// </summary>
        /// <param name="filePaths">Files to scan.</param>
        /// <param name="mappingConfig">Mapping configuration.</param>
        /// <returns>Dictionary mapping file paths to their file scan results (data types and scenarios).</returns>
        public static Dictionary<string, FileScanResult> PreScanFilesForScenarios(
            string[] filePaths,
            EasyAF.Import.MappingConfig mappingConfig)
        {
            var fileResults = new Dictionary<string, FileScanResult>();
            var importManager = new EasyAF.Import.ImportManager();

            foreach (var filePath in filePaths)
            {
                try
                {
                    // Import into temporary dataset to extract scenarios and data types
                    var tempDataSet = new DataSet();
                    importManager.Import(filePath, mappingConfig, tempDataSet);

                    // Get scenarios from this file
                    var scenarios = tempDataSet.GetAvailableScenarios().ToList();

                    // Detect which data types this file contains
                    var dataTypes = new List<string>();
                    if ((tempDataSet.ArcFlashEntries?.Count ?? 0) > 0)
                        dataTypes.Add("ArcFlash");
                    if ((tempDataSet.ShortCircuitEntries?.Count ?? 0) > 0)
                        dataTypes.Add("ShortCircuit");

                    // If no scenario-based data types, it's a non-scenario file
                    var isNonScenarioFile = dataTypes.Count == 0;

                    var result = new FileScanResult
                    {
                        Scenarios = scenarios,
                        DataTypes = dataTypes,
                        IsNonScenarioFile = isNonScenarioFile
                    };

                    fileResults[filePath] = result;

                    Log.Debug("Pre-scan {File}: DataTypes=[{DataTypes}], Scenarios=[{Scenarios}]",
                        System.IO.Path.GetFileName(filePath),
                        dataTypes.Count > 0 ? string.Join(", ", dataTypes) : "None",
                        scenarios.Count > 0 ? string.Join(", ", scenarios) : "none");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error pre-scanning file {File} for scenarios", System.IO.Path.GetFileName(filePath));
                    // Add empty result for files that failed to scan
                    fileResults[filePath] = new FileScanResult
                    {
                        Scenarios = new List<string>(),
                        DataTypes = new List<string>(),
                        IsNonScenarioFile = true
                    };
                }
            }

            return fileResults;
        }

        /// <summary>
        /// Filters a DataSet to only include entries for a specific scenario.
        /// </summary>
        /// <param name="source">Source DataSet to filter.</param>
        /// <param name="scenario">Scenario name to filter by.</param>
        /// <returns>Filtered DataSet containing only the specified scenario.</returns>
        public static DataSet FilterDataSetByScenario(DataSet source, string scenario)
        {
            var filtered = new DataSet();

            // Filter scenario-based data types (ArcFlash, ShortCircuit)
            if (source.ArcFlashEntries != null)
            {
                filtered.ArcFlashEntries = new Dictionary<CompositeKey, ArcFlash>();
                foreach (var kvp in source.ArcFlashEntries.Where(e => e.Value.Scenario == scenario))
                {
                    filtered.ArcFlashEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.ShortCircuitEntries != null)
            {
                filtered.ShortCircuitEntries = new Dictionary<CompositeKey, ShortCircuit>();
                foreach (var kvp in source.ShortCircuitEntries.Where(e => e.Value.Scenario == scenario))
                {
                    filtered.ShortCircuitEntries[kvp.Key] = kvp.Value;
                }
            }

            // Non-scenario data types are included as-is (they don't have Scenario property)
            filtered.BusEntries = source.BusEntries;
            filtered.LVBreakerEntries = source.LVBreakerEntries;
            filtered.FuseEntries = source.FuseEntries;
            filtered.CableEntries = source.CableEntries;
            filtered.AFDEntries = source.AFDEntries;
            filtered.ATSEntries = source.ATSEntries;
            filtered.BatteryEntries = source.BatteryEntries;
            filtered.BuswayEntries = source.BuswayEntries;
            filtered.CapacitorEntries = source.CapacitorEntries;
            filtered.CLReactorEntries = source.CLReactorEntries;
            filtered.CTEntries = source.CTEntries;
            filtered.FilterEntries = source.FilterEntries;
            filtered.GeneratorEntries = source.GeneratorEntries;
            filtered.HVBreakerEntries = source.HVBreakerEntries;
            filtered.InverterEntries = source.InverterEntries;
            filtered.LoadEntries = source.LoadEntries;
            filtered.MCCEntries = source.MCCEntries;
            filtered.MeterEntries = source.MeterEntries;
            filtered.MotorEntries = source.MotorEntries;
            filtered.PanelEntries = source.PanelEntries;
            filtered.PhotovoltaicEntries = source.PhotovoltaicEntries;
            filtered.POCEntries = source.POCEntries;
            filtered.RectifierEntries = source.RectifierEntries;
            filtered.RelayEntries = source.RelayEntries;
            filtered.ShuntEntries = source.ShuntEntries;
            filtered.SwitchEntries = source.SwitchEntries;
            filtered.Transformer2WEntries = source.Transformer2WEntries;
            filtered.Transformer3WEntries = source.Transformer3WEntries;
            filtered.TransmissionLineEntries = source.TransmissionLineEntries;
            filtered.UPSEntries = source.UPSEntries;
            filtered.UtilityEntries = source.UtilityEntries;
            filtered.ZigzagTransformerEntries = source.ZigzagTransformerEntries;

            return filtered;
        }

        /// <summary>
        /// Renames scenarios in a DataSet.
        /// </summary>
        /// <param name="dataSet">DataSet to modify.</param>
        /// <param name="oldScenario">Original scenario name.</param>
        /// <param name="newScenario">New scenario name.</param>
        public static void RenameScenarioInDataSet(DataSet dataSet, string oldScenario, string newScenario)
        {
            // Rename in ArcFlash entries - rebuild dictionary with new keys
            if (dataSet.ArcFlashEntries != null)
            {
                var entriesToRename = dataSet.ArcFlashEntries
                    .Where(kvp => kvp.Value.Scenario == oldScenario)
                    .ToList();

                foreach (var kvp in entriesToRename)
                {
                    // Remove old entry
                    dataSet.ArcFlashEntries.Remove(kvp.Key);
                    
                    // Update scenario name in value
                    kvp.Value.Scenario = newScenario;
                    
                    // Rebuild composite key with new scenario name
                    var newKey = EasyAF.Import.CompositeKeyHelper.BuildCompositeKey(kvp.Value, typeof(ArcFlash));
                    if (newKey != null)
                    {
                        dataSet.ArcFlashEntries[newKey] = kvp.Value;
                    }
                }
            }

            // Rename in ShortCircuit entries - rebuild dictionary with new keys
            if (dataSet.ShortCircuitEntries != null)
            {
                var entriesToRename = dataSet.ShortCircuitEntries
                    .Where(kvp => kvp.Value.Scenario == oldScenario)
                    .ToList();

                foreach (var kvp in entriesToRename)
                {
                    // Remove old entry
                    dataSet.ShortCircuitEntries.Remove(kvp.Key);
                    
                    // Update scenario name in value
                    kvp.Value.Scenario = newScenario;
                    
                    // Rebuild composite key with new scenario name
                    var newKey = EasyAF.Import.CompositeKeyHelper.BuildCompositeKey(kvp.Value, typeof(ShortCircuit));
                    if (newKey != null)
                    {
                        dataSet.ShortCircuitEntries[newKey] = kvp.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Merges source DataSet into target DataSet.
        /// Entries with matching keys will be overwritten.
        /// </summary>
        /// <param name="source">Source DataSet to merge from.</param>
        /// <param name="target">Target DataSet to merge into.</param>
        public static void MergeDataSets(DataSet source, DataSet target)
        {
            // Merge all data type dictionaries - assign back to properties
            if (source.BusEntries != null && source.BusEntries.Count > 0)
            {
                target.BusEntries ??= new Dictionary<CompositeKey, Bus>();
                foreach (var kvp in source.BusEntries)
                {
                    target.BusEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.LVBreakerEntries != null && source.LVBreakerEntries.Count > 0)
            {
                target.LVBreakerEntries ??= new Dictionary<CompositeKey, LVBreaker>();
                foreach (var kvp in source.LVBreakerEntries)
                {
                    target.LVBreakerEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.FuseEntries != null && source.FuseEntries.Count > 0)
            {
                target.FuseEntries ??= new Dictionary<CompositeKey, Fuse>();
                foreach (var kvp in source.FuseEntries)
                {
                    target.FuseEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.CableEntries != null && source.CableEntries.Count > 0)
            {
                target.CableEntries ??= new Dictionary<CompositeKey, Cable>();
                foreach (var kvp in source.CableEntries)
                {
                    target.CableEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.ArcFlashEntries != null && source.ArcFlashEntries.Count > 0)
            {
                target.ArcFlashEntries ??= new Dictionary<CompositeKey, ArcFlash>();
                foreach (var kvp in source.ArcFlashEntries)
                {
                    target.ArcFlashEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.ShortCircuitEntries != null && source.ShortCircuitEntries.Count > 0)
            {
                target.ShortCircuitEntries ??= new Dictionary<CompositeKey, ShortCircuit>();
                foreach (var kvp in source.ShortCircuitEntries)
                {
                    target.ShortCircuitEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.AFDEntries != null && source.AFDEntries.Count > 0)
            {
                target.AFDEntries ??= new Dictionary<CompositeKey, AFD>();
                foreach (var kvp in source.AFDEntries)
                {
                    target.AFDEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.ATSEntries != null && source.ATSEntries.Count > 0)
            {
                target.ATSEntries ??= new Dictionary<CompositeKey, ATS>();
                foreach (var kvp in source.ATSEntries)
                {
                    target.ATSEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.BatteryEntries != null && source.BatteryEntries.Count > 0)
            {
                target.BatteryEntries ??= new Dictionary<CompositeKey, Battery>();
                foreach (var kvp in source.BatteryEntries)
                {
                    target.BatteryEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.BuswayEntries != null && source.BuswayEntries.Count > 0)
            {
                target.BuswayEntries ??= new Dictionary<CompositeKey, Busway>();
                foreach (var kvp in source.BuswayEntries)
                {
                    target.BuswayEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.CapacitorEntries != null && source.CapacitorEntries.Count > 0)
            {
                target.CapacitorEntries ??= new Dictionary<CompositeKey, Capacitor>();
                foreach (var kvp in source.CapacitorEntries)
                {
                    target.CapacitorEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.CLReactorEntries != null && source.CLReactorEntries.Count > 0)
            {
                target.CLReactorEntries ??= new Dictionary<CompositeKey, CLReactor>();
                foreach (var kvp in source.CLReactorEntries)
                {
                    target.CLReactorEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.CTEntries != null && source.CTEntries.Count > 0)
            {
                target.CTEntries ??= new Dictionary<CompositeKey, CT>();
                foreach (var kvp in source.CTEntries)
                {
                    target.CTEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.FilterEntries != null && source.FilterEntries.Count > 0)
            {
                target.FilterEntries ??= new Dictionary<CompositeKey, Filter>();
                foreach (var kvp in source.FilterEntries)
                {
                    target.FilterEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.GeneratorEntries != null && source.GeneratorEntries.Count > 0)
            {
                target.GeneratorEntries ??= new Dictionary<CompositeKey, Generator>();
                foreach (var kvp in source.GeneratorEntries)
                {
                    target.GeneratorEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.HVBreakerEntries != null && source.HVBreakerEntries.Count > 0)
            {
                target.HVBreakerEntries ??= new Dictionary<CompositeKey, HVBreaker>();
                foreach (var kvp in source.HVBreakerEntries)
                {
                    target.HVBreakerEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.InverterEntries != null && source.InverterEntries.Count > 0)
            {
                target.InverterEntries ??= new Dictionary<CompositeKey, Inverter>();
                foreach (var kvp in source.InverterEntries)
                {
                    target.InverterEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.LoadEntries != null && source.LoadEntries.Count > 0)
            {
                target.LoadEntries ??= new Dictionary<CompositeKey, Load>();
                foreach (var kvp in source.LoadEntries)
                {
                    target.LoadEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.MCCEntries != null && source.MCCEntries.Count > 0)
            {
                target.MCCEntries ??= new Dictionary<CompositeKey, MCC>();
                foreach (var kvp in source.MCCEntries)
                {
                    target.MCCEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.MeterEntries != null && source.MeterEntries.Count > 0)
            {
                target.MeterEntries ??= new Dictionary<CompositeKey, Meter>();
                foreach (var kvp in source.MeterEntries)
                {
                    target.MeterEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.MotorEntries != null && source.MotorEntries.Count > 0)
            {
                target.MotorEntries ??= new Dictionary<CompositeKey, Motor>();
                foreach (var kvp in source.MotorEntries)
                {
                    target.MotorEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.PanelEntries != null && source.PanelEntries.Count > 0)
            {
                target.PanelEntries ??= new Dictionary<CompositeKey, Panel>();
                foreach (var kvp in source.PanelEntries)
                {
                    target.PanelEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.PhotovoltaicEntries != null && source.PhotovoltaicEntries.Count > 0)
            {
                target.PhotovoltaicEntries ??= new Dictionary<CompositeKey, Photovoltaic>();
                foreach (var kvp in source.PhotovoltaicEntries)
                {
                    target.PhotovoltaicEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.POCEntries != null && source.POCEntries.Count > 0)
            {
                target.POCEntries ??= new Dictionary<CompositeKey, POC>();
                foreach (var kvp in source.POCEntries)
                {
                    target.POCEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.RectifierEntries != null && source.RectifierEntries.Count > 0)
            {
                target.RectifierEntries ??= new Dictionary<CompositeKey, Rectifier>();
                foreach (var kvp in source.RectifierEntries)
                {
                    target.RectifierEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.RelayEntries != null && source.RelayEntries.Count > 0)
            {
                target.RelayEntries ??= new Dictionary<CompositeKey, Relay>();
                foreach (var kvp in source.RelayEntries)
                {
                    target.RelayEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.ShuntEntries != null && source.ShuntEntries.Count > 0)
            {
                target.ShuntEntries ??= new Dictionary<CompositeKey, Shunt>();
                foreach (var kvp in source.ShuntEntries)
                {
                    target.ShuntEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.SwitchEntries != null && source.SwitchEntries.Count > 0)
            {
                target.SwitchEntries ??= new Dictionary<CompositeKey, Switch>();
                foreach (var kvp in source.SwitchEntries)
                {
                    target.SwitchEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.Transformer2WEntries != null && source.Transformer2WEntries.Count > 0)
            {
                target.Transformer2WEntries ??= new Dictionary<CompositeKey, Transformer2W>();
                foreach (var kvp in source.Transformer2WEntries)
                {
                    target.Transformer2WEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.Transformer3WEntries != null && source.Transformer3WEntries.Count > 0)
            {
                target.Transformer3WEntries ??= new Dictionary<CompositeKey, Transformer3W>();
                foreach (var kvp in source.Transformer3WEntries)
                {
                    target.Transformer3WEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.TransmissionLineEntries != null && source.TransmissionLineEntries.Count > 0)
            {
                target.TransmissionLineEntries ??= new Dictionary<CompositeKey, TransmissionLine>();
                foreach (var kvp in source.TransmissionLineEntries)
                {
                    target.TransmissionLineEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.UPSEntries != null && source.UPSEntries.Count > 0)
            {
                target.UPSEntries ??= new Dictionary<CompositeKey, UPS>();
                foreach (var kvp in source.UPSEntries)
                {
                    target.UPSEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.UtilityEntries != null && source.UtilityEntries.Count > 0)
            {
                target.UtilityEntries ??= new Dictionary<CompositeKey, Utility>();
                foreach (var kvp in source.UtilityEntries)
                {
                    target.UtilityEntries[kvp.Key] = kvp.Value;
                }
            }

            if (source.ZigzagTransformerEntries != null && source.ZigzagTransformerEntries.Count > 0)
            {
                target.ZigzagTransformerEntries ??= new Dictionary<CompositeKey, ZigzagTransformer>();
                foreach (var kvp in source.ZigzagTransformerEntries)
                {
                    target.ZigzagTransformerEntries[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    /// <summary>
    /// Result of pre-scanning a file for scenarios and data types.
    /// </summary>
    public class FileScanResult
    {
        public List<string> Scenarios { get; set; } = new();
        public List<string> DataTypes { get; set; } = new();
        public bool IsNonScenarioFile { get; set; }
    }
}
