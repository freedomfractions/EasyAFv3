using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an electrical bus/switchgear with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// Buses are the primary nodes in an electrical distribution system where equipment connects.
/// This model supports Buses, Panels, and MCCs from EasyPower (all use "Buses" class name).
/// </para>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Buses" class in EasyPower CSV exports.
/// </para>
/// </remarks>
[EasyPowerClass("Buses")]
public class Bus
{
    // ========================================
    // IDENTITY & BASIC ELECTRICAL
    // ========================================
    
    /// <summary>Bus identifier. (Column: Buses)</summary>
    [Category("Identity")]
    [Description("Bus identifier")]
    [Required]
    public string? Buses { get; set; }
    
    /// <summary>Alias for Buses (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id 
    { 
        get => Buses; 
        set => Buses = value; 
    }
    
    /// <summary>AC / DC designation. (Column: AC/DC)</summary>
    [Category("Electrical")]
    [Description("AC or DC power system designation")]
    public string? AcDc { get; set; }
    
    /// <summary>Status of the bus/equipment. (Column: Status)</summary>
    [Category("Identity")]
    [Description("Operational status of the bus")]
    public string? Status { get; set; }
    
    /// <summary>Base kV of the bus. (Column: Base kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("Nominal voltage rating")]
    [Required]
    public string? BaseKV { get; set; }
    
    /// <summary>Number of phases. (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("Number of phases (1, 2, or 3)")]
    [Required]
    public string? NoOfPhases { get; set; }
    
    /// <summary>Service type designation. (Column: Service)</summary>
    [Category("Electrical")]
    [Description("Service type or classification")]
    public string? Service { get; set; }

    // ========================================
    // GROUPING / ORGANIZATION
    // ========================================
    
    /// <summary>Logical area grouping. (Column: Area)</summary>
    [Category("Location")]
    [Description("Logical area or region grouping")]
    public string? Area { get; set; }
    
    /// <summary>Zone grouping. (Column: Zone)</summary>
    [Category("Location")]
    [Description("Zone or sub-area designation")]
    public string? Zone { get; set; }
    
    /// <summary>Device code or identifier. (Column: Device Code)</summary>
    [Category("Identity")]
    [Description("Device code or classification")]
    public string? DeviceCode { get; set; }

    // ========================================
    // PHYSICAL / NAMEPLATE
    // ========================================
    
    /// <summary>Manufacturer of the bus. (Column: Manufacturer)</summary>
    [Category("Physical")]
    [Description("Equipment manufacturer")]
    public string? Manufacturer { get; set; }
    
    /// <summary>Type or model of the bus. (Column: Type)</summary>
    [Category("Physical")]
    [Description("Equipment type or model designation")]
    public string? Type { get; set; }
    
    /// <summary>Bus rating in Amperes. (Column: Bus Rating (A))</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Continuous current rating of the bus")]
    public string? BusRatingA { get; set; }
    
    /// <summary>Short-circuit withstand strength in kA. (Column: Bus Bracing (kA))</summary>
    [Category("Protection")]
    [Units("kA")]
    [Description("Short-circuit withstand capacity (bus bracing strength)")]
    public string? BusBracingKA { get; set; }
    
    /// <summary>Standard to which the bus is tested. (Column: Test Standard)</summary>
    [Category("Physical")]
    [Description("Test standard used for equipment qualification")]
    public string? TestStandard { get; set; }
    
    /// <summary>Material of the bus conductor. (Column: Material)</summary>
    [Category("Physical")]
    [Description("Conductor material (e.g., Copper, Aluminum)")]
    public string? Material { get; set; }
    
    /// <summary>Mounting arrangement of the bus. (Column: Mounting)</summary>
    [Category("Physical")]
    [Description("Physical mounting configuration")]
    public string? Mounting { get; set; }

    // ========================================
    // DOWNSTREAM LOAD METRICS
    // ========================================
    
    /// <summary>Connected kVA downstream. (Column: Dn Conn kVA)</summary>
    [Category("Electrical")]
    [Units("kVA")]
    [Description("Total connected load downstream")]
    public string? DnConnKVA { get; set; }
    
    /// <summary>Connected FLA downstream. (Column: Dn Conn FLA)</summary>
    [Category("Electrical")]
    [Units("A")]
    [Description("Total connected full-load amperes downstream")]
    public string? DnConnFLA { get; set; }
    
    /// <summary>Demand kVA downstream. (Column: Dn Demand kVA)</summary>
    [Category("Electrical")]
    [Units("kVA")]
    [Description("Demand load downstream (with diversity factors)")]
    public string? DnDemandKVA { get; set; }
    
    /// <summary>Demand FLA downstream. (Column: Dn Demand FLA)</summary>
    [Category("Electrical")]
    [Units("A")]
    [Description("Demand full-load amperes downstream")]
    public string? DnDemandFLA { get; set; }
    
