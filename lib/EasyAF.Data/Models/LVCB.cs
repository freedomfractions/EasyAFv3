using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a low voltage circuit breaker with comprehensive properties from EasyPower exports.
/// Trip unit settings are flattened onto this class for simplified mapping and data processing.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// Low voltage circuit breakers (LVCBs) are protective devices rated below 1000V AC or 1500V DC.
/// This model includes nameplate data, protection settings, trip unit parameters, and reliability metrics.
/// </para>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "LV Breakers" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Trip Unit Design:</strong> All trip unit properties are flattened with consistent naming
/// (no nested TripUnit object). Legacy TripUnit property has been removed as of v0.2.0.
/// </para>
/// </remarks>
[EasyPowerClass("LV Breakers")]
public class LVCB
{
    // ========================================
    // IDENTITY & BASIC ELECTRICAL
    // ========================================
    
    /// <summary>Unique identifier for the breaker. (Column: LV Breakers)</summary>
    [Category("Identity")]
    [Description("Unique identifier for the circuit breaker")]
    [Required]
    public string? Id { get; set; }
    
    /// <summary>AC / DC designation. (Column: AC/DC)</summary>
    [Category("Electrical")]
    [Description("AC or DC power system designation")]
    public string? AcDc { get; set; }
    
    /// <summary>Status of the breaker. (Column: Status)</summary>
    [Category("Identity")]
    [Description("Operational status")]
    public string? Status { get; set; }
    
    /// <summary>Number of phases. (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("Number of phases (1, 2, or 3)")]
    [Required]
    public string? Phases { get; set; }
    
    /// <summary>Bus to which the breaker is connected. (Column: On Bus)</summary>
    [Category("Electrical")]
    [Description("Source bus connection")]
    [Required]
    public string? Bus { get; set; }
    
    /// <summary>Base kV of the system. (Column: Base kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("System nominal voltage")]
    public string? Voltage { get; set; }
    
    /// <summary>Connection type. (Column: Conn Type)</summary>
    [Category("Electrical")]
    [Description("Connection configuration")]
    public string? ConnectionType { get; set; }
    
    /// <summary>Breaker class designation. (Column: Class)</summary>
    [Category("Physical")]
    [Description("Breaker class or category")]
    public string? Class { get; set; }
    
    /// <summary>Optional features or settings. (Column: Options)</summary>
    [Category("Physical")]
    [Description("Special options or features")]
    public string? Options { get; set; }

    // ========================================
    // BREAKER NAMEPLATE
    // ========================================
    
    /// <summary>Breaker manufacturer. (Column: Breaker Mfr)</summary>
    [Category("Physical")]
    [Description("Circuit breaker manufacturer")]
    public string? Manufacturer { get; set; }
    
    /// <summary>Breaker type. (Column: Breaker Type)</summary>
    [Category("Physical")]
    [Description("Breaker type designation")]
    public string? BreakerType { get; set; }
    
    /// <summary>Breaker style/model. (Column: Breaker Style)</summary>
    [Category("Physical")]
    [Description("Breaker style or model number")]
    public string? Style { get; set; }
    
    /// <summary>Continuous current rating. (Column: Cont Current (A))</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Continuous current rating")]
    public string? ContCurrentA { get; set; }
    
    /// <summary>Frame size. (Column: Frame (A))</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Frame size rating")]
    public string? FrameSize { get; set; }

    // ========================================
    // ZONE SELECTIVE INTERLOCKING (ZSI)
    // ========================================
    
    /// <summary>Short-time ZSI setting. (Column: ST ZSI)</summary>
    [Category("Protection")]
    [Description("Short-time zone selective interlocking")]
    public string? StZsi { get; set; }
    
    /// <summary>Short-time ZSI I²t setting. (Column: ST ZSI I2T)</summary>
    [Category("Protection")]
    [Description("Short-time ZSI I²t mode")]
    public string? StZsiI2t { get; set; }
    
    /// <summary>Short-time ZSI delay. (Column: ST ZSI Delay)</summary>
    [Category("Protection")]
    [Description("Short-time ZSI delay setting")]
    public string? StZsiDelay { get; set; }
    
    /// <summary>Instantaneous ZSI setting. (Column: Inst ZSI)</summary>
    [Category("Protection")]
    [Description("Instantaneous zone selective interlocking")]
    public string? InstZsi { get; set; }
    
    /// <summary>Ground fault ZSI setting. (Column: Gnd ZSI)</summary>
    [Category("Protection")]
    [Description("Ground fault zone selective interlocking")]
    public string? GndZsi { get; set; }
    
