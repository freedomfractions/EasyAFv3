using Prism.Mvvm;

namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents metadata about a property that can be mapped.
    /// </summary>
    /// <remarks>
    /// This is used in the UI to display property information and track mapping state.
    /// Note: This is NOT the same as EasyAF.Data properties - this is UI metadata.
    /// </remarks>
    public class PropertyInfo : BindableBase
    {
        private bool _isMapped;
        private string? _mappedColumn;

        /// <summary>
        /// Gets or sets the property name (e.g., "Name", "kV", "TripUnit.Rating").
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property's data type (e.g., "String", "Double", "Int32").
        /// </summary>
        public string PropertyType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional description of the property's purpose.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether this property is currently mapped to a column.
        /// </summary>
        public bool IsMapped
        {
            get => _isMapped;
            set => SetProperty(ref _isMapped, value);
        }

        /// <summary>
        /// Gets or sets the name of the column this property is mapped to.
        /// Null if not mapped.
        /// </summary>
        public string? MappedColumn
        {
            get => _mappedColumn;
            set => SetProperty(ref _mappedColumn, value);
        }

        /// <summary>
        /// Gets or sets whether this property is required for a valid mapping configuration.
        /// </summary>
        /// <remarks>
        /// <para>
        /// CROSS-MODULE EDIT: 2025-01-16 Required Property Validation
        /// Modified for: Mark critical properties as required (e.g., Id, Name, kV)
        /// Related modules: Map (PropertyDiscoveryService - defines required rules)
        /// Rollback instructions: Remove this property and all required property logic
        /// </para>
        /// <para>
        /// Required properties are determined by:
        /// - Universal requirements (Id, Name for all types)
        /// - Type-specific defaults (e.g., Bus.kV, ArcFlash.Scenario)
        /// - Optional settings override (future: user-configurable)
        /// </para>
        /// <para>
        /// UI Impact: Required properties display with a red asterisk (*) indicator.
        /// Validation Impact: Saving a map with unmapped required properties shows a warning.
        /// </para>
        /// </remarks>
        public bool IsRequired { get; set; }
    }
}
