using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Motor with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Motors" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Motors")]
public class Motor
{
    /// <summary>Motors (Column: Motors)</summary>
    [Category("General")]
    [Description("Motors")]
    [Required]
    public string? Motors { get; set; }

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

    /// <summary>Unit (Column: Unit)</summary>
    [Category("General")]
    [Description("Unit")]
    public string? Unit { get; set; }

    /// <summary>Model (Column: Model)</summary>
    [Category("Physical")]
    [Description("Model")]
    public string? Model { get; set; }

    /// <summary>Motor kV (Column: Motor kV)</summary>
    [Category("Electrical")]
    [Description("Motor kV")]
    public string? MotorkV { get; set; }

    /// <summary>Conn (Column: Conn)</summary>
    [Category("General")]
    [Description("Conn")]
    public string? Conn { get; set; }

    /// <summary>HP or kW (Column: HP or kW)</summary>
    [Category("Electrical")]
    [Description("HP or kW")]
    [Required]
    public string? HPorkW { get; set; }

    /// <summary>AFD Setting (Column: AFD Setting)</summary>
    [Category("Control")]
    [Description("AFD Setting")]
    public string? AFDSetting { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>Load Class (Column: Load Class)</summary>
    [Category("Demand")]
    [Description("Load Class")]
    public string? LoadClass { get; set; }

    /// <summary>RPM (Column: RPM)</summary>
    [Category("General")]
    [Description("RPM")]
    public string? RPM { get; set; }

    /// <summary>FLA (Column: FLA)</summary>
    [Category("General")]
    [Description("FLA")]
    public string? FLA { get; set; }

    /// <summary>Ia (Column: Ia)</summary>
    [Category("General")]
    [Description("Ia")]
    public string? Ia { get; set; }

    /// <summary>Power Factor (Column: Power Factor)</summary>
    [Category("General")]
    [Description("Power Factor")]
    public string? PowerFactor { get; set; }

    /// <summary>Efficiency (Column: Efficiency)</summary>
    [Category("General")]
    [Description("Efficiency")]
    public string? Efficiency { get; set; }

    /// <summary>kVA/HP (Column: kVA/HP)</summary>
    [Category("Electrical")]
    [Description("kVA/HP")]
    [Required]
    public string? KVAHP { get; set; }

    /// <summary>Impedance Settings (Column: Impedance Settings)</summary>
    [Category("Electrical")]
    [Description("Impedance Settings")]
    public string? ImpedanceSettings { get; set; }

    /// <summary>ANSI Code (Column: ANSI Code)</summary>
    [Category("General")]
    [Description("ANSI Code")]
    public string? ANSICode { get; set; }

    /// <summary>Connected % (Column: Connected %)</summary>
    [Category("General")]
    [Description("Connected %")]
    public string? Connected% { get; set; }

    /// <summary>X''dv or Xlr or R (Column: X''dv or Xlr or R (Ohms))</summary>
    [Category("General")]
    [Description("X''dv or Xlr or R")]
    [Units("?")]
    public string? XdvorXlrorR { get; set; }

    /// <summary>X0 (Column: X0)</summary>
    [Category("General")]
    [Description("X0")]
    public string? X0 { get; set; }

    /// <summary>X/R (Column: X/R)</summary>
    [Category("General")]
    [Description("X/R")]
    public string? XR { get; set; }

    /// <summary>Quantity (Column: Quantity)</summary>
    [Category("General")]
    [Description("Quantity")]
    public string? Quantity { get; set; }

    /// <summary>RG Ohm (Column: RG Ohm)</summary>
    [Category("General")]
    [Description("RG Ohm")]
    public string? RGOhm { get; set; }

    /// <summary>XG Ohm (Column: XG Ohm)</summary>
    [Category("General")]
    [Description("XG Ohm")]
    public string? XGOhm { get; set; }

    /// <summary>Load Model (Column: Load Model)</summary>
    [Category("Physical")]
    [Description("Load Model")]
    public string? LoadModel { get; set; }

    /// <summary>Motor kVA (Column: Motor kVA)</summary>
    [Category("Electrical")]
    [Description("Motor kVA")]
    [Required]
    public string? MotorkVA { get; set; }

    /// <summary>Demand Factor (Column: Demand Factor)</summary>
    [Category("Demand")]
    [Description("Demand Factor")]
    public string? DemandFactor { get; set; }

    /// <summary>PF Load Type (Column: PF Load Type)</summary>
    [Category("Physical")]
    [Description("PF Load Type")]
    public string? PFLoadType { get; set; }

    /// <summary>Load Scaling (Column: Load Scaling)</summary>
    [Category("Demand")]
    [Description("Load Scaling")]
    public string? LoadScaling { get; set; }

    /// <summary>Load Speed Exp (Column: Load Speed Exp)</summary>
    [Category("Demand")]
    [Description("Load Speed Exp")]
    public string? LoadSpeedExp { get; set; }

    /// <summary>SCADA kW (Column: SCADA kW)</summary>
    [Category("Electrical")]
    [Description("SCADA kW")]
    public string? SCADAkW { get; set; }

    /// <summary>SCADA jkVAR (Column: SCADA jkVAR)</summary>
    [Category("Electrical")]
    [Description("SCADA jkVAR")]
    [Required]
    public string? SCADAjkVAR { get; set; }

    /// <summary>SCADA Type (Column: SCADA Type)</summary>
    [Category("Physical")]
    [Description("SCADA Type")]
    public string? SCADAType { get; set; }

    /// <summary>SCADA Scaling (Column: SCADA Scaling)</summary>
    [Category("General")]
    [Description("SCADA Scaling")]
    public string? SCADAScaling { get; set; }

    /// <summary>Starting kVA/HP (Column: Starting kVA/HP)</summary>
    [Category("Electrical")]
    [Description("Starting kVA/HP")]
    [Required]
    public string? StartingkVAHP { get; set; }

    /// <summary>Starting LR Mult (Column: Starting LR Mult)</summary>
    [Category("General")]
    [Description("Starting LR Mult")]
    public string? StartingLRMult { get; set; }

    /// <summary>Starting PF (Column: Starting PF)</summary>
    [Category("General")]
    [Description("Starting PF")]
    public string? StartingPF { get; set; }

    /// <summary>Hrm Load Type (Column: Hrm Load Type)</summary>
    [Category("Physical")]
    [Description("Hrm Load Type")]
    public string? HrmLoadType { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRCValue { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRCFactor { get; set; }

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

    /// <summary>R Gnd pu (Column: R Gnd pu)</summary>
    [Category("General")]
    [Description("R Gnd pu")]
    public string? RGndpu { get; set; }

    /// <summary>X Gnd pu (Column: X Gnd pu)</summary>
    [Category("General")]
    [Description("X Gnd pu")]
    public string? XGndpu { get; set; }

    /// <summary>Gnd Amp Class (Column: Gnd Amp Class)</summary>
    [Category("Electrical")]
    [Description("Gnd Amp Class")]
    public string? GndAmpClass { get; set; }

    /// <summary>"IEC RMf / X""d " (Column: "IEC RMf / X""d ")</summary>
    [Category("General")]
    [Description(""IEC RMf / X""d "")]
    public string? "IECRMfX""d" { get; set; }

    /// <summary>Int MF (Column: Int MF)</summary>
    [Category("General")]
    [Description("Int MF")]
    public string? IntMF { get; set; }

    /// <summary>Hrm R1 pu (Column: Hrm R1 pu)</summary>
    [Category("General")]
    [Description("Hrm R1 pu")]
    public string? HrmR1pu { get; set; }

    /// <summary>Hrm X1 pu (Column: Hrm X1 pu)</summary>
    [Category("General")]
    [Description("Hrm X1 pu")]
    public string? HrmX1pu { get; set; }

    /// <summary>TCC Starter (Column: TCC Starter)</summary>
    [Category("Protection")]
    [Description("TCC Starter")]
    public string? TCCStarter { get; set; }

    /// <summary>Plot TCC (Column: Plot TCC)</summary>
    [Category("Protection")]
    [Description("Plot TCC")]
    public string? PlotTCC { get; set; }

    /// <summary>Service Factor (Column: Service Factor)</summary>
    [Category("General")]
    [Description("Service Factor")]
    public string? ServiceFactor { get; set; }

    /// <summary>Locked Rotor Mult (Column: Locked Rotor Mult)</summary>
    [Category("General")]
    [Description("Locked Rotor Mult")]
    public string? LockedRotorMult { get; set; }

    /// <summary>Asym Offset (Column: Asym Offset)</summary>
    [Category("General")]
    [Description("Asym Offset")]
    public string? AsymOffset { get; set; }

    /// <summary>Reduced Inrush Mult (Column: Reduced Inrush Mult)</summary>
    [Category("General")]
    [Description("Reduced Inrush Mult")]
    public string? ReducedInrushMult { get; set; }

    /// <summary>Accel Time (Column: Accel Time)</summary>
    [Category("General")]
    [Description("Accel Time")]
    public string? AccelTime { get; set; }

    /// <summary>Stall Time (Column: Stall Time)</summary>
    [Category("General")]
    [Description("Stall Time")]
    public string? StallTime { get; set; }

    /// <summary>Stall Time To (Column: Stall Time To)</summary>
    [Category("General")]
    [Description("Stall Time To")]
    public string? StallTimeTo { get; set; }

    /// <summary>Largest Motor HP (Column: Largest Motor HP)</summary>
    [Category("Protection")]
    [Description("Largest Motor HP")]
    [Required]
    public string? LargestMotorHP { get; set; }

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

    /// <summary>Alias for Motors (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Motors;
        set => Motors = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Motor"/> class.
    /// </summary>
    public Motor() { }

    /// <summary>
    /// Returns a string representation of the Motor.
    /// </summary>
    public override string ToString()
    {
        return $"Motor: {Motors}";
    }
}

