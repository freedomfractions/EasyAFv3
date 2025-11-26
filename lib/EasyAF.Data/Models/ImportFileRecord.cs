using System;
using System.Collections.Generic;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Records metadata about a file imported into the project.
    /// </summary>
    public class ImportFileRecord
    {
        /// <summary>
        /// Full path of the imported file.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Friendly filename for display (without path).
        /// </summary>
        public string FileName => System.IO.Path.GetFileName(FilePath);

        /// <summary>
        /// Timestamp when this file was imported (UTC).
        /// </summary>
        public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Data types imported from this file (e.g., "ArcFlash", "ShortCircuit", "Bus").
        /// </summary>
        public List<string> DataTypes { get; set; } = new();

        /// <summary>
        /// Scenario mappings from this file.
        /// Key: Original scenario name from file
        /// Value: Target scenario name (after renaming)
        /// Empty if file contains no scenarios (e.g., Bus-only file).
        /// </summary>
        public Dictionary<string, string> ScenarioMappings { get; set; } = new();

        /// <summary>
        /// Whether this file was imported into New Data (true) or Old Data (false).
        /// </summary>
        public bool IsNewData { get; set; } = true;

        /// <summary>
        /// Number of entries imported from this file.
        /// </summary>
        public int EntryCount { get; set; }

        /// <summary>
        /// Path to the mapping file (.ezmap) used for this import.
        /// </summary>
        public string? MappingPath { get; set; }

        /// <summary>
        /// Creates a new import file record.
        /// </summary>
        public ImportFileRecord() { }

        /// <summary>
        /// Creates a new import file record with specified values.
        /// </summary>
        public ImportFileRecord(string filePath, bool isNewData, List<string> dataTypes, Dictionary<string, string> scenarioMappings, int entryCount)
        {
            FilePath = filePath;
            IsNewData = isNewData;
            DataTypes = dataTypes;
            ScenarioMappings = scenarioMappings;
            EntryCount = entryCount;
        }

        /// <summary>
        /// Gets a display-friendly summary of scenario mappings.
        /// </summary>
        public string GetScenarioMappingsSummary()
        {
            if (ScenarioMappings.Count == 0)
                return "(no scenarios)";

            var mappings = new List<string>();
            foreach (var kvp in ScenarioMappings)
            {
                if (kvp.Key == kvp.Value)
                    mappings.Add(kvp.Key); // No rename
                else
                    mappings.Add($"{kvp.Key} ? {kvp.Value}"); // Renamed
            }

            return string.Join(", ", mappings);
        }
    }
}
