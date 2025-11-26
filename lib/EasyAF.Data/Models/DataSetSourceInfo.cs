using System.Collections.Generic;

namespace EasyAF.Data.Models
{
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
        /// Inner Key: Scenario name
        /// Value: File path of the last import for this type+scenario combination
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> CompositeDataTypeSources { get; set; } = new();

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
