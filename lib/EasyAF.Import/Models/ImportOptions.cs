using System.Collections.Generic;

namespace EasyAF.Import.Models
{
    /// <summary>
    /// Configuration options for data import operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Controls parsing behavior and data validation during import.
    /// Used by ImportManager to customize import behavior.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// <code>
    /// var options = new ImportOptions
    /// {
    ///     SkipBlankRows = true,
    ///     TrimWhitespace = true,
    ///     ScenarioOverrides = new Dictionary&lt;string, string&gt;
    ///     {
    ///         { "Main-Min", "Baseline Scenario" }
    ///     }
    /// };
    /// importManager.Import(filePath, mapping, dataSet, options);
    /// </code>
    /// </para>
    /// </remarks>
    public class ImportOptions
    {
        /// <summary>
        /// Skip rows where all mapped columns are blank.
        /// </summary>
        /// <remarks>
        /// Default: true (recommended for CSV files with trailing blank rows).
        /// </remarks>
        public bool SkipBlankRows { get; set; } = true;

        /// <summary>
        /// Trim leading/trailing whitespace from all values.
        /// </summary>
        /// <remarks>
        /// Default: true (recommended for Excel exports with extra spaces).
        /// </remarks>
        public bool TrimWhitespace { get; set; } = true;

        /// <summary>
        /// Stop import on first validation error.
        /// </summary>
        /// <remarks>
        /// Default: false (collects all errors for batch reporting).
        /// Set to true for strict validation mode.
        /// </remarks>
        public bool StopOnFirstError { get; set; } = false;

        /// <summary>
        /// Dictionary of scenario name overrides (originalName ? newName).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used to rename scenarios during import (Composite projects).
        /// Example: File contains "Main-Min", user wants "Baseline Scenario".
        /// </para>
        /// <para>
        /// Only applicable to scenario-aware data types (ArcFlash, ShortCircuit).
        /// </para>
        /// </remarks>
        public Dictionary<string, string>? ScenarioOverrides { get; set; }

        /// <summary>
        /// List of specific scenarios to import (null = import all).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used for selective scenario import from multi-scenario files.
        /// Example: File contains 4 scenarios, user only wants 2.
        /// </para>
        /// <para>
        /// If null or empty, all scenarios are imported.
        /// If populated, only listed scenarios are imported (others are skipped).
        /// </para>
        /// </remarks>
        public List<string>? SelectedScenarios { get; set; }

        /// <summary>
        /// Merge strategy when scenario already exists in target DataSet.
        /// </summary>
        /// <remarks>
        /// Default: Replace (overwrite existing data for scenario).
        /// </remarks>
        public ScenarioMergeStrategy MergeStrategy { get; set; } = ScenarioMergeStrategy.Replace;

        /// <summary>
        /// Initializes a new instance with default options.
        /// </summary>
        public ImportOptions() { }
    }

    /// <summary>
    /// Strategy for handling scenario name collisions during import.
    /// </summary>
    public enum ScenarioMergeStrategy
    {
        /// <summary>
        /// Replace existing scenario data with imported data.
        /// </summary>
        /// <remarks>
        /// Default behavior. Overwrites all entries for the scenario.
        /// </remarks>
        Replace,

        /// <summary>
        /// Skip importing scenarios that already exist.
        /// </summary>
        /// <remarks>
        /// Preserves existing data, only imports new scenarios.
        /// </remarks>
        SkipExisting,

        /// <summary>
        /// Merge imported data with existing (add new entries, keep existing).
        /// </summary>
        /// <remarks>
        /// Complex strategy: Adds entries with new IDs, keeps entries with existing IDs.
        /// Use with caution (may result in duplicate or stale data).
        /// </remarks>
        Merge
    }
}
