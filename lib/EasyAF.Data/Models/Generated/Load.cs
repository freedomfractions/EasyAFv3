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
    [Category("Identity")]
    [Description("Loads")]
    [Required]
    public string? Loads { get; set; }

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

    /// <summary>To Bus ID (Column: To Bus ID)</summary>
    [Category("Electrical")]
    [Description("To Bus ID")]
    public string? ToBusID { get; set; }

    /// <summary>To Device ID (Column: To Device ID)</summary>
    [Category("Identity")]
    [Description("To Device ID")]
    public string? ToDeviceID { get; set; }

    /// <summary>To Device Type (Column: To Device Type)</summary>
    [Category("Physical")]
    [Description("To Device Type")]
    public string? ToDeviceType { get; set; }

    /// <summary>To Base kV (Column: To Base kV)</summary>
    [Category("Electrical")]
    [Description("To Base kV")]
    public string? ToBaseKV { get; set; }

    /// <summary>Conn (Column: Conn)</summary>
    [Category("General")]
    [Description("Conn")]
    public string? Conn { get; set; }

    /// <summary>Load Model (Column: Load Model)</summary>
    [Category("Demand")]
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
    [Category("Electrical")]
    [Description("Const MVA MW")]
    public string? ConstMVAMW { get; set; }

    /// <summary>Const MVA MVAR (Column: Const MVA MVAR)</summary>
    [Category("Electrical")]
    [Description("Const MVA MVAR")]
    public string? ConstMVAMVAR { get; set; }

    /// <summary>Const MVA Amps (Column: Const MVA Amps)</summary>
    [Category("Electrical")]
    [Description("Const MVA Amps")]
    public string? ConstMVAAmps { get; set; }

    /// <summary>Const MVA PF (Column: Const MVA PF)</summary>
    [Category("Electrical")]
    [Description("Const MVA PF")]
    public string? ConstMVAPF { get; set; }

    /// <summary>Const MVA Scaling % (Column: Const MVA Scaling %)</summary>
    [Category("Electrical")]
    [Description("Const MVA Scaling %")]
    public string? ConstMVAScalingPercent { get; set; }

    /// <summary>Const I MW (Column: Const I MW)</summary>
    [Category("General")]
    [Description("Const I MW")]
    public string? ConstIMW { get; set; }

    /// <summary>Const I MVAR (Column: Const I MVAR)</summary>
    [Category("Electrical")]
    [Description("Const I MVAR")]
    public string? ConstIMVAR { get; set; }

    /// <summary>Const I Amps (Column: Const I Amps)</summary>
    [Category("General")]
    [Description("Const I Amps")]
    public string? ConstIAmps { get; set; }

    /// <summary>Const I PF (Column: Const I PF)</summary>
    [Category("General")]
    [Description("Const I PF")]
    public string? ConstIPF { get; set; }

    /// <summary>Const I Scaling % (Column: Const I Scaling %)</summary>
    [Category("General")]
    [Description("Const I Scaling %")]
    public string? ConstIScalingPercent { get; set; }

    /// <summary>Const Z MW (Column: Const Z MW)</summary>
    [Category("General")]
    [Description("Const Z MW")]
    public string? ConstZMW { get; set; }

    /// <summary>Const Z MVAR (Column: Const Z MVAR)</summary>
    [Category("Electrical")]
    [Description("Const Z MVAR")]
    public string? ConstZMVAR { get; set; }

    /// <summary>Const Z Amps (Column: Const Z Amps)</summary>
    [Category("General")]
    [Description("Const Z Amps")]
    public string? ConstZAmps { get; set; }

    /// <summary>Const Z PF (Column: Const Z PF)</summary>
    [Category("General")]
    [Description("Const Z PF")]
    public string? ConstZPF { get; set; }

    /// <summary>Const Z Scaling % (Column: Const Z Scaling %)</summary>
    [Category("General")]
    [Description("Const Z Scaling %")]
    public string? ConstZScalingPercent { get; set; }

    /// <summary>SCADA MVA MW (Column: SCADA MVA MW)</summary>
    [Category("Electrical")]
    [Description("SCADA MVA MW")]
    public string? ScadaMVAMW { get; set; }

    /// <summary>SCADA MVA MVAR (Column: SCADA MVA MVAR)</summary>
    [Category("Electrical")]
    [Description("SCADA MVA MVAR")]
    public string? ScadaMVAMVAR { get; set; }

    /// <summary>SCADA MVA Amps (Column: SCADA MVA Amps)</summary>
    [Category("Electrical")]
    [Description("SCADA MVA Amps")]
    public string? ScadaMVAAmps { get; set; }

    /// <summary>SCADA MVA PF (Column: SCADA MVA PF)</summary>
    [Category("Electrical")]
    [Description("SCADA MVA PF")]
    public string? ScadaMVAPF { get; set; }

    /// <summary>SCADA MVA Scaling% (Column: SCADA MVA Scaling%)</summary>
    [Category("Electrical")]
    [Description("SCADA MVA Scaling%")]
    public string? ScadaMVAScalingpercent { get; set; }

    /// <summary>SCADA I MW (Column: SCADA I MW)</summary>
    [Category("General")]
    [Description("SCADA I MW")]
    public string? ScadaIMW { get; set; }

    /// <summary>SCADA I MVAR (Column: SCADA I MVAR)</summary>
    [Category("Electrical")]
    [Description("SCADA I MVAR")]
    public string? ScadaIMVAR { get; set; }

    /// <summary>SCADA I Amps (Column: SCADA I Amps)</summary>
    [Category("General")]
    [Description("SCADA I Amps")]
    public string? ScadaIAmps { get; set; }

    /// <summary>SCADA I PF (Column: SCADA I PF)</summary>
    [Category("General")]
    [Description("SCADA I PF")]
    public string? ScadaIPF { get; set; }

    /// <summary>SCADA I Scaling% (Column: SCADA I Scaling%)</summary>
    [Category("General")]
    [Description("SCADA I Scaling%")]
    public string? ScadaIScalingpercent { get; set; }

    /// <summary>SCADA Z MW (Column: SCADA Z MW)</summary>
    [Category("General")]
    [Description("SCADA Z MW")]
    public string? ScadaZMW { get; set; }

    /// <summary>SCADA Z MVAR (Column: SCADA Z MVAR)</summary>
    [Category("Electrical")]
    [Description("SCADA Z MVAR")]
    public string? ScadaZMVAR { get; set; }

    /// <summary>SCADA Z Amps (Column: SCADA Z Amps)</summary>
    [Category("General")]
    [Description("SCADA Z Amps")]
    public string? ScadaZAmps { get; set; }

    /// <summary>SCADA Z PF (Column: SCADA Z PF)</summary>
    [Category("General")]
    [Description("SCADA Z PF")]
    public string? ScadaZPF { get; set; }

    /// <summary>SCADA Z Scaling% (Column: SCADA Z Scaling%)</summary>
    [Category("General")]
    [Description("SCADA Z Scaling%")]
    public string? ScadaZScalingpercent { get; set; }

    /// <summary>Hrm Load Type (Column: Hrm Load Type)</summary>
    [Category("Physical")]
    [Description("Hrm Load Type")]
    public string? HrmLoadType { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRcFactor { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRcValue { get; set; }

    /// <summary>I Hrm Rating Setting (Column: I Hrm Rating Setting)</summary>
    [Category("Control")]
    [Description("I Hrm Rating Setting")]
    public string? IHrmRatingSetting { get; set; }

    /// <summary>I Hrm Rating (Column: I Hrm Rating)</summary>
    [Category("General")]
    [Description("I Hrm Rating")]
    public string? IHrmRating { get; set; }

    /// <summary>Lib Load Mfr (Column: Lib Load Mfr)</summary>
    [Category("Demand")]
    [Description("Lib Load Mfr")]
    public string? LibLoadMfr { get; set; }

    /// <summary>Lib Load Type (Column: Lib Load Type)</summary>
    [Category("Physical")]
    [Description("Lib Load Type")]
    public string? LibLoadType { get; set; }

    /// <summary>Load Amps (Column: Load Amps)</summary>
    [Category("Demand")]
    [Description("Load Amps")]
    public string? LoadAmps { get; set; }

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

    /// <summary>Facility (Column: Facility)</summary>
    [Category("Location")]
    [Description("Facility")]
    public string? Facility { get; set; }

    /// <summary>Location Name (Column: Location Name)</summary>
    [Category("Location")]
    [Description("Location Name")]
    public string? LocationName { get; set; }

    /// <summary>Location Description (Column: Location Description)</summary>
    [Category("Location")]
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
    [Category("Identity")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for Load (convenience property for dictionary indexing - not serialized).</summary>
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

