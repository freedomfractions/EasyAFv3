using System.Collections.Generic;
using System.Linq;
using EasyAF.Data.Models;

namespace EasyAF.Data.Extensions
{
    /// <summary>
    /// Extension methods for DataSet to support scenario discovery and statistics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These extensions enable the Project Module to discover and display scenario-based
    /// statistics for composite-key data types (ArcFlash, ShortCircuit) that include
    /// a Scenario property in their composite keys.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// - Display scenarios in a tree view (Standard and Composite projects)
    /// - Show entry counts per scenario per datatype
    /// - Detect uniform vs. mixed scenario distributions
    /// - Enable scenario-specific operations (clear, replace, delete)
    /// </para>
    /// </remarks>
    public static class DataSetExtensions
    {
        /// <summary>
        /// Gets all unique scenario names present in this DataSet.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <returns>A sorted list of unique scenario names, or empty list if no scenarios exist.</returns>
        /// <remarks>
        /// <para>
        /// Scenarios are extracted from:
        /// - ArcFlashEntries (composite key: Id, Scenario)
        /// - ShortCircuitEntries (composite key: Id, Bus, Scenario)
        /// </para>
        /// <para>
        /// Other data types (Bus, LVCB, Fuse, etc.) do not have scenarios and are not included.
        /// </para>
        /// <para>
        /// <strong>Example Usage:</strong>
        /// </para>
        /// <code>
        /// var scenarios = dataSet.GetAvailableScenarios();
        /// // Returns: ["Emergency", "Generator", "Main-Max", "Main-Min"]
        /// </code>
        /// </remarks>
        public static List<string> GetAvailableScenarios(this DataSet? dataSet)
        {
            if (dataSet == null)
                return new List<string>();

            var scenarios = new HashSet<string>();

            // Extract scenarios from ArcFlash entries (Component[1] = Scenario)
            if (dataSet.ArcFlashEntries != null)
            {
                foreach (var key in dataSet.ArcFlashEntries.Keys)
                {
                    // ArcFlash: Components[0] = ArcFaultBusName, Components[1] = Scenario
                    if (key.Components.Length >= 2 && !string.IsNullOrWhiteSpace(key.Components[1]))
                        scenarios.Add(key.Components[1]);
                }
            }

            // Extract scenarios from ShortCircuit entries (Component[2] = Scenario)
            if (dataSet.ShortCircuitEntries != null)
            {
                foreach (var key in dataSet.ShortCircuitEntries.Keys)
                {
                    // ShortCircuit: Components[0] = BusName, Components[1] = EquipmentName, Components[2] = Scenario
                    if (key.Components.Length >= 3 && !string.IsNullOrWhiteSpace(key.Components[2]))
                        scenarios.Add(key.Components[2]);
                }
            }

            // Return sorted list for consistent UI display
            return scenarios.OrderBy(s => s).ToList();
        }

