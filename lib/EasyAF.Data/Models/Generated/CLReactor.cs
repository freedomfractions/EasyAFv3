using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a CLReactor with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "CL Reactors" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("CL Reactors")]
public class CLReactor
{
    /// <summary>CL Reactors (Column: CL Reactors)</summary>
    [Category("General")]
    [Description("CL Reactors")]
    [Required]
    public string? CLReactors { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>From Bus ID (Column: From Bus ID)</summary>
    [Category("Identity")]
    [Description("From Bus ID")]
    public string? FromBusID { get; set; }

    /// <summary>From Device ID (Column: From Device ID)</summary>
    [Category("General")]
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

    /// <summary>To Base kV (Column: To Base kV)</summary>
    [Category("Electrical")]
    [Description("To Base kV")]
    public string? ToBaseKV { get; set; }

    /// <summary>Ampacity (Column: Ampacity)</summary>
    [Category("Electrical")]
    [Description("Ampacity")]
    public string? Ampacity { get; set; }

    /// <summary>Imped kVA (Column: Imped kVA)</summary>
    [Category("Electrical")]
    [Description("Imped kVA")]
    [Required]
    public string? ImpedkVA { get; set; }

    /// <summary>Material (Column: Material)</summary>
    [Category("General")]
    [Description("Material")]
    public string? Material { get; set; }

    /// <summary>X1 (Column: X1)</summary>
    [Category("General")]
    [Description("X1")]
    public string? X1 { get; set; }

    /// <summary>X0 (Column: X0)</summary>
    [Category("General")]
    [Description("X0")]
    public string? X0 { get; set; }

    /// <summary>X/R (Column: X/R)</summary>
    [Category("General")]
    [Description("X/R")]
    public string? XR { get; set; }

    /// <summary>X1 pu (Column: X1 pu)</summary>
    [Category("General")]
    [Description("X1 pu")]
    public string? X1pu { get; set; }

    /// <summary>X0 pu (Column: X0 pu)</summary>
    [Category("General")]
    [Description("X0 pu")]
    public string? X0pu { get; set; }

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

    /// <summary>Failure Rate (Column: Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Description("Failure Rate")]
    [Units("/year")]
    public string? FailureRatePerYear { get; set; }

    /// <summary>Repair Time (Column: Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Repair Time")]
    [Units("h")]
    public string? RepairTimeH { get; set; }

    /// <summary>Replace Time (Column: Replace Time (h))</summary>
    [Category("Reliability")]
    [Description("Replace Time")]
    [Units("h")]
    public string? ReplaceTimeH { get; set; }

    /// <summary>Repair Cost (Column: Repair Cost)</summary>
    [Category("Reliability")]
    [Description("Repair Cost")]
    public string? RepairCost { get; set; }

    /// <summary>Replace Cost (Column: Replace Cost)</summary>
    [Category("Reliability")]
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

    /// <summary>Alias for CLReactors (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => CLReactors;
        set => CLReactors = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CLReactor"/> class.
    /// </summary>
    public CLReactor() { }

    /// <summary>
    /// Returns a string representation of the CLReactor.
    /// </summary>
    public override string ToString()
    {
        return $"CLReactor: {CLReactors}";
    }
}

