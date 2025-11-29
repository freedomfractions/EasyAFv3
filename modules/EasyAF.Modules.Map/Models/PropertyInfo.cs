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
        private double? _confidence;

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

        /// <summary>
        /// Gets or sets whether this is a computed property (not from CSV/import data).
        /// </summary>
        /// <remarks>
        /// <para>
        /// CROSS-MODULE EDIT: 2025-01-19 Computed Properties Support
        /// Modified for: Identify computed properties for optional filtering in Spec module
        /// Related modules: Data (LVBreaker.Computed.cs), Spec (PropertyPathPickerViewModel)
        /// Rollback instructions: Remove this property
        /// </para>
        /// <para>
        /// Computed properties:
        /// - Are marked with [Category("Computed")] attribute in the model
        /// - Are calculated at runtime (e.g., LVBreaker.IsAdjustable)
        /// - Are NOT part of CSV/Excel imports
        /// - Are useful for filtering and sorting in reports
        /// - Can be optionally hidden in property pickers via IncludeComputedProperties flag
        /// </para>
        /// </remarks>
        public bool IsComputed { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence score for this mapping (0.0 to 1.0), or null if not applicable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// CROSS-MODULE EDIT: 2025-01-26 Auto-Map Confidence Badges
        /// Modified for: Display confidence badges on auto-mapped properties
        /// Related modules: Map (DataTypeMappingView XAML, ConfidenceToColorConverter)
        /// Rollback instructions: Remove this property and associated UI elements
        /// </para>
        /// <para>
        /// Used to show visual feedback about auto-map quality:
        /// - null: Manually mapped or no confidence info
        /// - 0.9-1.0: High confidence (green badge showing "95%")
        /// - 0.7-0.89: Medium confidence (yellow badge showing "75%")
        /// - 0.6-0.69: Low confidence (yellow badge showing "65%")
        /// </para>
        /// <para>
        /// The badge fades out after save or manual edit to avoid visual clutter.
        /// </para>
        /// </remarks>
        public double? Confidence
        {
            get => _confidence;
            set => SetProperty(ref _confidence, value);
        }
    }
}