        /// <summary>
        /// Gets entry counts for all data types, grouped by scenario.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <returns>
        /// A dictionary where:
        /// - Key = data type name (e.g., "ArcFlash", "ShortCircuit", "Bus")
        /// - Value = dictionary of scenario ? count (or "(All)" for non-scenario types)
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Scenario-based types</strong> (ArcFlash, ShortCircuit):
        /// - Entries are grouped by scenario
        /// - Example: { "ArcFlash": { "Main-Max": 40, "Main-Min": 40, ... } }
        /// </para>
        /// <para>
        /// <strong>Non-scenario types</strong> (Bus, LVCB, Fuse, etc.):
        /// - All entries are grouped under "(All)" key
        /// - Example: { "Bus": { "(All)": 120 } }
        /// </para>
        /// <para>
        /// <strong>Example Output:</strong>
        /// </para>
        /// <code>
        /// {
        ///   "ArcFlash": { "Main-Max": 40, "Main-Min": 40, "Emergency": 40, "Generator": 40 },
        ///   "ShortCircuit": { "Main-Max": 40, "Main-Min": 40, "Emergency": 40, "Generator": 40 },
        ///   "Bus": { "(All)": 120 },
        ///   "LVCB": { "(All)": 85 },
        ///   "Fuse": { "(All)": 45 }
        /// }
        /// </code>
        /// </remarks>
        public static Dictionary<string, Dictionary<string, int>> GetStatisticsByScenario(this DataSet? dataSet)
        {
            var result = new Dictionary<string, Dictionary<string, int>>();

            if (dataSet == null)
                return result;

            // Scenario-based types
            AddScenarioBasedStatistics(result, "ArcFlash", dataSet.ArcFlashEntries?.Keys, scenarioComponentIndex: 1);
            AddScenarioBasedStatistics(result, "ShortCircuit", dataSet.ShortCircuitEntries?.Keys, scenarioComponentIndex: 2);

            // Non-scenario types (simple dictionaries)
            AddSimpleStatistics(result, "Bus", dataSet.BusEntries?.Count);
            AddSimpleStatistics(result, "LVCB", dataSet.LVBreakerEntries?.Count);
            AddSimpleStatistics(result, "Fuse", dataSet.FuseEntries?.Count);
            AddSimpleStatistics(result, "Cable", dataSet.CableEntries?.Count);

            // Extended equipment types
            AddSimpleStatistics(result, "AFD", dataSet.AFDEntries?.Count);
            AddSimpleStatistics(result, "ATS", dataSet.ATSEntries?.Count);
            AddSimpleStatistics(result, "Battery", dataSet.BatteryEntries?.Count);
            AddSimpleStatistics(result, "Busway", dataSet.BuswayEntries?.Count);
            AddSimpleStatistics(result, "Capacitor", dataSet.CapacitorEntries?.Count);
            AddSimpleStatistics(result, "CLReactor", dataSet.CLReactorEntries?.Count);
            AddSimpleStatistics(result, "CT", dataSet.CTEntries?.Count);
            AddSimpleStatistics(result, "Filter", dataSet.FilterEntries?.Count);
            AddSimpleStatistics(result, "Generator", dataSet.GeneratorEntries?.Count);
            AddSimpleStatistics(result, "HVBreaker", dataSet.HVBreakerEntries?.Count);
            AddSimpleStatistics(result, "Inverter", dataSet.InverterEntries?.Count);
            AddSimpleStatistics(result, "Load", dataSet.LoadEntries?.Count);
            AddSimpleStatistics(result, "MCC", dataSet.MCCEntries?.Count);
            AddSimpleStatistics(result, "Meter", dataSet.MeterEntries?.Count);
            AddSimpleStatistics(result, "Motor", dataSet.MotorEntries?.Count);
            AddSimpleStatistics(result, "Panel", dataSet.PanelEntries?.Count);
            AddSimpleStatistics(result, "Photovoltaic", dataSet.PhotovoltaicEntries?.Count);
            AddSimpleStatistics(result, "POC", dataSet.POCEntries?.Count);
            AddSimpleStatistics(result, "Rectifier", dataSet.RectifierEntries?.Count);
            AddSimpleStatistics(result, "Relay", dataSet.RelayEntries?.Count);
            AddSimpleStatistics(result, "Shunt", dataSet.ShuntEntries?.Count);
            AddSimpleStatistics(result, "Switch", dataSet.SwitchEntries?.Count);
            AddSimpleStatistics(result, "Transformer2W", dataSet.Transformer2WEntries?.Count);
            AddSimpleStatistics(result, "Transformer3W", dataSet.Transformer3WEntries?.Count);
            AddSimpleStatistics(result, "TransmissionLine", dataSet.TransmissionLineEntries?.Count);
            AddSimpleStatistics(result, "UPS", dataSet.UPSEntries?.Count);
            AddSimpleStatistics(result, "Utility", dataSet.UtilityEntries?.Count);
            AddSimpleStatistics(result, "ZigzagTransformer", dataSet.ZigzagTransformerEntries?.Count);

            return result;
        }

