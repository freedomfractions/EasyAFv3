using System.Collections.Generic;
using System.Linq;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Extension methods for accessing Map module settings.
    /// </summary>
    public static class MapSettingsExtensions
    {
        private const string SETTINGS_KEY = "MapModule.DataTypeVisibility";

        /// <summary>
        /// Gets the data type visibility settings for the Map module.
        /// </summary>
        public static DataTypeVisibilitySettings GetMapVisibilitySettings(this ISettingsService settingsService)
        {
            var settings = settingsService.GetSetting<DataTypeVisibilitySettings>(SETTINGS_KEY);
            return settings ?? new DataTypeVisibilitySettings();
        }

        /// <summary>
        /// Saves the data type visibility settings for the Map module.
        /// </summary>
        public static void SetMapVisibilitySettings(this ISettingsService settingsService, DataTypeVisibilitySettings settings)
        {
            settingsService.SetSetting(SETTINGS_KEY, settings);
        }

        /// <summary>
        /// Checks if a data type is enabled for display in the Map Editor.
        /// </summary>
        public static bool IsDataTypeEnabled(this ISettingsService settingsService, string dataTypeName)
        {
            var settings = settingsService.GetMapVisibilitySettings();
            if (!settings.DataTypes.ContainsKey(dataTypeName))
                return true; // Default to enabled (opt-out model)

            return settings.DataTypes[dataTypeName].Enabled;
        }

        /// <summary>
        /// Gets the list of enabled properties for a data type.
        /// </summary>
        public static List<string> GetEnabledProperties(this ISettingsService settingsService, string dataTypeName)
        {
            var settings = settingsService.GetMapVisibilitySettings();
            var config = settings.GetOrCreateConfig(dataTypeName);
            return config.EnabledProperties;
        }

        /// <summary>
        /// Sets the list of enabled properties for a data type.
        /// </summary>
        public static void SetEnabledProperties(this ISettingsService settingsService, string dataTypeName, List<string> enabledProperties)
        {
            var settings = settingsService.GetMapVisibilitySettings();
            var config = settings.GetOrCreateConfig(dataTypeName);
            config.EnabledProperties = enabledProperties;
            settingsService.SetMapVisibilitySettings(settings);
        }

        /// <summary>
        /// Checks if a specific property is enabled for a data type.
        /// </summary>
        public static bool IsPropertyEnabled(this ISettingsService settingsService, string dataTypeName, string propertyName)
        {
            var settings = settingsService.GetMapVisibilitySettings();
            var config = settings.GetOrCreateConfig(dataTypeName);
            return config.IsPropertyEnabled(propertyName);
        }
    }
}
