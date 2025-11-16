namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents a property on a target data type that can be mapped to a source column.
    /// </summary>
    /// <remarks>
    /// This class is used to display available properties from EasyAF.Data.Models classes
    /// (like Bus, LVCB, ArcFlash, etc.) in the mapping UI.
    /// </remarks>
    public class PropertyInfo
    {
        /// <summary>
        /// Gets or sets the name of the property (e.g., "Id", "Voltage", "Manufacturer").
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data type this property belongs to (e.g., "Bus", "LVCB").
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the C# type of the property (e.g., "String", "Int32", "Double").
        /// </summary>
        /// <remarks>
        /// Most EasyAF properties are strings to preserve raw data from source files.
        /// </remarks>
        public string PropertyType { get; set; } = "string";

        /// <summary>
        /// Gets or sets an optional description extracted from XML documentation comments.
        /// </summary>
        /// <remarks>
        /// This helps users understand what each property represents when mapping.
        /// For example: "Unique identifier / name of the bus. (Column: Buses or Bus Name)"
        /// </remarks>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether this property has been mapped to a source column.
        /// </summary>
        public bool IsMapped { get; set; }

        /// <summary>
        /// Gets or sets the source column this property is mapped to (if any).
        /// </summary>
        public string? MappedColumn { get; set; }
    }
}
