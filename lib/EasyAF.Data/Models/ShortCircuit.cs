using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a short-circuit (equipment duty) study result entry with comprehensive duty calculations from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
[EasyPowerClass("Equipment Duty Scenario Report")]
public class ShortCircuit
{
    // ========================================
    // IDENTITY (Composite Key)
    // ========================================
    
    /// <summary>Bus name where equipment is located. (Column: Bus Name)</summary>
    [Category("Identity")]
    [Description("Bus where equipment is located")]
    [Required]
    public string? BusName { get; set; }
    
    /// <summary>Equipment name/identifier. (Column: Equipment Name)</summary>
    [Category("Identity")]
    [Description("Equipment identifier being evaluated")]
    [Required]
    public string? EquipmentName { get; set; }
    
    /// <summary>Alias for EquipmentName (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id 
    { 
        get => EquipmentName; 
        set => EquipmentName = value; 
    }
    
    /// <summary>Alias for BusName (backward compatibility - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Bus 
    { 
        get => BusName; 
        set => BusName = value; 
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
    
    /// <summary>Fault type. (Column: Fault Type)</summary>
    [Category("Study Results")]
    [Description("Type of fault (3-phase, L-G, L-L, etc.)")]
    public string? FaultType { get; set; }
    
    /// <summary>Voltage per unit. (Column: Vpu)</summary>
    [Category("Study Results")]
    [Units("pu")]
    [Description("Per-unit voltage during fault")]
    public string? Vpu { get; set; }
    
    /// <summary>Bus base voltage. (Column: Bus Base kV)</summary>
    [Category("Study Results")]
    [Units("kV")]
    [Description("Bus nominal voltage")]
    public string? BusBaseKV { get; set; }
    
    /// <summary>Bus number of phases. (Column: Bus No. of Phases)</summary>
    [Category("Study Results")]
    [Description("Number of phases at bus")]
    public string? BusNoOfPhases { get; set; }

    // ========================================
    // EQUIPMENT DETAILS
    // ========================================
    
    /// <summary>Equipment manufacturer. (Column: Equipment Manufacturer)</summary>
    [Category("Study Results")]
    [Description("Equipment manufacturer")]
    public string? EquipmentManufacturer { get; set; }
    
    /// <summary>Equipment style. (Column: Equipment Style)</summary>
    [Category("Study Results")]
    [Description("Equipment model or style")]
    public string? EquipmentStyle { get; set; }
    
    /// <summary>Test standard. (Column: Test Standard)</summary>
    [Category("Study Results")]
    [Description("Short-circuit test standard")]
    public string? TestStandard { get; set; }
    
    /// <summary>1/2 cycle rating in kA. (Column: 1/2 Cycle Rating (kA))</summary>
    [Category("Study Results")]
    [Units("kA")]
    [Description("Equipment half-cycle interrupting rating")]
    public string? HalfCycleRatingKA { get; set; }
    
    /// <summary>1/2 cycle duty in kA. (Column: 1/2 Cycle Duty (kA))</summary>
    [Category("Study Results")]
    [Units("kA")]
    [Description("Calculated half-cycle fault current duty")]
    public string? HalfCycleDutyKA { get; set; }
    
    /// <summary>1/2 cycle duty percentage. (Column: 1/2 Cycle Duty (%))</summary>
    [Category("Study Results")]
    [Units("%")]
    [Description("Duty as percentage of rating")]
    public string? HalfCycleDutyPercent { get; set; }
    
    /// <summary>Comments. (Column: Comments)</summary>
    [Category("Metadata")]
    [Description("Study notes or comments")]
    public string? Comments { get; set; }

    public ShortCircuit() { }

    /// <summary>
    /// Returns a string representation of the short-circuit study result.
    /// </summary>
    public override string ToString()
    {
        return $"EquipmentName: {EquipmentName}, BusName: {BusName}, Scenario: {Scenario}, Duty: {HalfCycleDutyKA} kA ({HalfCycleDutyPercent}%)";
    }
    
    public bool IsOverDutied()
    {
        if (double.TryParse(HalfCycleDutyKA, out double duty) && double.TryParse(HalfCycleRatingKA, out double rating))
            return duty > rating;
        return false;
    }
}
