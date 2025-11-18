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
    [Category("Identity")]
    [Description("POCs")]
    [Required]
    public string? POCs { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>Tool ID (Column: Tool ID)</summary>
    [Category("Identity")]
    [Description("Tool ID")]
    public string? ToolID { get; set; }

    /// <summary>Ref # (Column: Ref #)</summary>
    [Category("General")]
    [Description("Ref #")]
    public string? RefNum { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BaseKV { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("General")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Service (Column: Service)</summary>
    [Category("General")]
    [Description("Service")]
    public string? Service { get; set; }

    /// <summary>Area (Column: Area)</summary>
    [Category("General")]
    [Description("Area")]
    public string? Area { get; set; }

    /// <summary>Zone (Column: Zone)</summary>
    [Category("General")]
    [Description("Zone")]
    public string? Zone { get; set; }

    /// <summary>Bus Rating (A) (Column: Bus Rating (A))</summary>
    [Category("Electrical")]
    [Description("Bus Rating (A)")]
    [Units("A")]
    public string? BusRatingA { get; set; }

    /// <summary>Bus Bracing (kA) (Column: Bus Bracing (kA))</summary>
    [Category("Electrical")]
    [Description("Bus Bracing (kA)")]
    [Units("kA")]
    public string? BusBracing { get; set; }

    /// <summary>Power Type (Column: Power Type)</summary>
    [Category("Physical")]
    [Description("Power Type")]
    public string? PowerType { get; set; }

    /// <summary>Description (Column: Description)</summary>
    [Category("General")]
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
    [Category("Demand")]
    [Description("Load Model")]
    public string? LoadModel { get; set; }

    /// <summary>Load Class (Column: Load Class)</summary>
    [Category("Demand")]
    [Description("Load Class")]
    public string? LoadClass { get; set; }

    /// <summary>Const MVA MW (Column: Const MVA MW)</summary>
    [Category("Electrical")]
    [Description("Const MVA MW")]
    public string? ConstMVAMW { get; set; }

    /// <summary>Const MVA MVAR (Column: Const MVA MVAR)</summary>
    [Category("Electrical")]
    [Description("Const MVA MVAR")]
    public string? ConstMVAMVAR { get; set; }

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

    /// <summary>SCADA Z Scaling% (Column: SCADA Z Scaling%)</summary>
    [Category("General")]
    [Description("SCADA Z Scaling%")]
    public string? ScadaZScalingpercent { get; set; }

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

    /// <summary>Equipment (Column: Equipment)</summary>
    [Category("General")]
    [Description("Equipment")]
    public string? Equipment { get; set; }

    /// <summary>AF Solution (Column: AF Solution)</summary>
    [Category("General")]
    [Description("AF Solution")]
    public string? AFSolution { get; set; }

    /// <summary>Forced To Energy (cal/cm2) (Column: Forced To Energy (cal/cm2))</summary>
    [Category("General")]
    [Description("Forced To Energy (cal/cm2)")]
    [Units("calcm²")]
    public string? ForcedToEnergyCalPerCm2 { get; set; }

    /// <summary>Forced To Arc Boundary (in) (Column: Forced To Arc Boundary (in))</summary>
    [Category("General")]
    [Description("Forced To Arc Boundary (in)")]
    [Units("in")]
    public string? ForcedToArcBoundaryInches { get; set; }

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

    /// <summary>Working Distance (in) (Column: Working Distance (in))</summary>
    [Category("General")]
    [Description("Working Distance (in)")]
    [Units("in")]
    public string? WorkingDistanceInches { get; set; }

    /// <summary>Electrode Gap Setting (Column: Electrode Gap Setting)</summary>
    [Category("Control")]
    [Description("Electrode Gap Setting")]
    public string? ElectrodeGapSetting { get; set; }

    /// <summary>Electrode Gap (mm) (Column: Electrode Gap (mm))</summary>
    [Category("General")]
    [Description("Electrode Gap (mm)")]
    [Units("mm")]
    public string? ElectrodeGapMM { get; set; }

    /// <summary>Electrode Configuration Setting (Column: Electrode Configuration Setting)</summary>
    [Category("Control")]
    [Description("Electrode Configuration Setting")]
    public string? ElectrodeConfigurationSetting { get; set; }

    /// <summary>Electrode Configuration (Column: Electrode Configuration)</summary>
    [Category("General")]
    [Description("Electrode Configuration")]
    public string? ElectrodeConfiguration { get; set; }

    /// <summary>Enclosure Size Setting (Column: Enclosure Size Setting)</summary>
    [Category("Control")]
    [Description("Enclosure Size Setting")]
    public string? EnclosureSizeSetting { get; set; }

    /// <summary>Enclosure Height (in) (Column: Enclosure Height (in))</summary>
    [Category("General")]
    [Description("Enclosure Height (in)")]
    [Units("in")]
    public string? EnclosureHeightInches { get; set; }

    /// <summary>Enclosure Width (in) (Column: Enclosure Width (in))</summary>
    [Category("General")]
    [Description("Enclosure Width (in)")]
    [Units("in")]
    public string? EnclosureWidthInches { get; set; }

    /// <summary>Enclosure Depth (in) (Column: Enclosure Depth (in))</summary>
    [Category("General")]
    [Description("Enclosure Depth (in)")]
    [Units("in")]
    public string? EnclosureDepthInches { get; set; }

    /// <summary>Label Comment (Column: Label Comment)</summary>
    [Category("Metadata")]
    [Description("Label Comment")]
    public string? LabelComment { get; set; }

    /// <summary># Labels To Print (Column: # Labels To Print)</summary>
    [Category("General")]
    [Description("# Labels To Print")]
    public string? NumLabelsToPrint { get; set; }

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

    /// <summary>Downtime Cost (h) (Column: Downtime Cost (h))</summary>
    [Category("General")]
    [Description("Downtime Cost (h)")]
    [Units("h")]
    public string? DowntimeCostHours { get; set; }

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

    /// <summary>Alias for POC (convenience property for dictionary indexing - not serialized).</summary>
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

