using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an electric motor with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// Motors convert electrical energy to mechanical energy and are one of the most common loads
/// in industrial and commercial electrical systems.
/// </para>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Motors" class in EasyPower CSV exports.
/// </para>
/// </remarks>
[EasyPowerClass("Motors")]
public class Motor
{
    // ========================================
    // IDENTITY & BASIC INFO
    // ========================================
    
    /// <summary>Motor identifier. (Column: Motors)</summary>
    [Category("Identity")]
    [Description("Motor identifier")]
    [Required]
    public string? Motors { get; set; }
    
    /// <summary>Alias for Motors (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id 
    { 
        get => Motors; 
        set => Motors = value; 
    }
    
    /// <summary>AC / DC designation. (Column: AC/DC)</summary>
    [Category("Electrical")]
    [Description("AC or DC motor designation")]
    public string? AcDc { get; set; }
    
    /// <summary>Status of the motor. (Column: Status)</summary>
    [Category("Identity")]
    [Description("Operational status")]
    public string? Status { get; set; }
    
    /// <summary>Number of phases. (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("Number of phases (1 or 3)")]
    public string? NoOfPhases { get; set; }
    
    /// <summary>Bus connection. (Column: On Bus)</summary>
    [Category("Electrical")]
    [Description("Source bus connection")]
    [Required]
    public string? OnBus { get; set; }
    
    /// <summary>Base kV. (Column: Base kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("System nominal voltage")]
    public string? BaseKV { get; set; }

    // ========================================
    // NAMEPLATE RATINGS
    // ========================================
    
    /// <summary>Motor horsepower. (Column: HP)</summary>
    [Category("Physical")]
    [Units("HP")]
    [Description("Nameplate horsepower rating")]
    [Required]
    public string? HP { get; set; }
    
    /// <summary>Motor RPM. (Column: RPM)</summary>
    [Category("Physical")]
    [Units("RPM")]
    [Description("Nameplate speed")]
    public string? RPM { get; set; }
    
    /// <summary>Efficiency. (Column: Efficiency%)</summary>
    [Category("Physical")]
    [Units("%")]
    [Description("Full-load efficiency")]
    public string? EfficiencyPercent { get; set; }
    
    /// <summary>Power factor. (Column: PF%)</summary>
    [Category("Physical")]
    [Units("%")]
    [Description("Full-load power factor")]
    public string? PFPercent { get; set; }
    
    /// <summary>Service factor. (Column: Service Factor)</summary>
    [Category("Physical")]
    [Description("Service factor (typically 1.0 or 1.15)")]
    public string? ServiceFactor { get; set; }
    
    /// <summary>Full-load amps. (Column: FLA)</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Full-load current")]
    public string? FLA { get; set; }
    
    /// <summary>Full-load kW. (Column: kW)</summary>
    [Category("Physical")]
    [Units("kW")]
    [Description("Full-load kilowatts")]
    public string? KW { get; set; }

    // ========================================
    // MOTOR TYPE & DESIGN
    // ========================================
    
    /// <summary>Motor type. (Column: Type)</summary>
    [Category("Physical")]
    [Description("Motor type (Induction, Synchronous, etc.)")]
    public string? Type { get; set; }
    
    /// <summary>Motor design. (Column: Design)</summary>
    [Category("Physical")]
    [Description("NEMA design letter (A, B, C, D)")]
    public string? Design { get; set; }
    
    /// <summary>Motor code. (Column: Code)</summary>
    [Category("Physical")]
    [Description("NEMA code letter for starting kVA")]
    public string? Code { get; set; }
    
    /// <summary>Enclosure type. (Column: Enclosure)</summary>
    [Category("Physical")]
    [Description("Enclosure type (TEFC, ODP, etc.)")]
    public string? Enclosure { get; set; }

    // ========================================
    // STARTING CHARACTERISTICS
    // ========================================
    
    /// <summary>Starting method. (Column: Starting)</summary>
    [Category("Control")]
    [Description("Starting method (Across-the-line, Reduced voltage, VFD, etc.)")]
    public string? Starting { get; set; }
    
    /// <summary>Locked rotor amps. (Column: LRA)</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Locked rotor current")]
    public string? LRA { get; set; }
    
    /// <summary>Locked rotor kVA. (Column: LR kVA)</summary>
    [Category("Physical")]
    [Units("kVA")]
    [Description("Locked rotor kVA")]
    public string? LRkVA { get; set; }
    
    /// <summary>Starting kVA multiplier. (Column: Start kVA x FL)</summary>
    [Category("Physical")]
    [Description("Ratio of starting kVA to full-load kVA")]
    public string? StartKVAxFL { get; set; }

    // ========================================
    // OPERATING CONDITIONS
    // ========================================
    
    /// <summary>Load percentage. (Column: Load%)</summary>
    [Category("Electrical")]
    [Units("%")]
    [Description("Operating load as percentage of full load")]
    public string? LoadPercent { get; set; }
    
    /// <summary>Operating mode. (Column: Mode)</summary>
    [Category("Electrical")]
    [Description("Operating mode (Running, Starting, etc.)")]
    public string? Mode { get; set; }
    
    /// <summary>Operating power factor. (Column: Operating PF%)</summary>
    [Category("Electrical")]
    [Units("%")]
    [Description("Actual operating power factor")]
    public string? OperatingPFPercent { get; set; }
    
    /// <summary>Operating efficiency. (Column: Operating Eff%)</summary>
    [Category("Electrical")]
    [Units("%")]
    [Description("Actual operating efficiency")]
    public string? OperatingEffPercent { get; set; }

    // ========================================
    // CONTRIBUTION TO FAULT
    // ========================================
    
    /// <summary>Motor contribution flag. (Column: Contribute)</summary>
    [Category("Protection")]
    [Description("Include motor contribution in fault calculations")]
    public string? Contribute { get; set; }
    
    /// <summary>X/R ratio. (Column: X/R)</summary>
    [Category("Protection")]
    [Description("Motor impedance X/R ratio")]
    public string? XRRatio { get; set; }

    // ========================================
    // PROTECTION
    // ========================================
    
    /// <summary>Overload protection size. (Column: OL Size)</summary>
    [Category("Protection")]
    [Units("A")]
    [Description("Overload relay size")]
    public string? OLSize { get; set; }
    
    /// <summary>Branch circuit protection. (Column: Branch Prot)</summary>
    [Category("Protection")]
    [Description("Branch circuit protective device")]
    public string? BranchProt { get; set; }

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
    [Description("Planned action when motor fails")]
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
    /// Initializes a new instance of the <see cref="Motor"/> class.
    /// </summary>
    public Motor() { }

    /// <summary>
    /// Returns a string representation of the motor.
    /// </summary>
    public override string ToString()
    {
        return $"Motor: {Motors}, OnBus: {OnBus}, {HP} HP, {RPM} RPM, FLA: {FLA}A";
    }
}