    /// <summary>Ground ZSI I²t setting. (Column: Gnd ZSI I2T)</summary>
    [Category("Protection")]
    [Description("Ground fault ZSI I²t mode")]
    public string? GndZsiI2t { get; set; }
    
    /// <summary>Ground ZSI delay. (Column: Gnd ZSI Delay)</summary>
    [Category("Protection")]
    [Description("Ground fault ZSI delay setting")]
    public string? GndZsiDelay { get; set; }
    
    /// <summary>Self-restraint setting. (Column: Self Restrain)</summary>
    [Category("Protection")]
    [Description("Self-restraint coordination mode")]
    public string? SelfRestrain { get; set; }
    
    /// <summary>Thermal ZSI setting. (Column: T-ZSI)</summary>
    [Category("Protection")]
    [Description("Thermal zone selective interlocking")]
    public string? TZsi { get; set; }

    // ========================================
    // INTEGRAL FUSE (if applicable)
    // ========================================
    
    /// <summary>Fuse manufacturer. (Column: Fuse Mfr)</summary>
    [Category("Protection")]
    [Description("Integral fuse manufacturer")]
    public string? FuseMfr { get; set; }
    
    /// <summary>Fuse type. (Column: Fuse Type)</summary>
    [Category("Protection")]
    [Description("Integral fuse type")]
    public string? FuseType { get; set; }
    
    /// <summary>Fuse style. (Column: Fuse Style)</summary>
    [Category("Protection")]
    [Description("Integral fuse style")]
    public string? FuseStyle { get; set; }
    
    /// <summary>Fuse size. (Column: Fuse Size)</summary>
    [Category("Protection")]
    [Description("Integral fuse ampere rating")]
    public string? FuseSize { get; set; }

    // ========================================
    // MOTOR OVERLOAD (if applicable)
    // ========================================
    
    /// <summary>Motor overload manufacturer. (Column: Mtr O/L Mfr)</summary>
    [Category("Protection")]
    [Description("Motor overload relay manufacturer")]
    public string? MtrOlMfr { get; set; }
    
    /// <summary>Motor overload type. (Column: Mtr O/L Type)</summary>
    [Category("Protection")]
    [Description("Motor overload relay type")]
    public string? MtrOlType { get; set; }
    
    /// <summary>Motor overload style. (Column: Mtr O/L Style)</summary>
    [Category("Protection")]
    [Description("Motor overload relay style")]
    public string? MtrOlStyle { get; set; }
    
    /// <summary>Motor full-load amperes. (Column: Motor FLA)</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Protected motor full-load current")]
    public string? MotorFla { get; set; }
    
    /// <summary>Motor service factor. (Column: Service Factor)</summary>
    [Category("Protection")]
    [Description("Motor service factor")]
    public string? ServiceFactor { get; set; }

    // ========================================
    // SHORT CIRCUIT RATINGS
    // ========================================
    
    /// <summary>Rating standard. (Column: Standard)</summary>
    [Category("Protection")]
    [Description("Rating standard (ANSI, IEC, etc.)")]
    public string? Standard { get; set; }
    
    /// <summary>SC rating basis. (Column: SC Rating Based On)</summary>
    [Category("Protection")]
    [Description("Short-circuit rating determination method")]
    public string? ScRatingBasedOn { get; set; }
    
    /// <summary>Short-circuit interrupting rating kA. (Column: SC Int kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("ANSI interrupting capacity (AIC)")]
    public string? AIC { get; set; }
    
    /// <summary>IEC breaking capacity. (Column: IEC Breaking kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC breaking capacity")]
    public string? IecBreakingKA { get; set; }
    
    /// <summary>Short-circuit test standard. (Column: SC Test Std)</summary>
    [Category("Protection")]
    [Description("Test standard for SC rating")]
    public string? ScTestStd { get; set; }

    // ========================================
    // TCC (Time-Current Curve) SETTINGS - ANSI
    // ========================================
    
    /// <summary>TCC clipping option. (Column: TCC Clipping)</summary>
    [Category("Protection")]
    [Description("Time-current curve clipping mode")]
    public string? TccClipping { get; set; }
    
    /// <summary>TCC momentary current. (Column: TCC Mom kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("TCC momentary (first-cycle) current limit")]
    public string? TccMomKA { get; set; }
    
    /// <summary>TCC interrupting current. (Column: TCC Int kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("TCC interrupting current limit")]
    public string? TccIntKA { get; set; }
    
