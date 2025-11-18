using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Load with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Loads" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Loads")]
public class Load
{
    /// <summary>Loads (Column: Loads)</summary>
    [Category("Demand")]
    [Description("Loads")]
    [Required]
    public string? Loads { get; set; }

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

    /// <summary>To Bus ID (Column: To Bus ID)</summary>
    [Category("Identity")]
    [Description("To Bus ID")]
    public string? ToBusID { get; set; }

    /// <summary>To Device ID (Column: To Device ID)</summary>
    [Category("General")]
    [Description("To Device ID")]
    public string? ToDeviceID { get; set; }

    /// <summary>To Device Type (Column: To Device Type)</summary>
    [Category("Physical")]
    [Description("To Device Type")]
    public string? ToDeviceType { get; set; }

    /// <summary>To Base kV (Column: To Base kV)</summary>
    [Category("Electrical")]
    [Description("To Base kV")]
    public string? ToBasekV { get; set; }

    /// <summary>Conn (Column: Conn)</summary>
    [Category("General")]
    [Description("Conn")]
    public string? Conn { get; set; }

    /// <summary>Load Model (Column: Load Model)</summary>
    [Category("Physical")]
    [Description("Load Model")]
    public string? LoadModel { get; set; }

    /// <summary>Load Class (Column: Load Class)</summary>
    [Category("Demand")]
    [Description("Load Class")]
    public string? LoadClass { get; set; }

    /// <summary>Power Type (Column: Power Type)</summary>
    [Category("Physical")]
    [Description("Power Type")]
    public string? PowerType { get; set; }

    /// <summary>Load Unit (Column: Load Unit)</summary>
    [Category("Demand")]
    [Description("Load Unit")]
    public string? LoadUnit { get; set; }

    /// <summary>Code Factors (Column: Code Factors)</summary>
    [Category("General")]
    [Description("Code Factors")]
    public string? CodeFactors { get; set; }

    /// <summary>Demand Factor (Column: Demand Factor)</summary>
    [Category("Demand")]
    [Description("Demand Factor")]
    public string? DemandFactor { get; set; }

    /// <summary>Quantity (Column: Quantity)</summary>
    [Category("General")]
    [Description("Quantity")]
    public string? Quantity { get; set; }

    /// <summary>Const MVA MW (Column: Const MVA MW)</summary>
    [Category("Protection")]
    [Description("Const MVA MW")]
    [Required]
    public string? ConstMVAMW { get; set; }

    /// <summary>Const MVA MVAR (Column: Const MVA MVAR)</summary>
    [Category("Protection")]
    [Description("Const MVA MVAR")]
    [Required]
    public string? ConstMVAMVAR { get; set; }

    /// <summary>Const MVA Amps (Column: Const MVA Amps)</summary>
    [Category("Electrical")]
    [Description("Const MVA Amps")]
    [Required]
    public string? ConstMVAAmps { get; set; }

    /// <summary>Const MVA PF (Column: Const MVA PF)</summary>
    [Category("Protection")]
    [Description("Const MVA PF")]
    [Required]
    public string? ConstMVAPF { get; set; }

    /// <summary>Const MVA Scaling % (Column: Const MVA Scaling %)</summary>
    [Category("Protection")]
    [Description("Const MVA Scaling %")]
    [Required]
    public string? ConstMVAScaling% { get; set; }

    /// <summary>Const I MW (Column: Const I MW)</summary>
    [Category("Protection")]
    [Description("Const I MW")]
    public string? ConstIMW { get; set; }

    /// <summary>Const I MVAR (Column: Const I MVAR)</summary>
    [Category("Protection")]
    [Description("Const I MVAR")]
    [Required]
    public string? ConstIMVAR { get; set; }

    /// <summary>Const I Amps (Column: Const I Amps)</summary>
    [Category("Electrical")]
    [Description("Const I Amps")]
    public string? ConstIAmps { get; set; }

    /// <summary>Const I PF (Column: Const I PF)</summary>
    [Category("Protection")]
    [Description("Const I PF")]
    public string? ConstIPF { get; set; }

    /// <summary>Const I Scaling % (Column: Const I Scaling %)</summary>
    [Category("Protection")]
    [Description("Const I Scaling %")]
    public string? ConstIScaling% { get; set; }

    /// <summary>Const Z MW (Column: Const Z MW)</summary>
    [Category("Protection")]
    [Description("Const Z MW")]
    public string? ConstZMW { get; set; }

    /// <summary>Const Z MVAR (Column: Const Z MVAR)</summary>
    [Category("Protection")]
    [Description("Const Z MVAR")]
    [Required]
    public string? ConstZMVAR { get; set; }

    /// <summary>Const Z Amps (Column: Const Z Amps)</summary>
    [Category("Electrical")]
    [Description("Const Z Amps")]
    public string? ConstZAmps { get; set; }

    /// <summary>Const Z PF (Column: Const Z PF)</summary>
    [Category("Protection")]
    [Description("Const Z PF")]
    public string? ConstZPF { get; set; }

    /// <summary>Const Z Scaling % (Column: Const Z Scaling %)</summary>
    [Category("Protection")]
    [Description("Const Z Scaling %")]
    public string? ConstZScaling% { get; set; }

    /// <summary>SCADA MVA MW (Column: SCADA MVA MW)</summary>
    [Category("General")]
    [Description("SCADA MVA MW")]
    [Required]
    public string? SCADAMVAMW { get; set; }

