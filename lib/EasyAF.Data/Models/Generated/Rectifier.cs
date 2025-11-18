using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Rectifier with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Rectifiers" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Rectifiers")]
public class Rectifier
{
    /// <summary>Rectifiers (Column: Rectifiers)</summary>
    [Category("General")]
    [Description("Rectifiers")]
    [Required]
    public string? Rectifiers { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Input Bus (Column: Input Bus)</summary>
    [Category("Identity")]
    [Description("Input Bus")]
    public string? InputBus { get; set; }

    /// <summary>Input Device ID (Column: Input Device ID)</summary>
    [Category("General")]
    [Description("Input Device ID")]
    public string? InputDeviceID { get; set; }

    /// <summary>Input Device Type (Column: Input Device Type)</summary>
    [Category("Physical")]
    [Description("Input Device Type")]
    public string? InputDeviceType { get; set; }

    /// <summary>Output Bus (Column: Output Bus)</summary>
    [Category("Identity")]
    [Description("Output Bus")]
    public string? OutputBus { get; set; }

    /// <summary>Output Device ID (Column: Output Device ID)</summary>
    [Category("General")]
    [Description("Output Device ID")]
    public string? OutputDeviceID { get; set; }

    /// <summary>Output Device Type (Column: Output Device Type)</summary>
    [Category("Physical")]
    [Description("Output Device Type")]
    public string? OutputDeviceType { get; set; }

    /// <summary>DC Rated kV (Column: DC Rated kV)</summary>
    [Category("Electrical")]
    [Description("DC Rated kV")]
    public string? DCRatedkV { get; set; }

    /// <summary>DC Rated FLA (Column: DC Rated FLA)</summary>
    [Category("General")]
    [Description("DC Rated FLA")]
    public string? DCRatedFLA { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>Fault X FLA (Column: Fault X FLA)</summary>
    [Category("Electrical")]
    [Description("Fault X FLA")]
    public string? FaultXFLA { get; set; }

    /// <summary>Fault Time (Column: Fault Time)</summary>
    [Category("Electrical")]
    [Description("Fault Time")]
    public string? FaultTime { get; set; }

    /// <summary>Fault Time Unit (Column: Fault Time Unit)</summary>
    [Category("Electrical")]
    [Description("Fault Time Unit")]
    public string? FaultTimeUnit { get; set; }

    /// <summary>Thyristor Control (Column: Thyristor Control)</summary>
    [Category("Control")]
    [Description("Thyristor Control")]
    public string? ThyristorControl { get; set; }

    /// <summary>Input PF (Column: Input PF)</summary>
    [Category("General")]
    [Description("Input PF")]
    public string? InputPF { get; set; }

    /// <summary>% Efficiency (Column: % Efficiency)</summary>
    [Category("General")]
    [Description("% Efficiency")]
    public string? %Efficiency { get; set; }

    /// <summary>Ctl kV pu (Column: Ctl kV pu)</summary>
    [Category("Electrical")]
    [Description("Ctl kV pu")]
    public string? CtlkVpu { get; set; }

    /// <summary>Current Set Point (Column: Current Set Point)</summary>
    [Category("Electrical")]
    [Description("Current Set Point")]
    public string? CurrentSetPoint { get; set; }

    /// <summary>Alfa Min Deg (Column: Alfa Min Deg)</summary>
    [Category("General")]
    [Description("Alfa Min Deg")]
    public string? AlfaMinDeg { get; set; }

    /// <summary>Alfa Max Deg (Column: Alfa Max Deg)</summary>
    [Category("General")]
    [Description("Alfa Max Deg")]
    public string? AlfaMaxDeg { get; set; }

    /// <summary>Comm Z1 % (Column: Comm Z1 %)</summary>
    [Category("General")]
    [Description("Comm Z1 %")]
    public string? CommZ1% { get; set; }

    /// <summary>Comm X/R (Column: Comm X/R)</summary>
    [Category("General")]
    [Description("Comm X/R")]
    public string? CommXR { get; set; }

    /// <summary>Comm kVA (Column: Comm kVA)</summary>
    [Category("Electrical")]
    [Description("Comm kVA")]
    [Required]
    public string? CommkVA { get; set; }

    /// <summary>Hrm Load Type (Column: Hrm Load Type)</summary>
    [Category("Physical")]
    [Description("Hrm Load Type")]
    public string? HrmLoadType { get; set; }

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

    /// <summary>Lib Load Mfr (Column: Lib Load Mfr)</summary>
    [Category("Physical")]
    [Description("Lib Load Mfr")]
    public string? LibLoadMfr { get; set; }

    /// <summary>Lib Load Type (Column: Lib Load Type)</summary>
    [Category("Physical")]
    [Description("Lib Load Type")]
    public string? LibLoadType { get; set; }

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

    /// <summary>Alias for Rectifiers (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Rectifiers;
        set => Rectifiers = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rectifier"/> class.
    /// </summary>
    public Rectifier() { }

    /// <summary>
    /// Returns a string representation of the Rectifier.
    /// </summary>
    public override string ToString()
    {
        return $"Rectifier: {Rectifiers}";
    }
}

