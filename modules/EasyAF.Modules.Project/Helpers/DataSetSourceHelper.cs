using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyAF.Data.Models;
using Serilog;

namespace EasyAF.Modules.Project.Helpers
{
    /// <summary>
    /// Helper class for working with DataSet source tracking.
    /// Provides reflection-based utilities to detect composite types and manage source information.
    /// </summary>
    public static class DataSetSourceHelper
    {
        // Cache for composite type detection to avoid repeated reflection
        private static readonly Dictionary<Type, bool> _compositeTypeCache = new();
        
        // Cache for DataSet entry properties
        private static PropertyInfo[]? _dataSetProperties;

        /// <summary>
        /// Determines if an entry type is composite (has a Scenario property).
        /// Uses caching to avoid repeated reflection calls.
        /// </summary>
        /// <param name="entryType">The entry type to check (e.g., typeof(ArcFlash)).</param>
        /// <returns>True if the type has a Scenario property, false otherwise.</returns>
        public static bool IsCompositeType(Type entryType)
        {
            if (entryType == null)
                return false;

            if (_compositeTypeCache.TryGetValue(entryType, out var isComposite))
                return isComposite;

            // Check if type has a "Scenario" property
            var hasScenario = entryType.GetProperty("Scenario", BindingFlags.Public | BindingFlags.Instance) != null;
            _compositeTypeCache[entryType] = hasScenario;

            return hasScenario;
        }

        /// <summary>
        /// Gets all entry collection properties from a DataSet.
        /// Properties ending with "Entries" are considered entry collections.
        /// </summary>
        /// <returns>Array of PropertyInfo for all entry collections.</returns>
        public static PropertyInfo[] GetDataSetEntryProperties()
        {
            if (_dataSetProperties != null)
                return _dataSetProperties;

            _dataSetProperties = typeof(DataSet)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name.EndsWith("Entries") && p.PropertyType.IsGenericType)
                .ToArray();

            return _dataSetProperties;
        }

        /// <summary>
        /// Gets the element type of a DataSet entry collection.
        /// For example, Dictionary&lt;CompositeKey, ArcFlash&gt; returns typeof(ArcFlash).
        /// </summary>
        /// <param name="property">The property representing an entry collection.</param>
        /// <returns>The entry type, or null if unable to determine.</returns>
        public static Type? GetEntryType(PropertyInfo property)
        {
            if (!property.PropertyType.IsGenericType)
                return null;

            var genericArgs = property.PropertyType.GetGenericArguments();
            
            // Dictionary<CompositeKey, TEntry> - get TEntry (second argument)
            if (genericArgs.Length >= 2)
                return genericArgs[1];

            return null;
        }

        /// <summary>
        /// Updates source tracking information after an import operation.
        /// Scans the dataset to determine what data came from which file.
        /// </summary>
        /// <param name="dataSet">The dataset to scan.</param>
        /// <param name="sourceInfo">The source tracking object to update.</param>
        /// <param name="filePath">The source file path.</param>
        public static void UpdateSourceTracking(DataSet dataSet, DataSetSourceInfo sourceInfo, string filePath)
        {
            if (dataSet == null || sourceInfo == null || string.IsNullOrWhiteSpace(filePath))
                return;

            foreach (var property in GetDataSetEntryProperties())
            {
                var collection = property.GetValue(dataSet);
                if (collection == null)
                    continue;

                // Get count via reflection (works for Dictionary<,>)
                var countProperty = collection.GetType().GetProperty("Count");
                if (countProperty == null)
                    continue;

                var count = (int?)countProperty.GetValue(collection);
                if (count == null || count == 0)
                    continue;

                // Get entry type to check if composite
                var entryType = GetEntryType(property);
                if (entryType == null)
                    continue;

                var propertyName = property.Name;

                if (IsCompositeType(entryType))
                {
                    // Composite type - track per scenario
                    UpdateCompositeSourceTracking(collection, sourceInfo, propertyName, filePath);
                }
                else
                {
                    // Non-composite type - simple source tracking
                    sourceInfo.DataTypeSources[propertyName] = filePath;
                    Log.Debug("Source tracking: {PropertyName} ? {File}", propertyName, System.IO.Path.GetFileName(filePath));
                }
            }
        }

        /// <summary>
        /// Updates source tracking for composite types (entries with scenarios).
        /// Extracts scenario information from the collection.
        /// </summary>
        private static void UpdateCompositeSourceTracking(object collection, DataSetSourceInfo sourceInfo, string propertyName, string filePath)
        {
            // Get the dictionary values (entries)
            var valuesProperty = collection.GetType().GetProperty("Values");
            if (valuesProperty == null)
                return;

            var values = valuesProperty.GetValue(collection) as System.Collections.IEnumerable;
            if (values == null)
                return;

            // Extract unique scenarios
            var scenarios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var entry in values)
            {
                if (entry == null)
                    continue;

                var scenarioProperty = entry.GetType().GetProperty("Scenario");
                if (scenarioProperty == null)
                    continue;

                var scenario = scenarioProperty.GetValue(entry) as string;
                if (!string.IsNullOrWhiteSpace(scenario))
                    scenarios.Add(scenario);
            }

            // Ensure nested dictionary exists
            if (!sourceInfo.CompositeDataTypeSources.ContainsKey(propertyName))
                sourceInfo.CompositeDataTypeSources[propertyName] = new Dictionary<string, string>();

            // Update source for each scenario
            foreach (var scenario in scenarios)
            {
                sourceInfo.CompositeDataTypeSources[propertyName][scenario] = filePath;
                Log.Debug("Source tracking: {PropertyName}[{Scenario}] ? {File}", 
                    propertyName, scenario, System.IO.Path.GetFileName(filePath));
            }
        }

        /// <summary>
        /// Gets a user-friendly display name for a data type property.
        /// Converts "ArcFlashEntries" to "Arc Flash".
        /// </summary>
        /// <param name="propertyName">The property name (e.g., "ArcFlashEntries").</param>
        /// <returns>Friendly display name.</returns>
        public static string GetFriendlyDataTypeName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return propertyName;

            // Remove "Entries" suffix
            var name = propertyName.EndsWith("Entries") 
                ? propertyName.Substring(0, propertyName.Length - "Entries".Length) 
                : propertyName;

            // Insert spaces before capital letters (camelCase to Title Case)
            return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
