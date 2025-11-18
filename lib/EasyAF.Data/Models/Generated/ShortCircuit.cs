using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a ShortCircuit with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Equipment Duty Scenario Report" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Equipment Duty Scenario Report")]
public class ShortCircuit
{
    /// <summary>Bus Name (Column: Bus Name)</summary>
    [Category("Identity")]
    [Description("Bus Name")]
    [Required]
    public string? BusName { get; set; }

    /// <summary>Equipment Name (Column: Equipment Name)</summary>
    [Category("General")]
    [Description("Equipment Name")]
    public string? EquipmentName { get; set; }

    /// <summary>Worst Case (Column: Worst Case)</summary>
    [Category("Protection")]
    [Description("Worst Case")]
    public string? WorstCase { get; set; }

    /// <summary>Scenario (Column: Scenario)</summary>
    [Category("General")]
    [Description("Scenario")]
    [Required]
    public string? Scenario { get; set; }

    /// <summary>Fault Type (Column: Fault Type)</summary>
    [Category("Electrical")]
    [Description("Fault Type")]
    public string? FaultType { get; set; }

    /// <summary>Vpu (Column: Vpu)</summary>
    [Category("General")]
    [Description("Vpu")]
    public string? Vpu { get; set; }

    /// <summary>Bus Base kV (Column: Bus Base kV)</summary>
    [Category("Electrical")]
    [Description("Bus Base kV")]
    public string? BusBaseKV { get; set; }

    /// <summary>Bus No. of Phases (Column: Bus No. of Phases)</summary>
    [Category("Electrical")]
    [Description("Bus No. of Phases")]
    public string? BusNoOfPhases { get; set; }

    /// <summary>Equipment Manufacturer (Column: Equipment Manufacturer)</summary>
    [Category("Physical")]
    [Description("Equipment Manufacturer")]
    public string? EquipmentManufacturer { get; set; }

    /// <summary>Equipment Style (Column: Equipment Style)</summary>
    [Category("Physical")]
    [Description("Equipment Style")]
    public string? EquipmentStyle { get; set; }

    /// <summary>Test Standard (Column: Test Standard)</summary>
    [Category("Physical")]
    [Description("Test Standard")]
    public string? TestStandard { get; set; }

    /// <summary>1/2 Cycle Rating (Column: 1/2 Cycle Rating (kA))</summary>
    [Category("Protection")]
    [Description("1/2 Cycle Rating")]
    [Units("kA")]
    public string? HalfCycleRatingKA { get; set; }

    /// <summary>1/2 Cycle Duty (Column: 1/2 Cycle Duty (kA))</summary>
    [Category("Electrical")]
    [Description("1/2 Cycle Duty")]
    [Units("kA")]
    public string? HalfCycleDutyKA { get; set; }

    /// <summary>1/2 Cycle Duty (%) (Column: 1/2 Cycle Duty (%))</summary>
    [Category("Electrical")]
    [Description("1/2 Cycle Duty (%)")]
    [Units("%")]
    public string? HalfCycleDutyPercent { get; set; }

    /// <summary>Comments (Column: Comments)</summary>
    [Category("Metadata")]
    [Description("Comments")]
    public string? Comments { get; set; }

    /// <summary>Alias for BusName (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => BusName;
        set => BusName = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortCircuit"/> class.
    /// </summary>
    public ShortCircuit() { }

    /// <summary>
    /// Returns a string representation of the ShortCircuit.
    /// </summary>
    public override string ToString()
    {
        return $"ShortCircuit: {BusName}";
    }
}
