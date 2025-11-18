using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Transformer with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "2W Transformers" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("2W Transformers")]
public class Transformer
{
    /// <summary>2W Transformers (Column: 2W Transformers)</summary>
    [Category("General")]
    [Description("2W Transformers")]
    [Required]
    public string? Transformers2W { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Center Tap (Column: Center Tap)</summary>
    [Category("Control")]
    [Description("Center Tap")]
    public string? CenterTap { get; set; }

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

    /// <summary>From Conn (Column: From Conn)</summary>
    [Category("General")]
    [Description("From Conn")]
    public string? FromConn { get; set; }

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

    /// <summary>To Conn (Column: To Conn)</summary>
    [Category("General")]
    [Description("To Conn")]
    public string? ToConn { get; set; }

    /// <summary>IEC Conn Type (Column: IEC Conn Type)</summary>
    [Category("Physical")]
    [Description("IEC Conn Type")]
    public string? IECConnType { get; set; }

    /// <summary>Standard (Column: Standard)</summary>
    [Category("General")]
    [Description("Standard")]
    public string? Standard { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>Class (Column: Class)</summary>
    [Category("General")]
    [Description("Class")]
    public string? Class { get; set; }

    /// <summary>Temp (Column: Temp)</summary>
    [Category("General")]
    [Description("Temp")]
    public string? Temp { get; set; }

    /// <summary>Form (Column: Form)</summary>
    [Category("General")]
    [Description("Form")]
    public string? Form { get; set; }

    /// <summary>Design (Column: Design)</summary>
    [Category("General")]
    [Description("Design")]
    public string? Design { get; set; }

    /// <summary>From Nom kV (Column: From Nom kV)</summary>
    [Category("Electrical")]
    [Description("From Nom kV")]
    public string? FromNomkV { get; set; }

    /// <summary>From Tap kV (Column: From Tap kV)</summary>
    [Category("Electrical")]
    [Description("From Tap kV")]
    public string? FromTapkV { get; set; }

    /// <summary>From Gnd R (Column: From Gnd R)</summary>
    [Category("General")]
    [Description("From Gnd R")]
    public string? FromGndR { get; set; }

    /// <summary>From Gnd jX (Column: From Gnd jX)</summary>
    [Category("General")]
    [Description("From Gnd jX")]
    public string? FromGndjX { get; set; }

    /// <summary>From Gnd Amp Class (Column: From Gnd Amp Class)</summary>
    [Category("Electrical")]
    [Description("From Gnd Amp Class")]
    public string? FromGndAmpClass { get; set; }

    /// <summary>To Nom kV (Column: To Nom kV)</summary>
    [Category("Electrical")]
    [Description("To Nom kV")]
    public string? ToNomkV { get; set; }

    /// <summary>To Tap kV (Column: To Tap kV)</summary>
    [Category("Electrical")]
    [Description("To Tap kV")]
    public string? ToTapkV { get; set; }

    /// <summary>To Gnd R (Column: To Gnd R)</summary>
    [Category("General")]
    [Description("To Gnd R")]
    public string? ToGndR { get; set; }

    /// <summary>To Gnd jX (Column: To Gnd jX)</summary>
    [Category("General")]
    [Description("To Gnd jX")]
    public string? ToGndjX { get; set; }

    /// <summary>To Gnd Amp Class (Column: To Gnd Amp Class)</summary>
    [Category("Electrical")]
    [Description("To Gnd Amp Class")]
    public string? ToGndAmpClass { get; set; }

    /// <summary>MVA (Column: MVA)</summary>
    [Category("General")]
    [Description("MVA")]
    [Required]
    public string? MVA { get; set; }

    /// <summary>MVA O/L (Column: MVA O/L)</summary>
    [Category("General")]
    [Description("MVA O/L")]
    [Required]
    public string? MVAOL { get; set; }

    /// <summary>Z MVA Base (Column: Z MVA Base)</summary>
    [Category("General")]
    [Description("Z MVA Base")]
    [Required]
    public string? ZMVABase { get; set; }

    /// <summary>Z (Column: Z)</summary>
    [Category("General")]
    [Description("Z")]
    public string? Z { get; set; }

    /// <summary>Z0 (Column: Z0)</summary>
    [Category("General")]
    [Description("Z0")]
    public string? Z0 { get; set; }

    /// <summary>X/R (Column: X/R)</summary>
    [Category("General")]
    [Description("X/R")]
    public string? XR { get; set; }

    /// <summary>X0/R0 (Column: X0/R0)</summary>
    [Category("General")]
    [Description("X0/R0")]
    public string? X0R0 { get; set; }

    /// <summary>Loss (Column: Loss (kW))</summary>
    [Category("Electrical")]
    [Description("Loss")]
    [Units("kW")]
    public string? Loss { get; set; }

    /// <summary>LTC Tap (Column: LTC Tap)</summary>
    [Category("Control")]
    [Description("LTC Tap")]
    public string? LTCTap { get; set; }

    /// <summary>LTC Step (Column: LTC Step)</summary>
    [Category("Control")]
    [Description("LTC Step")]
    public string? LTCStep { get; set; }

    /// <summary>LTC Min Tap (Column: LTC Min Tap)</summary>
    [Category("Control")]
    [Description("LTC Min Tap")]
    public string? LTCMinTap { get; set; }

    /// <summary>LTC Max Tap (Column: LTC Max Tap)</summary>
    [Category("Control")]
    [Description("LTC Max Tap")]
    public string? LTCMaxTap { get; set; }

    /// <summary>Ctl Type (Column: Ctl Type)</summary>
    [Category("Physical")]
    [Description("Ctl Type")]
    public string? CtlType { get; set; }

    /// <summary>Ctl Value (Column: Ctl Value)</summary>
    [Category("General")]
    [Description("Ctl Value")]
    public string? CtlValue { get; set; }

    /// <summary>Zps R1 pu (Column: Zps R1 pu)</summary>
    [Category("General")]
    [Description("Zps R1 pu")]
    public string? ZpsR1pu { get; set; }

    /// <summary>Zps X1 pu (Column: Zps X1 pu)</summary>
    [Category("General")]
    [Description("Zps X1 pu")]
    public string? ZpsX1pu { get; set; }

    /// <summary>Zps R0 pu (Column: Zps R0 pu)</summary>
    [Category("General")]
    [Description("Zps R0 pu")]
    public string? ZpsR0pu { get; set; }

    /// <summary>Zps X0 pu (Column: Zps X0 pu)</summary>
    [Category("General")]
    [Description("Zps X0 pu")]
    public string? ZpsX0pu { get; set; }

    /// <summary>Rps0+3Rpsg (Column: Rps0+3Rpsg)</summary>
    [Category("General")]
    [Description("Rps0+3Rpsg")]
    public string? Rps0+3Rpsg { get; set; }

    /// <summary>Xps0+3Xpsg (Column: Xps0+3Xpsg)</summary>
    [Category("General")]
    [Description("Xps0+3Xpsg")]
    public string? Xps0+3Xpsg { get; set; }

    /// <summary>From Gnd R1 pu (Column: From Gnd R1 pu)</summary>
    [Category("General")]
    [Description("From Gnd R1 pu")]
    public string? FromGndR1pu { get; set; }

    /// <summary>From Gnd jX pu (Column: From Gnd jX pu)</summary>
    [Category("General")]
    [Description("From Gnd jX pu")]
    public string? FromGndjXpu { get; set; }

    /// <summary>To Gnd R1 pu (Column: To Gnd R1 pu)</summary>
    [Category("General")]
    [Description("To Gnd R1 pu")]
    public string? ToGndR1pu { get; set; }

    /// <summary>To Gnd jX pu (Column: To Gnd jX pu)</summary>
    [Category("General")]
    [Description("To Gnd jX pu")]
    public string? ToGndjXpu { get; set; }

    /// <summary>Sec Gnd (Column: Sec Gnd)</summary>
    [Category("General")]
    [Description("Sec Gnd")]
    public string? SecGnd { get; set; }

    /// <summary>IEC pT (Column: IEC pT)</summary>
    [Category("General")]
    [Description("IEC pT")]
    public string? IECpT { get; set; }

    /// <summary>TCC Standard (Column: TCC Standard)</summary>
    [Category("Protection")]
    [Description("TCC Standard")]
    public string? TCCStandard { get; set; }

    /// <summary>TCC FLA Based On (Column: TCC FLA Based On)</summary>
    [Category("Protection")]
    [Description("TCC FLA Based On")]
    public string? TCCFLABasedOn { get; set; }

    /// <summary>Plot 100% Withstand (Column: Plot 100% Withstand)</summary>
    [Category("General")]
    [Description("Plot 100% Withstand")]
    public string? Plot100%Withstand { get; set; }

    /// <summary>Plot Unbl Derating (Column: Plot Unbl Derating)</summary>
    [Category("Physical")]
    [Description("Plot Unbl Derating")]
    public string? PlotUnblDerating { get; set; }

    /// <summary>Freq Fault Curve (Column: Freq Fault Curve)</summary>
    [Category("Electrical")]
    [Description("Freq Fault Curve")]
    public string? FreqFaultCurve { get; set; }

    /// <summary>AutoCoord Setting (Column: AutoCoord Setting)</summary>
    [Category("Control")]
    [Description("AutoCoord Setting")]
    public string? AutoCoordSetting { get; set; }

    /// <summary>Z System (Column: Z System)</summary>
    [Category("General")]
    [Description("Z System")]
    public string? ZSystem { get; set; }

    /// <summary>TCC Min Damage Time (Column: TCC Min Damage Time)</summary>
    [Category("Protection")]
    [Description("TCC Min Damage Time")]
    public string? TCCMinDamageTime { get; set; }

    /// <summary>TCC Max Plot Time (Column: TCC Max Plot Time)</summary>
    [Category("Protection")]
    [Description("TCC Max Plot Time")]
    public string? TCCMaxPlotTime { get; set; }

    /// <summary>TCC Inrush FLA Mult (Column: TCC Inrush FLA Mult)</summary>
    [Category("Protection")]
    [Description("TCC Inrush FLA Mult")]
    public string? TCCInrushFLAMult { get; set; }

    /// <summary>TCC Inrush Cycles (Column: TCC Inrush Cycles)</summary>
    [Category("Protection")]
    [Description("TCC Inrush Cycles")]
    public string? TCCInrushCycles { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRCFactor { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRCValue { get; set; }

    /// <summary>Hrm Pec-r % (Column: Hrm Pec-r %)</summary>
    [Category("General")]
    [Description("Hrm Pec-r %")]
    public string? HrmPecr% { get; set; }

    /// <summary>I Hrm Rating Setting (Column: I Hrm Rating Setting)</summary>
    [Category("Physical")]
    [Description("I Hrm Rating Setting")]
    public string? IHrmRatingSetting { get; set; }

    /// <summary>From I Hrm Rating (Column: From I Hrm Rating)</summary>
    [Category("Physical")]
    [Description("From I Hrm Rating")]
    public string? FromIHrmRating { get; set; }

    /// <summary>To I Hrm Rating (Column: To I Hrm Rating)</summary>
    [Category("Physical")]
    [Description("To I Hrm Rating")]
    public string? ToIHrmRating { get; set; }

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

    /// <summary>Alias for Transformers2W (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Transformers2W;
        set => Transformers2W = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transformer"/> class.
    /// </summary>
    public Transformer() { }

    /// <summary>
    /// Returns a string representation of the Transformer.
    /// </summary>
    public override string ToString()
    {
        return $"Transformer: {Transformers2W}";
    }
}

