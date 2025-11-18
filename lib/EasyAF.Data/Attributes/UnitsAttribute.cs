using System;

namespace EasyAF.Data.Attributes;

/// <summary>
/// Specifies the units of measurement for a property value.
/// Omit this attribute for unitless properties (e.g., Id, Name, Status, Manufacturer).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Design Philosophy:</strong>
/// Only apply this attribute to properties that represent physical measurements or quantities with units.
/// String properties without units (names, identifiers, status codes) should NOT have this attribute.
/// </para>
/// <para>
/// <strong>Common Units:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Category</term>
/// <description>Units</description>
/// </listheader>
/// <item>
/// <term>Voltage</term>
/// <description>"kV", "V"</description>
/// </item>
/// <item>
/// <term>Current</term>
/// <description>"A", "kA"</description>
/// </item>
/// <item>
/// <term>Power</term>
/// <description>"kVA", "kW", "MVA", "MW"</description>
/// </item>
/// <item>
/// <term>Time</term>
/// <description>"s", "ms", "cycles", "h"</description>
/// </item>
/// <item>
/// <term>Energy</term>
/// <description>"cal/cm²", "J", "kWh"</description>
/// </item>
/// <item>
/// <term>Length</term>
/// <description>"ft", "in", "m", "mm"</description>
/// </item>
/// <item>
/// <term>Percentage</term>
/// <description>"%"</description>
/// </item>
/// <item>
/// <term>Cost</term>
/// <description>"$", "USD"</description>
/// </item>
/// <item>
/// <term>Impedance</term>
/// <description>"?", "m?", "pu"</description>
/// </item>
/// <item>
/// <term>Rate</term>
/// <description>"/year", "/h"</description>
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Properties WITH units:
/// [Category("Electrical")]
/// [Units("kV")]
/// [Description("Nominal voltage rating")]
/// public string? Voltage { get; set; }
/// 
/// [Category("Protection")]
/// [Units("kA")]
/// [Description("Available interrupting capacity")]
/// public string? AIC { get; set; }
/// 
/// [Category("Study Results")]
/// [Units("cal/cm²")]
/// [Description("Calculated incident energy")]
/// public string? IncidentEnergy { get; set; }
/// 
/// // Properties WITHOUT units (NO ATTRIBUTE):
/// [Category("Identity")]
/// [Description("Unique identifier")]
/// public string? Id { get; set; }
/// 
/// [Category("Physical")]
/// [Description("Equipment manufacturer")]
/// public string? Manufacturer { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class UnitsAttribute : Attribute
{
    /// <summary>
    /// Gets the units of measurement for this property.
    /// </summary>
    /// <remarks>
    /// Examples: "kV", "A", "kA", "s", "cal/cm²", "%", "ft", "in".
    /// Use standard SI units or industry-standard abbreviations.
    /// </remarks>
    public string Units { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitsAttribute"/> class.
    /// </summary>
    /// <param name="units">
    /// The units string (e.g., "kV", "A", "kA", "s", "cal/cm²").
    /// Should be concise and use standard abbreviations.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="units"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="units"/> is empty or whitespace.</exception>
    public UnitsAttribute(string units)
    {
        if (units == null)
            throw new ArgumentNullException(nameof(units));
        if (string.IsNullOrWhiteSpace(units))
            throw new ArgumentException("Units cannot be empty or whitespace.", nameof(units));
        
        Units = units.Trim();
    }
}
