using System;

namespace EasyAF.Data.Attributes;

/// <summary>
/// Marks a property as required for successful data import and processing.
/// Properties marked as required should be validated during mapping and import operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong>
/// Required properties are essential for:
/// - Uniquely identifying records (e.g., Id, Bus)
/// - Performing calculations (e.g., Voltage for power calculations)
/// - Ensuring data integrity (e.g., Scenario for study results)
/// </para>
/// <para>
/// <strong>Validation Behavior:</strong>
/// - Map module will show warnings if required properties are not mapped
/// - Import engine should reject records with missing required fields
/// - Diff operations highlight changes to required fields more prominently
/// </para>
/// <para>
/// <strong>Guidelines for Marking Properties as Required:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Id/Name properties are typically required (unique identifiers)</description></item>
/// <item><description>Properties used in composite keys are required (e.g., Scenario in ArcFlash)</description></item>
/// <item><description>Properties essential for calculations are required (e.g., Voltage, Phases)</description></item>
/// <item><description>Don't over-use - only mark truly critical properties as required</description></item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> This attribute is read-only and thread-safe.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Bus example:
/// [Category("Identity")]
/// [Description("Unique identifier for the bus")]
/// [Required]  // ? Must be present for all buses
/// public string? Id { get; set; }
/// 
/// [Category("Electrical")]
/// [Units("kV")]
/// [Description("Nominal voltage rating")]
/// [Required]  // ? Essential for calculations
/// public string? Voltage { get; set; }
/// 
/// [Category("Electrical")]
/// [Description("Number of phases (1, 2, or 3)")]
/// [Required]  // ? Essential for fault calculations
/// public string? Phases { get; set; }
/// 
/// [Category("Physical")]
/// [Description("Equipment manufacturer")]
/// // NOT required - manufacturer is nice to have but not essential
/// public string? Manufacturer { get; set; }
/// 
/// // ArcFlash example (composite key):
/// [Category("Identity")]
/// [Description("Bus or location identifier")]
/// [Required]  // ? Part of composite key (Id, Scenario)
/// public string? Id { get; set; }
/// 
/// [Category("Study Results")]
/// [Description("Scenario name (e.g., Main-Min, Main-Max)")]
/// [Required]  // ? Part of composite key (Id, Scenario)
/// public string? Scenario { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class RequiredAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredAttribute"/> class.
    /// This is a marker attribute with no additional parameters.
    /// </summary>
    public RequiredAttribute()
    {
        // Marker attribute - no additional data needed
    }
}
