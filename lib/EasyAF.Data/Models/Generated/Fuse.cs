using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Fuse with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Fuses" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Fuses")]
public class Fuse
{
    /// <summary>Fuses (Column: Fuses)</summary>
    [Category("General")]
    [Description("Fuses")]
    [Required]
    public string? Fuses { get; set; }

    /// <summary>AC/DC (Column: AC/DC)</summary>
    [Category("General")]
    [Description("AC/DC")]
    public string? ACDC { get; set; }

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

    /// <summary>Standard (Column: Standard)</summary>
    [Category("General")]
    [Description("Standard")]
    public string? Standard { get; set; }

    /// <summary>Normal State (Column: Normal State)</summary>
    [Category("General")]
    [Description("Normal State")]
    public string? NormalState { get; set; }

    /// <summary>Options (Column: Options)</summary>
    [Category("General")]
    [Description("Options")]
    public string? Options { get; set; }

    /// <summary>Fuse Mfr (Column: Fuse Mfr)</summary>
    [Category("Physical")]
    [Description("Fuse Mfr")]
    public string? FuseMfr { get; set; }

    /// <summary>Fuse Type (Column: Fuse Type)</summary>
    [Category("Physical")]
    [Description("Fuse Type")]
    public string? FuseType { get; set; }

    /// <summary>Fuse Style (Column: Fuse Style)</summary>
    [Category("Physical")]
    [Description("Fuse Style")]
    public string? FuseStyle { get; set; }

    /// <summary>Model (Column: Model)</summary>
    [Category("Physical")]
    [Description("Model")]
    public string? Model { get; set; }

    /// <summary>TCC kV (Column: TCC kV)</summary>
    [Category("Electrical")]
    [Description("TCC kV")]
    public string? TCCkV { get; set; }

    /// <summary>Size (Column: Size)</summary>
    [Category("Physical")]
    [Description("Size")]
    public string? Size { get; set; }

    /// <summary>SC Int kA (Column: SC Int kA)</summary>
    [Category("Electrical")]
    [Description("SC Int kA")]
    public string? SCIntkA { get; set; }

    /// <summary>SC Test X/R (Column: SC Test X/R)</summary>
    [Category("Electrical")]
    [Description("SC Test X/R")]
    public string? SCTestXR { get; set; }

    /// <summary>SC Test Std (Column: SC Test Std)</summary>
    [Category("Electrical")]
    [Description("SC Test Std")]
    public string? SCTestStd { get; set; }

    /// <summary>TCC Clipping (Column: TCC Clipping)</summary>
    [Category("Protection")]
    [Description("TCC Clipping")]
    public string? TCCClipping { get; set; }

    /// <summary>TCC Mom kA (Column: TCC Mom kA)</summary>
    [Category("Protection")]
    [Description("TCC Mom kA")]
    public string? TCCMomkA { get; set; }

    /// <summary>TCC Int kA (Column: TCC Int kA)</summary>
    [Category("Protection")]
    [Description("TCC Int kA")]
    public string? TCCIntkA { get; set; }

    /// <summary>TCC 30 Cyc kA (Column: TCC 30 Cyc kA)</summary>
    [Category("Protection")]
    [Description("TCC 30 Cyc kA")]
    public string? TCC30CyckA { get; set; }

    /// <summary>IEC Breaking kA (Column: IEC Breaking kA)</summary>
    [Category("General")]
    [Description("IEC Breaking kA")]
    public string? IECBreakingkA { get; set; }

    /// <summary>IEC TCC Initial kA (Column: IEC TCC Initial kA)</summary>
    [Category("Protection")]
    [Description("IEC TCC Initial kA")]
    public string? IECTCCInitialkA { get; set; }

    /// <summary>IEC TCC Breaking kA (Column: IEC TCC Breaking kA)</summary>
    [Category("Protection")]
    [Description("IEC TCC Breaking kA")]
    public string? IECTCCBreakingkA { get; set; }

    /// <summary>IEC TCC Breaking Time (Column: IEC TCC Breaking Time)</summary>
    [Category("Protection")]
    [Description("IEC TCC Breaking Time")]
    public string? IECTCCBreakingTime { get; set; }

    /// <summary>IEC TCC SS kA (Column: IEC TCC SS kA)</summary>
    [Category("Protection")]
    [Description("IEC TCC SS kA")]
    public string? IECTCCSSkA { get; set; }

    /// <summary>Switch Manufacturer (Column: Switch Manufacturer)</summary>
    [Category("Physical")]
    [Description("Switch Manufacturer")]
    public string? SwitchManufacturer { get; set; }

    /// <summary>Switch Type (Column: Switch Type)</summary>
    [Category("Physical")]
    [Description("Switch Type")]
    public string? SwitchType { get; set; }

    /// <summary>Switch Style (Column: Switch Style)</summary>
    [Category("Physical")]
    [Description("Switch Style")]
    public string? SwitchStyle { get; set; }

    /// <summary>Switch Cont A (Column: Switch Cont A)</summary>
    [Category("General")]
    [Description("Switch Cont A")]
    public string? SwitchContA { get; set; }

    /// <summary>Switch Mom kA (Column: Switch Mom kA)</summary>
    [Category("General")]
    [Description("Switch Mom kA")]
    public string? SwitchMomkA { get; set; }

    /// <summary>Mtr O/L Mfr (Column: Mtr O/L Mfr)</summary>
    [Category("Physical")]
    [Description("Mtr O/L Mfr")]
    public string? MtrOLMfr { get; set; }

    /// <summary>Mtr O/L Type (Column: Mtr O/L Type)</summary>
    [Category("Physical")]
    [Description("Mtr O/L Type")]
    public string? MtrOLType { get; set; }

    /// <summary>Mtr O/L Style (Column: Mtr O/L Style)</summary>
    [Category("Physical")]
    [Description("Mtr O/L Style")]
    public string? MtrOLStyle { get; set; }

    /// <summary>Motor FLA (Column: Motor FLA)</summary>
    [Category("General")]
    [Description("Motor FLA")]
    public string? MotorFLA { get; set; }

    /// <summary>Service Factor (Column: Service Factor)</summary>
    [Category("General")]
    [Description("Service Factor")]
    public string? ServiceFactor { get; set; }

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

    /// <summary>Alias for Fuses (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Fuses;
        set => Fuses = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Fuse"/> class.
    /// </summary>
    public Fuse() { }

    /// <summary>
    /// Returns a string representation of the Fuse.
    /// </summary>
    public override string ToString()
    {
        return $"Fuse: {Fuses}";
    }
}
