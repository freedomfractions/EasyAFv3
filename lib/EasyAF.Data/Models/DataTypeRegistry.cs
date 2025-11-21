using System.Collections.Generic;
using System.Linq;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Static registry of all known data types in the DataSet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides metadata for the Project Editor UI:
    /// - User-friendly display names
    /// - Segoe MDL2 Assets icons
    /// - Scenario support flags
    /// - Property names for reflection access
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// <code>
    /// var allTypes = DataTypeRegistry.GetAllDataTypes();
    /// var arcFlash = DataTypeRegistry.GetDataType("ArcFlash");
    /// var scenarioAwareTypes = DataTypeRegistry.GetScenarioAwareDataTypes();
    /// </code>
    /// </para>
    /// </remarks>
    public static class DataTypeRegistry
    {
        private static readonly List<DataTypeInfo> _allDataTypes;

        /// <summary>
        /// Static constructor initializes the registry with all known data types.
        /// </summary>
        static DataTypeRegistry()
        {
            _allDataTypes = new List<DataTypeInfo>
            {
                // === CALCULATION RESULTS (Scenario-Aware) ===
                new DataTypeInfo(
                    name: "ArcFlash",
                    displayName: "Arc Flash",
                    icon: "\uE945", // Lightning bolt
                    hasScenarios: true,
                    propertyName: "ArcFlashEntries",
                    category: DataTypeCategory.Calculation
                ),
                new DataTypeInfo(
                    name: "ShortCircuit",
                    displayName: "Short Circuit",
                    icon: "\uE7EF", // Warning triangle
                    hasScenarios: true,
                    propertyName: "ShortCircuitEntries",
                    category: DataTypeCategory.Calculation
                ),

                // === EQUIPMENT (Alphabetically, No Scenarios) ===
                new DataTypeInfo(
                    name: "AFD",
                    displayName: "Adjustable Frequency Drives",
                    icon: "\uE950", // Gears
                    hasScenarios: false,
                    propertyName: "AFDEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "ATS",
                    displayName: "Automatic Transfer Switches",
                    icon: "\uE8AB", // Refresh/switch
                    hasScenarios: false,
                    propertyName: "ATSEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Battery",
                    displayName: "Batteries",
                    icon: "\uE996", // Battery
                    hasScenarios: false,
                    propertyName: "BatteryEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Bus",
                    displayName: "Buses",
                    icon: "\uE8B8", // Circuit board
                    hasScenarios: false,
                    propertyName: "BusEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Busway",
                    displayName: "Busways",
                    icon: "\uE8BE", // Cable/connector
                    hasScenarios: false,
                    propertyName: "BuswayEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Cable",
                    displayName: "Cables",
                    icon: "\uE8BE", // Cable
                    hasScenarios: false,
                    propertyName: "CableEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Capacitor",
                    displayName: "Capacitors",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "CapacitorEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "CLReactor",
                    displayName: "Current Limiting Reactors",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "CLReactorEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "CT",
                    displayName: "Current Transformers",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "CTEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Filter",
                    displayName: "Filters",
                    icon: "\uE71C", // Filter funnel
                    hasScenarios: false,
                    propertyName: "FilterEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Fuse",
                    displayName: "Fuses",
                    icon: "\uE7F4", // Switch/circuit
                    hasScenarios: false,
                    propertyName: "FuseEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Generator",
                    displayName: "Generators",
                    icon: "\uE945", // Power symbol
                    hasScenarios: false,
                    propertyName: "GeneratorEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "HVBreaker",
                    displayName: "HV Breakers",
                    icon: "\uE7F4", // Switch
                    hasScenarios: false,
                    propertyName: "HVBreakerEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Inverter",
                    displayName: "Inverters",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "InverterEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Load",
                    displayName: "Loads",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "LoadEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "LVBreaker",
                    displayName: "LV Breakers",
                    icon: "\uE7F4", // Switch
                    hasScenarios: false,
                    propertyName: "LVBreakerEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "MCC",
                    displayName: "Motor Control Centers",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "MCCEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Meter",
                    displayName: "Meters",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "MeterEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Motor",
                    displayName: "Motors",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "MotorEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Panel",
                    displayName: "Panels",
                    icon: "\uE8B8", // Circuit board
                    hasScenarios: false,
                    propertyName: "PanelEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Photovoltaic",
                    displayName: "Photovoltaic Arrays",
                    icon: "\uE706", // Sun/solar
                    hasScenarios: false,
                    propertyName: "PhotovoltaicEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "POC",
                    displayName: "Points of Connection",
                    icon: "\uE8BE", // Connection point
                    hasScenarios: false,
                    propertyName: "POCEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Rectifier",
                    displayName: "Rectifiers",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "RectifierEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Relay",
                    displayName: "Relays",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "RelayEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Shunt",
                    displayName: "Shunts",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "ShuntEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Switch",
                    displayName: "Switches",
                    icon: "\uE7F4", // Switch
                    hasScenarios: false,
                    propertyName: "SwitchEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Transformer2W",
                    displayName: "2-Winding Transformers",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "Transformer2WEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Transformer3W",
                    displayName: "3-Winding Transformers",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "Transformer3WEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "TransmissionLine",
                    displayName: "Transmission Lines",
                    icon: "\uE8BE", // Cable
                    hasScenarios: false,
                    propertyName: "TransmissionLineEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "UPS",
                    displayName: "Uninterruptible Power Supplies",
                    icon: "\uE996", // Battery/power
                    hasScenarios: false,
                    propertyName: "UPSEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "Utility",
                    displayName: "Utilities",
                    icon: "\uE945", // Power symbol
                    hasScenarios: false,
                    propertyName: "UtilityEntries",
                    category: DataTypeCategory.Equipment
                ),
                new DataTypeInfo(
                    name: "ZigzagTransformer",
                    displayName: "Zigzag Transformers",
                    icon: "\uE950", // Generic component
                    hasScenarios: false,
                    propertyName: "ZigzagTransformerEntries",
                    category: DataTypeCategory.Equipment
                ),
            };
        }

        /// <summary>
        /// Gets all registered data types.
        /// </summary>
        /// <returns>List of all DataTypeInfo objects.</returns>
        /// <remarks>
        /// <para>
        /// Returns the complete list of known data types (36 total):
        /// - 2 scenario-aware (ArcFlash, ShortCircuit)
        /// - 34 equipment types
        /// </para>
        /// <para>
        /// Order: Calculation types first, then equipment alphabetically.
        /// </para>
        /// </remarks>
        public static List<DataTypeInfo> GetAllDataTypes()
        {
            return _allDataTypes.ToList(); // Return copy to prevent modification
        }

        /// <summary>
        /// Gets metadata for a specific data type by name.
        /// </summary>
        /// <param name="name">The data type name (case-insensitive).</param>
        /// <returns>DataTypeInfo if found; null otherwise.</returns>
        /// <remarks>
        /// <para>
        /// <strong>Example:</strong>
        /// <code>
        /// var info = DataTypeRegistry.GetDataType("ArcFlash");
        /// // Result: DataTypeInfo with DisplayName="Arc Flash", HasScenarios=true
        /// </code>
        /// </para>
        /// <para>
        /// Case-insensitive: "arcflash", "ArcFlash", "ARCFLASH" all match.
        /// </para>
        /// </remarks>
        public static DataTypeInfo? GetDataType(string name)
        {
            return _allDataTypes.FirstOrDefault(dt =>
                string.Equals(dt.Name, name, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all scenario-aware data types (ArcFlash, ShortCircuit).
        /// </summary>
        /// <returns>List of DataTypeInfo with HasScenarios=true.</returns>
        /// <remarks>
        /// <para>
        /// Used to determine which data types support scenario expansion in tree view.
        /// Currently returns 2 types: ArcFlash and ShortCircuit.
        /// </para>
        /// </remarks>
        public static List<DataTypeInfo> GetScenarioAwareDataTypes()
        {
            return _allDataTypes.Where(dt => dt.HasScenarios).ToList();
        }

        /// <summary>
        /// Gets all equipment data types (no scenarios).
        /// </summary>
        /// <returns>List of DataTypeInfo with HasScenarios=false.</returns>
        /// <remarks>
        /// <para>
        /// Returns 34 equipment types (Bus, LVBreaker, Fuse, Cable, etc.).
        /// Ordered alphabetically by Name.
        /// </para>
        /// </remarks>
        public static List<DataTypeInfo> GetEquipmentDataTypes()
        {
            return _allDataTypes
                .Where(dt => !dt.HasScenarios)
                .OrderBy(dt => dt.DisplayName)
                .ToList();
        }

        /// <summary>
        /// Gets data types by category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>List of DataTypeInfo matching the category.</returns>
        public static List<DataTypeInfo> GetDataTypesByCategory(DataTypeCategory category)
        {
            return _allDataTypes.Where(dt => dt.Category == category).ToList();
        }

        /// <summary>
        /// Checks if a data type exists in the registry.
        /// </summary>
        /// <param name="name">The data type name (case-insensitive).</param>
        /// <returns>True if the data type is registered; false otherwise.</returns>
        public static bool IsKnownDataType(string name)
        {
            return GetDataType(name) != null;
        }

        /// <summary>
        /// Checks if a data type supports scenarios.
        /// </summary>
        /// <param name="name">The data type name.</param>
        /// <returns>True if the data type has scenarios; false otherwise or if not found.</returns>
        public static bool HasScenarios(string name)
        {
            var info = GetDataType(name);
            return info?.HasScenarios ?? false;
        }
    }
}
