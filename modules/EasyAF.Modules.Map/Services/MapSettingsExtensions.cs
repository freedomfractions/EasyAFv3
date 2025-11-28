using System.Collections.Generic;
using EasyAF.Core.Contracts;
using EasyAF.Core.Services; // NEW: Use global settings

namespace EasyAF.Modules.Map.Services
{
    // CROSS-MODULE EDIT: 2025-11-28 Task 26 - Global Data Type Filtering
    // Modified for: DEPRECATED - All methods moved to Core.Services.DataTypeSettingsExtensions
    // Related modules: Map (this file), Core (DataTypeSettingsExtensions)
    // Rollback instructions: Restore original Map-only implementation

    /// <summary>
    /// Extension methods for accessing Map module settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DEPRECATED: This class is obsolete and kept only for backward compatibility.
    /// All functionality has been moved to <see cref="DataTypeSettingsExtensions"/> in EasyAF.Core.
    /// </para>
    /// <para>
    /// <strong>DO NOT ADD NEW METHODS HERE.</strong> Use Core.Services.DataTypeSettingsExtensions directly.
    /// </para>
    /// <para>
    /// This file will be removed in a future version once all references are migrated.
    /// </para>
    /// </remarks>
    [System.Obsolete("Use EasyAF.Core.Services.DataTypeSettingsExtensions instead", false)]
    public static class MapSettingsExtensions
    {
        // All methods removed - use DataTypeSettingsExtensions in EasyAF.Core instead
        // This class is kept as an empty shell to avoid breaking existing using statements
    }
}
