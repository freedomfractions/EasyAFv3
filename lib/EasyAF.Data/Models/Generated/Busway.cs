using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Busway with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Busways" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Busways")]
public class Busway
{
    /// <summary>Busways (Column: Busways)</summary>
    [Category("Identity")]
    [Description("Busways")]
    [Required]
    public string? Busways { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoofPhases { get; set; }

    /// <summary>From Bus ID (Column: From Bus ID)</summary>
    [Category("Identity")]
    [Description("From Bus ID")]
    public string? FromBusID { get; set; }

    /// <summary>From Bus Type (Column: From Bus Type)</summary>
    [Category("Physical")]
    [Description("From Bus Type")]
    public string? FromBusType { get; set; }

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
    public string? FromBasekV { get; set; }

    /// <summary>To Bus ID (Column: To Bus ID)</summary>
    [Category("Identity")]
    [Description("To Bus ID")]
    public string? ToBusID { get; set; }

    /// <summary>To Bus Type (Column: To Bus Type)</summary>
    [Category("Physical")]
    [Description("To Bus Type")]
    public string? ToBusType { get; set; }

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
    public string? ToBasekV { get; set; }

    /// <summary>Unit (Column: Unit)</summary>
    [Category("General")]
    [Description("Unit")]
    public string? Unit { get; set; }

    /// <summary>Manufacturer (Column: Manufacturer)</summary>
    [Category("Physical")]
    [Description("Manufacturer")]
    public string? Manufacturer { get; set; }

    /// <summary>Length (Column: Length)</summary>
    [Category("General")]
    [Description("Length")]
    public string? Length { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>Ampacity (Column: Ampacity)</summary>
    [Category("Electrical")]
    [Description("Ampacity")]
    public string? Ampacity { get; set; }

    /// <summary>Material (Column: Material)</summary>
    [Category("General")]
    [Description("Material")]
    public string? Material { get; set; }

    /// <summary>R1 (Column: R1)</summary>
    [Category("General")]
    [Description("R1")]
    public string? R1 { get; set; }

    /// <summary>X1 (Column: X1)</summary>
    [Category("General")]
    [Description("X1")]
    public string? X1 { get; set; }

    /// <summary>R0 (Column: R0)</summary>
    [Category("General")]
    [Description("R0")]
    public string? R0 { get; set; }

    /// <summary>X0 (Column: X0)</summary>
    [Category("General")]
    [Description("X0")]
    public string? X0 { get; set; }

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

    /// <summary>IEC SC Temp (Column: IEC SC Temp (C))</summary>
    [Category("Electrical")]
    [Description("IEC SC Temp")]
    [Units("°C")]
    public string? IECSCTemp { get; set; }

    /// <summary>IEC R1 Cmax pu (Column: IEC R1 Cmax pu)</summary>
    [Category("General")]
    [Description("IEC R1 Cmax pu")]
    public string? IECR1Cmaxpu { get; set; }

    /// <summary>IEC R0 Cmax pu (Column: IEC R0 Cmax pu)</summary>
    [Category("General")]
    [Description("IEC R0 Cmax pu")]
    public string? IECR0Cmaxpu { get; set; }

    /// <summary>IEC R1 Cmin pu (Column: IEC R1 Cmin pu)</summary>
    [Category("General")]
    [Description("IEC R1 Cmin pu")]
    public string? IECR1Cminpu { get; set; }

    /// <summary>IEC R0 Cmin pu (Column: IEC R0 Cmin pu)</summary>
    [Category("General")]
    [Description("IEC R0 Cmin pu")]
    public string? IECR0Cminpu { get; set; }

    /// <summary>Rating at Max Temp (Column: Rating at Max Temp)</summary>
    [Category("Physical")]
    [Description("Rating at Max Temp")]
    public string? RatingatMaxTemp { get; set; }

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

    /// <summary>Comp Failure Rate (Column: Comp Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Description("Comp Failure Rate")]
    [Units("/year")]
    public string? CompFailureRate { get; set; }

    /// <summary>Comp Repair Time (Column: Comp Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Comp Repair Time")]
    [Units("h")]
    public string? CompRepairTime { get; set; }

    /// <summary>Comp Replace Time (Column: Comp Replace Time (h))</summary>
    [Category("Reliability")]
    [Description("Comp Replace Time")]
    [Units("h")]
    public string? CompReplaceTime { get; set; }

    /// <summary>Comp Repair Cost (Column: Comp Repair Cost)</summary>
    [Category("Reliability")]
    [Description("Comp Repair Cost")]
    public string? CompRepairCost { get; set; }

    /// <summary>Comp Replace Cost (Column: Comp Replace Cost)</summary>
    [Category("Reliability")]
    [Description("Comp Replace Cost")]
    public string? CompReplaceCost { get; set; }

    /// <summary>Comp Action Upon Failure (Column: Comp Action Upon Failure)</summary>
    [Category("Reliability")]
    [Description("Comp Action Upon Failure")]
    public string? CompActionUponFailure { get; set; }

    /// <summary>Conn Failure Rate (Column: Conn Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Description("Conn Failure Rate")]
    [Units("/year")]
    public string? ConnFailureRate { get; set; }

    /// <summary>Conn Repair Time (Column: Conn Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Conn Repair Time")]
    [Units("h")]
    public string? ConnRepairTime { get; set; }

    /// <summary>Conn Replace Time (Column: Conn Replace Time (h))</summary>
    [Category("Reliability")]
    [Description("Conn Replace Time")]
    [Units("h")]
    public string? ConnReplaceTime { get; set; }

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

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for Busways (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Busways;
        set => Busways = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Busway"/> class.
    /// </summary>
    public Busway() { }

    /// <summary>
    /// Returns a string representation of the Busway.
    /// </summary>
    public override string ToString()
    {
        return $"Busway: {Busways}";
    }
}
