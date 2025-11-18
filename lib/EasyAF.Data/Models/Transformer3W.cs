using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Transformer3W with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "3W Transformers" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("3W Transformers")]
public class Transformer3W
{
    /// <summary>3W Transformers (Column: 3W Transformers)</summary>
    [Category("Identity")]
    [Description("3W Transformers")]
    [Required]
    public string? Transformer3Ws { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>Pri Bus ID (Column: Pri Bus ID)</summary>
    [Category("Electrical")]
    [Description("Pri Bus ID")]
    public string? PriBusID { get; set; }

    /// <summary>Pri Device ID (Column: Pri Device ID)</summary>
    [Category("Identity")]
    [Description("Pri Device ID")]
    public string? PriDeviceID { get; set; }

    /// <summary>Pri Device Type (Column: Pri Device Type)</summary>
    [Category("Physical")]
    [Description("Pri Device Type")]
    public string? PriDeviceType { get; set; }

    /// <summary>Pri Base kV (Column: Pri Base kV)</summary>
    [Category("Electrical")]
    [Description("Pri Base kV")]
    public string? PriBaseKV { get; set; }

    /// <summary>Pri Conn (Column: Pri Conn)</summary>
    [Category("General")]
    [Description("Pri Conn")]
    public string? PriConn { get; set; }

    /// <summary>Sec Bus ID (Column: Sec Bus ID)</summary>
    [Category("Electrical")]
    [Description("Sec Bus ID")]
    public string? SecBusID { get; set; }

    /// <summary>Sec Device ID (Column: Sec Device ID)</summary>
    [Category("Identity")]
    [Description("Sec Device ID")]
    public string? SecDeviceID { get; set; }

    /// <summary>Sec Device Type (Column: Sec Device Type)</summary>
    [Category("Physical")]
    [Description("Sec Device Type")]
    public string? SecDeviceType { get; set; }

    /// <summary>Sec Base kV (Column: Sec Base kV)</summary>
    [Category("Electrical")]
    [Description("Sec Base kV")]
    public string? SecBaseKV { get; set; }

    /// <summary>Sec Conn (Column: Sec Conn)</summary>
    [Category("General")]
    [Description("Sec Conn")]
    public string? SecConn { get; set; }

    /// <summary>Ter Bus ID (Column: Ter Bus ID)</summary>
    [Category("Electrical")]
    [Description("Ter Bus ID")]
    public string? TerBusID { get; set; }

    /// <summary>Ter Device ID (Column: Ter Device ID)</summary>
    [Category("Identity")]
    [Description("Ter Device ID")]
    public string? TerDeviceID { get; set; }

    /// <summary>Ter Device Type (Column: Ter Device Type)</summary>
    [Category("Physical")]
    [Description("Ter Device Type")]
    public string? TerDeviceType { get; set; }

    /// <summary>Ter Base kV (Column: Ter Base kV)</summary>
    [Category("Electrical")]
    [Description("Ter Base kV")]
    public string? TerBaseKV { get; set; }

    /// <summary>Ter Conn (Column: Ter Conn)</summary>
    [Category("General")]
    [Description("Ter Conn")]
    public string? TerConn { get; set; }

    /// <summary>Unit (Column: Unit)</summary>
    [Category("General")]
    [Description("Unit")]
    public string? Unit { get; set; }

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

    /// <summary>Pri Nom kV (Column: Pri Nom kV)</summary>
    [Category("Electrical")]
    [Description("Pri Nom kV")]
    public string? PriNomKV { get; set; }

    /// <summary>Pri Tap kV (Column: Pri Tap kV)</summary>
    [Category("Electrical")]
    [Description("Pri Tap kV")]
    public string? PriTapKV { get; set; }

    /// <summary>Pri MVA (Column: Pri MVA)</summary>
    [Category("Electrical")]
    [Description("Pri MVA")]
    public string? PriMVA { get; set; }

    /// <summary>Pri MVA O/L (Column: Pri MVA O/L)</summary>
    [Category("Electrical")]
    [Description("Pri MVA O/L")]
    public string? PriMVAOL { get; set; }

    /// <summary>Pri Gnd R (Column: Pri Gnd R)</summary>
    [Category("General")]
    [Description("Pri Gnd R")]
    public string? PriGndR { get; set; }

    /// <summary>Pri Gnd jX (Column: Pri Gnd jX)</summary>
    [Category("General")]
    [Description("Pri Gnd jX")]
    public string? PriGndJX { get; set; }

    /// <summary>Pri Gnd Amp Class (Column: Pri Gnd Amp Class)</summary>
    [Category("General")]
    [Description("Pri Gnd Amp Class")]
    public string? PriGndAmpClass { get; set; }

    /// <summary>Sec Nom kV (Column: Sec Nom kV)</summary>
    [Category("Electrical")]
    [Description("Sec Nom kV")]
    public string? SecNomKV { get; set; }

    /// <summary>Sec Tap kV (Column: Sec Tap kV)</summary>
    [Category("Electrical")]
    [Description("Sec Tap kV")]
    public string? SecTapKV { get; set; }

    /// <summary>Sec MVA (Column: Sec MVA)</summary>
    [Category("Electrical")]
    [Description("Sec MVA")]
    public string? SecMVA { get; set; }

    /// <summary>Sec MVA O/L (Column: Sec MVA O/L)</summary>
    [Category("Electrical")]
    [Description("Sec MVA O/L")]
    public string? SecMVAOL { get; set; }

    /// <summary>Sec Gnd R (Column: Sec Gnd R)</summary>
    [Category("General")]
    [Description("Sec Gnd R")]
    public string? SecGndR { get; set; }

    /// <summary>Sec Gnd jX (Column: Sec Gnd jX)</summary>
    [Category("General")]
    [Description("Sec Gnd jX")]
    public string? SecGndJX { get; set; }

    /// <summary>Sec Gnd Amp Class (Column: Sec Gnd Amp Class)</summary>
    [Category("General")]
    [Description("Sec Gnd Amp Class")]
    public string? SecGndAmpClass { get; set; }

    /// <summary>Ter Nom kV (Column: Ter Nom kV)</summary>
    [Category("Electrical")]
    [Description("Ter Nom kV")]
    public string? TerNomKV { get; set; }

    /// <summary>Ter Tap kV (Column: Ter Tap kV)</summary>
    [Category("Electrical")]
    [Description("Ter Tap kV")]
    public string? TerTapKV { get; set; }

    /// <summary>Ter MVA (Column: Ter MVA)</summary>
    [Category("Electrical")]
    [Description("Ter MVA")]
    public string? TerMVA { get; set; }

    /// <summary>Ter MVA O/L (Column: Ter MVA O/L)</summary>
    [Category("Electrical")]
    [Description("Ter MVA O/L")]
    public string? TerMVAOL { get; set; }

    /// <summary>Ter Gnd R (Column: Ter Gnd R)</summary>
    [Category("General")]
    [Description("Ter Gnd R")]
    public string? TerGndR { get; set; }

    /// <summary>Ter Gnd jX (Column: Ter Gnd jX)</summary>
    [Category("General")]
    [Description("Ter Gnd jX")]
    public string? TerGndJX { get; set; }

    /// <summary>Ter Gnd Amp Class (Column: Ter Gnd Amp Class)</summary>
    [Category("General")]
    [Description("Ter Gnd Amp Class")]
    public string? TerGndAmpClass { get; set; }

    /// <summary>LTC1 Tap (Column: LTC1 Tap)</summary>
    [Category("Control")]
    [Description("LTC1 Tap")]
    public string? Ltc1Tap { get; set; }

    /// <summary>LTC1 Step (Column: LTC1 Step)</summary>
    [Category("General")]
    [Description("LTC1 Step")]
    public string? Ltc1Step { get; set; }

    /// <summary>LTC1 Min Tap (Column: LTC1 Min Tap)</summary>
    [Category("Control")]
    [Description("LTC1 Min Tap")]
    public string? Ltc1MinTap { get; set; }

    /// <summary>LTC1 Max Tap (Column: LTC1 Max Tap)</summary>
    [Category("Control")]
    [Description("LTC1 Max Tap")]
    public string? Ltc1MaxTap { get; set; }

    /// <summary>LTC1 Ctl Type (Column: LTC1 Ctl Type)</summary>
    [Category("Physical")]
    [Description("LTC1 Ctl Type")]
    public string? Ltc1CtlType { get; set; }

    /// <summary>LTC1 Ctl Value (Column: LTC1 Ctl Value)</summary>
    [Category("Control")]
    [Description("LTC1 Ctl Value")]
    public string? Ltc1CtlValue { get; set; }

    /// <summary>LTC2 Tap (Column: LTC2 Tap)</summary>
    [Category("Control")]
    [Description("LTC2 Tap")]
    public string? Ltc2Tap { get; set; }

    /// <summary>LTC2 Step (Column: LTC2 Step)</summary>
    [Category("General")]
    [Description("LTC2 Step")]
    public string? Ltc2Step { get; set; }

    /// <summary>LTC2 Min Tap (Column: LTC2 Min Tap)</summary>
    [Category("Control")]
    [Description("LTC2 Min Tap")]
    public string? Ltc2MinTap { get; set; }

    /// <summary>LTC2 Max Tap (Column: LTC2 Max Tap)</summary>
    [Category("Control")]
    [Description("LTC2 Max Tap")]
    public string? Ltc2MaxTap { get; set; }

    /// <summary>LTC2 Ctl Type (Column: LTC2 Ctl Type)</summary>
    [Category("Physical")]
    [Description("LTC2 Ctl Type")]
    public string? Ltc2CtlType { get; set; }

    /// <summary>LTC2 Ctl Value (Column: LTC2 Ctl Value)</summary>
    [Category("Control")]
    [Description("LTC2 Ctl Value")]
    public string? Ltc2CtlValue { get; set; }

    /// <summary>Zps MVA Base (Column: Zps MVA Base)</summary>
    [Category("Electrical")]
    [Description("Zps MVA Base")]
    public string? ZpsMVABase { get; set; }

    /// <summary>Zpt MVA Base (Column: Zpt MVA Base)</summary>
    [Category("Electrical")]
    [Description("Zpt MVA Base")]
    public string? ZptMVABase { get; set; }

    /// <summary>Zst MVA Base (Column: Zst MVA Base)</summary>
    [Category("Electrical")]
    [Description("Zst MVA Base")]
    public string? ZstMVABase { get; set; }

    /// <summary>Zps R1 (Column: Zps R1)</summary>
    [Category("General")]
    [Description("Zps R1")]
    public string? ZpsR1 { get; set; }

    /// <summary>Zps X1 (Column: Zps X1)</summary>
    [Category("General")]
    [Description("Zps X1")]
    public string? ZpsX1 { get; set; }

    /// <summary>Zps R0 (Column: Zps R0)</summary>
    [Category("General")]
    [Description("Zps R0")]
    public string? ZpsR0 { get; set; }

    /// <summary>Zps X0 (Column: Zps X0)</summary>
    [Category("General")]
    [Description("Zps X0")]
    public string? ZpsX0 { get; set; }

    /// <summary>Zpt R1 (Column: Zpt R1)</summary>
    [Category("General")]
    [Description("Zpt R1")]
    public string? ZptR1 { get; set; }

    /// <summary>Zpt X1 (Column: Zpt X1)</summary>
    [Category("General")]
    [Description("Zpt X1")]
    public string? ZptX1 { get; set; }

    /// <summary>Zpt R0 (Column: Zpt R0)</summary>
    [Category("General")]
    [Description("Zpt R0")]
    public string? ZptR0 { get; set; }

    /// <summary>Zpt X0 (Column: Zpt X0)</summary>
    [Category("General")]
    [Description("Zpt X0")]
    public string? ZptX0 { get; set; }

    /// <summary>Zst R1 (Column: Zst R1)</summary>
    [Category("General")]
    [Description("Zst R1")]
    public string? ZstR1 { get; set; }

    /// <summary>Zst X1 (Column: Zst X1)</summary>
    [Category("General")]
    [Description("Zst X1")]
    public string? ZstX1 { get; set; }

    /// <summary>Zst R0 (Column: Zst R0)</summary>
    [Category("General")]
    [Description("Zst R0")]
    public string? ZstR0 { get; set; }

    /// <summary>Zst X0 (Column: Zst X0)</summary>
    [Category("General")]
    [Description("Zst X0")]
    public string? ZstX0 { get; set; }

    /// <summary>Zpn R1 pu (Column: Zpn R1 pu)</summary>
    [Category("General")]
    [Description("Zpn R1 pu")]
    public string? ZpnR1Pu { get; set; }

    /// <summary>Zp-n X1 pu (Column: Zp-n X1 pu)</summary>
    [Category("General")]
    [Description("Zp-n X1 pu")]
    public string? ZpNX1Pu { get; set; }

    /// <summary>Zps R0 pu (Column: Zps R0 pu)</summary>
    [Category("General")]
    [Description("Zps R0 pu")]
    public string? ZpsR0Pu { get; set; }

    /// <summary>Zps X0 pu (Column: Zps X0 pu)</summary>
    [Category("General")]
    [Description("Zps X0 pu")]
    public string? ZpsX0Pu { get; set; }

    /// <summary>Rpn0+3Rpg (Column: Rpn0+3Rpg)</summary>
    [Category("General")]
    [Description("Rpn0+3Rpg")]
    public string? Rpn0plus3rpg { get; set; }

    /// <summary>Xpn0+3Xpg (Column: Xpn0+3Xpg)</summary>
    [Category("General")]
    [Description("Xpn0+3Xpg")]
    public string? Xpn0plus3xpg { get; set; }

    /// <summary>Zs-n R1 pu (Column: Zs-n R1 pu)</summary>
    [Category("General")]
    [Description("Zs-n R1 pu")]
    public string? ZsNR1Pu { get; set; }

    /// <summary>Zs-n X1 pu (Column: Zs-n X1 pu)</summary>
    [Category("General")]
    [Description("Zs-n X1 pu")]
    public string? ZsNX1Pu { get; set; }

    /// <summary>Zpt R0 pu (Column: Zpt R0 pu)</summary>
    [Category("General")]
    [Description("Zpt R0 pu")]
    public string? ZptR0Pu { get; set; }

    /// <summary>Zpt X0 pu (Column: Zpt X0 pu)</summary>
    [Category("General")]
    [Description("Zpt X0 pu")]
    public string? ZptX0Pu { get; set; }

    /// <summary>Rsn0+3Rsg (Column: Rsn0+3Rsg)</summary>
    [Category("General")]
    [Description("Rsn0+3Rsg")]
    public string? Rsn0plus3rsg { get; set; }

    /// <summary>Xsn0+3Xsg (Column: Xsn0+3Xsg)</summary>
    [Category("General")]
    [Description("Xsn0+3Xsg")]
    public string? Xsn0plus3xsg { get; set; }

    /// <summary>Zt-n X1 pu (Column: Zt-n X1 pu)</summary>
    [Category("General")]
    [Description("Zt-n X1 pu")]
    public string? ZtNX1Pu { get; set; }

    /// <summary>Zt-n X1 pu (Column: Zt-n X1 pu)</summary>
    [Category("General")]
    [Description("Zt-n X1 pu")]
    public string? ZtNX1Pu2 { get; set; }

    /// <summary>Zst R0 pu (Column: Zst R0 pu)</summary>
    [Category("General")]
    [Description("Zst R0 pu")]
    public string? ZstR0Pu { get; set; }

    /// <summary>Zst X0 pu (Column: Zst X0 pu)</summary>
    [Category("General")]
    [Description("Zst X0 pu")]
    public string? ZstX0Pu { get; set; }

    /// <summary>Rtn0+3Rtg (Column: Rtn0+3Rtg)</summary>
    [Category("General")]
    [Description("Rtn0+3Rtg")]
    public string? Rtn0plus3rtg { get; set; }

    /// <summary>Xtn0+3Xtg (Column: Xtn0+3Xtg)</summary>
    [Category("General")]
    [Description("Xtn0+3Xtg")]
    public string? Xtn0plus3xtg { get; set; }

    /// <summary>Pri Gnd R1 pu (Column: Pri Gnd R1 pu)</summary>
    [Category("General")]
    [Description("Pri Gnd R1 pu")]
    public string? PriGndR1Pu { get; set; }

    /// <summary>Pri Gnd jX pu (Column: Pri Gnd jX pu)</summary>
    [Category("General")]
    [Description("Pri Gnd jX pu")]
    public string? PriGndJXPu { get; set; }

    /// <summary>Sec Gnd R1 pu (Column: Sec Gnd R1 pu)</summary>
    [Category("General")]
    [Description("Sec Gnd R1 pu")]
    public string? SecGndR1Pu { get; set; }

    /// <summary>Sec Gnd jX pu (Column: Sec Gnd jX pu)</summary>
    [Category("General")]
    [Description("Sec Gnd jX pu")]
    public string? SecGndJXPu { get; set; }

    /// <summary>Ter Gnd R1 pu (Column: Ter Gnd R1 pu)</summary>
    [Category("General")]
    [Description("Ter Gnd R1 pu")]
    public string? TerGndR1Pu { get; set; }

    /// <summary>Ter Gnd jX pu (Column: Ter Gnd jX pu)</summary>
    [Category("General")]
    [Description("Ter Gnd jX pu")]
    public string? TerGndJXPu { get; set; }

    /// <summary>TCC Standard (Column: TCC Standard)</summary>
    [Category("General")]
    [Description("TCC Standard")]
    public string? TCCStandard { get; set; }

    /// <summary>TCC FLA Based On (Column: TCC FLA Based On)</summary>
    [Category("General")]
    [Description("TCC FLA Based On")]
    public string? TCCFLABasedOn { get; set; }

    /// <summary>Plot 100% Withstand (Column: Plot 100% Withstand)</summary>
    [Category("General")]
    [Description("Plot 100% Withstand")]
    public string? Plot100percentWithstand { get; set; }

    /// <summary>Plot Unbl Derating (Column: Plot Unbl Derating)</summary>
    [Category("General")]
    [Description("Plot Unbl Derating")]
    public string? PlotUnblDerating { get; set; }

    /// <summary>TCC Plot Side (Column: TCC Plot Side)</summary>
    [Category("General")]
    [Description("TCC Plot Side")]
    public string? TCCPlotSide { get; set; }

    /// <summary>Freq Fault Curve (Column: Freq Fault Curve)</summary>
    [Category("General")]
    [Description("Freq Fault Curve")]
    public string? FreqFaultCurve { get; set; }

    /// <summary>AutoCoord Setting (Column: AutoCoord Setting)</summary>
    [Category("Control")]
    [Description("AutoCoord Setting")]
    public string? AutocoordSetting { get; set; }

    /// <summary>Z System (Column: Z System)</summary>
    [Category("General")]
    [Description("Z System")]
    public string? ZSystem { get; set; }

    /// <summary>TCC Max Plot Time (Column: TCC Max Plot Time)</summary>
    [Category("General")]
    [Description("TCC Max Plot Time")]
    public string? TCCMaxPlotTime { get; set; }

    /// <summary>TCC Min Damage Time (Column: TCC Min Damage Time)</summary>
    [Category("General")]
    [Description("TCC Min Damage Time")]
    public string? TCCMinDamageTime { get; set; }

    /// <summary>TCC Inrush FLA Mult (Column: TCC Inrush FLA Mult)</summary>
    [Category("General")]
    [Description("TCC Inrush FLA Mult")]
    public string? TCCInrushFLAMult { get; set; }

    /// <summary>TCC Inrush Cycles (Column: TCC Inrush Cycles)</summary>
    [Category("General")]
    [Description("TCC Inrush Cycles")]
    public string? TCCInrushCycles { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRcFactor { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRcValue { get; set; }

    /// <summary>Hrm Pec-r % (Column: Hrm Pec-r %)</summary>
    [Category("General")]
    [Description("Hrm Pec-r %")]
    public string? HrmPecRPercent { get; set; }

    /// <summary>I Hrm Rating Setting (Column: I Hrm Rating Setting)</summary>
    [Category("Control")]
    [Description("I Hrm Rating Setting")]
    public string? IHrmRatingSetting { get; set; }

    /// <summary>Pri I Hrm Rating (Column: Pri I Hrm Rating)</summary>
    [Category("General")]
    [Description("Pri I Hrm Rating")]
    public string? PriIHrmRating { get; set; }

    /// <summary>Sec I Hrm Rating (Column: Sec I Hrm Rating)</summary>
    [Category("General")]
    [Description("Sec I Hrm Rating")]
    public string? SecIHrmRating { get; set; }

    /// <summary>Ter I Hrm Rating (Column: Ter I Hrm Rating)</summary>
    [Category("General")]
    [Description("Ter I Hrm Rating")]
    public string? TerIHrmRating { get; set; }

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

    /// <summary>Alias for Transformer3W (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Transformer3Ws;
        set => Transformer3Ws = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transformer3W"/> class.
    /// </summary>
    public Transformer3W() { }

    /// <summary>
    /// Returns a string representation of the Transformer3W.
    /// </summary>
    public override string ToString()
    {
        return $"Transformer3W: {Transformer3Ws}";
    }
}

