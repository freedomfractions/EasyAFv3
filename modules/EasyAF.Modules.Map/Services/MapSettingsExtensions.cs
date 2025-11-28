using System.Collections.Generic;
using EasyAF.Core.Contracts;
using EasyAF.Core.Services; // NEW: Use global settings

namespace EasyAF.Modules.Map.Services
{
    // CROSS-MODULE EDIT: 2025-11-28 Task 26 - Global Data Type Filtering
    // Modified for: Redirect to global settings (backward compatibility wrapper)
    // Related modules: Map (this file), Core (DataTypeSettingsExtensions)
    // Rollback instructions: Restore original Map-only implementation

    /// <summary>
    /// Extension methods for accessing Map module settings.
    /// </summary>
    /// <remarks>
    /// DEPRECATED: This class now wraps global DataTypeSettingsExtensions.
    /// Use Core.Services.DataTypeSettingsExtensions directly for new code.
    /// </remarks>
    public static class MapSettingsExtensions
    {
        // DEPRECATED: Redirect to global settings
        public static Core.Models.DataTypeVisibilitySettings GetMapVisibilitySettings(this ISettingsService settingsService)
        {
            return settingsService.GetDataTypeVisibilitySettings();
        }

        public static void SetMapVisibilitySettings(this ISettingsService settingsService, Core.Models.DataTypeVisibilitySettings settings)
        {
            settingsService.SetDataTypeVisibilitySettings(settings);
        }

        public static bool IsDataTypeEnabled(this ISettingsService settingsService, string dataTypeName)
        {
            return settingsService.IsDataTypeEnabled(dataTypeName);
        }

        public static List<string> GetEnabledProperties(this ISettingsService settingsService, string dataTypeName)
        {
            return settingsService.GetEnabledProperties(dataTypeName);
        }

        public static void SetEnabledProperties(this ISettingsService settingsService, string dataTypeName, List<string> enabledProperties)
        {
            settingsService.SetEnabledProperties(dataTypeName, enabledProperties);
        }

        public static bool IsPropertyEnabled(this ISettingsService settingsService, string dataTypeName, string propertyName)
        {
            return settingsService.IsPropertyEnabled(dataTypeName, propertyName);
        }
    }
}