    /// <summary>TCC 30-cycle current. (Column: TCC 30 Cyc kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("TCC 30-cycle current limit")]
    public string? Tcc30CycKA { get; set; }
    
    /// <summary>TCC ground momentary current. (Column: TCC Gnd Mom kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("TCC ground fault momentary current limit")]
    public string? TccGndMomKA { get; set; }
    
    /// <summary>TCC ground interrupting current. (Column: TCC Gnd Int kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("TCC ground fault interrupting current limit")]
    public string? TccGndIntKA { get; set; }
    
    /// <summary>TCC ground 30-cycle current. (Column: TCC Gnd 30 Cyc kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("TCC ground fault 30-cycle current limit")]
    public string? TccGnd30CycKA { get; set; }

    // ========================================
    // TCC SETTINGS - IEC
    // ========================================
    
    /// <summary>IEC TCC initial current. (Column: IEC TCC Initial kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC TCC initial symmetrical current")]
    public string? IecTccInitialKA { get; set; }
    
    /// <summary>IEC TCC breaking current. (Column: IEC TCC Breaking kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC TCC breaking current")]
    public string? IecTccBreakingKA { get; set; }
    
    /// <summary>IEC TCC breaking time. (Column: IEC TCC Breaking Time)</summary>
    [Category("Protection")]
    [Units("s")]
    [Description("IEC TCC breaking time")]
    public string? IecTccBreakingTime { get; set; }
    
    /// <summary>IEC TCC steady-state current. (Column: IEC TCC SS kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC TCC steady-state current")]
    public string? IecTccSsKA { get; set; }
    
    /// <summary>IEC TCC ground initial current. (Column: IEC TCC Gnd Initial kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC TCC ground fault initial current")]
    public string? IecTccGndInitialKA { get; set; }
    
    /// <summary>IEC TCC ground breaking current. (Column: IEC TCC Gnd Breaking kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC TCC ground fault breaking current")]
    public string? IecTccGndBreakingKA { get; set; }
    
    /// <summary>IEC TCC ground breaking time. (Column: IEC TCC Gnd Breaking Time)</summary>
    [Category("Protection")]
    [Units("s")]
    [Description("IEC TCC ground fault breaking time")]
    public string? IecTccGndBreakingTime { get; set; }
    
    /// <summary>IEC TCC ground steady-state current. (Column: IEC TCC Gnd SS kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC TCC ground fault steady-state current")]
    public string? IecTccGndSsKA { get; set; }

    // ========================================
    // OPERATING STATE
    // ========================================
    
    /// <summary>Normal operating state. (Column: Normal State)</summary>
    [Category("Electrical")]
    [Description("Normal breaker position (Open/Closed)")]
    public string? NormalState { get; set; }
    
    /// <summary>PCC demand kVA. (Column: PCC kVA Demand)</summary>
    [Category("Electrical")]
    [Units("kVA")]
    [Description("Point of common coupling demand kVA")]
    public string? PccKvaDemand { get; set; }
    
    /// <summary>PCC current ratio. (Column: PCC Isc/ILoad)</summary>
    [Category("Electrical")]
    [Description("Ratio of short-circuit to load current at PCC")]
    public string? PccIscILoad { get; set; }

    // ========================================
    // RELIABILITY DATA
    // ========================================
    
    /// <summary>Failure rate per year. (Column: Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Units("/year")]
    [Description("Expected failure rate (failures per year)")]
    public string? FailureRatePerYear { get; set; }
    
    /// <summary>Repair time in hours. (Column: Repair Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Mean time to repair")]
    public string? RepairTimeHours { get; set; }
    
    /// <summary>Replacement time in hours. (Column: Replace Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Mean time to replace")]
    public string? ReplaceTimeHours { get; set; }
    
    /// <summary>Repair cost. (Column: Repair Cost)</summary>
    [Category("Reliability")]
    [Units("$")]
    [Description("Estimated repair cost")]
    public string? RepairCost { get; set; }
    
    /// <summary>Replacement cost. (Column: Replace Cost)</summary>
    [Category("Reliability")]
    [Units("$")]
    [Description("Estimated replacement cost")]
    public string? ReplaceCost { get; set; }
    
    /// <summary>Action upon failure. (Column: Action Upon Failure)</summary>
    [Category("Reliability")]
    [Description("Planned action when breaker fails")]
    public string? ActionUponFailure { get; set; }
    
    /// <summary>Reliability data source. (Column: Reliability Source)</summary>
    [Category("Reliability")]
    [Description("Source of reliability data")]
    public string? ReliabilitySource { get; set; }
    