    /// <summary>Code kVA for downstream equipment. (Column: Dn Code kVA)</summary>
    [Category("Electrical")]
    [Units("kVA")]
    [Description("NEC code-calculated load downstream")]
    public string? DnCodeKVA { get; set; }
    
    /// <summary>Code FLA for downstream equipment. (Column: Dn Code FLA)</summary>
    [Category("Electrical")]
    [Units("A")]
    [Description("NEC code-calculated amperes downstream")]
    public string? DnCodeFLA { get; set; }

    // ========================================
    // ARC FLASH CONFIGURATION
    // ========================================
    
    /// <summary>Equipment type designation for arc flash. (Column: Equipment)</summary>
    [Category("Study Results")]
    [Description("Equipment type for arc flash calculations")]
    public string? Equipment { get; set; }
    
    /// <summary>Arc flash solution method. (Column: AF Solution)</summary>
    [Category("Study Results")]
    [Description("Arc flash calculation solution method")]
    public string? AfSolution { get; set; }
    
    /// <summary>Forced incident energy value. (Column: Forced To Energy (cal/cm2))</summary>
    [Category("Study Results")]
    [Units("cal/cm²")]
    [Description("User-forced incident energy override")]
    public string? ForcedToEnergyCalPerCm2 { get; set; }
    
    /// <summary>Forced arc flash boundary distance. (Column: Forced To Arc Boundary (in))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("User-forced arc flash boundary override")]
    public string? ForcedToArcBoundaryIn { get; set; }
    
    /// <summary>Arc flash calculation option. (Column: AF Option)</summary>
    [Category("Study Results")]
    [Description("Arc flash calculation options/settings")]
    public string? AfOption { get; set; }
    
    /// <summary>Arc flash output designation. (Column: AF Output)</summary>
    [Category("Study Results")]
    [Description("Arc flash output format or type")]
    public string? AfOutput { get; set; }
    
    /// <summary>Working distance setting method. (Column: Working Distance Setting)</summary>
    [Category("Study Results")]
    [Description("Method for determining working distance")]
    public string? WorkingDistanceSetting { get; set; }
    
    /// <summary>Working distance in inches. (Column: Working Distance (in))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("Working distance for arc flash calculations")]
    public string? WorkingDistanceIn { get; set; }
    
    /// <summary>Electrode gap setting method. (Column: Electrode Gap Setting)</summary>
    [Category("Study Results")]
    [Description("Method for determining electrode gap")]
    public string? ElectrodeGapSetting { get; set; }
    
    /// <summary>Electrode gap in millimeters. (Column: Electrode Gap (mm))</summary>
    [Category("Study Results")]
    [Units("mm")]
    [Description("Electrode gap for arc flash calculations")]
    public string? ElectrodeGapMM { get; set; }
    
    /// <summary>Electrode configuration setting method. (Column: Electrode Configuration Setting)</summary>
    [Category("Study Results")]
    [Description("Method for determining electrode configuration")]
    public string? ElectrodeConfigurationSetting { get; set; }
    
    /// <summary>Electrode configuration type. (Column: Electrode Configuration)</summary>
    [Category("Study Results")]
    [Description("Electrode configuration (VCB, VCBB, HCB, etc.)")]
    public string? ElectrodeConfiguration { get; set; }
    
    /// <summary>Enclosure size setting method. (Column: Enclosure Size Setting)</summary>
    [Category("Study Results")]
    [Description("Method for determining enclosure dimensions")]
    public string? EnclosureSizeSetting { get; set; }
    
    /// <summary>Enclosure height in inches. (Column: Enclosure Height (in))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("Enclosure height for arc flash box calculations")]
    public string? EnclosureHeightIn { get; set; }
    
    /// <summary>Enclosure width in inches. (Column: Enclosure Width (in))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("Enclosure width for arc flash box calculations")]
    public string? EnclosureWidthIn { get; set; }
    
    /// <summary>Enclosure depth in inches. (Column: Enclosure Depth (in))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("Enclosure depth for arc flash box calculations")]
    public string? EnclosureDepthIn { get; set; }
    
    /// <summary>Stored arc flash boundary distance. (Column: Stored AFB (in))</summary>
    [Category("Study Results")]
    [Units("in")]
    [Description("Stored arc flash boundary from previous calculation")]
    public string? StoredAfbIn { get; set; }
    
    /// <summary>Stored incident energy. (Column: Stored AF IE (cal/cm2))</summary>
    [Category("Study Results")]
    [Units("cal/cm²")]
    [Description("Stored incident energy from previous calculation")]
    public string? StoredAfIeCalPerCm2 { get; set; }
    
    /// <summary>Stored PPE details for arc flash. (Column: Stored AF PPE)</summary>
    [Category("Study Results")]
    [Description("Stored PPE category or description")]
    public string? StoredAfPpe { get; set; }

    // ========================================
    // SHORT CIRCUIT RESULTS
    // ========================================
    
