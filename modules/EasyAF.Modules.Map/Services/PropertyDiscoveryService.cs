using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyAF.Modules.Map.Models;
using EasyAF.Data.Models;
using EasyAF.Core.Contracts;
using EasyAF.Core.Services; // NEW: For DataTypeSettingsExtensions
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;
using EasyAF.Data.Attributes;

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
        private readonly Dictionary<string, string> _dataTypeDescriptions = new(); // NEW: Cache for descriptions

        // CROSS-MODULE EDIT: 2025-01-16 Required Property Validation
        // Modified for: Read [Required] attribute from model properties dynamically (removed hardcoded lists)
        // Related modules: Data (all model classes with [Required] attributes)
        // Rollback instructions: Restore UniversalRequiredProperties and DefaultRequiredProperties constants
        
        /// <summary>
        /// Properties that are required for ALL data types.
        /// </summary>
        /// <remarks>
        /// These are foundational properties that every EasyAF data type must have mapped:
        /// - Primary identifier column (e.g., LVBreakers, Fuses, Buses, etc.) - varies by type
        /// - Name: Human-readable identifier for equipment (where applicable)
        /// NOTE: The actual ID property varies by data type and matches the CSV column name.
        /// </remarks>
        private static readonly string[] UniversalRequiredProperties = { "Name" };

        /// <summary>
        /// Type-specific required properties beyond the universal ones.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These properties are critical for specific data types (using exact CSV column names):
        /// - Bus: Buses (ID), BaseKV (voltage rating for calculations), NoOfPhases
        /// - LVCB: LVBreakers (ID), OnBus, BaseKV
        /// - Fuse: Fuses (ID), OnBus, BaseKV
        /// - Cable: Cables (ID), NoOfPhases
        /// - ArcFlash: ArcFaultBusName (ID), Scenario (multi-scenario differentiation)
        /// - ShortCircuit: EquipmentName (ID), BusName (location), Scenario (multi-scenario)
        /// </para>
        /// <para>
        /// Future Enhancement: Allow settings.json to override these defaults:
        /// {
        ///   "Map": {
        ///     "RequiredProperties": {
        ///       "Bus": ["Buses", "BaseKV", "NoOfPhases", "CustomField"]
        ///     }
        ///   }
        /// }
        /// </para>
        /// </remarks>
        private static readonly Dictionary<string, string[]> DefaultRequiredProperties = new()
        {
            { "Bus", new[] { "Buses", "BaseKV", "NoOfPhases" } },
            { "LVCB", new[] { "LVBreakers", "OnBus", "BaseKV" } },
            { "Fuse", new[] { "Fuses", "OnBus", "BaseKV" } },
            { "Cable", new[] { "Cables", "NoOfPhases" } },
            { "ArcFlash", new[] { "ArcFaultBusName", "Scenario" } },
            { "ShortCircuit", new[] { "EquipmentName", "BusName", "Scenario" } }
        };

        public PropertyDiscoveryService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _propertyCache = new Dictionary<string, List<MapPropertyInfo>>();
            
            // NEW: Pre-load data type descriptions from [EasyPowerClass] attributes
            LoadDataTypeDescriptions();
        }

        /// <summary>
        /// Gets the user-friendly description for a data type (e.g., "Electrical buses/switchgear" for "Bus").
        /// </summary>
        /// <param name="dataTypeName">The class name (e.g., "Bus", "LVBreaker").</param>
        /// <returns>User-friendly description from [EasyPowerClass] attribute, or the class name if not found.</returns>
        public string GetDataTypeDescription(string dataTypeName)
        {
            return _dataTypeDescriptions.TryGetValue(dataTypeName, out var description) 
                ? description 
                : dataTypeName;
        }

        /// <summary>
        /// Loads [EasyPowerClass] attribute descriptions for all data types.
        /// </summary>
        /// <remarks>
        /// This method scans EasyAF.Data.Models for classes with [EasyPowerClass("Description")] attributes
        /// and caches their user-friendly descriptions for UI display.
        /// </remarks>
        private void LoadDataTypeDescriptions()
        {
            try
            {
                var assembly = typeof(Bus).Assembly;
                var types = assembly.GetTypes()
                    .Where(t => t.Namespace == "EasyAF.Data.Models" && t.IsClass && t.IsPublic);

                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<EasyAF.Data.Attributes.EasyPowerClassAttribute>();
                    if (attribute != null && !string.IsNullOrWhiteSpace(attribute.EasyPowerClassName))
                    {
                        _dataTypeDescriptions[type.Name] = attribute.EasyPowerClassName;
                        Log.Debug("Loaded description for {Type}: {Description}", type.Name, attribute.EasyPowerClassName);
                    }
                }

                Log.Information("Loaded {Count} data type descriptions", _dataTypeDescriptions.Count);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load data type descriptions from attributes");
            }
        }

        /// <summary>
        /// Gets the list of available data types (classes in EasyAF.Data.Models).
        /// </summary>
        /// <remarks>
        /// Discovers data types by reflecting on the DataSet class and extracting
        /// the generic type arguments from properties ending with "Entries".
        /// This approach is dynamic and will automatically pick up new data types
        /// added to the DataSet without requiring code changes.
        /// </remarks>
        public List<string> GetAvailableDataTypes()
        {
            try
            {
                // CROSS-MODULE EDIT: 2025-01-16 Data Type Discovery via DataSet Reflection
                // Modified for: Discover data types from DataSet properties (not all classes in namespace)
                // Related modules: Data (DataSet model)
                // Rollback instructions: Revert to hardcoded list or old reflection logic
                
                // Use reflection on DataSet to find all "*Entries" properties
                // and extract their generic type arguments (Bus, LVCB, Fuse, Cable, ArcFlash, ShortCircuit)
                var dataSetType = typeof(DataSet);
                var dataModelTypes = dataSetType.GetProperties()
                    .Where(p => p.Name.EndsWith("Entries") && p.PropertyType.IsGenericType)
                    .Select(p =>
                    {
                        // Get the dictionary's value type (e.g., Dictionary<string, Bus> ? Bus)
                        var genericArgs = p.PropertyType.GetGenericArguments();
                        // For Dictionary<string, T>, T is at index 1
                        // For Dictionary<(string, string), T>, T is still at index 1
                        return genericArgs.Length >= 2 ? genericArgs[1].Name : null;
                    })
                    .Where(name => !string.IsNullOrEmpty(name))
                    .OrderBy(name => name)
                    .ToList();

                Log.Debug("Discovered {Count} data types from DataSet properties", dataModelTypes.Count);
                return dataModelTypes;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to discover data types from DataSet");
                
                // Fallback to known list if reflection fails
                var fallback = new List<string> { "Bus", "LVCB", "Fuse", "Cable", "ArcFlash", "ShortCircuit" };
                Log.Warning("Using fallback data type list ({Count} types)", fallback.Count);
                return fallback;
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
                    // CROSS-MODULE EDIT: 2025-01-19 Include Computed Properties
                    // Modified for: Allow properties with [Category("Computed")] even if they have [JsonIgnore]
                    // Related modules: Data (LVBreaker.Computed.cs and future computed properties)
                    // Rollback instructions: Remove Category check, filter out all JsonIgnore properties
                    .Where(p =>
                    {
                        // Check if it's a computed property (has [Category("Computed")])
                        var categoryAttr = p.GetCustomAttribute<CategoryAttribute>();
                        bool isComputed = categoryAttr != null && 
                                        string.Equals(categoryAttr.Category, "Computed", StringComparison.OrdinalIgnoreCase);
                        
                        // If it's a computed property, always include it (ignore JsonIgnore)
                        if (isComputed)
                            return true;
                        
                        // Otherwise, filter out properties with JsonIgnore (like Id alias)
                        return !p.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), false).Any()
                            && !p.GetCustomAttributes(typeof(Newtonsoft.Json.JsonIgnoreAttribute), false).Any();
                    })
                    .Select(p =>
                    {
                        // Check if it's a computed property
                        var categoryAttr = p.GetCustomAttribute<CategoryAttribute>();
                        bool isComputed = categoryAttr != null && 
                                        string.Equals(categoryAttr.Category, "Computed", StringComparison.OrdinalIgnoreCase);

                        return new MapPropertyInfo
                        {
                            PropertyName = p.Name,
                            PropertyType = GetFriendlyTypeName(p.PropertyType),
                            Description = GetPropertyDescription(p),
                            IsRequired = requiredNames.Contains(p.Name),
                            IsComputed = isComputed  // NEW: Mark computed properties
                        };
                    })
                    // CROSS-MODULE EDIT: 2025-01-18 Required Properties Float to Top
                    // Modified for: Sort required properties to the top for visibility in map editor
                    // Related modules: Map (DataTypeMappingView displays this list)
                    // Rollback instructions: Remove OrderByDescending, use OrderBy(PropertyName) only
                    .OrderByDescending(p => p.IsRequired)  // Required properties first
                    .ThenBy(p => p.PropertyName)           // Then alphabetically
                    .ToList();

                // Cache the results
                _propertyCache[dataTypeName] = properties;

                var computedCount = properties.Count(p => p.IsComputed);
                Log.Information("Discovered {Count} properties for {DataType} ({RequiredCount} required, {ComputedCount} computed)", 
                    properties.Count, dataTypeName, properties.Count(p => p.IsRequired), computedCount);

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
        /// Modified for: Read [Required] attribute from model properties dynamically
        /// Related modules: Data (all model classes with [Required] attributes)
        /// Rollback instructions: Revert to hardcoded DefaultRequiredProperties dictionary
        /// </para>
        /// <para>
        /// Required property determination:
        /// Reads the [Required] attribute directly from the model class properties.
        /// This ensures the Map Editor always reflects the current model requirements
        /// without needing manual updates to hardcoded lists.
        /// </para>
        /// </remarks>
        private HashSet<string> GetRequiredPropertyNames(string dataTypeName)
        {
            var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Get the type from EasyAF.Data.Models
                var type = typeof(Bus).Assembly
                    .GetTypes()
                    .FirstOrDefault(t => t.Name == dataTypeName && t.Namespace == "EasyAF.Data.Models");

                if (type == null)
                {
                    Log.Warning("Could not find type {DataType} for required property detection", dataTypeName);
                    return required;
                }

                // Scan properties for [Required] attribute
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite);

                foreach (var prop in properties)
                {
                    // Check if property has [Required] attribute
                    var hasRequiredAttr = prop.GetCustomAttribute<RequiredAttribute>() != null;
                    
                    if (hasRequiredAttr)
                    {
                        required.Add(prop.Name);
                        Log.Debug("Property {DataType}.{Property} marked as Required (has [Required] attribute)", 
                            dataTypeName, prop.Name);
                    }
                }

                Log.Information("Found {Count} required properties for {DataType} via [Required] attribute: {Properties}", 
                    required.Count, dataTypeName, string.Join(", ", required));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to detect required properties for {DataType}", dataTypeName);
            }

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
        /// Gets a description for a property from its [Description] attribute.
        /// </summary>
        /// <param name="property">The property to get the description for.</param>
        /// <returns>The description from the [Description] attribute, or null if not present.</returns>
        private string? GetPropertyDescription(System.Reflection.PropertyInfo property)
        {
            // CROSS-MODULE EDIT: 2025-01-18 Property Description Extraction
            // Modified for: Read [Description] attribute from model properties
            // Related modules: Data (all model classes with [Description] attributes)
            // Rollback instructions: Return null (placeholder implementation)
            
            try
            {
                // Try to get the [Description] attribute from the property
                var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr != null && !string.IsNullOrWhiteSpace(descriptionAttr.Description))
                {
                    return descriptionAttr.Description;
                }

                // Fallback: No description attribute found
                return null;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to extract description for property {PropertyName}", property.Name);
                return null;
            }
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
