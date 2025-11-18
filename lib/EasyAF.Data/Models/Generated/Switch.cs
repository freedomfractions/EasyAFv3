using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Switch with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Switches" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Switches")]
public class Switch
{
    /// <summary>Switches (Column: Switches)</summary>
    [Category("General")]
    [Description("Switches")]
    [Required]
    public string? Switches { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>On Bus (Column: On Bus)</summary>
    [Category("Identity")]
    [Description("On Bus")]
    [Required]
    public string? OnBus { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BaseKV { get; set; }

    /// <summary>Conn Type (Column: Conn Type)</summary>
    [Category("Physical")]
    [Description("Conn Type")]
    public string? ConnType { get; set; }

    /// <summary>Normal State (Column: Normal State)</summary>
    [Category("General")]
    [Description("Normal State")]
    public string? NormalState { get; set; }

    /// <summary>Manufacturer (Column: Manufacturer)</summary>
    [Category("Physical")]
    [Description("Manufacturer")]
    public string? Manufacturer { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>Style (Column: Style)</summary>
    [Category("Physical")]
    [Description("Style")]
    public string? Style { get; set; }

    /// <summary>Cont Current (Column: Cont Current (A))</summary>
    [Category("Electrical")]
    [Description("Cont Current")]
    [Units("A")]
    public string? ContCurrent { get; set; }

    /// <summary>Standard (Column: Standard)</summary>
    [Category("General")]
    [Description("Standard")]
    public string? Standard { get; set; }

    /// <summary>Assumed Open Time (Column: Assumed Open Time)</summary>
    [Category("General")]
    [Description("Assumed Open Time")]
    public string? AssumedOpenTime { get; set; }

    /// <summary>SC Mom kA (Column: SC Mom kA)</summary>
    [Category("Electrical")]
    [Description("SC Mom kA")]
    public string? SCMomkA { get; set; }

    /// <summary>IEC Peak Withstand kA (Column: IEC Peak Withstand kA)</summary>
    [Category("General")]
    [Description("IEC Peak Withstand kA")]
    public string? IECPeakWithstandkA { get; set; }

    /// <summary>SC Test Std (Column: SC Test Std)</summary>
    [Category("Electrical")]
    [Description("SC Test Std")]
    public string? SCTestStd { get; set; }

    /// <summary>PCC kVA Demand (Column: PCC kVA Demand)</summary>
    [Category("Electrical")]
    [Description("PCC kVA Demand")]
    public string? PCCkVADemand { get; set; }

    /// <summary>PCC Isc/ILoad (Column: PCC Isc/ILoad)</summary>
    [Category("Demand")]
    [Description("PCC Isc/ILoad")]
    public string? PCCIscILoad { get; set; }

    /// <summary>Failure Rate (Column: Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Description("Failure Rate")]
    [Units("/year")]
    public string? FailureRatePerYear { get; set; }

    /// <summary>Repair Time (Column: Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Repair Time")]
    [Units("h")]
    public string? RepairTimeH { get; set; }

    /// <summary>Replace Time (Column: Replace Time (h))</summary>
    [Category("Reliability")]
    [Description("Replace Time")]
    [Units("h")]
    public string? ReplaceTimeH { get; set; }

    /// <summary>Repair Cost (Column: Repair Cost)</summary>
    [Category("Reliability")]
    [Description("Repair Cost")]
    public string? RepairCost { get; set; }

    /// <summary>Replace Cost (Column: Replace Cost)</summary>
    [Category("Reliability")]
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
    public string? SCFailureMode% { get; set; }

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for Switches (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Switches;
        set => Switches = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Switch"/> class.
    /// </summary>
    public Switch() { }

    /// <summary>
    /// Returns a string representation of the Switch.
    /// </summary>
    public override string ToString()
    {
        return $"Switch: {Switches}";
    }
}

