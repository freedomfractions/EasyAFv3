using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Import;
using EasyAF.Modules.Map.Models;
using Serilog;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Service for detecting and cleaning up orphaned mappings.
    /// </summary>
    /// <remarks>
    /// Orphaned mappings occur when:
    /// - A sample file is removed but mappings to its columns remain
    /// - A table/sheet is no longer available but mappings reference it
    /// - File paths change and columns can't be located
    /// </remarks>
    public class OrphanedMappingDetector
    {
        private readonly ColumnExtractionService _columnExtraction;

        public OrphanedMappingDetector()
        {
            _columnExtraction = new ColumnExtractionService();
        }

        /// <summary>
        /// Finds all mappings that will become orphaned if the specified file is removed.
        /// </summary>
        /// <param name="document">The map document to analyze.</param>
        /// <param name="filePathToRemove">The file path that will be removed.</param>
        /// <returns>Dictionary of data type to list of affected mappings.</returns>
        public Dictionary<string, List<MappingEntry>> FindOrphanedMappings(
            MapDocument document, 
            string filePathToRemove)
        {
            var orphanedMappings = new Dictionary<string, List<MappingEntry>>();

            try
            {
                // Extract columns from the file being removed
                var columnsInFile = _columnExtraction.ExtractColumns(filePathToRemove);
                var allColumnsInFile = new HashSet<string>(
                    columnsInFile.Values.SelectMany(cols => cols.Select(c => c.ColumnName)),
                    StringComparer.OrdinalIgnoreCase);

                Log.Debug("File {FilePath} contains {Count} unique columns", 
                    filePathToRemove, allColumnsInFile.Count);

                // Check each data type's mappings
                foreach (var dataTypeMappings in document.MappingsByDataType)
                {
                    var dataType = dataTypeMappings.Key;
                    var mappings = dataTypeMappings.Value;

                    // Find mappings that reference columns in the file being removed
                    var affectedMappings = mappings
                        .Where(m => allColumnsInFile.Contains(m.ColumnHeader))
                        .ToList();

                    if (affectedMappings.Any())
                    {
                        orphanedMappings[dataType] = affectedMappings;
                        Log.Information("Found {Count} orphaned mappings for {DataType} in file {FilePath}",
                            affectedMappings.Count, dataType, filePathToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to analyze file {FilePath} for orphaned mappings", filePathToRemove);
            }

            return orphanedMappings;
        }

        /// <summary>
        /// Removes the specified orphaned mappings from the document.
        /// </summary>
        /// <param name="document">The map document to clean.</param>
        /// <param name="orphanedMappings">Dictionary of data type to mappings to remove.</param>
        /// <returns>Total number of mappings removed.</returns>
        public int RemoveOrphanedMappings(
            MapDocument document,
            Dictionary<string, List<MappingEntry>> orphanedMappings)
        {
            int totalRemoved = 0;

            foreach (var dataTypeMappings in orphanedMappings)
            {
                var dataType = dataTypeMappings.Key;
                var mappingsToRemove = dataTypeMappings.Value;

                foreach (var mapping in mappingsToRemove)
                {
                    document.RemoveMapping(dataType, mapping.PropertyName);
                    totalRemoved++;
                }

                Log.Information("Removed {Count} orphaned mappings from {DataType}",
                    mappingsToRemove.Count, dataType);
            }

            return totalRemoved;
        }

        /// <summary>
        /// Gets a human-readable summary of orphaned mappings.
        /// </summary>
        /// <param name="orphanedMappings">Dictionary of data type to affected mappings.</param>
        /// <returns>Formatted string describing the orphaned mappings.</returns>
        public string GetOrphanedMappingsSummary(Dictionary<string, List<MappingEntry>> orphanedMappings)
        {
            if (!orphanedMappings.Any())
            {
                return "No orphaned mappings found.";
            }

            var summary = new System.Text.StringBuilder();
            summary.AppendLine("The file you're removing has active mappings:\n");

            foreach (var dataTypeMappings in orphanedMappings.OrderBy(kvp => kvp.Key))
            {
                var dataType = dataTypeMappings.Key;
                var mappings = dataTypeMappings.Value;

                summary.AppendLine($"{dataType}:");
                foreach (var mapping in mappings.OrderBy(m => m.PropertyName))
                {
                    summary.AppendLine($"  • {mapping.ColumnHeader} ? {mapping.PropertyName}");
                }
                summary.AppendLine();
            }

            return summary.ToString();
        }
    }
}
