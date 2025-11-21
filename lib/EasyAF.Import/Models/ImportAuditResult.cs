using System.Collections.Generic;
using System.Linq;

namespace EasyAF.Import.Models
{
    /// <summary>
    /// Results from auditing a file before import.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides preview information about what will be imported:
    /// - Discovered scenarios
    /// - Data type counts
    /// - Validation warnings/errors
    /// </para>
    /// <para>
    /// Used by import dialogs to show preview before committing import.
    /// </para>
    /// </remarks>
    public class ImportAuditResult
    {
        /// <summary>
        /// Path to the audited file.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Detected data types in the file (e.g., "ArcFlash", "ShortCircuit").
        /// </summary>
        /// <remarks>
        /// Determined by examining mapped column headers and target types in mapping config.
        /// </remarks>
        public List<string> DetectedDataTypes { get; set; } = new List<string>();

        /// <summary>
        /// Discovered scenario names (empty for equipment data types).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Extracted from "Scenario" column values for ArcFlash and ShortCircuit.
        /// Equipment types (Bus, LVBreaker, etc.) will have empty list.
        /// </para>
        /// <para>
        /// Case-insensitive unique values.
        /// </para>
        /// </remarks>
        public List<string> DiscoveredScenarios { get; set; } = new List<string>();

        /// <summary>
        /// Per-data-type entry counts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example:
        /// <code>
        /// {
        ///   "ArcFlash": 168,  // 4 scenarios × 42 entries each
        ///   "ShortCircuit": 152  // 4 scenarios × 38 entries each
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public Dictionary<string, int> DataTypeCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Per-scenario entry counts (for scenario-aware data types).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example:
        /// <code>
        /// {
        ///   "Main-Min": 80,    // 42 ArcFlash + 38 ShortCircuit
        ///   "Main-Max": 80,
        ///   "Service-Min": 80,
        ///   "Service-Max": 80
        /// }
        /// </code>
        /// </para>
        /// <para>
        /// Empty for equipment-only files (Bus, LVBreaker, etc.).
        /// </para>
        /// </remarks>
        public Dictionary<string, int> ScenarioCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Total number of rows in the file.
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of rows that will be imported (after filtering blank rows, etc.).
        /// </summary>
        public int ValidRows { get; set; }

        /// <summary>
        /// Validation warnings (non-fatal issues).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Examples:
        /// - Missing optional columns
        /// - Blank values in non-required fields
        /// - Inconsistent data formats
        /// </para>
        /// </remarks>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Validation errors (fatal issues that prevent import).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Examples:
        /// - Missing required columns
        /// - Invalid data types
        /// - Mapping configuration errors
        /// </para>
        /// </remarks>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Indicates whether the file can be imported (no fatal errors).
        /// </summary>
        public bool CanImport => Errors.Count == 0;

        /// <summary>
        /// Indicates whether the file contains scenario-aware data.
        /// </summary>
        public bool HasScenarios => DiscoveredScenarios.Count > 0;

        /// <summary>
        /// Indicates whether all scenarios have uniform entry counts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// TRUE: All scenarios have the same number of entries (expected).
        /// FALSE: Scenarios have different counts (may indicate incomplete data).
        /// </para>
        /// </remarks>
        public bool HasUniformScenarios
        {
            get
            {
                if (ScenarioCounts.Count <= 1) return true;
                return ScenarioCounts.Values.Distinct().Count() == 1;
            }
        }

        /// <summary>
        /// Summary message for display in UI.
        /// </summary>
        public string Summary
        {
            get
            {
                if (!CanImport)
                    return $"Cannot import: {Errors.Count} error(s)";

                if (HasScenarios)
                    return $"{ValidRows} rows, {DiscoveredScenarios.Count} scenario(s)";

                return $"{ValidRows} rows, {DataTypeCounts.Count} data type(s)";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportAuditResult"/> class.
        /// </summary>
        public ImportAuditResult() { }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString() => Summary;
    }
}