    /// <summary>Reliability category. (Column: Reliability Category)</summary>
    [Category("Reliability")]
    [Description("Reliability classification category")]
    public string? ReliabilityCategory { get; set; }
    
    /// <summary>Reliability class. (Column: Reliability Class)</summary>
    [Category("Reliability")]
    [Description("Reliability class designation")]
    public string? ReliabilityClass { get; set; }
    
    /// <summary>Short-circuit failure mode percentage. (Column: SC Failure Mode %)</summary>
    [Category("Reliability")]
    [Units("%")]
    [Description("Percentage of failures due to short circuits")]
    public string? ScFailureModePercent { get; set; }

    // ========================================
    // TRIP UNIT - MANUFACTURER & TYPE
    // ========================================
    
    /// <summary>Trip unit manufacturer. (Column: Mfr)</summary>
    [Category("Protection")]
    [Description("Trip unit manufacturer")]
    public string? TripUnitManufacturer { get; set; }
    
    /// <summary>Trip unit adjustable flag. (Column: Adjustable)</summary>
    [Category("Protection")]
    [Description("Indicates if trip unit settings are adjustable")]
    public string? TripUnitAdjustable { get; set; }
    
    /// <summary>Trip unit type. (Column: Type)</summary>
    [Category("Protection")]
    [Description("Trip unit type designation")]
    public string? TripUnitType { get; set; }
    
    /// <summary>Trip unit style. (Column: Style)</summary>
    [Category("Protection")]
    [Description("Trip unit style or model")]
    public string? TripUnitStyle { get; set; }
    
    /// <summary>Trip plug setting. (Column: Trip Plug)</summary>
    [Category("Protection")]
    [Description("Trip plug or rating plug setting")]
    public string? TripUnitTripPlug { get; set; }
    
    /// <summary>Sensor frame size. (Column: Sensor Frame)</summary>
    [Category("Protection")]
    [Description("Current sensor frame size")]
    public string? TripUnitSensorFrame { get; set; }

    // ========================================
    // TRIP UNIT - LONG TIME (LTPU/LTD)
    // ========================================
    
    /// <summary>Long-time pickup setting. (Column: LTPU)</summary>
    [Category("Protection")]
    [Description("Long-time pickup multiplier or setting")]
    public string? TripUnitLtpu { get; set; }
    
    /// <summary>Long-time pickup multiplier. (Column: LTPU Mult)</summary>
    [Category("Protection")]
    [Description("Long-time pickup multiplier factor")]
    public string? TripUnitLtpuMult { get; set; }
    
    /// <summary>Long-time pickup in amperes. (Column: LTPU (A))</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Long-time pickup current")]
    public string? TripUnitLtpuAmps { get; set; }
    
    /// <summary>Long-time delay band. (Column: LTD Band)</summary>
    [Category("Protection")]
    [Description("Long-time delay band or time dial setting")]
    public string? TripUnitLtdBand { get; set; }
    
    /// <summary>Long-time delay curve. (Column: LTD Curve)</summary>
    [Category("Protection")]
    [Description("Long-time delay curve type")]
    public string? TripUnitLtdCurve { get; set; }
    
    /// <summary>Trip adjust setting. (Column: Trip Adjust)</summary>
    [Category("Protection")]
    [Description("Trip adjustment setting")]
    public string? TripUnitTripAdjust { get; set; }
    
    /// <summary>Trip pickup setting. (Column: Trip Pickup)</summary>
    [Category("Protection")]
    [Description("Trip pickup setting")]
    public string? TripUnitTripPickup { get; set; }

    // ========================================
    // TRIP UNIT - SHORT TIME (STPU/STD)
    // ========================================
    
    /// <summary>Short-time pickup setting. (Column: STPU)</summary>
    [Category("Protection")]
    [Description("Short-time pickup multiplier or setting")]
    public string? TripUnitStpu { get; set; }
    
    /// <summary>Short-time pickup in amperes. (Column: STPU (A))</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Short-time pickup current")]
    public string? TripUnitStpuAmps { get; set; }
    
    /// <summary>Short-time delay band. (Column: STD Band)</summary>
    [Category("Protection")]
    [Description("Short-time delay band setting")]
    public string? TripUnitStdBand { get; set; }
    
    /// <summary>Short-time pickup I²t mode. (Column: STPU I2T)</summary>
    [Category("Protection")]
    [Description("Short-time I²t (energy-based) mode")]
    public string? TripUnitStpuI2t { get; set; }
    