    /// <summary>Short-circuit symmetry in kA (ANSI standard). (Column: SC Sym kA (ANSI))</summary>
    [Category("Study Results")]
    [Units("kA")]
    [Description("Symmetrical short-circuit current (ANSI method)")]
    public string? ScSymKAAnsi { get; set; }
    
    /// <summary>Short-circuit symmetry in kA (IEC standard). (Column: SC Sym kA (IEC))</summary>
    [Category("Study Results")]
    [Units("kA")]
    [Description("Symmetrical short-circuit current (IEC method)")]
    public string? ScSymKAIEC { get; set; }

    // ========================================
    // LABELING / DOCUMENTATION
    // ========================================
    
    /// <summary>Comments or notes for labeling. (Column: Label Comment)</summary>
    [Category("Metadata")]
    [Description("Comments to appear on equipment labels")]
    public string? LabelComment { get; set; }
    
    /// <summary>Number of labels to print. (Column: # Labels To Print)</summary>
    [Category("Metadata")]
    [Description("Quantity of labels to generate")]
    public string? LabelsToPrint { get; set; }

    // ========================================
    // RELIABILITY DATA
    // ========================================
    
    /// <summary>Failure rate per year. (Column: Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Units("/year")]
    [Description("Expected failure rate (failures per year)")]
    public string? FailureRatePerYear { get; set; }
    
    /// <summary>Estimated repair time in hours. (Column: Repair Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Mean time to repair after failure")]
    public string? RepairTimeHours { get; set; }
    
    /// <summary>Estimated replacement time in hours. (Column: Replace Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Mean time to replace equipment")]
    public string? ReplaceTimeHours { get; set; }
    
    /// <summary>Cost associated with repair. (Column: Repair Cost)</summary>
    [Category("Reliability")]
    [Units("$")]
    [Description("Estimated cost to repair")]
    public string? RepairCost { get; set; }
    
    /// <summary>Cost associated with replacement. (Column: Replace Cost)</summary>
    [Category("Reliability")]
    [Units("$")]
    [Description("Estimated cost to replace")]
    public string? ReplaceCost { get; set; }
    
    /// <summary>Action to take upon failure. (Column: Action Upon Failure)</summary>
    [Category("Reliability")]
    [Description("Planned action when equipment fails")]
    public string? ActionUponFailure { get; set; }
    
    /// <summary>Cost of downtime per hour. (Column: Downtime Cost (h))</summary>
    [Category("Reliability")]
    [Units("$/h")]
    [Description("Cost per hour of equipment downtime")]
    public string? DowntimeCostHours { get; set; }
    
    /// <summary>Source of reliability data. (Column: Reliability Source)</summary>
    [Category("Reliability")]
    [Description("Data source for reliability metrics")]
    public string? ReliabilitySource { get; set; }
    
    /// <summary>Category of reliability. (Column: Reliability Category)</summary>
    [Category("Reliability")]
    [Description("Reliability classification category")]
    public string? ReliabilityCategory { get; set; }
    
    /// <summary>Class of reliability. (Column: Reliability Class)</summary>
    [Category("Reliability")]
    [Description("Reliability class designation")]
    public string? ReliabilityClass { get; set; }

    // ========================================
    // LOCATION / FACILITY
    // ========================================
    
    /// <summary>Facility where the bus is located. (Column: Facility)</summary>
    [Category("Location")]
    [Description("Facility or site name")]
    public string? Facility { get; set; }
    
    /// <summary>Name of the location. (Column: Location Name)</summary>
    [Category("Location")]
    [Description("Specific location name within facility")]
    public string? LocationName { get; set; }
    
    /// <summary>Description of the location. (Column: Location Description)</summary>
    [Category("Location")]
    [Description("Detailed location description")]
    public string? LocationDescription { get; set; }
    
    /// <summary>X coordinate for bus location. (Column: X)</summary>
    [Category("Location")]
    [Description("X coordinate on facility drawing")]
    public string? X { get; set; }
    
    /// <summary>Y coordinate for bus location. (Column: Y)</summary>
    [Category("Location")]
    [Description("Y coordinate on facility drawing")]
    public string? Y { get; set; }
    
    /// <summary>Floor number where the bus is located. (Column: Floor)</summary>
    [Category("Location")]
    [Description("Floor or level designation")]
    public string? Floor { get; set; }

    // ========================================
    // METADATA
    // ========================================
    
    /// <summary>Status of the data entry. (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data quality or completion status")]
    public string? DataStatus { get; set; }
    
    /// <summary>Additional comments or notes. (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("General comments or notes")]
    public string? Comments { get; set; }

    // ========================================
    // METHODS
    // ========================================

    /// <summary>
    /// Initializes a new instance of the <see cref="Bus"/> class.
    /// </summary>
    public Bus() { }

    /// <summary>
    /// Returns a summary string representation of the bus.
    /// </summary>
    public override string ToString()
    {
        return $"Buses: {Buses}, Voltage: {BaseKV}, Phases: {NoOfPhases}";
    }
}
