using System;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Metadata describing a data type available in the DataSet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by the Project Editor to:
    /// - Display data types in tree view
    /// - Show user-friendly names and icons
    /// - Determine which data types support scenarios
    /// - Route import operations to correct dictionary
    /// </para>
    /// <para>
    /// <strong>Scenario Support:</strong> Only ArcFlash and ShortCircuit have scenario support
    /// (composite keys). Equipment types (Bus, LVBreaker, Fuse, etc.) do NOT have scenarios.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var arcFlashInfo = new DataTypeInfo
    /// {
    ///     Name = "ArcFlash",
    ///     DisplayName = "Arc Flash",
    ///     Icon = "\uE945", // Lightning bolt
    ///     HasScenarios = true,
    ///     Category = DataTypeCategory.Calculation,
    ///     PropertyName = "ArcFlashEntries"
    /// };
    /// </code>
    /// </example>
    public class DataTypeInfo
    {
        /// <summary>
        /// Internal class name (e.g., "ArcFlash", "Bus", "LVBreaker").
        /// </summary>
        /// <remarks>
        /// Must match the C# class name in EasyAF.Data.Models namespace.
        /// Used for reflection-based property access.
        /// </remarks>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User-friendly display name (e.g., "Arc Flash", "Buses", "LV Breakers").
        /// </summary>
        /// <remarks>
        /// Shown in Project Summary tree view and dialog titles.
        /// Can include spaces and punctuation for readability.
        /// </remarks>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Segoe MDL2 Assets icon glyph (e.g., "\uE945" for lightning).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used in tree view and tabs to visually identify data types.
        /// Common glyphs:
        /// - \uE945: Lightning (Arc Flash)
        /// - \uE7EF: Warning triangle (Short Circuit)
        /// - \uE8B8: Bus/circuit board (Buses)
        /// - \uE7F4: Switch (LV Breakers, Fuses)
        /// - \uE8BE: Cable (Cables)
        /// </para>
        /// <para>
        /// See: https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font
        /// </para>
        /// </remarks>
        public string Icon { get; set; } = "\uE8B8"; // Default: circuit board

        /// <summary>
        /// Indicates whether this data type supports multiple scenarios (composite keys).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>TRUE for:</strong>
        /// - ArcFlash (keyed by Id, Scenario)
        /// - ShortCircuit (keyed by Id, Bus, Scenario)
        /// </para>
        /// <para>
        /// <strong>FALSE for:</strong>
        /// - All equipment types (Bus, LVBreaker, Fuse, Cable, etc.)
        /// - Equipment data is NOT scenario-specific (same equipment across scenarios)
        /// </para>
        /// <para>
        /// When TRUE, the Project Summary tree view will allow expansion to show
        /// per-scenario statistics. When FALSE, only aggregate statistics are shown.
        /// </para>
        /// </remarks>
        public bool HasScenarios { get; set; } = false;

        /// <summary>
        /// Property name on DataSet class (e.g., "ArcFlashEntries", "BusEntries").
        /// </summary>
        /// <remarks>
        /// Used for reflection-based access to DataSet dictionaries.
        /// Must match the property name exactly (case-sensitive).
        /// Example: DataSet.GetType().GetProperty(PropertyName)
        /// </remarks>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Logical grouping category for data type.
        /// </summary>
        /// <remarks>
        /// Used to organize data types in UI (e.g., "Calculation Results" vs "Equipment").
        /// Can be used for future filtering or grouping in tree view.
        /// </remarks>
        public DataTypeCategory Category { get; set; } = DataTypeCategory.Equipment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeInfo"/> class.
        /// </summary>
        public DataTypeInfo() { }

        /// <summary>
        /// Initializes a new instance with all properties.
        /// </summary>
        public DataTypeInfo(string name, string displayName, string icon, bool hasScenarios, string propertyName, DataTypeCategory category)
        {
            Name = name;
            DisplayName = displayName;
            Icon = icon;
            HasScenarios = hasScenarios;
            PropertyName = propertyName;
            Category = category;
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString() => $"{DisplayName} ({Name}) - Scenarios: {HasScenarios}";
    }

    /// <summary>
    /// Logical categories for data types.
    /// </summary>
    public enum DataTypeCategory
    {
        /// <summary>Equipment data (Bus, LVBreaker, Fuse, Cable, etc.).</summary>
        Equipment,

        /// <summary>Calculation results (ArcFlash, ShortCircuit).</summary>
        Calculation,

        /// <summary>Protection devices (future: relays, coordination data).</summary>
        Protection
    }
}
