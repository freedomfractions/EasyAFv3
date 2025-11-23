using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyAF.Modules.Project.Models
{
    /// <summary>
    /// Tracks metadata about a data import operation.
    /// </summary>
    /// <remarks>
    /// Provides audit trail for imports, useful for debugging and understanding
    /// where data came from.
    /// </remarks>
    public class ImportSession
    {
        /// <summary>
        /// Gets or sets when the import occurred (UTC).
        /// </summary>
        public DateTime ImportedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the source file name(s) that were imported.
        /// </summary>
        /// <remarks>
        /// Multiple files separated by semicolons for batch imports.
        /// </remarks>
        public string SourceFiles { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the count of entries imported per data type.
        /// </summary>
        /// <remarks>
        /// Key = Data type name (e.g., "ArcFlash", "Bus", "LVCB")
        /// Value = Number of entries imported
        /// </remarks>
        public Dictionary<string, int> ImportedCounts { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of scenarios discovered/imported (for composite models).
        /// </summary>
        /// <remarks>
        /// Empty for standard model imports.
        /// </remarks>
        public List<string> Scenarios { get; set; } = new();

        /// <summary>
        /// Gets or sets whether this was imported into New Data (true) or Old Data (false).
        /// </summary>
        public bool IsNewData { get; set; }

        /// <summary>
        /// Gets or sets the mapping file used for this import.
        /// </summary>
        public string? MappingFile { get; set; }

        /// <summary>
        /// Gets or sets any warnings that occurred during import.
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Gets or sets any errors that occurred during import.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Gets a friendly display string for the import session.
        /// </summary>
        public string DisplaySummary
        {
            get
            {
                var timestamp = ImportedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                var target = IsNewData ? "New Data" : "Old Data";
                var fileCount = SourceFiles.Split(';', StringSplitOptions.RemoveEmptyEntries).Length;
                var totalCount = ImportedCounts.Values.Sum();
                
                return $"{timestamp} - {fileCount} file(s) ? {target} ({totalCount} entries)";
            }
        }

        /// <summary>
        /// Gets a detailed multi-line summary for logging.
        /// </summary>
        public string DetailedSummary
        {
            get
            {
                var lines = new List<string>
                {
                    $"Import Session - {ImportedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}",
                    $"Target: {(IsNewData ? "New Data" : "Old Data")}",
                    $"Files: {SourceFiles}",
                    $"Mapping: {MappingFile ?? "(none)"}"
                };

                if (Scenarios.Count > 0)
                {
                    lines.Add($"Scenarios: {string.Join(", ", Scenarios)}");
                }

                if (ImportedCounts.Count > 0)
                {
                    lines.Add("Imported:");
                    foreach (var kvp in ImportedCounts.OrderBy(x => x.Key))
                    {
                        lines.Add($"  • {kvp.Key}: {kvp.Value}");
                    }
                }

                if (Warnings.Count > 0)
                {
                    lines.Add($"Warnings: {Warnings.Count}");
                }

                if (Errors.Count > 0)
                {
                    lines.Add($"Errors: {Errors.Count}");
                }

                return string.Join(Environment.NewLine, lines);
            }
        }
    }
}
