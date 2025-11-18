using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a POC with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "POCs" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("POCs")]
public class POC
{
    /// <summary>POCs (Column: POCs)</summary>
    [Category("General")]
    [Description("POCs")]
    [Required]
    public string? POCs { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>Tool ID (Column: Tool ID)</summary>
    [Category("General")]
    [Description("Tool ID")]
    public string? ToolID { get; set; }

    /// <summary>Ref # (Column: Ref #)</summary>
    [Category("General")]
    [Description("Ref #")]
    public string? RefNumber { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BaseKV { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Service (Column: Service)</summary>
    [Category("General")]
    [Description("Service")]
    public string? Service { get; set; }

    /// <summary>Area (Column: Area)</summary>
    [Category("Location")]
    [Description("Area")]
    public string? Area { get; set; }

    /// <summary>Zone (Column: Zone)</summary>
    [Category("Location")]
    [Description("Zone")]
    public string? Zone { get; set; }

    /// <summary>Bus Rating (Column: Bus Rating (A))</summary>
    [Category("Physical")]
    [Description("Bus Rating")]
    [Units("A")]
    public string? BusRating { get; set; }

    /// <summary>Bus Bracing (Column: Bus Bracing (kA))</summary>
    [Category("Identity")]
    [Description("Bus Bracing")]
    [Units("kA")]
    public string? BusBracing { get; set; }

    /// <summary>Power Type (Column: Power Type)</summary>
    [Category("Physical")]
    [Description("Power Type")]
    public string? PowerType { get; set; }

    /// <summary>Description (Column: Description)</summary>
    [Category("Metadata")]
    [Description("Description")]
    public string? Description { get; set; }

    /// <summary>Unit (Column: Unit)</summary>
    [Category("General")]
    [Description("Unit")]
    public string? Unit { get; set; }

    /// <summary>Library Load (Column: Library Load)</summary>
    [Category("Demand")]
    [Description("Library Load")]
    public string? LibraryLoad { get; set; }

    /// <summary>Load Model (Column: Load Model)</summary>
    [Category("Physical")]
    [Description("Load Model")]
    public string? LoadModel { get; set; }

    /// <summary>Load Class (Column: Load Class)</summary>
    [Category("Demand")]
    [Description("Load Class")]
    public string? LoadClass { get; set; }

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

    /// <summary>SCADA Z Scaling% (Column: SCADA Z Scaling%)</summary>
    [Category("General")]
    [Description("SCADA Z Scaling%")]
    public string? SCADAZScaling% { get; set; }

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

    /// <summary>Equipment (Column: Equipment)</summary>
    [Category("General")]
    [Description("Equipment")]
    public string? Equipment { get; set; }

    /// <summary>AF Solution (Column: AF Solution)</summary>
    [Category("General")]
    [Description("AF Solution")]
    public string? AFSolution { get; set; }

    /// <summary>Forced To Energy (Column: Forced To Energy (cal/cm2))</summary>
    [Category("General")]
    [Description("Forced To Energy")]
    [Units("cal/cm²")]
    public string? ForcedToEnergy { get; set; }

    /// <summary>Forced To Arc Boundary (Column: Forced To Arc Boundary (in))</summary>
    [Category("General")]
    [Description("Forced To Arc Boundary")]
    [Units("in")]
    public string? ForcedToArcBoundary { get; set; }

    /// <summary>AF Option (Column: AF Option)</summary>
    [Category("General")]
    [Description("AF Option")]
    public string? AFOption { get; set; }

    /// <summary>AF Output (Column: AF Output)</summary>
    [Category("General")]
    [Description("AF Output")]
    public string? AFOutput { get; set; }

    /// <summary>Working Distance Setting (Column: Working Distance Setting)</summary>
    [Category("Control")]
    [Description("Working Distance Setting")]
    public string? WorkingDistanceSetting { get; set; }

    /// <summary>Working Distance (Column: Working Distance (in))</summary>
    [Category("General")]
    [Description("Working Distance")]
    [Units("in")]
    public string? WorkingDistance { get; set; }

    /// <summary>Electrode Gap Setting (Column: Electrode Gap Setting)</summary>
    [Category("Control")]
    [Description("Electrode Gap Setting")]
    public string? ElectrodeGapSetting { get; set; }

    /// <summary>Electrode Gap (Column: Electrode Gap (mm))</summary>
    [Category("General")]
    [Description("Electrode Gap")]
    [Units("mm")]
    public string? ElectrodeGap { get; set; }

    /// <summary>Electrode Configuration Setting (Column: Electrode Configuration Setting)</summary>
    [Category("Control")]
    [Description("Electrode Configuration Setting")]
    public string? ElectrodeConfigurationSetting { get; set; }

    /// <summary>Electrode Configuration (Column: Electrode Configuration)</summary>
    [Category("General")]
    [Description("Electrode Configuration")]
    public string? ElectrodeConfiguration { get; set; }

    /// <summary>Enclosure Size Setting (Column: Enclosure Size Setting)</summary>
    [Category("Physical")]
    [Description("Enclosure Size Setting")]
    public string? EnclosureSizeSetting { get; set; }

    /// <summary>Enclosure Height (Column: Enclosure Height (in))</summary>
    [Category("General")]
    [Description("Enclosure Height")]
    [Units("in")]
    public string? EnclosureHeight { get; set; }

    /// <summary>Enclosure Width (Column: Enclosure Width (in))</summary>
    [Category("General")]
    [Description("Enclosure Width")]
    [Units("in")]
    public string? EnclosureWidth { get; set; }

    /// <summary>Enclosure Depth (Column: Enclosure Depth (in))</summary>
    [Category("General")]
    [Description("Enclosure Depth")]
    [Units("in")]
    public string? EnclosureDepth { get; set; }

    /// <summary>Label Comment (Column: Label Comment)</summary>
    [Category("Metadata")]
    [Description("Label Comment")]
    public string? LabelComment { get; set; }

    /// <summary># Labels To Print (Column: # Labels To Print)</summary>
    [Category("General")]
    [Description("# Labels To Print")]
    public string? NumberLabelsToPrint { get; set; }

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

    /// <summary>Downtime Cost (Column: Downtime Cost (h))</summary>
    [Category("Reliability")]
    [Description("Downtime Cost")]
    [Units("h")]
    public string? DowntimeCostH { get; set; }

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

    /// <summary>Alias for POCs (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => POCs;
        set => POCs = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="POC"/> class.
    /// </summary>
    public POC() { }

    /// <summary>
    /// Returns a string representation of the POC.
    /// </summary>
    public override string ToString()
    {
        return $"POC: {POCs}";
    }
}

