using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a LVBreaker with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "LV Breakers" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("LV Breakers")]
public class LVBreaker
{
    /// <summary>LV Breakers (Column: LV Breakers)</summary>
    [Category("Identity")]
    [Description("LV Breakers")]
    [Required]
    public string? LVBreakers { get; set; }

    /// <summary>AC/DC (Column: AC/DC)</summary>
    [Category("General")]
    [Description("AC/DC")]
    public string? AcDc { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("General")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>On Bus (Column: On Bus)</summary>
    [Category("Electrical")]
    [Description("On Bus")]
    public string? OnBus { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BaseKV { get; set; }

    /// <summary>Conn Type (Column: Conn Type)</summary>
    [Category("Physical")]
    [Description("Conn Type")]
    public string? ConnType { get; set; }

    /// <summary>Class (Column: Class)</summary>
    [Category("General")]
    [Description("Class")]
    public string? Class { get; set; }

    /// <summary>Options (Column: Options)</summary>
    [Category("General")]
    [Description("Options")]
    public string? Options { get; set; }

    /// <summary>Breaker Mfr (Column: Breaker Mfr)</summary>
    [Category("General")]
    [Description("Breaker Mfr")]
    public string? BreakerMfr { get; set; }

    /// <summary>Breaker Type (Column: Breaker Type)</summary>
    [Category("Physical")]
    [Description("Breaker Type")]
    public string? BreakerType { get; set; }

    /// <summary>Breaker Style (Column: Breaker Style)</summary>
    [Category("Physical")]
    [Description("Breaker Style")]
    public string? BreakerStyle { get; set; }

    /// <summary>Frame (A) (Column: Frame (A))</summary>
    [Category("General")]
    [Description("Frame (A)")]
    [Units("A")]
    [Required]
    public string? FrameA { get; set; }

    /// <summary>Trip (Column: Trip)</summary>
    [Category("Protection")]
    [Description("Trip")]
    public string? Trip { get; set; }

    /// <summary>Trip Mfr (Column: Trip Mfr)</summary>
    [Category("Protection")]
    [Description("Trip Mfr")]
    public string? TripMfr { get; set; }

    /// <summary>Trip Type (Column: Trip Type)</summary>
    [Category("Physical")]
    [Description("Trip Type")]
    public string? TripType { get; set; }

    /// <summary>Trip Style (Column: Trip Style)</summary>
    [Category("Physical")]
    [Description("Trip Style")]
    public string? TripStyle { get; set; }

    /// <summary>Sensor Frame (Column: Sensor Frame)</summary>
    [Category("General")]
    [Description("Sensor Frame")]
    public string? SensorFrame { get; set; }

    /// <summary>Plug/Tap/Trip (Column: Plug/Tap/Trip)</summary>
    [Category("Protection")]
    [Description("Plug/Tap/Trip")]
    public string? PlugTapTrip { get; set; }

    /// <summary>LTPU Setting (Column: LTPU Setting)</summary>
    [Category("Control")]
    [Description("LTPU Setting")]
    public string? LTPUSetting { get; set; }

    /// <summary>LTPU Mult (Column: LTPU Mult)</summary>
    [Category("General")]
    [Description("LTPU Mult")]
    public string? LTPUMult { get; set; }

    /// <summary>Trip (A) (Column: Trip (A))</summary>
    [Category("Protection")]
    [Description("Trip (A)")]
    [Units("A")]
    [Required]
    public string? TripA { get; set; }

    /// <summary>LT Curve (Column: LT Curve)</summary>
    [Category("General")]
    [Description("LT Curve")]
    public string? LtCurve { get; set; }

    /// <summary>LTD Band (Column: LTD Band)</summary>
    [Category("General")]
    [Description("LTD Band")]
    public string? LtdBand { get; set; }

    /// <summary>Trip Adjust (Column: Trip Adjust)</summary>
    [Category("Protection")]
    [Description("Trip Adjust")]
    public string? TripAdjust { get; set; }

    /// <summary>Trip Pickup (Column: Trip Pickup)</summary>
    [Category("Protection")]
    [Description("Trip Pickup")]
    public string? TripPickup { get; set; }

    /// <summary>STPU Setting (Column: STPU Setting)</summary>
    [Category("Control")]
    [Description("STPU Setting")]
    public string? STPUSetting { get; set; }

    /// <summary>STPU Band (Column: STPU Band)</summary>
    [Category("General")]
    [Description("STPU Band")]
    public string? STPUBand { get; set; }

    /// <summary>STPU I2T (Column: STPU I2T)</summary>
    [Category("General")]
    [Description("STPU I2T")]
    public string? STPUI2t { get; set; }

    /// <summary>STPU (A) (Column: STPU (A))</summary>
    [Category("General")]
    [Description("STPU (A)")]
    [Units("A")]
    public string? STPUA { get; set; }

    /// <summary>Inst Setting (Column: Inst Setting)</summary>
    [Category("Control")]
    [Description("Inst Setting")]
    public string? InstSetting { get; set; }

    /// <summary>Inst Override (Column: Inst Override)</summary>
    [Category("General")]
    [Description("Inst Override")]
    public string? InstOverride { get; set; }

    /// <summary>Inst (A) (Column: Inst (A))</summary>
    [Category("General")]
    [Description("Inst (A)")]
    [Units("A")]
    public string? InstA { get; set; }

    /// <summary>Inst Ovr Pickup (A) (Column: Inst Ovr Pickup (A))</summary>
    [Category("General")]
    [Description("Inst Ovr Pickup (A)")]
    [Units("A")]
    public string? InstOvrPickupA { get; set; }

    /// <summary>Maint Mode (Column: Maint Mode)</summary>
    [Category("General")]
    [Description("Maint Mode")]
    public string? MaintMode { get; set; }

    /// <summary>Maint Setting (Column: Maint Setting)</summary>
    [Category("Control")]
    [Description("Maint Setting")]
    public string? MaintSetting { get; set; }

    /// <summary>Maint (A) (Column: Maint (A))</summary>
    [Category("General")]
    [Description("Maint (A)")]
    [Units("A")]
    public string? MaintA { get; set; }

    /// <summary>Gnd Sensor (Column: Gnd Sensor)</summary>
    [Category("General")]
    [Description("Gnd Sensor")]
    public string? GndSensor { get; set; }

    /// <summary>Gnd Pickup (Column: Gnd Pickup)</summary>
    [Category("General")]
    [Description("Gnd Pickup")]
    public string? GndPickup { get; set; }

    /// <summary>Gnd Delay (Column: Gnd Delay)</summary>
    [Category("General")]
    [Description("Gnd Delay")]
    public string? GndDelay { get; set; }

    /// <summary>Gnd I2T (Column: Gnd I2T)</summary>
    [Category("General")]
    [Description("Gnd I2T")]
    public string? GndI2t { get; set; }

    /// <summary>Gnd (A) (Column: Gnd (A))</summary>
    [Category("General")]
    [Description("Gnd (A)")]
    [Units("A")]
    public string? GndA { get; set; }

    /// <summary>Gnd Maint Pickup (Column: Gnd Maint Pickup)</summary>
    [Category("General")]
    [Description("Gnd Maint Pickup")]
    public string? GndMaintPickup { get; set; }

    /// <summary>Gnd Maint (A) (Column: Gnd Maint (A))</summary>
    [Category("General")]
    [Description("Gnd Maint (A)")]
    [Units("A")]
    public string? GndMaintA { get; set; }

    /// <summary>ST ZSI (Column: ST ZSI)</summary>
    [Category("General")]
    [Description("ST ZSI")]
    public string? StZSI { get; set; }

    /// <summary>ST ZSI I2T (Column: ST ZSI I2T)</summary>
    [Category("General")]
    [Description("ST ZSI I2T")]
    public string? StZSII2t { get; set; }

    /// <summary>ST ZSI Delay (Column: ST ZSI Delay)</summary>
    [Category("General")]
    [Description("ST ZSI Delay")]
    public string? StZSIDelay { get; set; }

    /// <summary>Inst ZSI (Column: Inst ZSI)</summary>
    [Category("General")]
    [Description("Inst ZSI")]
    public string? InstZSI { get; set; }

    /// <summary>Gnd ZSI (Column: Gnd ZSI)</summary>
    [Category("General")]
    [Description("Gnd ZSI")]
    public string? GndZSI { get; set; }

    /// <summary>Gnd ZSI I2T (Column: Gnd ZSI I2T)</summary>
    [Category("General")]
    [Description("Gnd ZSI I2T")]
    public string? GndZSII2t { get; set; }

    /// <summary>Gnd ZSI Delay (Column: Gnd ZSI Delay)</summary>
    [Category("General")]
    [Description("Gnd ZSI Delay")]
    public string? GndZSIDelay { get; set; }

    /// <summary>Self Restrain (Column: Self Restrain)</summary>
    [Category("General")]
    [Description("Self Restrain")]
    public string? SelfRestrain { get; set; }

    /// <summary>T-ZSI (Column: T-ZSI)</summary>
    [Category("General")]
    [Description("T-ZSI")]
    public string? TZSI { get; set; }

    /// <summary>Fuse Mfr (Column: Fuse Mfr)</summary>
    [Category("Protection")]
    [Description("Fuse Mfr")]
    public string? FuseMfr { get; set; }

    /// <summary>Fuse Type (Column: Fuse Type)</summary>
    [Category("Physical")]
    [Description("Fuse Type")]
    public string? FuseType { get; set; }

    /// <summary>Fuse Style (Column: Fuse Style)</summary>
    [Category("Physical")]
    [Description("Fuse Style")]
    public string? FuseStyle { get; set; }

    /// <summary>Fuse Size (Column: Fuse Size)</summary>
    [Category("Protection")]
    [Description("Fuse Size")]
    public string? FuseSize { get; set; }

    /// <summary>Mtr O/L Mfr (Column: Mtr O/L Mfr)</summary>
    [Category("General")]
    [Description("Mtr O/L Mfr")]
    public string? MtrOLMfr { get; set; }

    /// <summary>Mtr O/L Type (Column: Mtr O/L Type)</summary>
    [Category("Physical")]
    [Description("Mtr O/L Type")]
    public string? MtrOLType { get; set; }

    /// <summary>Mtr O/L Style (Column: Mtr O/L Style)</summary>
    [Category("Physical")]
    [Description("Mtr O/L Style")]
    public string? MtrOLStyle { get; set; }

    /// <summary>Motor FLA (Column: Motor FLA)</summary>
    [Category("General")]
    [Description("Motor FLA")]
    public string? MotorFLA { get; set; }

    /// <summary>Service Factor (Column: Service Factor)</summary>
    [Category("General")]
    [Description("Service Factor")]
    public string? ServiceFactor { get; set; }

    /// <summary>Standard (Column: Standard)</summary>
    [Category("General")]
    [Description("Standard")]
    public string? Standard { get; set; }

    /// <summary>SC Rating Based On (Column: SC Rating Based On)</summary>
    [Category("General")]
    [Description("SC Rating Based On")]
    public string? SCRatingBasedOn { get; set; }

    /// <summary>SC Int kA (Column: SC Int kA)</summary>
    [Category("General")]
    [Description("SC Int kA")]
    public string? SCIntKA { get; set; }

    /// <summary>IEC Breaking kA (Column: IEC Breaking kA)</summary>
    [Category("General")]
    [Description("IEC Breaking kA")]
    public string? IECBreakingKA { get; set; }

    /// <summary>SC Test Std (Column: SC Test Std)</summary>
    [Category("General")]
    [Description("SC Test Std")]
    public string? SCTestStd { get; set; }

    /// <summary>TCC Clipping (Column: TCC Clipping)</summary>
    [Category("General")]
    [Description("TCC Clipping")]
    public string? TCCClipping { get; set; }

    /// <summary>TCC Mom kA (Column: TCC Mom kA)</summary>
    [Category("General")]
    [Description("TCC Mom kA")]
    public string? TCCMomKA { get; set; }

    /// <summary>TCC Int kA (Column: TCC Int kA)</summary>
    [Category("General")]
    [Description("TCC Int kA")]
    public string? TCCIntKA { get; set; }

    /// <summary>TCC 30 Cyc kA (Column: TCC 30 Cyc kA)</summary>
    [Category("General")]
    [Description("TCC 30 Cyc kA")]
    public string? TCC30CycKA { get; set; }

    /// <summary>TCC Gnd Mom kA (Column: TCC Gnd Mom kA)</summary>
    [Category("General")]
    [Description("TCC Gnd Mom kA")]
    public string? TCCGndMomKA { get; set; }

    /// <summary>TCC Gnd Int kA (Column: TCC Gnd Int kA)</summary>
    [Category("General")]
    [Description("TCC Gnd Int kA")]
    public string? TCCGndIntKA { get; set; }

    /// <summary>TCC Gnd 30 Cyc kA (Column: TCC Gnd 30 Cyc kA)</summary>
    [Category("General")]
    [Description("TCC Gnd 30 Cyc kA")]
    public string? TCCGnd30CycKA { get; set; }

    /// <summary>IEC TCC Initial kA (Column: IEC TCC Initial kA)</summary>
    [Category("General")]
    [Description("IEC TCC Initial kA")]
    public string? IECTCCInitialKA { get; set; }

    /// <summary>IEC TCC Breaking kA (Column: IEC TCC Breaking kA)</summary>
    [Category("General")]
    [Description("IEC TCC Breaking kA")]
    public string? IECTCCBreakingKA { get; set; }

    /// <summary>IEC TCC Breaking Time (Column: IEC TCC Breaking Time)</summary>
    [Category("General")]
    [Description("IEC TCC Breaking Time")]
    public string? IECTCCBreakingTime { get; set; }

    /// <summary>IEC TCC SS kA (Column: IEC TCC SS kA)</summary>
    [Category("General")]
    [Description("IEC TCC SS kA")]
    public string? IECTCCSSKA { get; set; }

    /// <summary>IEC TCC Gnd Initial kA (Column: IEC TCC Gnd Initial kA)</summary>
    [Category("General")]
    [Description("IEC TCC Gnd Initial kA")]
    public string? IECTCCGndInitialKA { get; set; }

    /// <summary>IEC TCC Gnd Breaking kA (Column: IEC TCC Gnd Breaking kA)</summary>
    [Category("General")]
    [Description("IEC TCC Gnd Breaking kA")]
    public string? IECTCCGndBreakingKA { get; set; }

    /// <summary>IEC TCC Gnd Breaking Time (Column: IEC TCC Gnd Breaking Time)</summary>
    [Category("General")]
    [Description("IEC TCC Gnd Breaking Time")]
    public string? IECTCCGndBreakingTime { get; set; }

    /// <summary>IEC TCC Gnd SS kA (Column: IEC TCC Gnd SS kA)</summary>
    [Category("General")]
    [Description("IEC TCC Gnd SS kA")]
    public string? IECTCCGndSSKA { get; set; }

    /// <summary>Normal State (Column: Normal State)</summary>
    [Category("General")]
    [Description("Normal State")]
    public string? NormalState { get; set; }

    /// <summary>PCC kVA Demand (Column: PCC kVA Demand)</summary>
    [Category("Electrical")]
    [Description("PCC kVA Demand")]
    public string? PCCKVADemand { get; set; }

    /// <summary>PCC Isc/ILoad (Column: PCC Isc/ILoad)</summary>
    [Category("Demand")]
    [Description("PCC Isc/ILoad")]
    public string? PCCIscIload { get; set; }

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

    /// <summary>SC Failure Mode % (Column: SC Failure Mode %)</summary>
    [Category("Reliability")]
    [Description("SC Failure Mode %")]
    public string? SCFailureModePercent { get; set; }

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Identity")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for LVBreaker (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => LVBreakers;
        set => LVBreakers = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LVBreaker"/> class.
    /// </summary>
    public LVBreaker() { }

    /// <summary>
    /// Returns a string representation of the LVBreaker.
    /// </summary>
    public override string ToString()
    {
        return $"LVBreaker: {LVBreakers}";
    }
}

