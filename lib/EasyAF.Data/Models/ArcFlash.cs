using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an arc flash study result entry with comprehensive calculation results from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// Arc flash study results document the incident energy, arc flash boundary, and PPE requirements
/// for electrical equipment under fault conditions.
/// </para>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Arc Flash Scenario Report" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Composite Key:</strong> (Id, Scenario) uniquely identifies each arc flash study result.
/// </para>
/// </remarks>
[EasyPowerClass("Arc Flash Scenario Report")]
public class ArcFlash
{
    // ========================================
    // IDENTITY (Composite Key)
    // ========================================
    
    /// <summary>Arc fault bus name. (Column: Arc Fault Bus Name)</summary>
    [Category("Identity")]
    [Description("Bus or location where arc flash was calculated")]
    [Required]
    public string? ArcFaultBusName { get; set; }
    
    /// <summary>Alias for ArcFaultBusName (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id 
    { 
        get => ArcFaultBusName; 
        set => ArcFaultBusName = value; 
    }
    
    /// <summary>Scenario name. (Column: Scenario)</summary>
    [Category("Study Results")]
    [Description("Study scenario name (e.g., Main-Min, Main-Max)")]
    [Required]
    public string? Scenario { get; set; }
    
    /// <summary>Worst case flag. (Column: Worst Case)</summary>
    [Category("Study Results")]
    [Description("Indicates if this is the worst-case scenario")]
    public string? WorstCase { get; set; }

    // ========================================
    // BUS & SYSTEM INFO
    // ========================================
    
    /// <summary>Arc fault bus voltage. (Column: Arc Fault Bus kV)</summary>
    [Category("Study Results")]
    [Units("kV")]
    [Description("Bus voltage level")]
    public string? BusKV { get; set; }

    // ========================================
    // UPSTREAM PROTECTIVE DEVICE
    // ========================================
    
    /// <summary>Upstream trip device name. (Column: Upstream Trip Device Name)</summary>
    [Category("Study Results")]
    [Description("Upstream protective device identifier")]
    public string? UpstreamDevice { get; set; }
    
    /// <summary>Upstream trip device function. (Column: Upstream Trip Device Function)</summary>
    [Category("Study Results")]
    [Description("Upstream device trip function or element")]
    public string? UpstreamDeviceFunction { get; set; }

    // ========================================
    // ARC FLASH PARAMETERS
    // ========================================
    
    /// <summary>Equipment type. (Column: Equip Type)</summary>
    [Category("Study Results")]
    [Description("Equipment type for arc flash calculation")]
    public string? EquipType { get; set; }
    
    /// <summary>Electrode configuration. (Column: Electrode Configuration)</summary>
    [Category("Study Results")]
    [Description("Electrode configuration (VCB, VCBB, HCB, etc.)")]
    public string? ElectrodeConfiguration { get; set; }
    
    /// <summary>Electrode gap in millimeters. (Column: Electrode Gap (mm))</summary>
    [Category("Study Results")]
    [Units("mm")]
    [Description("Electrode gap for arc flash calculation")]
    public string? ElectrodeGapMM { get; set; }

    // ========================================
    // FAULT CURRENTS
    // ========================================
    
    /// <summary>Bus bolted fault current. (Column: Bus Bolted Fault (kA))</summary>
    [Category("Study Results")]
    [Units("kA")]
    [Description("Bolted short-circuit current at bus")]
    public string? BoltedFaultKA { get; set; }
    
    /// <summary>Bus arc fault current. (Column: Bus Arc Fault (kA))</summary>
    [Category("Study Results")]
    [Units("kA")]
    [Description("Arcing fault current")]
    public string? ArcFaultKA { get; set; }

    // ========================================
    // TIMING
    // ========================================
    
    /// <summary>Trip time in seconds. (Column: Trip Time (sec))</summary>
    [Category("Study Results")]
    [Units("s")]
    [Description("Protective device trip time")]
    public string? TripTimeSec { get; set; }
    
    /// <summary>Opening time in seconds. (Column: Opening Time (sec))</summary>
    [Category("Study Results")]
    [Units("s")]
    [Description("Device opening time (mechanical delay)")]
    public string? OpeningTimeSec { get; set; }
    
    /// <summary>Arc time in seconds. (Column: Arc Time (sec))</summary>
    [Category("Study Results")]
    [Units("s")]
    [Description("Total arcing time (trip + opening)")]
    public string? ArcTimeSec { get; set; }

    // ========================================
    // CALCULATED RESULTS
    // ========================================
    
    /// <summary>Estimated arc flash boundary. (Column: Est Arc Flash Boundary (inches))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("Calculated arc flash boundary distance")]
    public string? ArcFlashBoundaryIn { get; set; }
    
    /// <summary>Working distance. (Column: Working Distance (inches))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("Assumed working distance for calculation")]
    public string? WorkingDistanceIn { get; set; }
    
    /// <summary>Incident energy. (Column: Incident Energy (cal/cm2))</summary>
    [Category("Study Results")]
    [Units("cal/cm²")]
    [Description("Calculated incident energy at working distance")]
    public string? IncidentEnergyCalPerCm2 { get; set; }

    // ========================================
    // METADATA
    // ========================================
    
    /// <summary>Comments. (Column: Comments)</summary>
    [Category("Metadata")]
    [Description("Study notes or comments")]
    public string? Comments { get; set; }

    // ========================================
    // METHODS
    // ========================================

    /// <summary>
    /// Initializes a new instance of the <see cref="ArcFlash"/> class.
    /// </summary>
    public ArcFlash() { }

    /// <summary>
    /// Returns a string representation of the arc flash study result.
    /// </summary>
    public override string ToString()
    {
        return $"ArcFaultBusName: {ArcFaultBusName}, Scenario: {Scenario}, IE: {IncidentEnergyCalPerCm2} cal/cm², AFB: {ArcFlashBoundaryIn} in";
    }
}
