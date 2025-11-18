using System;

namespace EasyAF.Data.Attributes;

/// <summary>
/// Maps an EasyAF model class to its corresponding EasyPower export class name.
/// Used for automatic table detection and assignment when creating import mappings.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong>
/// EasyPower exports data with a "class" column that identifies the equipment type
/// (e.g., "Buses", "LV Breakers", "Cables"). This attribute correlates EasyAF's
/// internal class names (short, clean) to EasyPower's export names (verbose, exact).
/// </para>
/// <para>
/// <strong>Use Case: Auto-Detection in Map Module</strong>
/// When a user loads a sample file with a "class" column:
/// </para>
/// <list type="number">
/// <item><description>Map module reads the class value (e.g., "Buses")</description></item>
/// <item><description>PropertyDiscoveryService finds the class with [EasyPowerClass("Buses")]</description></item>
/// <item><description>Auto-suggests mapping to the Bus data type</description></item>
/// </list>
/// <para>
/// <strong>Class Name Mapping:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>EasyAF Class</term>
/// <description>EasyPower Class Name</description>
/// </listheader>
/// <item>
/// <term>Bus</term>
/// <description>"Buses"</description>
/// </item>
/// <item>
/// <term>LVCB</term>
/// <description>"LV Breakers"</description>
/// </item>
/// <item>
/// <term>Cable</term>
/// <description>"Cables"</description>
/// </item>
/// <item>
/// <term>Fuse</term>
/// <description>"Fuses"</description>
/// </item>
/// <item>
/// <term>ArcFlash</term>
/// <description>"Arc Flash Scenario Report"</description>
/// </item>
/// <item>
/// <term>ShortCircuit</term>
/// <description>"Equipment Duty Scenario Report"</description>
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// [EasyPowerClass("Buses")]
/// public class Bus
/// {
///     [Category("Identity")]
///     [Required]
///     public string? Id { get; set; }
///     // ...
/// }
/// 
/// // Usage in PropertyDiscoveryService:
/// var busType = typeof(Bus);
/// var attr = busType.GetCustomAttribute&lt;EasyPowerClassAttribute&gt;();
/// Console.WriteLine(attr.EasyPowerClassName); // Output: "Buses"
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class EasyPowerClassAttribute : Attribute
{
    /// <summary>
    /// Gets the EasyPower export class name for this model.
    /// </summary>
    /// <remarks>
    /// This should exactly match the value in the "class" column of EasyPower CSV exports.
    /// Case-sensitive comparison recommended.
    /// </remarks>
    public string EasyPowerClassName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EasyPowerClassAttribute"/> class.
    /// </summary>
    /// <param name="easyPowerClassName">
    /// The exact class name as it appears in EasyPower exports (e.g., "Buses", "LV Breakers").
    /// Should match the value in the CSV "class" column exactly (case-sensitive).
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="easyPowerClassName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="easyPowerClassName"/> is empty or whitespace.</exception>
    public EasyPowerClassAttribute(string easyPowerClassName)
    {
        if (easyPowerClassName == null)
            throw new ArgumentNullException(nameof(easyPowerClassName));
        if (string.IsNullOrWhiteSpace(easyPowerClassName))
            throw new ArgumentException("EasyPower class name cannot be empty or whitespace.", nameof(easyPowerClassName));
        
        EasyPowerClassName = easyPowerClassName.Trim();
    }
}
