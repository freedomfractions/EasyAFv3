using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a CT with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "CTs" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("CTs")]
public class CT
{
    /// <summary>CTs (Column: CTs)</summary>
    [Category("General")]
    [Description("CTs")]
    [Required]
    public string? CTs { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Item Connection (Column: Item Connection)</summary>
    [Category("General")]
    [Description("Item Connection")]
    public string? ItemConnection { get; set; }

    /// <summary>Bus Connection (Column: Bus Connection)</summary>
    [Category("Identity")]
    [Description("Bus Connection")]
    public string? BusConnection { get; set; }

    /// <summary>CT Function (Column: CT Function)</summary>
    [Category("General")]
    [Description("CT Function")]
    public string? CTFunction { get; set; }

    /// <summary>Connected (Column: Connected)</summary>
    [Category("General")]
    [Description("Connected")]
    public string? Connected { get; set; }

    /// <summary>No. of CTs (Column: No. of CTs)</summary>
    [Category("General")]
    [Description("No. of CTs")]
    public string? NoOfCTs { get; set; }

    /// <summary>Full CT Ratio (Column: Full CT Ratio)</summary>
    [Category("General")]
    [Description("Full CT Ratio")]
    public string? FullCTRatio { get; set; }

    /// <summary>Set CT Ratio (Column: Set CT Ratio)</summary>
    [Category("General")]
    [Description("Set CT Ratio")]
    public string? SetCTRatio { get; set; }

    /// <summary>One-line Graphics (Column: One-line Graphics)</summary>
    [Category("General")]
    [Description("One-line Graphics")]
    public string? OnelineGraphics { get; set; }

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for CTs (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => CTs;
        set => CTs = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CT"/> class.
    /// </summary>
    public CT() { }

    /// <summary>
    /// Returns a string representation of the CT.
    /// </summary>
    public override string ToString()
    {
        return $"CT: {CTs}";
    }
}

