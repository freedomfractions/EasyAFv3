using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a HVBreaker with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "HV Breakers" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("HV Breakers")]
public class HVBreaker
{
    /// <summary>HV Breakers (Column: HV Breakers)</summary>
    [Category("General")]
    [Description("HV Breakers")]
    [Required]
    public string? HVBreakers { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoofPhases { get; set; }

    /// <summary>On Bus (Column: On Bus)</summary>
    [Category("Identity")]
    [Description("On Bus")]
    [Required]
    public string? OnBus { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BasekV { get; set; }

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

    /// <summary>SC Test Std (Column: SC Test Std)</summary>
    [Category("Electrical")]
    [Description("SC Test Std")]
    public string? SCTestStd { get; set; }

    /// <summary>Cycles (Column: Cycles)</summary>
    [Category("General")]
    [Description("Cycles")]
    public string? Cycles { get; set; }

    /// <summary>C&L/Crest kA (Column: C&L/Crest kA)</summary>
    [Category("Protection")]
    [Description("C&L/Crest kA")]
    public string? C&LCrestkA { get; set; }

    /// <summary>Max kV (Column: Max kV)</summary>
    [Category("Electrical")]
    [Description("Max kV")]
    public string? MaxkV { get; set; }

    /// <summary>Rated kA @ Max kV (Column: Rated kA @ Max kV)</summary>
    [Category("Electrical")]
    [Description("Rated kA @ Max kV")]
    public string? RatedkA@MaxkV { get; set; }

    /// <summary>K-factor (Column: K-factor)</summary>
    [Category("General")]
    [Description("K-factor")]
    public string? Kfactor { get; set; }

    /// <summary>Int kA (Column: Int kA)</summary>
    [Category("General")]
    [Description("Int kA")]
    public string? IntkA { get; set; }

    /// <summary>kA Min (Column: kA Min)</summary>
    [Category("General")]
    [Description("kA Min")]
    public string? KAMin { get; set; }

    /// <summary>kA Max (Column: kA Max)</summary>
    [Category("General")]
    [Description("kA Max")]
    public string? KAMax { get; set; }

    /// <summary>IEC Rated kV (Column: IEC Rated kV)</summary>
    [Category("Electrical")]
    [Description("IEC Rated kV")]
    public string? IECRatedkV { get; set; }

    /// <summary>IEC Making kA (Column: IEC Making kA)</summary>
    [Category("General")]
    [Description("IEC Making kA")]
    public string? IECMakingkA { get; set; }

    /// <summary>IEC Opening Time (Column: IEC Opening Time (s))</summary>
    [Category("General")]
    [Description("IEC Opening Time")]
    [Units("s")]
    public string? IECOpeningTime { get; set; }

    /// <summary>IEC Breaking kA (Column: IEC Breaking kA)</summary>
    [Category("General")]
    [Description("IEC Breaking kA")]
    public string? IECBreakingkA { get; set; }

    /// <summary>IEC DC Time Const (Column: IEC DC Time Const (s))</summary>
    [Category("Protection")]
    [Description("IEC DC Time Const")]
    [Units("s")]
    public string? IECDCTimeConst { get; set; }

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
    public string? FailureRate { get; set; }

    /// <summary>Repair Time (Column: Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Repair Time")]
    [Units("h")]
    public string? RepairTime { get; set; }

    /// <summary>Replace Time (Column: Replace Time (h))</summary>
    [Category("Reliability")]
    [Description("Replace Time")]
    [Units("h")]
    public string? ReplaceTime { get; set; }

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

    /// <summary>Alias for HVBreakers (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => HVBreakers;
        set => HVBreakers = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HVBreaker"/> class.
    /// </summary>
    public HVBreaker() { }

    /// <summary>
    /// Returns a string representation of the HVBreaker.
    /// </summary>
    public override string ToString()
    {
        return $"HVBreaker: {HVBreakers}";
    }
}
