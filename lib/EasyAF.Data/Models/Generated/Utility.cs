using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an Utility with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Utilities" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Utilities")]
public class Utility
{
    /// <summary>Utilities (Column: Utilities)</summary>
    [Category("General")]
    [Description("Utilities")]
    [Required]
    public string? Utilities { get; set; }

    /// <summary>AC/DC (Column: AC/DC)</summary>
    [Category("General")]
    [Description("AC/DC")]
    public string? AcDc { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Service (Column: Service)</summary>
    [Category("General")]
    [Description("Service")]
    public string? Service { get; set; }

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

    /// <summary>Util kV (Column: Util kV)</summary>
    [Category("Electrical")]
    [Description("Util kV")]
    public string? UtilkV { get; set; }

    /// <summary>Fault Unit (Column: Fault Unit)</summary>
    [Category("Electrical")]
    [Description("Fault Unit")]
    public string? FaultUnit { get; set; }

    /// <summary>3Ph SC1 (Column: 3Ph SC1)</summary>
    [Category("General")]
    [Description("3Ph SC1")]
    public string? 3PhSC1 { get; set; }

    /// <summary>3Ph SC2 (Column: 3Ph SC2)</summary>
    [Category("General")]
    [Description("3Ph SC2")]
    public string? 3PhSC2 { get; set; }

    /// <summary>1PH SC1 (Column: 1PH SC1)</summary>
    [Category("General")]
    [Description("1PH SC1")]
    public string? 1PHSC1 { get; set; }

    /// <summary>1PH SC2 (Column: 1PH SC2)</summary>
    [Category("General")]
    [Description("1PH SC2")]
    public string? 1PHSC2 { get; set; }

    /// <summary>SLG SC1 (Column: SLG SC1)</summary>
    [Category("General")]
    [Description("SLG SC1")]
    public string? SLGSC1 { get; set; }

    /// <summary>SLG SC2 (Column: SLG SC2)</summary>
    [Category("General")]
    [Description("SLG SC2")]
    public string? SLGSC2 { get; set; }

    /// <summary>Model (Column: Model)</summary>
    [Category("Physical")]
    [Description("Model")]
    public string? Model { get; set; }

    /// <summary>MW (Column: MW)</summary>
    [Category("General")]
    [Description("MW")]
    public string? MW { get; set; }

    /// <summary>MVAR (Column: MVAR)</summary>
    [Category("General")]
    [Description("MVAR")]
    [Required]
    public string? MVAR { get; set; }

    /// <summary>Ctl kV pu (Column: Ctl kV pu)</summary>
    [Category("Electrical")]
    [Description("Ctl kV pu")]
    public string? CtlkVpu { get; set; }

    /// <summary>MVAR Min (Column: MVAR Min)</summary>
    [Category("General")]
    [Description("MVAR Min")]
    [Required]
    public string? MVARMin { get; set; }

    /// <summary>MVAR Max (Column: MVAR Max)</summary>
    [Category("General")]
    [Description("MVAR Max")]
    [Required]
    public string? MVARMax { get; set; }

    /// <summary>kV pu Min (Column: kV pu Min)</summary>
    [Category("Electrical")]
    [Description("kV pu Min")]
    public string? KVpuMin { get; set; }

    /// <summary>kV pu Max (Column: kV pu Max)</summary>
    [Category("Electrical")]
    [Description("kV pu Max")]
    public string? KVpuMax { get; set; }

    /// <summary>Ctl Angle (Column: Ctl Angle)</summary>
    [Category("General")]
    [Description("Ctl Angle")]
    public string? CtlAngle { get; set; }

    /// <summary>Ctl Bus (Column: Ctl Bus)</summary>
    [Category("Identity")]
    [Description("Ctl Bus")]
    public string? CtlBus { get; set; }

    /// <summary>Ctl Base kV (Column: Ctl Base kV)</summary>
    [Category("Electrical")]
    [Description("Ctl Base kV")]
    public string? CtlBasekV { get; set; }

    /// <summary>R1 pu (Column: R1 pu)</summary>
    [Category("General")]
    [Description("R1 pu")]
    public string? R1pu { get; set; }

    /// <summary>X1 pu (Column: X1 pu)</summary>
    [Category("General")]
    [Description("X1 pu")]
    public string? X1pu { get; set; }

    /// <summary>R0 pu (Column: R0 pu)</summary>
    [Category("General")]
    [Description("R0 pu")]
    public string? R0pu { get; set; }

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
    public string? ReplaceTime { get; set; }

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

    /// <summary>Alias for Utilities (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Utilities;
        set => Utilities = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Utility"/> class.
    /// </summary>
    public Utility() { }

    /// <summary>
    /// Returns a string representation of the Utility.
    /// </summary>
    public override string ToString()
    {
        return $"Utility: {Utilities}";
    }
}

