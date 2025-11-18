using System.Collections.Generic;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Service for discovering mappable properties from EasyAF.Data model types via reflection.
    /// </summary>
    /// <remarks>
    /// This service provides runtime discovery of available data types (Bus, LVCB, ArcFlash, etc.)
    /// and their properties, enabling dynamic mapping UI generation without hardcoding type information.
    /// </remarks>
    public interface IPropertyDiscoveryService
    {
        /// <summary>
        /// Gets all available data types that can be mapped to.
        /// </summary>
        /// <returns>
        /// List of data type names (e.g., "Bus", "LVCB", "ArcFlash", "ShortCircuit").
        /// </returns>
        /// <remarks>
        /// Types are discovered from the EasyAF.Data.Models namespace via reflection.
        /// Results are cached for performance.
        /// </remarks>
        List<string> GetAvailableDataTypes();

        /// <summary>
        /// Gets the user-friendly description for a data type.
        /// </summary>
        /// <param name="dataTypeName">The class name (e.g., "Bus", "LVBreaker").</param>
        /// <returns>User-friendly description from [EasyPowerClass] attribute, or the class name if not found.</returns>
        /// <remarks>
        /// <para>
        /// Examples:
        /// - "Bus" ? "Electrical buses/switchgear"
        /// - "LVBreaker" ? "Low Voltage Breakers"
        /// - "ArcFlash" ? "Arc Flash Scenario Report"
        /// </para>
        /// <para>
        /// Used to display equipment type names in UI dropdowns and filters instead of raw class names.
        /// </para>
        /// </remarks>
        string GetDataTypeDescription(string dataTypeName);

        /// <summary>
        /// Gets enabled properties for a specific data type (filtered by settings).
        /// </summary>
        /// <param name="dataTypeName">The name of the data type (e.g., "Bus", "LVCB").</param>
        /// <returns>
        /// List of PropertyInfo objects describing each property, including name, type, and description.
        /// Returns empty list if the data type is not found.
        /// </returns>
        /// <remarks>
        /// Results are cached after first retrieval. Properties include XML documentation
        /// comments when available to help users understand what each field represents.
        /// </remarks>
        List<PropertyInfo> GetPropertiesForType(string dataTypeName);

        /// <summary>
        /// Gets ALL properties for a data type, ignoring visibility settings.
        /// Used by settings UI to show all available properties for configuration.
        /// </summary>
        /// <param name="dataTypeName">The name of the data type (e.g., "Bus", "LVCB").</param>
        /// <returns>
        /// List of PropertyInfo objects describing each property, including name, type, and description.
        /// Returns empty list if the data type is not found.
        /// </returns>
        /// <remarks>
        /// Results are cached after first retrieval. Properties include XML documentation
        /// comments when available to help users understand what each field represents.
        /// </remarks>
        List<PropertyInfo> GetAllPropertiesForType(string dataTypeName);

        /// <summary>
        /// Gets nested properties for complex types (e.g., LVCB.TripUnit).
        /// </summary>
        /// <param name="parentType">The parent type name (e.g., "LVCB").</param>
        /// <param name="nestedType">The nested type name (e.g., "TripUnit").</param>
        /// <returns>
        /// List of PropertyInfo objects for the nested type.
        /// </returns>
        /// <remarks>
        /// Used for mapping to complex nested structures like LVCB trip unit settings.
        /// Currently searches for the nested type directly; future enhancement may
        /// validate parent-child relationships.
        /// </remarks>
        List<PropertyInfo> GetNestedProperties(string parentType, string nestedType);

        /// <summary>
        /// Checks if a data type exists in the discovered types.
        /// </summary>
        /// <param name="dataTypeName">The data type name to check.</param>
        /// <returns>True if the type exists and can be mapped to; otherwise false.</returns>
        bool IsValidDataType(string dataTypeName);

        /// <summary>
        /// Clears the internal property cache.
        /// </summary>
        /// <remarks>
        /// <para>
        /// CROSS-MODULE EDIT: 2025-01-16 Property Count Fix
        /// Modified for: Add cache invalidation when settings change
        /// Related modules: Map (MapDocumentViewModel, PropertyDiscoveryService)
        /// Rollback instructions: Remove this method from interface and implementation
        /// </para>
        /// <para>
        /// Should be called when property visibility settings change to ensure
        /// property counts and lists reflect the new settings.
        /// Performance impact: ~6ms one-time cost to re-reflect all data types.
        /// </para>
        /// </remarks>
        void ClearCache();
    }
}
