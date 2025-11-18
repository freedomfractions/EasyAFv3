using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an electrical bus with its key properties as exported from external tools (e.g., EasyPower).
/// All fields are strings to faithfully capture source text without premature parsing.
/// This allows maximum flexibility when mapping data from external sources that may change column names or formats.
/// </summary>
public class Bus
{
    // Identity & basics
    /// <summary>Unique identifier / name of the bus. (Column: Buses or Bus Name)</summary>
    [Category("Identity")]
    [Description("Unique identifier for the bus")]
    [Required]
    public string? Id { get; set; }
    
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
    public string? Voltage { get; set; }
    
    /// <summary>Number of phases. (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("Number of phases (1, 2, or 3)")]
    [Required]
    public string? Phases { get; set; }

    // Grouping / metadata
    /// <summary>Logical area grouping. (Column: Area)</summary>
    public string? Area { get; set; }
    
    /// <summary>Zone grouping. (Column: Zone)</summary>
    public string? Zone { get; set; }
    
    /// <summary>Device code or identifier. (Column: Device Code)</summary>
    public string? DeviceCode { get; set; }

    // Nameplate / construction
    /// <summary>Manufacturer of the bus. (Column: Manufacturer)</summary>
    public string? Manufacturer { get; set; }
    
    /// <summary>Type or model of the bus. (Column: Type)</summary>
    public string? Type { get; set; }
    
    /// <summary>Bus rating in Amperes. (Column: Bus Rating (A))</summary>
    public string? BusRatingA { get; set; }
    
    /// <summary>Short-circuit withstand strength in kA. (Column: Bus Bracing (kA))</summary>
    public string? BusBracingKA { get; set; }
    
    /// <summary>Standard to which the bus is tested. (Column: Test Standard)</summary>
    public string? TestStandard { get; set; }
    
    /// <summary>Material of the bus conductor. (Column: Material)</summary>
    public string? Material { get; set; }
    
    /// <summary>Mounting arrangement of the bus. (Column: Mounting)</summary>
    public string? Mounting { get; set; }

    // Downstream connected metrics (Dn ...)
    /// <summary>Connected kVA downstream. (Column: Dn Conn kVA)</summary>
    public string? DnConnKVA { get; set; }
    
    /// <summary>Connected FLA downstream. (Column: Dn Conn FLA)</summary>
    public string? DnConnFLA { get; set; }
    
    /// <summary>Demand kVA downstream. (Column: Dn Demand kVA)</summary>
    public string? DnDemandKVA { get; set; }
    
    /// <summary>Demand FLA downstream. (Column: Dn Demand FLA)</summary>
    public string? DnDemandFLA { get; set; }
    
    /// <summary>Code kVA for downstream equipment. (Column: Dn Code kVA)</summary>
    public string? DnCodeKVA { get; set; }
    
    /// <summary>Code FLA for downstream equipment. (Column: Dn Code FLA)</summary>
    public string? DnCodeFLA { get; set; }

    // Arc flash related
    /// <summary>Enclosure type for arc flash calculations. (Column: Enclosure Type)</summary>
    public string? EnclosureType { get; set; }
    
    /// <summary>Enclosure height (inches). (Column: Enclosure Height (in))</summary>
    public string? EnclosureHeightIn { get; set; }
    
    /// <summary>Enclosure width (inches). (Column: Enclosure Width (in))</summary>
    public string? EnclosureWidthIn { get; set; }
    
    /// <summary>Enclosure depth (inches). (Column: Enclosure Depth (in))</summary>
    public string? EnclosureDepthIn { get; set; }
    
    /// <summary>Stored AFB distance (inches). (Column: Stored AFB (in))</summary>
    public string? StoredAfbIn { get; set; }
    
    /// <summary>Stored AF IE conversion factor (cal/cm2). (Column: Stored AF IE (cal/cm2))</summary>
    public string? StoredAfIeCalPerCm2 { get; set; }
    
    /// <summary>Stored PPE details for arc flash. (Column: Stored AF PPE)</summary>
    public string? StoredAfPpe { get; set; }

    // Short circuit related
    /// <summary>Short-circuit symmetry in kA (ANSI standard). (Column: SC Sym kA (ANSI))</summary>
    public string? ScSymKAAnsi { get; set; }
    
    /// <summary>Short-circuit symmetry in kA (IEC standard). (Column: SC Sym kA (IEC))</summary>
    public string? ScSymKAIEC { get; set; }

    // Labeling / reliability / location
    /// <summary>Comments or notes for labeling. (Column: Label Comment)</summary>
    public string? LabelComment { get; set; }
    
    /// <summary>Number of labels to print. (Column: # Labels To Print)</summary>
    public string? LabelsToPrint { get; set; }
    
    /// <summary>Failure rate per year. (Column: Failure Rate (/year))</summary>
    public string? FailureRatePerYear { get; set; }
    
    /// <summary>Estimated repair time in hours. (Column: Repair Time (h))</summary>
    public string? RepairTimeHours { get; set; }
    
    /// <summary>Estimated replacement time in hours. (Column: Replace Time (h))</summary>
    public string? ReplaceTimeHours { get; set; }
    
    /// <summary>Cost associated with repair. (Column: Repair Cost)</summary>
    public string? RepairCost { get; set; }
    
    /// <summary>Cost associated with replacement. (Column: Replace Cost)</summary>
    public string? ReplaceCost { get; set; }
    
    /// <summary>Action to take upon failure. (Column: Action Upon Failure)</summary>
    public string? ActionUponFailure { get; set; }
    
    /// <summary>Cost of downtime per hour. (Column: Downtime Cost (h))</summary>
    public string? DowntimeCostHours { get; set; }
    
    /// <summary>Source of reliability data. (Column: Reliability Source)</summary>
    public string? ReliabilitySource { get; set; }
    
    /// <summary>Category of reliability. (Column: Reliability Category)</summary>
    public string? ReliabilityCategory { get; set; }
    
    /// <summary>Class of reliability. (Column: Reliability Class)</summary>
    public string? ReliabilityClass { get; set; }
    
    /// <summary>Facility where the bus is located. (Column: Facility)</summary>
    public string? Facility { get; set; }
    
    /// <summary>Name of the location. (Column: Location Name)</summary>
    public string? LocationName { get; set; }
    
    /// <summary>Description of the location. (Column: Location Description)</summary>
    public string? LocationDescription { get; set; }
    
    /// <summary>X coordinate for bus location. (Column: X)</summary>
    public string? X { get; set; }
    
    /// <summary>Y coordinate for bus location. (Column: Y)</summary>
    public string? Y { get; set; }
    
    /// <summary>Floor number where the bus is located. (Column: Floor)</summary>
    public string? Floor { get; set; }
    
    /// <summary>Status of the data entry. (Column: Data Status)</summary>
    public string? DataStatus { get; set; }
    
    /// <summary>Additional comments or notes. (Column: Comment)</summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bus"/> class.
    /// </summary>
    public Bus() { }

    /// <summary>
    /// Returns a string representation of the bus including its key identifying properties.
    /// </summary>
    /// <returns>A string describing the bus with ID, voltage, phases, area, zone, and status.</returns>
    public override string ToString()
    {
        return $"Id: {Id}, kV: {Voltage}, Phases: {Phases}, Area: {Area}, Zone: {Zone}, Status: {Status}";
    }
}
