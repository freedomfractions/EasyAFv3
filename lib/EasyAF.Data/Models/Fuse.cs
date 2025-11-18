using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a fuse with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// Fuses are overcurrent protective devices that interrupt by melting a conductor element.
/// This model includes nameplate data, ratings, TCC parameters, associated switching equipment, and reliability metrics.
/// </para>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Fuses" class in EasyPower CSV exports.
/// </para>
/// </remarks>
[EasyPowerClass("Fuses")]
public class Fuse
{
    // ========================================
    // IDENTITY & BASIC ELECTRICAL
    // ========================================
    
    /// <summary>Fuse identifier. (Column: Fuses)</summary>
    [Category("Identity")]
    [Description("Fuse identifier")]
    [Required]
    public string? Fuses { get; set; }
    
    /// <summary>Alias for Fuses (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id 
    { 
        get => Fuses; 
        set => Fuses = value; 
    }
    
    /// <summary>AC / DC designation. (Column: AC/DC)</summary>
    [Category("Electrical")]
    [Description("AC or DC power system designation")]
    public string? AcDc { get; set; }
    
    /// <summary>Status of the fuse. (Column: Status)</summary>
    [Category("Identity")]
    [Description("Operational status")]
    public string? Status { get; set; }
    
    /// <summary>Number of phases. (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("Number of phases (1, 2, or 3)")]
    [Required]
    public string? NoOfPhases { get; set; }
    
    /// <summary>Bus to which the fuse is connected. (Column: On Bus)</summary>
    [Category("Electrical")]
    [Description("Source bus connection")]
    [Required]
    public string? OnBus { get; set; }
    
    /// <summary>Base kV of the system. (Column: Base kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("System nominal voltage")]
    public string? BaseKV { get; set; }
    
    /// <summary>Connection type. (Column: Conn Type)</summary>
    [Category("Electrical")]
    [Description("Connection configuration")]
    public string? ConnectionType { get; set; }
    
    /// <summary>Rating standard. (Column: Standard)</summary>
    [Category("Protection")]
    [Description("Rating standard (ANSI, IEC, etc.)")]
    public string? Standard { get; set; }
    
    /// <summary>Normal operating state. (Column: Normal State)</summary>
    [Category("Electrical")]
    [Description("Normal operating position")]
    public string? NormalState { get; set; }
    
    /// <summary>Optional features. (Column: Options)</summary>
    [Category("Physical")]
    [Description("Special options or features")]
    public string? Options { get; set; }

    // ========================================
    // FUSE NAMEPLATE
    // ========================================
    
    /// <summary>Fuse manufacturer. (Column: Fuse Mfr)</summary>
    [Category("Physical")]
    [Description("Fuse manufacturer")]
    public string? Manufacturer { get; set; }
    
    /// <summary>Fuse type. (Column: Fuse Type)</summary>
    [Category("Physical")]
    [Description("Fuse type designation")]
    public string? Type { get; set; }
    
    /// <summary>Fuse style/class. (Column: Fuse Style)</summary>
    [Category("Physical")]
    [Description("Fuse style or class (e.g., K, J, L, RK5)")]
    public string? Style { get; set; }
    
    /// <summary>Fuse model. (Column: Model)</summary>
    [Category("Physical")]
    [Description("Fuse model number")]
    public string? Model { get; set; }
    
    /// <summary>TCC voltage rating. (Column: TCC kV)</summary>
    [Category("Protection")]
    [Units("kV")]
    [Description("Time-current curve voltage rating")]
    public string? TccKV { get; set; }
    
    /// <summary>Fuse ampere rating. (Column: Size)</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Fuse ampere rating")]
    public string? Size { get; set; }

    // ========================================
    // SHORT CIRCUIT RATINGS
    // ========================================
    
    /// <summary>Short-circuit interrupting rating. (Column: SC Int kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("Short-circuit interrupting capacity")]
    public string? ScIntKA { get; set; }
    
    /// <summary>SC test X/R ratio. (Column: SC Test X/R)</summary>
    [Category("Protection")]
    [Description("Test X/R ratio for short-circuit rating")]
    public string? ScTestXR { get; set; }
    
    /// <summary>Short-circuit test standard. (Column: SC Test Std)</summary>
    [Category("Protection")]
    [Description("Test standard for short-circuit rating")]
    public string? ScTestStd { get; set; }

    // ========================================
    // TCC (Time-Current Curve) - ANSI
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

    // ========================================
    // TCC - IEC
    // ========================================
    
    /// <summary>IEC breaking capacity. (Column: IEC Breaking kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("IEC breaking capacity")]
    public string? IecBreakingKA { get; set; }
    
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

    // ========================================
    // SWITCH (if integral with fuse)
    // ========================================
    
    /// <summary>Switch manufacturer. (Column: Switch Manufacturer)</summary>
    [Category("Physical")]
    [Description("Integral switch manufacturer")]
    public string? SwitchManufacturer { get; set; }
    
    /// <summary>Switch type. (Column: Switch Type)</summary>
    [Category("Physical")]
    [Description("Integral switch type")]
    public string? SwitchType { get; set; }
    
    /// <summary>Switch style. (Column: Switch Style)</summary>
    [Category("Physical")]
    [Description("Integral switch style")]
    public string? SwitchStyle { get; set; }
    
    /// <summary>Switch continuous current rating. (Column: Switch Cont A)</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Switch continuous current rating")]
    public string? SwitchContA { get; set; }
    
    /// <summary>Switch momentary rating. (Column: Switch Mom kA)</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("Switch momentary withstand rating")]
    public string? SwitchMomKA { get; set; }

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
    // OPERATING CONDITIONS
    // ========================================
    
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
    [Description("Planned action when fuse fails")]
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
    /// Initializes a new instance of the <see cref="Fuse"/> class.
    /// </summary>
    public Fuse() { }

    /// <summary>
    /// Returns a string representation of the fuse.
    /// </summary>
    public override string ToString()
    {
        return $"Fuses: {Fuses}, OnBus: {OnBus}, Mfr: {Manufacturer}, Type: {Type}, Style: {Style}, Size: {Size}";
    }
}
