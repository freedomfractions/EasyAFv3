using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Filter with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Filters" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Filters")]
public class Filter
{
    /// <summary>Filters (Column: Filters)</summary>
    [Category("General")]
    [Description("Filters")]
    [Required]
    public string? Filters { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>To Bus ID (Column: To Bus ID)</summary>
    [Category("Identity")]
    [Description("To Bus ID")]
    public string? ToBusID { get; set; }

    /// <summary>To Device ID (Column: To Device ID)</summary>
    [Category("General")]
    [Description("To Device ID")]
    public string? ToDeviceID { get; set; }

    /// <summary>To Device Type (Column: To Device Type)</summary>
    [Category("Physical")]
    [Description("To Device Type")]
    public string? ToDeviceType { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BaseKV { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>R (Column: R (Ohms))</summary>
    [Category("General")]
    [Description("R")]
    [Units("Ω")]
    public string? ROhms { get; set; }

    /// <summary>L-R (Column: L-R (Ohms))</summary>
    [Category("General")]
    [Description("L-R")]
    [Units("Ω")]
    public string? LROhms { get; set; }

    /// <summary>L-X (Column: L-X (Ohms))</summary>
    [Category("General")]
    [Description("L-X")]
    [Units("Ω")]
    public string? LXOhms { get; set; }

    /// <summary>C1 (MVAR) (Column: C1 (MVAR))</summary>
    [Category("General")]
    [Description("C1 (MVAR)")]
    [Required]
    [Units("MVAR")]
    public string? C1MVAR { get; set; }

    /// <summary>C1 (kV) (Column: C1 (kV))</summary>
    [Category("Electrical")]
    [Description("C1 (kV)")]
    [Units("kV")]
    public string? C1KV { get; set; }

    /// <summary>C2 (MVAR) (Column: C2 (MVAR))</summary>
    [Category("General")]
    [Description("C2 (MVAR)")]
    [Required]
    [Units("MVAR")]
    public string? C2MVAR { get; set; }

    /// <summary>C2 (kV) (Column: C2 (kV))</summary>
    [Category("Electrical")]
    [Description("C2 (kV)")]
    [Units("kV")]
    public string? C2KV { get; set; }

    /// <summary>G pu (R) (Column: G pu (R))</summary>
    [Category("General")]
    [Description("G pu (R)")]
    public string? GpuR { get; set; }

    /// <summary>G pu (L) (Column: G pu (L))</summary>
    [Category("General")]
    [Description("G pu (L)")]
    public string? GpuL { get; set; }

    /// <summary>B pu (L) (Column: B pu (L))</summary>
    [Category("General")]
    [Description("B pu (L)")]
    public string? BpuL { get; set; }

    /// <summary>MVAR pu (C1) (Column: MVAR pu (C1))</summary>
    [Category("General")]
    [Description("MVAR pu (C1)")]
    [Required]
    public string? MVARpuC1 { get; set; }

    /// <summary>MVAR pu (C2) (Column: MVAR pu (C2))</summary>
    [Category("General")]
    [Description("MVAR pu (C2)")]
    [Required]
    public string? MVARpuC2 { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRCFactor { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRCValue { get; set; }

    /// <summary>I Hrm Rating Setting (Column: I Hrm Rating Setting)</summary>
    [Category("Physical")]
    [Description("I Hrm Rating Setting")]
    public string? IHrmRatingSetting { get; set; }

    /// <summary>I Hrm Rating (Column: I Hrm Rating)</summary>
    [Category("Physical")]
    [Description("I Hrm Rating")]
    public string? IHrmRating { get; set; }

    /// <summary>Facility (Column: Facility)</summary>
    [Category("Location")]
    [Description("Facility")]
    public string? Facility { get; set; }

    /// <summary>Location Name (Column: Location Name)</summary>
    [Category("Location")]
    [Description("Location Name")]
    public string? LocationName { get; set; }

    /// <summary>Location Description (Column: Location Description)</summary>
    [Category("Metadata")]
    [Description("Location Description")]
    public string? LocationDescription { get; set; }

    /// <summary>X (Column: X)</summary>
    [Category("General")]
    [Description("X")]
    public string? X { get; set; }

    /// <summary>Y (Column: Y)</summary>
    [Category("General")]
    [Description("Y")]
    public string? Y { get; set; }

    /// <summary>Floor (Column: Floor)</summary>
    [Category("Location")]
    [Description("Floor")]
    public string? Floor { get; set; }

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for Filters (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Filters;
        set => Filters = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> class.
    /// </summary>
    public Filter() { }

    /// <summary>
    /// Returns a string representation of the Filter.
    /// </summary>
    public override string ToString()
    {
        return $"Filter: {Filters}";
    }
}

