using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Import;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services;
using Serilog;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Service for detecting and cleaning up invalid mappings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Invalid mappings occur when:
    /// - A mapping points to a property that has been hidden via settings
    /// - A property was removed from the target class (rare)
    /// - Property discovery service can't find the target property
    /// </para>
    /// <para>
    /// This service helps maintain mapping integrity when users change
    /// property visibility settings.
    /// </para>
    /// </remarks>
    public class InvalidMappingDetector
    {
        private readonly IPropertyDiscoveryService _propertyDiscovery;

        public InvalidMappingDetector(IPropertyDiscoveryService propertyDiscovery)
        {
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
        }

        /// <summary>
        /// Finds all mappings that reference properties that are no longer visible.
        /// </summary>
        /// <param name="document">The map document to analyze.</param>
        /// <returns>Dictionary of data type to list of invalid mappings.</returns>
        public Dictionary<string, List<MappingEntry>> FindInvalidMappings(MapDocument document)
        {
            var invalidMappings = new Dictionary<string, List<MappingEntry>>();

            try
            {
                // Check each data type's mappings
                foreach (var dataTypeMappings in document.MappingsByDataType)
                {
                    var dataType = dataTypeMappings.Key;
                    var mappings = dataTypeMappings.Value;

                    // Get currently visible properties for this data type
                    var visibleProperties = _propertyDiscovery.GetPropertiesForType(dataType);
                    var visiblePropertyNames = new HashSet<string>(
                        visibleProperties.Select(p => p.PropertyName),
                        StringComparer.OrdinalIgnoreCase);

                    Log.Debug("Data type {DataType} has {VisibleCount} visible properties, {MappingCount} mappings",
                        dataType, visibleProperties.Count, mappings.Count);

                    // Find mappings that reference properties NOT in the visible list
                    var affectedMappings = mappings
                        .Where(m => !visiblePropertyNames.Contains(m.PropertyName))
                        .ToList();

                    if (affectedMappings.Any())
                    {
                        invalidMappings[dataType] = affectedMappings;
                        Log.Information("Found {Count} invalid mappings for {DataType} (properties no longer visible)",
                            affectedMappings.Count, dataType);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to detect invalid mappings");
            }

            return invalidMappings;
        }

        /// <summary>
        /// Removes the specified invalid mappings from the document.
        /// </summary>
        /// <param name="document">The map document to clean.</param>
        /// <param name="invalidMappings">Dictionary of data type to mappings to remove.</param>
        /// <returns>Total number of mappings removed.</returns>
        public int RemoveInvalidMappings(
            MapDocument document,
            Dictionary<string, List<MappingEntry>> invalidMappings)
        {
            int totalRemoved = 0;

            foreach (var dataTypeMappings in invalidMappings)
            {
                var dataType = dataTypeMappings.Key;
                var mappingsToRemove = dataTypeMappings.Value;

                foreach (var mapping in mappingsToRemove)
                {
                    document.RemoveMapping(dataType, mapping.PropertyName);
                    totalRemoved++;
                }

                Log.Information("Removed {Count} invalid mappings from {DataType}",
                    mappingsToRemove.Count, dataType);
            }

            return totalRemoved;
        }

        /// <summary>
        /// Gets a human-readable summary of invalid mappings.
        /// </summary>
        /// <param name="invalidMappings">Dictionary of data type to invalid mappings.</param>
        /// <returns>Formatted string describing the invalid mappings.</returns>
        public string GetInvalidMappingsSummary(Dictionary<string, List<MappingEntry>> invalidMappings)
        {
            if (!invalidMappings.Any())
            {
                return "No invalid mappings found.";
            }

            var summary = new System.Text.StringBuilder();
            summary.AppendLine("The following mappings reference hidden properties:\n");

            foreach (var dataTypeMappings in invalidMappings.OrderBy(kvp => kvp.Key))
            {
                var dataType = dataTypeMappings.Key;
                var mappings = dataTypeMappings.Value;

                summary.AppendLine($"{dataType}:");
                foreach (var mapping in mappings.OrderBy(m => m.PropertyName))
                {
                    summary.AppendLine($"  • {mapping.ColumnHeader} ? {mapping.PropertyName} (hidden)");
                }
                summary.AppendLine();
            }

            return summary.ToString();
        }
    }
}
