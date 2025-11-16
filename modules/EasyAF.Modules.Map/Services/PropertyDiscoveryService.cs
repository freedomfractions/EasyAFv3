using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Discovers mappable properties from EasyAF.Data.Models classes via reflection.
    /// </summary>
    /// <remarks>
    /// This service uses reflection to discover all public classes in the EasyAF.Data.Models
    /// namespace and their public properties. Results are cached for performance.
    /// XML documentation is extracted when available to provide helpful descriptions.
    /// </remarks>
    public class PropertyDiscoveryService : IPropertyDiscoveryService
    {
        private readonly Dictionary<string, Type> _typeCache = new();
        private readonly Dictionary<string, List<MapPropertyInfo>> _propertyCache = new();
        private readonly Dictionary<string, XDocument?> _xmlDocCache = new();

        /// <summary>
        /// Initializes a new instance of the PropertyDiscoveryService.
        /// </summary>
        /// <remarks>
        /// Discovers and caches all available types from EasyAF.Data.Models on construction.
        /// This is a one-time operation per service instance.
        /// </remarks>
        public PropertyDiscoveryService()
        {
            DiscoverTypes();
        }

        /// <summary>
        /// Gets all available data types that can be mapped to.
        /// </summary>
        public List<string> GetAvailableDataTypes()
        {
            return _typeCache.Keys.OrderBy(k => k).ToList();
        }

        /// <summary>
        /// Gets all properties for a specific data type.
        /// </summary>
        public List<MapPropertyInfo> GetPropertiesForType(string dataTypeName)
        {
            // Check cache first
            if (_propertyCache.TryGetValue(dataTypeName, out var cached))
                return new List<MapPropertyInfo>(cached); // Return copy to prevent mutation

            if (!_typeCache.TryGetValue(dataTypeName, out var type))
            {
                Log.Warning("Data type not found: {DataType}", dataTypeName);
                return new List<MapPropertyInfo>();
            }

            var properties = new List<MapPropertyInfo>();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Load XML documentation for this type's assembly
            var xmlDoc = GetXmlDocumentation(type.Assembly);

            foreach (var prop in props)
            {
                var propInfo = new MapPropertyInfo
                {
                    PropertyName = prop.Name,
                    DataType = dataTypeName,
                    PropertyType = GetFriendlyTypeName(prop.PropertyType),
                    Description = ExtractXmlDocumentation(prop, xmlDoc)
                };

                properties.Add(propInfo);
            }

            // Cache the results
            _propertyCache[dataTypeName] = properties;
            
            Log.Debug("Discovered {Count} properties for {DataType}", properties.Count, dataTypeName);
            return new List<MapPropertyInfo>(properties);
        }

        /// <summary>
        /// Gets nested properties for complex types.
        /// </summary>
        public List<MapPropertyInfo> GetNestedProperties(string parentType, string nestedType)
        {
            // For now, just look up the nested type directly
            // Future enhancement: validate parent-child relationship
            return GetPropertiesForType(nestedType);
        }

        /// <summary>
        /// Checks if a data type is valid and available for mapping.
        /// </summary>
        public bool IsValidDataType(string dataTypeName)
        {
            return _typeCache.ContainsKey(dataTypeName);
        }

        /// <summary>
        /// Discovers all public classes in EasyAF.Data.Models namespace.
        /// </summary>
        private void DiscoverTypes()
        {
            try
            {
                var assembly = Assembly.Load("EasyAF.Data");
                var modelTypes = assembly.GetTypes()
                    .Where(t => t.Namespace == "EasyAF.Data.Models" 
                             && t.IsClass 
                             && t.IsPublic
                             && !t.IsAbstract)
                    .ToList();

                foreach (var type in modelTypes)
                {
                    _typeCache[type.Name] = type;
                    Log.Debug("Discovered data type: {TypeName}", type.Name);
                }

                Log.Information("Property discovery complete: found {Count} data types", _typeCache.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to discover data types from EasyAF.Data assembly");
            }
        }

        /// <summary>
        /// Gets the XML documentation file for an assembly.
        /// </summary>
        private XDocument? GetXmlDocumentation(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            if (assemblyName == null) return null;

            // Check cache
            if (_xmlDocCache.TryGetValue(assemblyName, out var cached))
                return cached;

            try
            {
                // Look for XML doc file next to the assembly
                var assemblyLocation = assembly.Location;
                var xmlPath = Path.ChangeExtension(assemblyLocation, ".xml");

                if (File.Exists(xmlPath))
                {
                    var doc = XDocument.Load(xmlPath);
                    _xmlDocCache[assemblyName] = doc;
                    Log.Debug("Loaded XML documentation for {Assembly}", assemblyName);
                    return doc;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load XML documentation for {Assembly}", assemblyName);
            }

            _xmlDocCache[assemblyName] = null;
            return null;
        }

        /// <summary>
        /// Extracts XML documentation summary for a property.
        /// </summary>
        private string? ExtractXmlDocumentation(System.Reflection.PropertyInfo prop, XDocument? xmlDoc)
        {
            if (xmlDoc == null) return null;

            try
            {
                // Build the XML member name (e.g., "P:EasyAF.Data.Models.Bus.Id")
                var memberName = $"P:{prop.DeclaringType?.FullName}.{prop.Name}";

                var memberElement = xmlDoc.Descendants("member")
                    .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);

                if (memberElement == null) return null;

                // Get the summary element
                var summary = memberElement.Element("summary")?.Value;
                
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    // Clean up whitespace and newlines
                    return summary.Trim()
                        .Replace("\r\n", " ")
                        .Replace("\n", " ")
                        .Replace("  ", " ");
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to extract XML doc for {Property}", prop.Name);
            }

            return null;
        }

        /// <summary>
        /// Converts a Type to a friendly display name.
        /// </summary>
        private string GetFriendlyTypeName(Type type)
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                return $"{underlyingType.Name}?";

            // Handle generic types
            if (type.IsGenericType)
            {
                var genericTypeName = type.Name.Split('`')[0];
                var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
                return $"{genericTypeName}<{genericArgs}>";
            }

            return type.Name;
        }
    }
}
