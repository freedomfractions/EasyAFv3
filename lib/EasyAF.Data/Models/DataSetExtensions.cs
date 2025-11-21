using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Extension methods for DataSet scenario discovery and statistics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides methods to:
    /// - Discover scenarios in imported data
    /// - Calculate per-scenario statistics
    /// - Detect scenario uniformity
    /// - Generate statistics for Project Summary UI
    /// </para>
    /// <para>
    /// <strong>Scenario Architecture:</strong>
    /// Only ArcFlash and ShortCircuit have scenarios (composite keys).
    /// Equipment types (Bus, LVBreaker, etc.) do NOT have scenarios.
    /// </para>
    /// </remarks>
    public static class DataSetExtensions
    {
        /// <summary>
        /// Gets the set of unique scenario names in the DataSet.
        /// </summary>
        /// <param name="dataSet">The DataSet to scan for scenarios.</param>
        /// <returns>HashSet of unique scenario names (case-insensitive).</returns>
        /// <remarks>
        /// <para>
        /// Scans ArcFlashEntries and ShortCircuitEntries for unique Scenario values.
        /// Combines scenarios from both data types (union).
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// <code>
        /// var scenarios = dataSet.GetAvailableScenarios();
        /// // Result: { "Main-Min", "Main-Max", "Service-Min", "Service-Max" }
        /// </code>
        /// </para>
        /// <para>
        /// <strong>Empty Result:</strong> Returns empty set if no scenario-aware data exists.
        /// </para>
        /// <para>
        /// <strong>Case Handling:</strong> Uses case-insensitive comparison (OrdinalIgnoreCase).
        /// "Main-Min" and "main-min" are treated as the same scenario.
        /// </para>
        /// </remarks>
        public static HashSet<string> GetAvailableScenarios(this DataSet dataSet)
        {
            var scenarios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Scan ArcFlash entries for scenarios
            if (dataSet.ArcFlashEntries != null)
            {
                foreach (var key in dataSet.ArcFlashEntries.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(key.Scenario))
                    {
                        scenarios.Add(key.Scenario);
                    }
                }
            }

            // Scan ShortCircuit entries for scenarios
            if (dataSet.ShortCircuitEntries != null)
            {
                foreach (var key in dataSet.ShortCircuitEntries.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(key.Scenario))
                    {
                        scenarios.Add(key.Scenario);
                    }
                }
            }

            return scenarios;
        }

        /// <summary>
        /// Gets statistics for a specific scenario across all data types.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <param name="scenario">The scenario name to filter by.</param>
        /// <returns>Dictionary of data type name to entry count for the scenario.</returns>
        /// <remarks>
        /// <para>
        /// Returns counts only for scenario-aware data types (ArcFlash, ShortCircuit).
        /// Equipment types are NOT included (they don't have scenarios).
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// <code>
        /// var stats = dataSet.GetScenarioStatistics("Main-Min");
        /// // Result: { "ArcFlash": 42, "ShortCircuit": 38 }
        /// </code>
        /// </para>
        /// <para>
        /// <strong>Empty Result:</strong> Returns empty dictionary if scenario doesn't exist.
        /// </para>
        /// </remarks>
        public static Dictionary<string, int> GetScenarioStatistics(this DataSet dataSet, string scenario)
        {
            var stats = new Dictionary<string, int>();

            // ArcFlash statistics
            if (dataSet.ArcFlashEntries != null)
            {
                var count = dataSet.ArcFlashEntries.Keys
                    .Count(k => string.Equals(k.Scenario, scenario, StringComparison.OrdinalIgnoreCase));
                if (count > 0)
                    stats["ArcFlash"] = count;
            }

            // ShortCircuit statistics
            if (dataSet.ShortCircuitEntries != null)
            {
                var count = dataSet.ShortCircuitEntries.Keys
                    .Count(k => string.Equals(k.Scenario, scenario, StringComparison.OrdinalIgnoreCase));
                if (count > 0)
                    stats["ShortCircuit"] = count;
            }

            return stats;
        }

        /// <summary>
        /// Gets aggregate statistics for a specific data type across all scenarios.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <param name="dataTypeName">The data type name (e.g., "ArcFlash", "Bus").</param>
        /// <returns>DataTypeStatistics with total count and per-scenario breakdown.</returns>
        /// <remarks>
        /// <para>
        /// For scenario-aware types (ArcFlash, ShortCircuit):
        /// - Populates Scenarios list with per-scenario counts
        /// - Calculates TotalEntries as sum of scenario counts
        /// - Sets HasUniformScenarios based on count distribution
        /// </para>
        /// <para>
        /// For equipment types (Bus, LVBreaker, etc.):
        /// - Sets TotalEntries to dictionary count
        /// - Scenarios list remains empty
        /// - HasUniformScenarios is always true
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// <code>
        /// var stats = dataSet.GetDataTypeStatistics("ArcFlash");
        /// // Result:
        /// // TotalEntries: 168
        /// // Scenarios: [Main-Min (42), Main-Max (42), Service-Min (42), Service-Max (42)]
        /// // HasUniformScenarios: true
        /// </code>
        /// </para>
        /// </remarks>
        public static DataTypeStatistics GetDataTypeStatistics(this DataSet dataSet, string dataTypeName)
        {
            var stats = new DataTypeStatistics(dataTypeName);

            // Handle scenario-aware data types
            if (dataTypeName.Equals("ArcFlash", StringComparison.OrdinalIgnoreCase))
            {
                if (dataSet.ArcFlashEntries != null)
                {
                    stats.TotalEntries = dataSet.ArcFlashEntries.Count;

                    // Group by scenario
                    var scenarioGroups = dataSet.ArcFlashEntries.Keys
                        .GroupBy(k => k.Scenario, StringComparer.OrdinalIgnoreCase)
                        .OrderBy(g => g.Key);

                    foreach (var group in scenarioGroups)
                    {
                        stats.Scenarios.Add(new ScenarioStatistics(group.Key, group.Count()));
                    }
                }
                return stats;
            }

            if (dataTypeName.Equals("ShortCircuit", StringComparison.OrdinalIgnoreCase))
            {
                if (dataSet.ShortCircuitEntries != null)
                {
                    stats.TotalEntries = dataSet.ShortCircuitEntries.Count;

                    // Group by scenario
                    var scenarioGroups = dataSet.ShortCircuitEntries.Keys
                        .GroupBy(k => k.Scenario, StringComparer.OrdinalIgnoreCase)
                        .OrderBy(g => g.Key);

                    foreach (var group in scenarioGroups)
                    {
                        stats.Scenarios.Add(new ScenarioStatistics(group.Key, group.Count()));
                    }
                }
                return stats;
            }

            // Handle equipment types (no scenarios)
            var propertyName = dataTypeName + "Entries";
            var property = typeof(DataSet).GetProperty(propertyName);
            if (property != null)
            {
                var dictionary = property.GetValue(dataSet);
                if (dictionary != null)
                {
                    // Get count via reflection (works for Dictionary<string, T>)
                    var countProperty = dictionary.GetType().GetProperty("Count");
                    if (countProperty != null)
                    {
                        stats.TotalEntries = (int)countProperty.GetValue(dictionary)!;
                    }
                }
            }

            return stats;
        }

        /// <summary>
        /// Gets statistics for all data types in the DataSet.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <returns>List of DataTypeStatistics for all non-empty data types.</returns>
        /// <remarks>
        /// <para>
        /// Scans all known data types (ArcFlash, ShortCircuit, Bus, LVBreaker, etc.)
        /// and returns statistics only for types with data.
        /// </para>
        /// <para>
        /// <strong>Data Types Scanned:</strong>
        /// - ArcFlash, ShortCircuit (scenario-aware)
        /// - Bus, LVBreaker, Fuse, Cable (equipment)
        /// - All 34 equipment types in DataSet
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// <code>
        /// var allStats = dataSet.GetAllDataTypeStatistics();
        /// // Result: List of statistics for non-empty data types
        /// </code>
        /// </para>
        /// </remarks>
        public static List<DataTypeStatistics> GetAllDataTypeStatistics(this DataSet dataSet)
        {
            var allStats = new List<DataTypeStatistics>();

            // Known data types (from PropertyDiscoveryService pattern)
            var dataTypes = new[]
            {
                // Scenario-aware
                "ArcFlash", "ShortCircuit",
                
                // Equipment (alphabetically)
                "AFD", "ATS", "Battery", "Bus", "Busway", "Cable", "Capacitor",
                "CLReactor", "CT", "Filter", "Fuse", "Generator", "HVBreaker",
                "Inverter", "Load", "LVBreaker", "MCC", "Meter", "Motor", "Panel",
                "Photovoltaic", "POC", "Rectifier", "Relay", "Shunt", "Switch",
                "Transformer2W", "Transformer3W", "TransmissionLine", "UPS",
                "Utility", "ZigzagTransformer"
            };

            foreach (var dataType in dataTypes)
            {
                var stats = dataSet.GetDataTypeStatistics(dataType);
                if (stats.TotalEntries > 0)
                {
                    allStats.Add(stats);
                }
            }

            return allStats;
        }

        /// <summary>
        /// Checks if all scenarios have uniform entry counts across data types.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <returns>True if all scenario-aware data types have uniform counts; false otherwise.</returns>
        /// <remarks>
        /// <para>
        /// Used to detect incomplete or mismatched scenario data.
        /// Expected pattern: All scenarios have the same number of entries for each data type.
        /// </para>
        /// <para>
        /// <strong>Example (Uniform):</strong>
        /// - ArcFlash: Main-Min (42), Main-Max (42), Service-Min (42), Service-Max (42)
        /// - ShortCircuit: Main-Min (38), Main-Max (38), Service-Min (38), Service-Max (38)
        /// Result: TRUE
        /// </para>
        /// <para>
        /// <strong>Example (Non-Uniform):</strong>
        /// - ArcFlash: Main-Min (42), Main-Max (40), Service-Min (42), Service-Max (38)
        /// Result: FALSE (should show warning in UI)
        /// </para>
        /// </remarks>
        public static bool HasUniformScenarios(this DataSet dataSet)
        {
            var arcFlashStats = dataSet.GetDataTypeStatistics("ArcFlash");
            var shortCircuitStats = dataSet.GetDataTypeStatistics("ShortCircuit");

            return arcFlashStats.HasUniformScenarios && shortCircuitStats.HasUniformScenarios;
        }

        /// <summary>
        /// Renames a scenario across all data types in the DataSet.
        /// </summary>
        /// <param name="dataSet">The DataSet to modify.</param>
        /// <param name="oldName">The current scenario name.</param>
        /// <param name="newName">The new scenario name.</param>
        /// <remarks>
        /// <para>
        /// Updates composite keys for ArcFlash and ShortCircuit entries.
        /// Used when users rename scenarios in Composite projects.
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// <code>
        /// dataSet.RenameScenario("Main-Min", "Baseline Scenario");
        /// // All ArcFlash and ShortCircuit entries with scenario "Main-Min"
        /// // now have scenario "Baseline Scenario"
        /// </code>
        /// </para>
        /// <para>
        /// <strong>Case Handling:</strong> Uses case-insensitive match for oldName.
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> This operation modifies the DataSet in place.
        /// Ensure Project.MarkDirty() is called after rename.
        /// </para>
        /// </remarks>
        public static void RenameScenario(this DataSet dataSet, string oldName, string newName)
        {
            // Rename in ArcFlash
            if (dataSet.ArcFlashEntries != null)
            {
                var toRename = dataSet.ArcFlashEntries
                    .Where(kv => string.Equals(kv.Key.Scenario, oldName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var entry in toRename)
                {
                    var oldKey = entry.Key;
                    var newKey = (oldKey.Id, newName);
                    dataSet.ArcFlashEntries.Remove(oldKey);
                    dataSet.ArcFlashEntries[newKey] = entry.Value;
                }
            }

            // Rename in ShortCircuit
            if (dataSet.ShortCircuitEntries != null)
            {
                var toRename = dataSet.ShortCircuitEntries
                    .Where(kv => string.Equals(kv.Key.Scenario, oldName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var entry in toRename)
                {
                    var oldKey = entry.Key;
                    var newKey = (oldKey.Id, oldKey.Bus, newName);
                    dataSet.ShortCircuitEntries.Remove(oldKey);
                    dataSet.ShortCircuitEntries[newKey] = entry.Value;
                }
            }
        }

        /// <summary>
        /// Deletes all entries for a specific scenario.
        /// </summary>
        /// <param name="dataSet">The DataSet to modify.</param>
        /// <param name="scenarioName">The scenario name to delete.</param>
        /// <returns>Number of total entries removed.</returns>
        /// <remarks>
        /// <para>
        /// Removes all ArcFlash and ShortCircuit entries matching the scenario name.
        /// Used in Composite projects when users delete a scenario.
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// <code>
        /// var removed = dataSet.DeleteScenario("Service-Min");
        /// // Result: 80 (42 ArcFlash + 38 ShortCircuit)
        /// </code>
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> This operation is destructive and cannot be undone
        /// (unless implemented via undo/redo system). Confirm with user before calling.
        /// </para>
        /// </remarks>
        public static int DeleteScenario(this DataSet dataSet, string scenarioName)
        {
            int removedCount = 0;

            // Delete from ArcFlash
            if (dataSet.ArcFlashEntries != null)
            {
                var toRemove = dataSet.ArcFlashEntries.Keys
                    .Where(k => string.Equals(k.Scenario, scenarioName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in toRemove)
                {
                    dataSet.ArcFlashEntries.Remove(key);
                    removedCount++;
                }
            }

            // Delete from ShortCircuit
            if (dataSet.ShortCircuitEntries != null)
            {
                var toRemove = dataSet.ShortCircuitEntries.Keys
                    .Where(k => string.Equals(k.Scenario, scenarioName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in toRemove)
                {
                    dataSet.ShortCircuitEntries.Remove(key);
                    removedCount++;
                }
            }

            return removedCount;
        }
    }
}