    /// <summary>SCADA MVA MVAR (Column: SCADA MVA MVAR)</summary>
    [Category("General")]
    [Description("SCADA MVA MVAR")]
    [Required]
    public string? SCADAMVAMVAR { get; set; }

    /// <summary>SCADA MVA Amps (Column: SCADA MVA Amps)</summary>
    [Category("Electrical")]
    [Description("SCADA MVA Amps")]
    [Required]
    public string? SCADAMVAAmps { get; set; }

    /// <summary>SCADA MVA PF (Column: SCADA MVA PF)</summary>
    [Category("General")]
    [Description("SCADA MVA PF")]
    [Required]
    public string? SCADAMVAPF { get; set; }

    /// <summary>SCADA MVA Scaling% (Column: SCADA MVA Scaling%)</summary>
    [Category("General")]
    [Description("SCADA MVA Scaling%")]
    [Required]
    public string? SCADAMVAScaling% { get; set; }

    /// <summary>SCADA I MW (Column: SCADA I MW)</summary>
    [Category("General")]
    [Description("SCADA I MW")]
    public string? SCADAIMW { get; set; }

    /// <summary>SCADA I MVAR (Column: SCADA I MVAR)</summary>
    [Category("General")]
    [Description("SCADA I MVAR")]
    [Required]
    public string? SCADAIMVAR { get; set; }

    /// <summary>SCADA I Amps (Column: SCADA I Amps)</summary>
    [Category("Electrical")]
    [Description("SCADA I Amps")]
    public string? SCADAIAmps { get; set; }

    /// <summary>SCADA I PF (Column: SCADA I PF)</summary>
    [Category("General")]
    [Description("SCADA I PF")]
    public string? SCADAIPF { get; set; }

    /// <summary>SCADA I Scaling% (Column: SCADA I Scaling%)</summary>
    [Category("General")]
    [Description("SCADA I Scaling%")]
    public string? SCADAIScaling% { get; set; }

    /// <summary>SCADA Z MW (Column: SCADA Z MW)</summary>
    [Category("General")]
    [Description("SCADA Z MW")]
    public string? SCADAZMW { get; set; }

    /// <summary>SCADA Z MVAR (Column: SCADA Z MVAR)</summary>
    [Category("General")]
    [Description("SCADA Z MVAR")]
    [Required]
    public string? SCADAZMVAR { get; set; }

    /// <summary>SCADA Z Amps (Column: SCADA Z Amps)</summary>
    [Category("Electrical")]
    [Description("SCADA Z Amps")]
    public string? SCADAZAmps { get; set; }

    /// <summary>SCADA Z PF (Column: SCADA Z PF)</summary>
    [Category("General")]
    [Description("SCADA Z PF")]
    public string? SCADAZPF { get; set; }

    /// <summary>SCADA Z Scaling% (Column: SCADA Z Scaling%)</summary>
    [Category("General")]
    [Description("SCADA Z Scaling%")]
    public string? SCADAZScaling% { get; set; }

    /// <summary>Hrm Load Type (Column: Hrm Load Type)</summary>
    [Category("Physical")]
    [Description("Hrm Load Type")]
    public string? HrmLoadType { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRCFactor { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRCValue { get; set; }

    /// <summary>I Hrm Rating Setting (Column: I Hrm Rating Setting)</summary>
    [Category("Physical")]
    [Description("I Hrm Rating Setting")]
    public string? IHrmRatingSetting { get; set; }

    /// <summary>I Hrm Rating (Column: I Hrm Rating)</summary>
    [Category("Physical")]
    [Description("I Hrm Rating")]
    public string? IHrmRating { get; set; }

    /// <summary>Lib Load Mfr (Column: Lib Load Mfr)</summary>
    [Category("Physical")]
    [Description("Lib Load Mfr")]
    public string? LibLoadMfr { get; set; }

    /// <summary>Lib Load Type (Column: Lib Load Type)</summary>
    [Category("Physical")]
    [Description("Lib Load Type")]
    public string? LibLoadType { get; set; }

    /// <summary>Load Amps (Column: Load Amps)</summary>
    [Category("Electrical")]
    [Description("Load Amps")]
    public string? LoadAmps { get; set; }

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

    /// <summary>Facility (Column: Facility)</summary>
    [Category("Location")]
    [Description("Facility")]
    public string? Facility { get; set; }

    /// <summary>Location Name (Column: Location Name)</summary>
    [Category("Location")]
    [Description("Location Name")]
    public string? LocationName { get; set; }

    /// <summary>Location Description (Column: Location Description)</summary>
    [Category("Metadata")]
    [Description("Location Description")]
    public string? LocationDescription { get; set; }

    /// <summary>X (Column: X)</summary>
    [Category("General")]
    [Description("X")]
    public string? X { get; set; }

    /// <summary>Y (Column: Y)</summary>
    [Category("General")]
    [Description("Y")]
    public string? Y { get; set; }

    /// <summary>Floor (Column: Floor)</summary>
    [Category("Location")]
    [Description("Floor")]
    public string? Floor { get; set; }

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for Loads (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Loads;
        set => Loads = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Load"/> class.
    /// </summary>
    public Load() { }

    /// <summary>
    /// Returns a string representation of the Load.
    /// </summary>
    public override string ToString()
    {
        return $"Load: {Loads}";
    }
}
