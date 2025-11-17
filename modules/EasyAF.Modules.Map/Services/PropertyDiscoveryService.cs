using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyAF.Modules.Map.Models;
using EasyAF.Data.Models;
using EasyAF.Core.Contracts;
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Service for discovering properties on data types that can be mapped.
    /// </summary>
    /// <remarks>
    /// This service uses reflection to discover public properties on classes in the
    /// EasyAF.Data.Models namespace. It also applies settings-based filtering to hide
    /// properties the user doesn't want to see.
    /// </remarks>
    public class PropertyDiscoveryService : IPropertyDiscoveryService
    {
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<string, List<MapPropertyInfo>> _propertyCache;

        // CROSS-MODULE EDIT: 2025-01-16 Required Property Validation
        // Modified for: Define universal and type-specific required properties
        // Related modules: Map (PropertyInfo model)
        // Rollback instructions: Remove UniversalRequiredProperties and DefaultRequiredProperties constants
        
        /// <summary>
        /// Properties that are required for ALL data types.
        /// </summary>
        /// <remarks>
        /// These are foundational properties that every EasyAF data type must have mapped:
        /// - Id: Primary key for uniquely identifying equipment
        /// - Name: Human-readable identifier for equipment
        /// </remarks>
        private static readonly string[] UniversalRequiredProperties = { "Id", "Name" };

        /// <summary>
        /// Type-specific required properties beyond the universal ones.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These properties are critical for specific data types:
        /// - Bus.kV: Voltage rating required for electrical calculations
        /// - LVCB.Type: Breaker type affects protection logic
        /// - ArcFlash.Scenario: Required for multi-scenario differentiation
        /// - ArcFlash.AFBoundary: Arc flash boundary is safety-critical
        /// - ShortCircuit.Bus: Location reference required for composite key
        /// - ShortCircuit.Scenario: Required for multi-scenario differentiation
        /// </para>
        /// <para>
        /// Future Enhancement: Allow settings.json to override these defaults:
        /// {
        ///   "Map": {
        ///     "RequiredProperties": {
        ///       "Bus": ["Id", "Name", "kV", "CustomField"]
        ///     }
        ///   }
        /// }
        /// </para>
        /// </remarks>
        private static readonly Dictionary<string, string[]> DefaultRequiredProperties = new()
        {
            { "Bus", new[] { "kV" } },
            { "LVCB", new[] { "Type" } },
            { "ArcFlash", new[] { "Scenario", "AFBoundary" } },
            { "ShortCircuit", new[] { "Bus", "Scenario" } }
        };

        public PropertyDiscoveryService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _propertyCache = new Dictionary<string, List<MapPropertyInfo>>();
        }

        /// <summary>
        /// Gets the list of available data types (classes in EasyAF.Data.Models).
        /// </summary>
        /// <remarks>
        /// Returns only the 6 data types that are part of the DataSet structure:
        /// Bus, LVCB, Fuse, Cable, ArcFlash, ShortCircuit.
        /// Other classes in EasyAF.Data.Models (DataSet, DataSetDiff, etc.) are excluded.
        /// </remarks>
        public List<string> GetAvailableDataTypes()
        {
            try
            {
                // CROSS-MODULE EDIT: 2025-01-16 Data Type Discovery Fix
                // Modified for: Only return the 6 data types that are in DataSet (exclude support classes)
                // Related modules: Data (DataSet model)
                // Rollback instructions: Revert to reflection-based discovery of all classes
                
                // Hardcoded list of data types that exist in DataSet
                // This is more reliable than reflection since it filters out support classes
                // (DataSet, DataSetDiff, EntryDiff, PropertyChange, DiffUtil, etc.)
                var dataModelTypes = new List<string>
                {
                    "Bus",
                    "LVCB",
                    "Fuse",
                    "Cable",
                    "ArcFlash",
                    "ShortCircuit"
                };

                Log.Debug("Returning {Count} known data types", dataModelTypes.Count);
                return dataModelTypes;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get data types");
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets the list of properties for a specific data type that are currently visible.
        /// </summary>
        /// <param name="dataTypeName">The data type name (e.g., "Bus", "LVCB").</param>
        /// <returns>List of visible properties with metadata.</returns>
        /// <remarks>
        /// This method applies settings-based filtering. Properties hidden via settings
        /// will not appear in the returned list.
        /// </remarks>
        public List<MapPropertyInfo> GetPropertiesForType(string dataTypeName)
        {
            try
            {
                // Get all properties (including hidden ones)
                var allProperties = GetAllPropertiesForType(dataTypeName);

                // Filter based on enabled properties setting
                var enabledProperties = _settingsService.GetEnabledProperties(dataTypeName);

                // If wildcard "*" is enabled, return all properties
                if (enabledProperties.Contains("*"))
                {
                    Log.Debug("Wildcard enabled for {DataType}, returning all {Count} properties", 
                        dataTypeName, allProperties.Count);
                    return allProperties;
                }

                // Otherwise filter to only enabled properties
                var filtered = allProperties
                    .Where(p => enabledProperties.Contains(p.PropertyName))
                    .ToList();

                Log.Debug("Filtered {DataType} properties: {Total} total, {Visible} visible", 
                    dataTypeName, allProperties.Count, filtered.Count);

                return filtered;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get properties for {DataType}", dataTypeName);
                return new List<MapPropertyInfo>();
            }
        }

        /// <summary>
        /// Gets ALL properties for a data type, including those hidden by settings.
        /// </summary>
        /// <param name="dataTypeName">The data type name (e.g., "Bus", "LVCB").</param>
        /// <returns>Complete list of properties with metadata.</returns>
        /// <remarks>
        /// This method is used by:
        /// - Property selector dialog (shows all properties so user can enable/disable them)
        /// - Settings management (needs to know what properties exist)
        /// Unlike GetPropertiesForType(), this ignores the enabled properties filter.
        /// </remarks>
        public List<MapPropertyInfo> GetAllPropertiesForType(string dataTypeName)
        {
            // Check cache first
            if (_propertyCache.TryGetValue(dataTypeName, out var cached))
            {
                Log.Debug("Returning cached properties for {DataType} ({Count} properties)", 
                    dataTypeName, cached.Count);
                return cached;
            }

            try
            {
                var type = typeof(Bus).Assembly
                    .GetTypes()
                    .FirstOrDefault(t => t.Name == dataTypeName && t.Namespace == "EasyAF.Data.Models");

                if (type == null)
                {
                    Log.Warning("Data type not found: {DataType}", dataTypeName);
                    return new List<MapPropertyInfo>();
                }

                // CROSS-MODULE EDIT: 2025-01-16 Required Property Validation
                // Modified for: Mark properties as required based on universal + type-specific rules
                // Related modules: Map (PropertyInfo model with IsRequired property)
                // Rollback instructions: Remove GetRequiredPropertyNames call and IsRequired assignment
                
                // Get the set of required property names for this data type
                var requiredNames = GetRequiredPropertyNames(dataTypeName);

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite)
                    .Select(p => new MapPropertyInfo
                    {
                        PropertyName = p.Name,
                        PropertyType = GetFriendlyTypeName(p.PropertyType),
                        Description = GetPropertyDescription(p),
                        IsRequired = requiredNames.Contains(p.Name)  // Mark as required if in the set
                    })
                    .OrderBy(p => p.PropertyName)
                    .ToList();

                // Cache the results
                _propertyCache[dataTypeName] = properties;

                Log.Information("Discovered {Count} properties for {DataType} ({RequiredCount} required)", 
                    properties.Count, dataTypeName, properties.Count(p => p.IsRequired));

                return properties;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to discover properties for {DataType}", dataTypeName);
                return new List<MapPropertyInfo>();
            }
        }

        /// <summary>
        /// Gets the set of required property names for a specific data type.
        /// </summary>
        /// <param name="dataTypeName">The data type name (e.g., "Bus", "LVCB").</param>
        /// <returns>HashSet of required property names.</returns>
        /// <remarks>
        /// <para>
        /// CROSS-MODULE EDIT: 2025-01-16 Required Property Validation
        /// Modified for: Implement hybrid required property detection (universal + type-specific + settings override)
        /// Related modules: None (self-contained in Map module)
        /// Rollback instructions: Remove this method entirely
        /// </para>
        /// <para>
        /// Required property determination (in order of precedence):
        /// 1. Universal required properties (Id, Name) - always included
        /// 2. Type-specific defaults from DefaultRequiredProperties dictionary
        /// 3. Settings override (future enhancement) - replaces defaults if present
        /// </para>
        /// <para>
        /// Example for Bus data type:
        /// - Universal: Id, Name
        /// - Type-specific: kV
        /// - Final result: { "Id", "Name", "kV" }
        /// </para>
        /// </remarks>
        private HashSet<string> GetRequiredPropertyNames(string dataTypeName)
        {
            var required = new HashSet<string>(UniversalRequiredProperties, StringComparer.OrdinalIgnoreCase);

            // Add type-specific defaults
            if (DefaultRequiredProperties.TryGetValue(dataTypeName, out var typeDefaults))
            {
                required.UnionWith(typeDefaults);
                Log.Debug("Added {Count} type-specific required properties for {DataType}", 
                    typeDefaults.Length, dataTypeName);
            }

            // Future Enhancement: Allow settings override
            // This would let power users customize required properties per data type
            // Example settings.json:
            // {
            //   "Map": {
            //     "RequiredProperties": {
            //       "Bus": ["Id", "Name", "kV", "Description"]
            //     }
            //   }
            // }
            //
            // Implementation (when needed):
            // var settingsOverride = _settingsService.GetModuleSettings("Map")
            //     ?.GetValue<string[]>($"RequiredProperties.{dataTypeName}");
            // if (settingsOverride != null)
            // {
            //     required.Clear();
            //     required.UnionWith(UniversalRequiredProperties);  // Always keep universals
            //     required.UnionWith(settingsOverride);
            //     Log.Information("Applied settings override for required properties: {DataType}", dataTypeName);
            // }

            Log.Debug("Required properties for {DataType}: {Properties}", 
                dataTypeName, string.Join(", ", required));

            return required;
        }

        /// <summary>
        /// Gets a friendly display name for a property type.
        /// </summary>
        private string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(string)) return "String";
            if (type == typeof(int)) return "Integer";
            if (type == typeof(double)) return "Double";
            if (type == typeof(bool)) return "Boolean";
            if (type == typeof(DateTime)) return "DateTime";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetFriendlyTypeName(Nullable.GetUnderlyingType(type)!) + "?";
            
            return type.Name;
        }

        /// <summary>
        /// Gets a description for a property (placeholder for future attributes).
        /// </summary>
        private string? GetPropertyDescription(System.Reflection.PropertyInfo property)
        {
            // Future enhancement: Read from [Description] attribute if present
            // For now, return null (no descriptions available)
            return null;
        }

        /// <summary>
        /// Gets nested properties for complex types (e.g., LVCB.TripUnit).
        /// </summary>
        /// <param name="parentType">The parent type name (e.g., "LVCB").</param>
        /// <param name="nestedType">The nested type name (e.g., "TripUnit").</param>
        /// <returns>List of PropertyInfo objects for the nested type.</returns>
        /// <remarks>
        /// Used for mapping to complex nested structures like LVCB trip unit settings.
        /// Currently searches for the nested type directly.
        /// </remarks>
        public List<MapPropertyInfo> GetNestedProperties(string parentType, string nestedType)
        {
            // For now, just return properties of the nested type directly
            // Future enhancement: Validate parent-child relationship
            return GetPropertiesForType(nestedType);
        }

        /// <summary>
        /// Checks if a data type exists in the discovered types.
        /// </summary>
        /// <param name="dataTypeName">The data type name to check.</param>
        /// <returns>True if the type exists and can be mapped to; otherwise false.</returns>
        public bool IsValidDataType(string dataTypeName)
        {
            var availableTypes = GetAvailableDataTypes();
            return availableTypes.Contains(dataTypeName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Clears the property cache (useful after settings changes).
        /// </summary>
        public void ClearCache()
        {
            _propertyCache.Clear();
            Log.Debug("Property cache cleared");
        }
    }
}
