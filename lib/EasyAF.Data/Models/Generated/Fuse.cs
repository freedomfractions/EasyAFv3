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
    [Category("Identity")]
    [Description("Fuses")]
    [Required]
    public string? Fuses { get; set; }

    /// <summary>AC/DC (Column: AC/DC)</summary>
    [Category("General")]
    [Description("AC/DC")]
    public string? AcDc { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("General")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>On Bus (Column: On Bus)</summary>
    [Category("Electrical")]
    [Description("On Bus")]
    public string? OnBus { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BaseKV { get; set; }

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
    [Category("Protection")]
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
    [Category("General")]
    [Description("Model")]
    public string? Model { get; set; }

    /// <summary>TCC kV (Column: TCC kV)</summary>
    [Category("Electrical")]
    [Description("TCC kV")]
    public string? TCCKV { get; set; }

    /// <summary>Size (Column: Size)</summary>
    [Category("General")]
    [Description("Size")]
    public string? Size { get; set; }

    /// <summary>SC Int kA (Column: SC Int kA)</summary>
    [Category("General")]
    [Description("SC Int kA")]
    public string? SCIntKA { get; set; }

    /// <summary>SC Test X/R (Column: SC Test X/R)</summary>
    [Category("General")]
    [Description("SC Test X/R")]
    public string? SCTestXR { get; set; }

    /// <summary>SC Test Std (Column: SC Test Std)</summary>
    [Category("General")]
    [Description("SC Test Std")]
    public string? SCTestStd { get; set; }

    /// <summary>TCC Clipping (Column: TCC Clipping)</summary>
    [Category("General")]
    [Description("TCC Clipping")]
    public string? TCCClipping { get; set; }

    /// <summary>TCC Mom kA (Column: TCC Mom kA)</summary>
    [Category("General")]
    [Description("TCC Mom kA")]
    public string? TCCMomKA { get; set; }

    /// <summary>TCC Int kA (Column: TCC Int kA)</summary>
    [Category("General")]
    [Description("TCC Int kA")]
    public string? TCCIntKA { get; set; }

    /// <summary>TCC 30 Cyc kA (Column: TCC 30 Cyc kA)</summary>
    [Category("General")]
    [Description("TCC 30 Cyc kA")]
    public string? TCC30CycKA { get; set; }

    /// <summary>IEC Breaking kA (Column: IEC Breaking kA)</summary>
    [Category("General")]
    [Description("IEC Breaking kA")]
    public string? IECBreakingKA { get; set; }

    /// <summary>IEC TCC Initial kA (Column: IEC TCC Initial kA)</summary>
    [Category("General")]
    [Description("IEC TCC Initial kA")]
    public string? IECTCCInitialKA { get; set; }

    /// <summary>IEC TCC Breaking kA (Column: IEC TCC Breaking kA)</summary>
    [Category("General")]
    [Description("IEC TCC Breaking kA")]
    public string? IECTCCBreakingKA { get; set; }

    /// <summary>IEC TCC Breaking Time (Column: IEC TCC Breaking Time)</summary>
    [Category("General")]
    [Description("IEC TCC Breaking Time")]
    public string? IECTCCBreakingTime { get; set; }

    /// <summary>IEC TCC SS kA (Column: IEC TCC SS kA)</summary>
    [Category("General")]
    [Description("IEC TCC SS kA")]
    public string? IECTCCSSKA { get; set; }

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
    public string? SwitchMomKA { get; set; }

    /// <summary>Mtr O/L Mfr (Column: Mtr O/L Mfr)</summary>
    [Category("General")]
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
    public string? PCCKVADemand { get; set; }

    /// <summary>PCC Isc/ILoad (Column: PCC Isc/ILoad)</summary>
    [Category("Demand")]
    [Description("PCC Isc/ILoad")]
    public string? PCCIscIload { get; set; }

    /// <summary>Failure Rate (/year) (Column: Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Description("Failure Rate (/year)")]
    [Units("/year")]
    public string? FailureRatePerYear { get; set; }

    /// <summary>Repair Time (h) (Column: Repair Time (h))</summary>
    [Category("Reliability")]
    [Description("Repair Time (h)")]
    [Units("h")]
    public string? RepairTimeHours { get; set; }

    /// <summary>Replace Time (h) (Column: Replace Time (h))</summary>
    [Category("General")]
    [Description("Replace Time (h)")]
    [Units("h")]
    public string? ReplaceTimeHours { get; set; }

    /// <summary>Repair Cost (Column: Repair Cost)</summary>
    [Category("Reliability")]
    [Description("Repair Cost")]
    public string? RepairCost { get; set; }

    /// <summary>Replace Cost (Column: Replace Cost)</summary>
    [Category("General")]
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
    public string? SCFailureModePercent { get; set; }

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Identity")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for Fuse (convenience property for dictionary indexing - not serialized).</summary>
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

