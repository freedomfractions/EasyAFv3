using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EasyAF.Import
{
    /// <summary>
    /// Helper class for discovering and building composite keys from data model classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class uses reflection to discover composite key properties from model classes
    /// based on the [Required] attribute. This eliminates hardcoded property names and makes
    /// the import system data-driven.
    /// </para>
    /// <para>
    /// <strong>Design Philosophy:</strong>
    /// - Model classes define their structure via [Required] attributes (source of truth)
    /// - Mapping configs translate CSV columns to properties (translation layer)
    /// - This helper discovers composite keys via reflection (discovery layer)
    /// </para>
    /// <para>
    /// <strong>Example Usage:</strong>
    /// </para>
    /// <code>
    /// // Discover key properties for ArcFlash
    /// var keyProps = CompositeKeyHelper.GetCompositeKeyProperties(typeof(ArcFlash));
    /// // Returns: ["ArcFaultBusName", "Scenario"] (discovered via [Required] attribute)
    /// 
    /// // Build composite key from instance
    /// var af = new ArcFlash { ArcFaultBusName = "BUS-1", Scenario = "Main-Max" };
    /// var key = CompositeKeyHelper.BuildCompositeKey(af, typeof(ArcFlash));
    /// // Returns: ("BUS-1", "Main-Max")
    /// </code>
    /// </remarks>
    public static class CompositeKeyHelper
    {
        // Cache for discovered key properties to avoid repeated reflection
        private static readonly Dictionary<Type, string[]> _keyPropertiesCache = new();
        private static readonly object _cacheLock = new object();

        /// <summary>
        /// Gets the composite key property names for a given type.
        /// </summary>
        /// <param name="type">The type to inspect (e.g., typeof(ArcFlash)).</param>
        /// <returns>
        /// Array of property names that form the composite key, ordered alphabetically.
        /// Properties are determined by the [Required] attribute.
        /// Returns empty array if no required properties exist.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Discovery Logic:</strong>
        /// 1. Find all properties with [Required] attribute
        /// 2. Filter out JsonIgnore properties (alias properties like "Id")
        /// 3. Sort alphabetically for consistent ordering
        /// </para>
        /// <para>
        /// <strong>Caching:</strong> Results are cached per type to avoid repeated reflection.
        /// </para>
        /// <para>
        /// <strong>Examples:</strong>
        /// - ArcFlash: ["ArcFaultBusName", "Scenario"] (2-part key)
        /// - ShortCircuit: ["BusName", "Scenario"] (2-part key, "Bus" is alias)
        /// - LVBreaker: ["LVBreakerName"] (single key)
        /// </para>
        /// </remarks>
        public static string[] GetCompositeKeyProperties(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Check cache first
            lock (_cacheLock)
            {
                if (_keyPropertiesCache.TryGetValue(type, out var cached))
                    return cached;
            }

            // Discover key properties via reflection
            var keyProperties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => 
                    p.GetCustomAttribute<RequiredAttribute>() != null &&  // Has [Required]
                    p.CanRead &&                                          // Is readable
                    !IsJsonIgnored(p))                                    // Not an alias property
                .Select(p => p.Name)
                .OrderBy(name => name)                                    // Consistent ordering
                .ToArray();

            // Cache the result
            lock (_cacheLock)
            {
                _keyPropertiesCache[type] = keyProperties;
            }

            return keyProperties;
        }

        /// <summary>
        /// Builds a composite key from an object instance.
        /// </summary>
        /// <param name="instance">The object instance to extract key values from.</param>
        /// <param name="type">The type of the instance (for key property discovery).</param>
        /// <returns>
        /// A tuple representing the composite key, or null if any key property is null/empty.
        /// The tuple size matches the number of key properties (2-tuple, 3-tuple, etc.).
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Return Value:</strong>
        /// - 1 key property: returns string (not tuple)
        /// - 2 key properties: returns (string, string)
        /// - 3 key properties: returns (string, string, string)
        /// - 4+ key properties: throws NotSupportedException (DataSet doesn't support this)
        /// </para>
        /// <para>
        /// <strong>Null Handling:</strong> If ANY key property is null/empty, returns null.
        /// This prevents incomplete keys from being added to the DataSet.
        /// </para>
        /// <para>
        /// <strong>Examples:</strong>
        /// </para>
        /// <code>
        /// // ArcFlash with 2-part key
        /// var af = new ArcFlash { ArcFaultBusName = "BUS-1", Scenario = "Main-Max" };
        /// var key = BuildCompositeKey(af, typeof(ArcFlash));
        /// // Returns: ("BUS-1", "Main-Max")
        /// 
        /// // ShortCircuit with incomplete key (Bus is null)
        /// var sc = new ShortCircuit { BusName = "BUS-1", Scenario = "Main-Max" }; // Bus property null!
        /// var key = BuildCompositeKey(sc, typeof(ShortCircuit));
        /// // Returns: null (incomplete key)
        /// </code>
        /// </remarks>
        /// <exception cref="ArgumentNullException">If instance or type is null.</exception>
        /// <exception cref="NotSupportedException">If more than 3 key properties exist.</exception>
        public static object? BuildCompositeKey(object instance, Type type)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var keyProps = GetCompositeKeyProperties(type);
            if (keyProps.Length == 0)
                return null; // No composite key defined

            // Extract values from instance
            var values = new string?[keyProps.Length];
            for (int i = 0; i < keyProps.Length; i++)
            {
                var prop = type.GetProperty(keyProps[i]);
                values[i] = prop?.GetValue(instance) as string;

                // If any key component is null/empty, the entire key is invalid
                if (string.IsNullOrWhiteSpace(values[i]))
                    return null;
            }

            // Build appropriate tuple based on number of key properties
            return keyProps.Length switch
            {
                1 => values[0]!, // Single key - return string directly
                2 => (values[0]!, values[1]!), // 2-tuple
                3 => (values[0]!, values[1]!, values[2]!), // 3-tuple
                _ => throw new NotSupportedException(
                    $"Composite keys with {keyProps.Length} properties are not supported. " +
                    $"DataSet dictionaries support up to 3-part keys. Type: {type.Name}")
            };
        }

        /// <summary>
        /// Checks if all key properties have non-null/non-empty values.
        /// </summary>
        /// <param name="instance">The object instance to check.</param>
        /// <param name="type">The type of the instance.</param>
        /// <returns>True if all key properties are populated; false otherwise.</returns>
        /// <remarks>
        /// <para>
        /// This is a convenience method for validation before attempting to build a key.
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// </para>
        /// <code>
        /// if (CompositeKeyHelper.HasCompleteKey(arcFlash, typeof(ArcFlash)))
        /// {
        ///     var key = CompositeKeyHelper.BuildCompositeKey(arcFlash, typeof(ArcFlash));
        ///     dataSet.ArcFlashEntries[key] = arcFlash;
        /// }
        /// </code>
        /// </remarks>
        public static bool HasCompleteKey(object instance, Type type)
        {
            if (instance == null || type == null)
                return false;

            var keyProps = GetCompositeKeyProperties(type);
            if (keyProps.Length == 0)
                return false;

            // Check that all key properties have values
            foreach (var propName in keyProps)
            {
                var prop = type.GetProperty(propName);
                var value = prop?.GetValue(instance) as string;
                
                if (string.IsNullOrWhiteSpace(value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a human-readable description of the composite key structure.
        /// </summary>
        /// <param name="type">The type to describe.</param>
        /// <returns>A string like "ArcFlash key: (ArcFaultBusName, Scenario)"</returns>
        /// <remarks>
        /// Used for logging and debugging to show what properties form the composite key.
        /// </remarks>
        public static string GetKeyDescription(Type type)
        {
            var keyProps = GetCompositeKeyProperties(type);
            if (keyProps.Length == 0)
                return $"{type.Name} has no composite key (no [Required] properties)";
            
            var propList = string.Join(", ", keyProps);
            return $"{type.Name} key: ({propList})";
        }

        /// <summary>
        /// Checks if a property has [JsonIgnore] attribute (used to filter out alias properties).
        /// </summary>
        private static bool IsJsonIgnored(PropertyInfo property)
        {
            // Check both System.Text.Json and Newtonsoft.Json attributes
            return property.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>() != null ||
                   property.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>() != null;
        }

        /// <summary>
        /// Clears the internal cache (useful for testing or dynamic type loading scenarios).
        /// </summary>
        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                _keyPropertiesCache.Clear();
            }
        }
    }
}
