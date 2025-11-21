using System.Collections.Generic;
using System.Linq;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Statistics for a specific scenario within a data type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to display per-scenario entry counts in the Project Summary tree view.
    /// Example:
    /// <code>
    /// Arc Flash
    ///   ? Main-Min (42 entries)
    ///   ? Main-Max (42 entries)
    ///   ? Service-Min (38 entries)
    /// </code>
    /// </para>
    /// <para>
    /// Only applicable to scenario-aware data types (ArcFlash, ShortCircuit).
    /// Equipment types do NOT have per-scenario statistics.
    /// </para>
    /// </remarks>
    public class ScenarioStatistics
    {
        /// <summary>
        /// Scenario name (e.g., "Main-Min", "Main-Max", "Service-Min").
        /// </summary>
        /// <remarks>
        /// <para>
        /// Scenario names come from the imported data (Scenario column).
        /// For Composite projects, users can manually assign/rename scenarios.
        /// For Standard projects, scenarios are auto-detected from the file.
        /// </para>
        /// <para>
        /// Case-sensitive comparison recommended (though UI may normalize).
        /// </para>
        /// </remarks>
        public string ScenarioName { get; set; } = string.Empty;

        /// <summary>
        /// Number of entries for this scenario.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For ArcFlash: Number of (Id, Scenario) tuples matching this scenario.
        /// For ShortCircuit: Number of (Id, Bus, Scenario) tuples matching this scenario.
        /// </para>
        /// <para>
        /// Example:
        /// - ArcFlash entries for "Main-Min" scenario: 42
        /// - ShortCircuit entries for "Main-Min" scenario: 38
        /// </para>
        /// </remarks>
        public int EntryCount { get; set; } = 0;

        /// <summary>
        /// Sort priority for display (0 = highest).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used to control display order in tree view:
        /// - Standard order: Main-Min, Main-Max, Service-Min, Service-Max
        /// - Alphabetical: For user-defined scenario names
        /// </para>
        /// <para>
        /// Default: 0 (no special priority, alphabetical sort).
        /// UI can assign priorities based on naming conventions.
        /// </para>
        /// </remarks>
        public int SortPriority { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioStatistics"/> class.
        /// </summary>
        public ScenarioStatistics() { }

        /// <summary>
        /// Initializes a new instance with scenario name and count.
        /// </summary>
        public ScenarioStatistics(string scenarioName, int entryCount)
        {
            ScenarioName = scenarioName;
            EntryCount = entryCount;
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString() => $"{ScenarioName}: {EntryCount} entries";
    }

    /// <summary>
    /// Aggregate statistics for a data type across all scenarios.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to display data type statistics in Project Summary tree view.
    /// For scenario-aware types: Shows per-scenario breakdown + total.
    /// For equipment types: Shows total count only.
    /// </para>
    /// <para>
    /// Example UI rendering:
    /// <code>
    /// Arc Flash (168 entries total) [? Mixed counts]
    ///   ? Main-Min (42 entries)
    ///   ? Main-Max (42 entries)
    ///   ? Service-Min (42 entries)
    ///   ? Service-Max (42 entries)
    /// 
    /// Buses (15 entries)  [no scenarios]
    /// </code>
    /// </para>
    /// </remarks>
    public class DataTypeStatistics
    {
        /// <summary>
        /// Data type name (e.g., "ArcFlash", "Bus").
        /// </summary>
        public string DataTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Total number of entries across all scenarios.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For scenario-aware types: Sum of all scenario entry counts.
        /// For equipment types: Direct count from dictionary.
        /// </para>
        /// <para>
        /// Example:
        /// - ArcFlash with 4 scenarios (42 each) = 168 total
        /// - Buses (no scenarios) = 15 total
        /// </para>
        /// </remarks>
        public int TotalEntries { get; set; } = 0;

        /// <summary>
        /// Per-scenario statistics (empty for equipment types).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Populated only for scenario-aware data types (ArcFlash, ShortCircuit).
        /// Equipment types will have an empty list.
        /// </para>
        /// <para>
        /// Sorted by SortPriority, then ScenarioName alphabetically.
        /// </para>
        /// </remarks>
        public List<ScenarioStatistics> Scenarios { get; set; } = new List<ScenarioStatistics>();

        /// <summary>
        /// Indicates whether all scenarios have the same entry count (uniform distribution).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>TRUE:</strong> All scenarios have identical counts (expected for most projects).
        /// Example: Main-Min (42), Main-Max (42), Service-Min (42), Service-Max (42)
        /// </para>
        /// <para>
        /// <strong>FALSE:</strong> Scenarios have different counts (may indicate incomplete data).
        /// Example: Main-Min (42), Main-Max (38), Service-Min (40), Service-Max (44)
        /// UI should show warning icon (?) when non-uniform.
        /// </para>
        /// <para>
        /// Always TRUE for equipment types (no scenarios).
        /// </para>
        /// </remarks>
        public bool HasUniformScenarios
        {
            get
            {
                if (Scenarios.Count <= 1) return true;
                return Scenarios.Select(s => s.EntryCount).Distinct().Count() == 1;
            }
        }

        /// <summary>
        /// Returns formatted statistics display string.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Examples:
        /// - Uniform: "42 entries" (all scenarios same)
        /// - Non-uniform: "? Mixed" (scenarios differ)
        /// - No data: "0 entries"
        /// </para>
        /// </remarks>
        public string StatisticsDisplay
        {
            get
            {
                if (TotalEntries == 0) return "0 entries";
                if (Scenarios.Count == 0) return $"{TotalEntries} entries";
                if (HasUniformScenarios) return $"{Scenarios[0].EntryCount} entries";
                return "? Mixed";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeStatistics"/> class.
        /// </summary>
        public DataTypeStatistics() { }

        /// <summary>
        /// Initializes a new instance with data type name.
        /// </summary>
        public DataTypeStatistics(string dataTypeName)
        {
            DataTypeName = dataTypeName;
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString() => $"{DataTypeName}: {TotalEntries} entries ({Scenarios.Count} scenarios)";
    }
}
