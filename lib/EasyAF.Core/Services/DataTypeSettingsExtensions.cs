using System.Collections.Generic;
using System.Linq;
using EasyAF.Core.Contracts;
using EasyAF.Core.Models;

namespace EasyAF.Core.Services
{
    // CROSS-MODULE EDIT: 2025-11-28 Task 26 - Global Data Type Filtering
    // Modified for: Move data type visibility settings from Map module to Core (global)
    // Related modules: Core (new file), Map (MapSettingsExtensions deprecated), Spec (uses global settings)
    // Rollback instructions: Delete this file, restore Map module settings only

    /// <summary>
    /// Extension methods for accessing global data type visibility settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These settings control which data types and properties are visible across ALL modules:
    /// - Map Editor: Which data types appear in the mapping UI
    /// - Spec Editor: Which data types/properties appear in PropertyPath picker
    /// - Project Module: Which data types appear in data views (future)
    /// </para>
    /// <para>
    /// Settings Path: "DataTypes.Visibility" (global, not module-specific)
    /// </para>
    /// </remarks>
    public static class DataTypeSettingsExtensions
    {
        private const string SETTINGS_KEY = "DataTypes.Visibility";

        /// <summary>
        /// Gets the global data type visibility settings.
        /// </summary>
        public static DataTypeVisibilitySettings GetDataTypeVisibilitySettings(this ISettingsService settingsService)
        {
            var settings = settingsService.GetSetting<DataTypeVisibilitySettings>(SETTINGS_KEY);
            return settings ?? new DataTypeVisibilitySettings();
        }

        /// <summary>
        /// Saves the global data type visibility settings.
        /// </summary>
        public static void SetDataTypeVisibilitySettings(this ISettingsService settingsService, DataTypeVisibilitySettings settings)
        {
            settingsService.SetSetting(SETTINGS_KEY, settings);
        }

        /// <summary>
        /// Checks if a data type is enabled for display globally.
        /// </summary>
        public static bool IsDataTypeEnabled(this ISettingsService settingsService, string dataTypeName)
        {
            var settings = settingsService.GetDataTypeVisibilitySettings();
            if (!settings.DataTypes.ContainsKey(dataTypeName))
                return true; // Default to enabled (opt-out model)

            return settings.DataTypes[dataTypeName].Enabled;
        }

        /// <summary>
        /// Gets the list of enabled properties for a data type (global settings).
        /// </summary>
        public static List<string> GetEnabledProperties(this ISettingsService settingsService, string dataTypeName)
        {
            var settings = settingsService.GetDataTypeVisibilitySettings();
            var config = settings.GetOrCreateConfig(dataTypeName);
            return config.EnabledProperties;
        }

        /// <summary>
        /// Sets the list of enabled properties for a data type (global settings).
        /// </summary>
        public static void SetEnabledProperties(this ISettingsService settingsService, string dataTypeName, List<string> enabledProperties)
        {
            var settings = settingsService.GetDataTypeVisibilitySettings();
            var config = settings.GetOrCreateConfig(dataTypeName);
            config.EnabledProperties = enabledProperties;
            settingsService.SetDataTypeVisibilitySettings(settings);
        }

        /// <summary>
        /// Checks if a specific property is enabled for a data type (global settings).
        /// </summary>
        public static bool IsPropertyEnabled(this ISettingsService settingsService, string dataTypeName, string propertyName)
        {
            var settings = settingsService.GetDataTypeVisibilitySettings();
            var config = settings.GetOrCreateConfig(dataTypeName);
            return config.IsPropertyEnabled(propertyName);
        }

        /// <summary>
        /// Gets a list of all enabled data types (globally).
        /// </summary>
        /// <returns>List of data type names that are enabled.</returns>
        public static List<string> GetEnabledDataTypes(this ISettingsService settingsService)
        {
            var settings = settingsService.GetDataTypeVisibilitySettings();
            return settings.DataTypes
                .Where(kvp => kvp.Value.Enabled)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Enables or disables a data type globally.
        /// </summary>
        public static void SetDataTypeEnabled(this ISettingsService settingsService, string dataTypeName, bool enabled)
        {
            var settings = settingsService.GetDataTypeVisibilitySettings();
            var config = settings.GetOrCreateConfig(dataTypeName);
            config.Enabled = enabled;
            settingsService.SetDataTypeVisibilitySettings(settings);
        }
    }
}
