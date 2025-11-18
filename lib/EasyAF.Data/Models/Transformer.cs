using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a 2-winding power or distribution transformer with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// Transformers convert voltage levels between different parts of the electrical distribution system.
/// This model includes nameplate data, impedance values, tap settings, losses, and thermal characteristics.
/// </para>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "2W Transformers" class in EasyPower CSV exports.
/// </para>
/// </remarks>
[EasyPowerClass("2W Transformers")]
public class Transformer
{
    // ========================================
    // IDENTITY & BASIC INFO
    // ========================================
    
    /// <summary>Transformer identifier. (Column: 2W Transformers)</summary>
    [Category("Identity")]
    [Description("Transformer identifier")]
    [Required]
    public string? Transformers2W { get; set; }
    
    /// <summary>Alias for Transformers2W (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id 
    { 
        get => Transformers2W; 
        set => Transformers2W = value; 
    }
    
    /// <summary>AC / DC designation. (Column: AC/DC)</summary>
    [Category("Electrical")]
    [Description("AC or DC power system designation")]
    public string? AcDc { get; set; }
    
    /// <summary>Status of the transformer. (Column: Status)</summary>
    [Category("Identity")]
    [Description("Operational status")]
    public string? Status { get; set; }
    
    /// <summary>Transformer type. (Column: Type)</summary>
    [Category("Physical")]
    [Description("Transformer type (Power, Distribution, etc.)")]
    public string? Type { get; set; }

    // ========================================
    // RATINGS
    // ========================================
    
    /// <summary>Transformer MVA rating. (Column: MVA)</summary>
    [Category("Physical")]
    [Units("MVA")]
    [Description("Nameplate MVA rating")]
    [Required]
    public string? MVA { get; set; }
    
    /// <summary>Primary nominal voltage. (Column: From Nom kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("Primary winding nominal voltage")]
    [Required]
    public string? FromNomKV { get; set; }
    
    /// <summary>Secondary nominal voltage. (Column: To Nom kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("Secondary winding nominal voltage")]
    [Required]
    public string? ToNomKV { get; set; }
    
    /// <summary>Primary tap voltage. (Column: From Tap kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("Primary winding tap voltage")]
    public string? FromTapKV { get; set; }
    
    /// <summary>Secondary tap voltage. (Column: To Tap kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("Secondary winding tap voltage")]
    public string? ToTapKV { get; set; }

    // ========================================
    // CONNECTION CONFIGURATION
    // ========================================
    
    /// <summary>Primary connection type. (Column: From Conn)</summary>
    [Category("Electrical")]
    [Description("Primary winding connection (Delta, Wye, etc.)")]
    public string? FromConn { get; set; }
    
    /// <summary>Secondary connection type. (Column: To Conn)</summary>
    [Category("Electrical")]
    [Description("Secondary winding connection (Delta, Wye, etc.)")]
    public string? ToConn { get; set; }
    
    /// <summary>Primary grounding. (Column: From Gnd)</summary>
    [Category("Electrical")]
    [Description("Primary winding grounding configuration")]
    public string? FromGnd { get; set; }
    
    /// <summary>Secondary grounding. (Column: To Gnd)</summary>
    [Category("Electrical")]
    [Description("Secondary winding grounding configuration")]
    public string? ToGnd { get; set; }
    
    /// <summary>Phase shift. (Column: Phase Shift)</summary>
    [Category("Electrical")]
    [Units("°")]
    [Description("Phase shift in degrees")]
    public string? PhaseShift { get; set; }

    // ========================================
    // IMPEDANCE
    // ========================================
    
    /// <summary>Positive sequence impedance. (Column: Z%)</summary>
    [Category("Electrical")]
    [Units("%")]
    [Description("Positive sequence impedance percent")]
    [Required]
    public string? ZPercent { get; set; }
    
    /// <summary>Zero sequence impedance. (Column: Z0%)</summary>
    [Category("Electrical")]
    [Units("%")]
    [Description("Zero sequence impedance percent")]
    public string? Z0Percent { get; set; }
    
