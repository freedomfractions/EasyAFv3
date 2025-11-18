using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a ZigzagTransformer with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Zigzag Transformers" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Zigzag Transformers")]
public class ZigzagTransformer
{
    /// <summary>Zigzag Transformers (Column: Zigzag Transformers)</summary>
    [Category("Identity")]
    [Description("Zigzag Transformers")]
    [Required]
    public string? ZigzagTransformers { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>From Bus ID (Column: From Bus ID)</summary>
    [Category("Electrical")]
    [Description("From Bus ID")]
    public string? FromBusID { get; set; }

    /// <summary>From Device ID (Column: From Device ID)</summary>
    [Category("Identity")]
    [Description("From Device ID")]
    public string? FromDeviceID { get; set; }

    /// <summary>From Device Type (Column: From Device Type)</summary>
    [Category("Physical")]
    [Description("From Device Type")]
    public string? FromDeviceType { get; set; }

    /// <summary>From Base kV (Column: From Base kV)</summary>
    [Category("Electrical")]
    [Description("From Base kV")]
    public string? FromBaseKV { get; set; }

    /// <summary>Rating (kV) (Column: Rating (kV))</summary>
    [Category("Electrical")]
    [Description("Rating (kV)")]
    [Units("kV")]
    public string? Rating { get; set; }

    /// <summary>Imped (Ohm) (Column: Imped (Ohm))</summary>
    [Category("General")]
    [Description("Imped (Ohm)")]
    [Units("Ω")]
    public string? ImpedOhm { get; set; }

    /// <summary>RG Ohm (Column: RG Ohm)</summary>
    [Category("General")]
    [Description("RG Ohm")]
    public string? RgOhm { get; set; }

    /// <summary>XG Ohm (Column: XG Ohm)</summary>
    [Category("General")]
    [Description("XG Ohm")]
    public string? XgOhm { get; set; }

    /// <summary>3RG Ohm (Column: 3RG Ohm)</summary>
    [Category("General")]
    [Description("3RG Ohm")]
    public string? ThreergOhm { get; set; }

    /// <summary>3RG Ohm + X0 (Column: 3RG Ohm + X0)</summary>
    [Category("General")]
    [Description("3RG Ohm + X0")]
    public string? ThreergOhmPlusX0 { get; set; }

    /// <summary>R0 pu (Column: R0 pu)</summary>
    [Category("General")]
    [Description("R0 pu")]
    public string? R0Pu { get; set; }

    /// <summary>X0 pu (Column: X0 pu)</summary>
    [Category("General")]
    [Description("X0 pu")]
    public string? X0Pu { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRcFactor { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRcValue { get; set; }

    /// <summary>I Hrm Rating Setting (Column: I Hrm Rating Setting)</summary>
    [Category("Control")]
    [Description("I Hrm Rating Setting")]
    public string? IHrmRatingSetting { get; set; }

    /// <summary>I Hrm Rating (Column: I Hrm Rating)</summary>
    [Category("General")]
    [Description("I Hrm Rating")]
    public string? IHrmRating { get; set; }

    /// <summary>Failure Rate (/year) (Column: Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Description("Failure Rate (/year)")]
    [Units("/year")]
    public string? FailureRatePerYear { get; set; }

    /// <summary>Repair Time (h) (Column: Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Repair Time (h)")]
    [Units("h")]
    public string? RepairTimeHours { get; set; }

    /// <summary>Replace Time (h) (Column: Replace Time (h))</summary>
    [Category("General")]
    [Description("Replace Time (h)")]
    [Units("h")]
    public string? ReplaceTimeHours { get; set; }

    /// <summary>Repair Cost (Column: Repair Cost)</summary>
    [Category("Reliability")]
    [Description("Repair Cost")]
    public string? RepairCost { get; set; }

    /// <summary>Replace Cost (Column: Replace Cost)</summary>
    [Category("General")]
    [Description("Replace Cost")]
    public string? ReplaceCost { get; set; }

    /// <summary>Action Upon Failure (Column: Action Upon Failure)</summary>
    [Category("Reliability")]
    [Description("Action Upon Failure")]
    public string? ActionUponFailure { get; set; }

    /// <summary>Reliability Source (Column: Reliability Source)</summary>
    [Category("Reliability")]
    [Description("Reliability Source")]
    public string? ReliabilitySource { get; set; }

    /// <summary>Reliability Category (Column: Reliability Category)</summary>
    [Category("Reliability")]
    [Description("Reliability Category")]
    public string? ReliabilityCategory { get; set; }

    /// <summary>Reliability Class (Column: Reliability Class)</summary>
    [Category("Reliability")]
    [Description("Reliability Class")]
    public string? ReliabilityClass { get; set; }

    /// <summary>Facility (Column: Facility)</summary>
    [Category("Location")]
    [Description("Facility")]
    public string? Facility { get; set; }

    /// <summary>Location Name (Column: Location Name)</summary>
    [Category("Location")]
    [Description("Location Name")]
    public string? LocationName { get; set; }

    /// <summary>Location Description (Column: Location Description)</summary>
    [Category("Location")]
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
    [Category("Identity")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for ZigzagTransformer (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => ZigzagTransformers;
        set => ZigzagTransformers = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZigzagTransformer"/> class.
    /// </summary>
    public ZigzagTransformer() { }

    /// <summary>
    /// Returns a string representation of the ZigzagTransformer.
    /// </summary>
    public override string ToString()
    {
        return $"ZigzagTransformer: {ZigzagTransformers}";
    }
}

