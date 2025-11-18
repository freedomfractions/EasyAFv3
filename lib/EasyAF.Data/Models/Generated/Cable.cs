using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Cable with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Cables" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Cables")]
public class Cable
{
    /// <summary>Cables (Column: Cables)</summary>
    [Category("General")]
    [Description("Cables")]
    [Required]
    public string? Cables { get; set; }

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
    public string? FromBaseKV { get; set; }

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
    public string? ToBaseKV { get; set; }

    /// <summary>Unit (Column: Unit)</summary>
    [Category("General")]
    [Description("Unit")]
    public string? Unit { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>No/Ph (Column: No/Ph)</summary>
    [Category("General")]
    [Description("No/Ph")]
    public string? NoPerPh { get; set; }

    /// <summary>Phase Num (Column: Phase Num)</summary>
    [Category("Electrical")]
    [Description("Phase Num")]
    public string? PhaseNum { get; set; }

    /// <summary>Size (Column: Size)</summary>
    [Category("Physical")]
    [Description("Size")]
    public string? Size { get; set; }

    /// <summary>Length (Column: Length)</summary>
    [Category("General")]
    [Description("Length")]
    public string? Length { get; set; }

    /// <summary>Op Temp (Column: Op Temp (C))</summary>
    [Category("General")]
    [Description("Op Temp")]
    [Units("°C")]
    public string? OpTempC { get; set; }

    /// <summary>Ambient Temp (Column: Ambient Temp (C))</summary>
    [Category("General")]
    [Description("Ambient Temp")]
    [Units("°C")]
    public string? AmbientTempC { get; set; }

    /// <summary>Insulation (Column: Insulation)</summary>
    [Category("General")]
    [Description("Insulation")]
    public string? Insulation { get; set; }

    /// <summary>Insulation Level (Column: Insulation Level)</summary>
    [Category("General")]
    [Description("Insulation Level")]
    public string? InsulationLevel { get; set; }

    /// <summary>Rating (Column: Rating (A))</summary>
    [Category("Physical")]
    [Description("Rating")]
    [Units("A")]
    public string? RatingA { get; set; }

    /// <summary>75 deg C Rating (Column: 75 deg C Rating (A))</summary>
    [Category("Physical")]
    [Description("75 deg C Rating")]
    [Units("A")]
    public string? Rating75DegCA { get; set; }

    /// <summary>Material (Column: Material)</summary>
    [Category("General")]
    [Description("Material")]
    public string? Material { get; set; }

    /// <summary>Raceway Type (Column: Raceway Type)</summary>
    [Category("Physical")]
    [Description("Raceway Type")]
    public string? RacewayType { get; set; }

    /// <summary>Raceway Mtl (Column: Raceway Mtl)</summary>
    [Category("General")]
    [Description("Raceway Mtl")]
    public string? RacewayMtl { get; set; }

    /// <summary>Conduit Size (Column: Conduit Size)</summary>
    [Category("Physical")]
    [Description("Conduit Size")]
    public string? ConduitSize { get; set; }

    /// <summary>Conduit Num (Column: Conduit Num)</summary>
    [Category("General")]
    [Description("Conduit Num")]
    public string? ConduitNum { get; set; }

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

    /// <summary>Xc (Column: Xc)</summary>
    [Category("General")]
    [Description("Xc")]
    public string? Xc { get; set; }

    /// <summary>Xc0 (Column: Xc0)</summary>
    [Category("General")]
    [Description("Xc0")]
    public string? Xc0 { get; set; }

    /// <summary>Gnd Num (Column: Gnd Num)</summary>
    [Category("General")]
    [Description("Gnd Num")]
    public string? GndNum { get; set; }

    /// <summary>Gnd Size (Column: Gnd Size)</summary>
    [Category("Physical")]
    [Description("Gnd Size")]
    public string? GndSize { get; set; }

    /// <summary>Gnd Mtl (Column: Gnd Mtl)</summary>
    [Category("General")]
    [Description("Gnd Mtl")]
    public string? GndMtl { get; set; }

    /// <summary>Gnd Type (Column: Gnd Type)</summary>
    [Category("Physical")]
    [Description("Gnd Type")]
    public string? GndType { get; set; }

    /// <summary>Gnd Insul (Column: Gnd Insul)</summary>
    [Category("General")]
    [Description("Gnd Insul")]
    public string? GndInsul { get; set; }

    /// <summary>Neutral Num (Column: Neutral Num)</summary>
    [Category("General")]
    [Description("Neutral Num")]
    public string? NeutralNum { get; set; }

    /// <summary>Neutral Insul (Column: Neutral Insul)</summary>
    [Category("General")]
    [Description("Neutral Insul")]
    public string? NeutralInsul { get; set; }

    /// <summary>Neutral Size (Column: Neutral Size)</summary>
    [Category("Physical")]
    [Description("Neutral Size")]
    public string? NeutralSize { get; set; }

    /// <summary>Neutral Mtl (Column: Neutral Mtl)</summary>
    [Category("General")]
    [Description("Neutral Mtl")]
    public string? NeutralMtl { get; set; }

    /// <summary>Neutral Insulation Level (Column: Neutral Insulation Level)</summary>
    [Category("General")]
    [Description("Neutral Insulation Level")]
    public string? NeutralInsulationLevel { get; set; }

    /// <summary>Neutral R (Column: Neutral R)</summary>
    [Category("General")]
    [Description("Neutral R")]
    public string? NeutralR { get; set; }

    /// <summary>Neutral X (Column: Neutral X)</summary>
    [Category("General")]
    [Description("Neutral X")]
    public string? NeutralX { get; set; }

    /// <summary>Neutral Xc (Column: Neutral Xc)</summary>
    [Category("General")]
    [Description("Neutral Xc")]
    public string? NeutralXc { get; set; }

    /// <summary>Neutral Rating (Column: Neutral Rating)</summary>
    [Category("Physical")]
    [Description("Neutral Rating")]
    public string? NeutralRating { get; set; }

    /// <summary>Conductor Lay (Column: Conductor Lay)</summary>
    [Category("General")]
    [Description("Conductor Lay")]
    public string? ConductorLay { get; set; }

    /// <summary>Conductor Form (Column: Conductor Form)</summary>
    [Category("General")]
    [Description("Conductor Form")]
    public string? ConductorForm { get; set; }

    /// <summary>Duct Config (Column: Duct Config)</summary>
    [Category("General")]
    [Description("Duct Config")]
    public string? DuctConfig { get; set; }

    /// <summary>Spacing (Column: Spacing)</summary>
    [Category("General")]
    [Description("Spacing")]
    public string? Spacing { get; set; }

    /// <summary>R1 pu (Column: R1 pu)</summary>
    [Category("General")]
    [Description("R1 pu")]
    public string? R1Pu { get; set; }

    /// <summary>X1 pu (Column: X1 pu)</summary>
    [Category("General")]
    [Description("X1 pu")]
    public string? X1Pu { get; set; }

    /// <summary>R0 pu (Column: R0 pu)</summary>
    [Category("General")]
    [Description("R0 pu")]
    public string? R0Pu { get; set; }

    /// <summary>X0 pu (Column: X0 pu)</summary>
    [Category("General")]
    [Description("X0 pu")]
    public string? X0Pu { get; set; }

    /// <summary>B1 pu (Column: B1 pu)</summary>
    [Category("General")]
    [Description("B1 pu")]
    public string? B1Pu { get; set; }

    /// <summary>B0 pu (Column: B0 pu)</summary>
    [Category("General")]
    [Description("B0 pu")]
    public string? B0Pu { get; set; }

    /// <summary>Neutral R Pu (Column: Neutral R Pu)</summary>
    [Category("General")]
    [Description("Neutral R Pu")]
    public string? NeutralRPu { get; set; }

    /// <summary>Neutral X Pu (Column: Neutral X Pu)</summary>
    [Category("General")]
    [Description("Neutral X Pu")]
    public string? NeutralXPu { get; set; }

    /// <summary>Neutral B Pu (Column: Neutral B Pu)</summary>
    [Category("General")]
    [Description("Neutral B Pu")]
    public string? NeutralBPu { get; set; }

    /// <summary>IEC Field Temp (Column: IEC Field Temp (C))</summary>
    [Category("General")]
    [Description("IEC Field Temp")]
    [Units("°C")]
    public string? IECFieldTempC { get; set; }

    /// <summary>IEC SC Temp (Column: IEC SC Temp (C))</summary>
    [Category("Electrical")]
    [Description("IEC SC Temp")]
    [Units("°C")]
    public string? IECSCTempC { get; set; }

    /// <summary>IEC R1 Cmax pu (Column: IEC R1 Cmax pu)</summary>
    [Category("General")]
    [Description("IEC R1 Cmax pu")]
    public string? IECR1CmaxPu { get; set; }

    /// <summary>IEC R0 Cmax pu (Column: IEC R0 Cmax pu)</summary>
    [Category("General")]
    [Description("IEC R0 Cmax pu")]
    public string? IECR0CmaxPu { get; set; }

    /// <summary>IEC R1 Cmin pu (Column: IEC R1 Cmin pu)</summary>
    [Category("General")]
    [Description("IEC R1 Cmin pu")]
    public string? IECR1CminPu { get; set; }

    /// <summary>IEC R0 Cmin pu (Column: IEC R0 Cmin pu)</summary>
    [Category("General")]
    [Description("IEC R0 Cmin pu")]
    public string? IECR0CminPu { get; set; }

    /// <summary>IEC Neutral R Cmax pu (Column: IEC Neutral R Cmax pu)</summary>
    [Category("General")]
    [Description("IEC Neutral R Cmax pu")]
    public string? IECNeutralRCmaxPu { get; set; }

    /// <summary>IEC Neutral R Cmin pu (Column: IEC Neutral R Cmin pu)</summary>
    [Category("General")]
    [Description("IEC Neutral R Cmin pu")]
    public string? IECNeutralRCminPu { get; set; }

    /// <summary>Rating at Max Temp (Column: Rating at Max Temp)</summary>
    [Category("Physical")]
    [Description("Rating at Max Temp")]
    public string? RatingAtMaxTemp { get; set; }

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
    public string? CompFailureRatePerYear { get; set; }

    /// <summary>Comp Repair Time (Column: Comp Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Comp Repair Time")]
    [Units("h")]
    public string? CompRepairTimeH { get; set; }

    /// <summary>Comp Replace Time (Column: Comp Replace Time (h))</summary>
    [Category("Reliability")]
    [Description("Comp Replace Time")]
    [Units("h")]
    public string? CompReplaceTimeH { get; set; }

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
    public string? ConnFailureRatePerYear { get; set; }

    /// <summary>Conn Repair Time (Column: Conn Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Conn Repair Time")]
    [Units("h")]
    public string? ConnRepairTimeH { get; set; }

    /// <summary>Conn Replace Time (Column: Conn Replace Time (h))</summary>
    [Category("Reliability")]
    [Description("Conn Replace Time")]
    [Units("h")]
    public string? ConnReplaceTimeH { get; set; }

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

    /// <summary>Alias for Cables (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Cables;
        set => Cables = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Cable"/> class.
    /// </summary>
    public Cable() { }

    /// <summary>
    /// Returns a string representation of the Cable.
    /// </summary>
    public override string ToString()
    {
        return $"Cable: {Cables}";
    }
}