    /// <summary>Short-time delay I²t setting. (Column: STD I2T)</summary>
    [Category("Protection")]
    [Description("Short-time delay I²t setting")]
    public string? TripUnitStdI2t { get; set; }

    // ========================================
    // TRIP UNIT - INSTANTANEOUS
    // ========================================
    
    /// <summary>Instantaneous pickup setting. (Column: Inst)</summary>
    [Category("Protection")]
    [Description("Instantaneous trip pickup setting")]
    public string? TripUnitInst { get; set; }
    
    /// <summary>Instantaneous override setting. (Column: Inst Override)</summary>
    [Category("Protection")]
    [Description("Instantaneous override mode")]
    public string? TripUnitInstOverride { get; set; }
    
    /// <summary>Instantaneous pickup in amperes. (Column: Inst (A))</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Instantaneous trip current")]
    public string? TripUnitInstAmps { get; set; }
    
    /// <summary>Instantaneous override pickup in amperes. (Column: Inst Override Pickup (A))</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Instantaneous override pickup current")]
    public string? TripUnitInstOverridePickupAmps { get; set; }

    // ========================================
    // TRIP UNIT - MAINTENANCE MODE
    // ========================================
    
    /// <summary>Maintenance mode setting. (Column: Maint)</summary>
    [Category("Protection")]
    [Description("Maintenance mode enabled/disabled")]
    public string? TripUnitMaint { get; set; }
    
    /// <summary>Maintenance setting value. (Column: Maint Setting)</summary>
    [Category("Protection")]
    [Description("Maintenance mode pickup setting")]
    public string? TripUnitMaintSetting { get; set; }
    
    /// <summary>Maintenance pickup in amperes. (Column: Maint (A))</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Maintenance mode pickup current")]
    public string? TripUnitMaintAmps { get; set; }

    // ========================================
    // TRIP UNIT - GROUND FAULT
    // ========================================
    
    /// <summary>Ground fault sensor type. (Column: Gnd Sensor)</summary>
    [Category("Protection")]
    [Description("Ground fault sensor type or configuration")]
    public string? TripUnitGndSensor { get; set; }
    
    /// <summary>Ground fault pickup setting. (Column: GFPU)</summary>
    [Category("Protection")]
    [Description("Ground fault pickup multiplier or setting")]
    public string? TripUnitGfpu { get; set; }
    
    /// <summary>Ground fault pickup in amperes. (Column: GFPU (A))</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Ground fault pickup current")]
    public string? TripUnitGfpuAmps { get; set; }
    
    /// <summary>Ground fault delay setting. (Column: GFD)</summary>
    [Category("Protection")]
    [Description("Ground fault delay time setting")]
    public string? TripUnitGfd { get; set; }
    
    /// <summary>Ground fault delay I²t mode. (Column: GFD I2T)</summary>
    [Category("Protection")]
    [Description("Ground fault I²t (energy-based) mode")]
    public string? TripUnitGfdI2t { get; set; }
    
    /// <summary>Ground fault maintenance pickup. (Column: Gnd Maint Pickup)</summary>
    [Category("Protection")]
    [Description("Ground fault maintenance mode pickup")]
    public string? TripUnitGndMaintPickup { get; set; }
    
    /// <summary>Ground fault maintenance in amperes. (Column: Gnd (A))</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Ground fault maintenance mode current")]
    public string? TripUnitGndMaintAmps { get; set; }

    // ========================================
    // METADATA
    // ========================================
    
    /// <summary>Data status. (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data quality or completion status")]
    public string? DataStatus { get; set; }
    
    /// <summary>Comments. (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("General comments or notes")]
    public string? Comments { get; set; }

    // ========================================
    // METHODS
    // ========================================

    /// <summary>
    /// Initializes a new instance of the <see cref="LVCB"/> class.
    /// </summary>
    public LVCB() { }

    /// <summary>
    /// Returns a summary string of the breaker's main properties.
    /// </summary>
    public override string ToString()
    {
        return $"Id: {Id}, Bus: {Bus}, Mfr: {Manufacturer}, Type: {BreakerType}, Style: {Style}, Frame: {FrameSize}, AIC: {AIC}";
    }

    /// <summary>
    /// Returns a string describing the breaker's interrupting capacity (AIC).
    /// </summary>
    public string AICToString() => $"AIC: {AIC}";
    
    /// <summary>
    /// Returns a string describing the breaker's frame size.
    /// </summary>
    public string FrameSizeToString() => $"FrameSize: {FrameSize}";
}
