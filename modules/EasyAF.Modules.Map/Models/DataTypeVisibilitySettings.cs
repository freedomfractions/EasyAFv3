using System.Collections.Generic;

namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Settings for controlling which data types and properties are visible in the Map Editor.
    /// </summary>
    public class DataTypeVisibilitySettings
    {
        /// <summary>
        /// Dictionary of data type configurations keyed by data type name.
        /// </summary>
        public Dictionary<string, DataTypeConfig> DataTypes { get; set; } = new();

        /// <summary>
        /// Gets the configuration for a specific data type, creating default if not exists.
        /// </summary>
        public DataTypeConfig GetOrCreateConfig(string dataTypeName)
        {
            if (!DataTypes.ContainsKey(dataTypeName))
            {
                DataTypes[dataTypeName] = new DataTypeConfig();
            }
            return DataTypes[dataTypeName];
        }
    }

    /// <summary>
    /// Configuration for a single data type's visibility and enabled properties.
    /// </summary>
    public class DataTypeConfig
    {
        /// <summary>
        /// Whether this data type should appear as a tab in the Map Editor.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// List of enabled property names. Use "*" to enable all properties.
        /// </summary>
        /// <remarks>
        /// By default, all properties are enabled (opt-out model).
        /// Users can selectively disable properties they don't need.
        /// </remarks>
        public List<string> EnabledProperties { get; set; } = new() { "*" };

        /// <summary>
        /// Checks if a property is enabled for mapping.
        /// </summary>
        public bool IsPropertyEnabled(string propertyName)
        {
            // If wildcard is present, all properties are enabled
            if (EnabledProperties.Contains("*"))
                return true;

            // Otherwise check explicit list
            return EnabledProperties.Contains(propertyName);
        }

        /// <summary>
        /// Checks if all properties are enabled (wildcard mode).
        /// </summary>
        public bool AllPropertiesEnabled => EnabledProperties.Contains("*");
    }
}