    /// <summary>X/R ratio. (Column: X/R)</summary>
    [Category("Electrical")]
    [Description("Reactance to resistance ratio")]
    public string? XRRatio { get; set; }
    
    /// <summary>X0/R0 ratio. (Column: X0/R0)</summary>
    [Category("Electrical")]
    [Description("Zero sequence reactance to resistance ratio")]
    public string? X0R0Ratio { get; set; }

    // ========================================
    // TAP CHANGER (LTC)
    // ========================================
    
    /// <summary>Load tap changer flag. (Column: LTC)</summary>
    [Category("Control")]
    [Description("Load tap changer enabled")]
    public string? LTC { get; set; }
    
    /// <summary>Tap step size. (Column: Step Size%)</summary>
    [Category("Control")]
    [Units("%")]
    [Description("Tap step size percentage")]
    public string? StepSizePercent { get; set; }
    
    /// <summary>Maximum tap voltage. (Column: Max Tap kV)</summary>
    [Category("Control")]
    [Units("kV")]
    [Description("Maximum tap voltage")]
    public string? MaxTapKV { get; set; }
    
    /// <summary>Minimum tap voltage. (Column: Min Tap kV)</summary>
    [Category("Control")]
    [Units("kV")]
    [Description("Minimum tap voltage")]
    public string? MinTapKV { get; set; }
    
    /// <summary>Tap control mode. (Column: Control)</summary>
    [Category("Control")]
    [Description("Tap control mode (Voltage, MVAR, etc.)")]
    public string? Control { get; set; }
    
    /// <summary>Control setpoint. (Column: Setpoint)</summary>
    [Category("Control")]
    [Description("Control setpoint value")]
    public string? Setpoint { get; set; }

    // ========================================
    // LOSSES & THERMAL
    // ========================================
    
    /// <summary>No-load losses. (Column: No Load Loss kW)</summary>
    [Category("Physical")]
    [Units("kW")]
    [Description("No-load (core) losses")]
    public string? NoLoadLossKW { get; set; }
    
    /// <summary>Full-load losses. (Column: Full Load Loss kW)</summary>
    [Category("Physical")]
    [Units("kW")]
    [Description("Full-load (copper) losses")]
    public string? FullLoadLossKW { get; set; }
    
    /// <summary>Temperature rise. (Column: Temp Rise C)</summary>
    [Category("Physical")]
    [Units("°C")]
    [Description("Temperature rise above ambient")]
    public string? TempRiseC { get; set; }
    
    /// <summary>Cooling type. (Column: Cooling)</summary>
    [Category("Physical")]
    [Description("Cooling method (ONAN, ONAF, etc.)")]
    public string? Cooling { get; set; }

    // ========================================
    // GROUNDING IMPEDANCE
    // ========================================
    
    /// <summary>Primary grounding resistance. (Column: From Gnd R Ohm)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Primary neutral grounding resistance")]
    public string? FromGndROhm { get; set; }
    
    /// <summary>Primary grounding reactance. (Column: From Gnd X Ohm)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Primary neutral grounding reactance")]
    public string? FromGndXOhm { get; set; }
    
    /// <summary>Secondary grounding resistance. (Column: To Gnd R Ohm)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Secondary neutral grounding resistance")]
    public string? ToGndROhm { get; set; }
    
    /// <summary>Secondary grounding reactance. (Column: To Gnd X Ohm)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Secondary neutral grounding reactance")]
    public string? ToGndXOhm { get; set; }

    // ========================================
    // RELIABILITY DATA
    // ========================================
    
    /// <summary>Failure rate per year. (Column: Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Units("/year")]
    [Description("Expected failure rate")]
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
    [Description("Planned action when transformer fails")]
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
    /// Initializes a new instance of the <see cref="Transformer"/> class.
    /// </summary>
    public Transformer() { }

    /// <summary>
    /// Returns a string representation of the transformer.
    /// </summary>
    public override string ToString()
    {
        return $"Transformer: {Transformers2W}, {FromNomKV}kV/{ToNomKV}kV, {MVA} MVA, Z={ZPercent}%";
    }
}
