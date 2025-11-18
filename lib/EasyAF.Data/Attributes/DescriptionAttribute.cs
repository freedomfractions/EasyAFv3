using System;

namespace EasyAF.Data.Attributes;

/// <summary>
/// Provides a human-readable description of a property's purpose and usage.
/// Typically displayed as tooltips in the UI or used in documentation generation.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Best Practices:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Keep descriptions concise (1-2 sentences)</description></item>
/// <item><description>Reference the original EasyPower column name in the description for traceability</description></item>
/// <item><description>Explain the property's purpose, not just repeat the name</description></item>
/// <item><description>Use sentence case (capitalize first word only)</description></item>
/// </list>
/// <para>
/// <strong>Description vs. XML Comments:</strong>
/// - XML comments (`/// &lt;summary&gt;`) are for developers reading the code
/// - Description attributes are for END USERS seeing properties in the UI
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Good descriptions:
/// [Category("Electrical")]
/// [Units("kV")]
/// [Description("Nominal voltage rating of the bus")]
/// public string? Voltage { get; set; }
/// 
/// [Category("Protection")]
/// [Units("kA")]
/// [Description("Available short-circuit interrupting capacity (ANSI standard)")]
/// public string? AIC { get; set; }
/// 
/// [Category("Study Results")]
/// [Units("cal/cm²")]
/// [Description("Calculated incident energy at working distance")]
/// public string? IncidentEnergy { get; set; }
/// 
/// // Poor descriptions (too terse or redundant):
/// [Description("Voltage")]  // ? Just repeats property name
/// public string? Voltage { get; set; }
/// 
/// [Description("The nominal voltage rating of the electrical bus in kilovolts as specified by the manufacturer...")]  // ? Too verbose
/// public string? Voltage { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class DescriptionAttribute : Attribute
{
    /// <summary>
    /// Gets the description text for this property.
    /// </summary>
    /// <remarks>
    /// Typically displayed as tooltip text in the UI or used in generated documentation.
    /// Should be concise (1-2 sentences) and user-friendly.
    /// </remarks>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class.
    /// </summary>
    /// <param name="description">
    /// The description text. Should be concise and user-friendly.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="description"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="description"/> is empty or whitespace.</exception>
    public DescriptionAttribute(string description)
    {
        if (description == null)
            throw new ArgumentNullException(nameof(description));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty or whitespace.", nameof(description));
        
        Description = description.Trim();
    }
}