        /// <summary>
        /// Gets entry counts for a specific scenario across all data types.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <param name="scenarioName">The scenario name to filter by.</param>
        /// <returns>
        /// A dictionary where:
        /// - Key = data type name
        /// - Value = count of entries for that scenario
        /// </returns>
        /// <remarks>
        /// <para>
        /// Only returns statistics for scenario-based types (ArcFlash, ShortCircuit).
        /// Non-scenario types (Bus, LVCB, etc.) are not included since they don't have scenarios.
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// </para>
        /// <code>
        /// var stats = dataSet.GetScenarioStatistics("Main-Max");
        /// // Returns: { "ArcFlash": 40, "ShortCircuit": 40 }
        /// </code>
        /// </remarks>
        public static Dictionary<string, int> GetScenarioStatistics(this DataSet? dataSet, string scenarioName)
        {
            var result = new Dictionary<string, int>();

            if (dataSet == null || string.IsNullOrWhiteSpace(scenarioName))
                return result;

            // Count ArcFlash entries for this scenario (Component[1])
            if (dataSet.ArcFlashEntries != null)
            {
                var count = dataSet.ArcFlashEntries.Keys.Count(k => 
                    k.Components.Length >= 2 && k.Components[1] == scenarioName);
                if (count > 0)
                    result["ArcFlash"] = count;
            }

            // Count ShortCircuit entries for this scenario (Component[2])
            if (dataSet.ShortCircuitEntries != null)
            {
                var count = dataSet.ShortCircuitEntries.Keys.Count(k => 
                    k.Components.Length >= 3 && k.Components[2] == scenarioName);
                if (count > 0)
                    result["ShortCircuit"] = count;
            }

            return result;
        }

        /// <summary>
        /// Checks if all scenarios have the same entry count for a given data type.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <param name="dataTypeName">The data type name (e.g., "ArcFlash").</param>
        /// <returns>
        /// True if all scenarios have identical counts (uniform distribution);
        /// False if counts vary across scenarios.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Used to determine whether to show a warning indicator (?) in the UI when
        /// scenario counts are inconsistent, which might indicate incomplete imports
        /// or data issues.
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// </para>
        /// <code>
        /// // All scenarios have 40 entries ? returns true
        /// var isUniform = dataSet.IsScenariosUniform("ArcFlash");
        /// 
        /// // Scenarios have [40, 40, 35, 40] entries ? returns false
        /// var isUniform = dataSet.IsScenariosUniform("ArcFlash");
        /// </code>
        /// </remarks>
        public static bool IsScenariosUniform(this DataSet? dataSet, string dataTypeName)
        {
            if (dataSet == null)
                return true; // No data = uniform by default

            var stats = dataSet.GetStatisticsByScenario();
            
            if (!stats.TryGetValue(dataTypeName, out var scenarioCounts))
                return true; // No entries = uniform

            if (scenarioCounts.Count <= 1)
                return true; // Only one scenario = uniform

            // Check if all counts are identical
            var firstCount = scenarioCounts.Values.First();
            return scenarioCounts.Values.All(count => count == firstCount);
        }

        #region Helper Methods

        /// <summary>
        /// Adds statistics for scenario-based data types (composite keys).
        /// </summary>
        private static void AddScenarioBasedStatistics(
            Dictionary<string, Dictionary<string, int>> result,
            string dataTypeName,
            IEnumerable<CompositeKey>? keys,
            int scenarioComponentIndex)
        {
            if (keys == null)
                return;

            var scenarioCounts = new Dictionary<string, int>();

            foreach (var key in keys)
            {
                // Extract scenario from Components array at specified index
                if (key.Components.Length > scenarioComponentIndex)
                {
                    var scenario = key.Components[scenarioComponentIndex];
                    
                    if (!string.IsNullOrWhiteSpace(scenario))
                    {
                        if (!scenarioCounts.ContainsKey(scenario))
                            scenarioCounts[scenario] = 0;

                        scenarioCounts[scenario]++;
                    }
                }
            }

            if (scenarioCounts.Count > 0)
                result[dataTypeName] = scenarioCounts;
        }

        /// <summary>
        /// Adds statistics for simple (non-scenario) data types.
        /// </summary>
        private static void AddSimpleStatistics(
            Dictionary<string, Dictionary<string, int>> result,
            string dataTypeName,
            int? count)
        {
            if (count == null || count == 0)
                return;

            result[dataTypeName] = new Dictionary<string, int>
            {
                { "(All)", count.Value }
            };
        }

        #endregion
    }
}
