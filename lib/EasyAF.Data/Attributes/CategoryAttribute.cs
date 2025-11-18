using System;

namespace EasyAF.Data.Attributes;

/// <summary>
/// Specifies the logical category/grouping for a property in the EasyAF data model.
/// Used by PropertyDiscoveryService to organize properties for UI display and filtering.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Standard Categories:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Identity</strong> - Unique identifiers, names, device codes</description></item>
/// <item><description><strong>Electrical</strong> - Voltage, current, phases, frequency</description></item>
/// <item><description><strong>Physical</strong> - Manufacturer, type, style, model, size</description></item>
/// <item><description><strong>Protection</strong> - Ratings, trip settings, fuses, interrupting capacity</description></item>
/// <item><description><strong>Location</strong> - Area, zone, facility, building, coordinates</description></item>
/// <item><description><strong>Reliability</strong> - Failure rates, repair costs, MTTR, replacement costs</description></item>
/// <item><description><strong>Study Results</strong> - Arc flash, short circuit, load flow calculations</description></item>
/// <item><description><strong>Metadata</strong> - Comments, data status, labels, timestamps</description></item>
/// </list>
/// <para>
/// Custom categories can be defined as needed for specific equipment types.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [Category("Electrical")]
/// [Units("kV")]
/// [Description("Nominal voltage rating of the bus")]
/// [Required]
/// public string? Voltage { get; set; }
/// 
/// [Category("Physical")]
/// [Description("Manufacturer of the equipment")]
/// public string? Manufacturer { get; set; }
/// 
/// [Category("Protection")]
/// [Units("kA")]
/// [Description("Available interrupting capacity")]
/// public string? AIC { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class CategoryAttribute : Attribute
{
    /// <summary>
    /// Gets the category name for this property.
    /// </summary>
    /// <remarks>
    /// Category names should be singular nouns (e.g., "Electrical" not "Electrical Properties").
    /// Use consistent casing (Title Case recommended).
    /// </remarks>
    public string Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryAttribute"/> class.
    /// </summary>
    /// <param name="category">The category name (e.g., "Electrical", "Physical", "Protection").</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="category"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="category"/> is empty or whitespace.</exception>
    public CategoryAttribute(string category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty or whitespace.", nameof(category));
        
        Category = category.Trim();
    }
}
