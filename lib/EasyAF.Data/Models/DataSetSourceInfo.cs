using System.Collections.Generic;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents a scenario source with optional renaming information.
    /// </summary>
    public class ScenarioSource
    {
        /// <summary>
        /// The file path that was the source of this scenario.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The original scenario name in the source file (before any renaming).
        /// </summary>
        public string? OriginalScenario { get; set; }

        /// <summary>
        /// The target scenario name (after renaming, or same as original if not renamed).
        /// </summary>
        public string TargetScenario { get; set; } = string.Empty;

        /// <summary>
        /// Whether this scenario was renamed during import.
        /// </summary>
        public bool WasRenamed => OriginalScenario != null && OriginalScenario != TargetScenario;
    }

    /// <summary>
    /// Tracks the source file paths for each data type and scenario in a dataset.
    /// Uses reflection-based detection to determine composite vs non-composite types.
    /// Last-writer-wins: Only the most recent source file is tracked per type/scenario.
    /// </summary>
    public class DataSetSourceInfo
    {
        /// <summary>
        /// Tracks source files for non-composite data types (no Scenario property).
        /// Key: DataSet property name (e.g., "BusEntries", "LVBreakerEntries")
        /// Value: File path of the last import for this type
        /// </summary>
        public Dictionary<string, string> DataTypeSources { get; set; } = new();

        /// <summary>
        /// Tracks source files for composite data types (have Scenario property).
        /// Outer Key: DataSet property name (e.g., "ArcFlashEntries", "ShortCircuitEntries")
        /// Inner Key: Target scenario name (after any renaming)
        /// Value: Source information including file path and original/target scenario names
        /// </summary>
        public Dictionary<string, Dictionary<string, ScenarioSource>> CompositeDataTypeSources { get; set; } = new();

        /// <summary>
        /// Clears all source tracking information.
        /// </summary>
        public void Clear()
        {
            DataTypeSources.Clear();
            CompositeDataTypeSources.Clear();
        }
    }
}
