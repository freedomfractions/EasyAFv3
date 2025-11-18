using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Panel with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Panels" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Panels")]
public class Panel
{
    /// <summary>Panels (Column: Panels)</summary>
    [Category("General")]
    [Description("Panels")]
    [Required]
    public string? Panels { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Service (Column: Service)</summary>
    [Category("General")]
    [Description("Service")]
    public string? Service { get; set; }

    /// <summary>Base kV (Column: Base kV)</summary>
    [Category("Electrical")]
    [Description("Base kV")]
    public string? BaseKV { get; set; }

    /// <summary>Area (Column: Area)</summary>
    [Category("Location")]
    [Description("Area")]
    public string? Area { get; set; }

    /// <summary>Zone (Column: Zone)</summary>
    [Category("Location")]
    [Description("Zone")]
    public string? Zone { get; set; }

    /// <summary>Unit (Column: Unit)</summary>
    [Category("General")]
    [Description("Unit")]
    public string? Unit { get; set; }

    /// <summary>Device Code (Column: Device Code)</summary>
    [Category("General")]
    [Description("Device Code")]
    public string? DeviceCode { get; set; }

    /// <summary>Description (Column: Description)</summary>
    [Category("Metadata")]
    [Description("Description")]
    public string? Description { get; set; }

    /// <summary>Location (Column: Location)</summary>
    [Category("Location")]
    [Description("Location")]
    public string? Location { get; set; }

    /// <summary>Manufacturer (Column: Manufacturer)</summary>
    [Category("Physical")]
    [Description("Manufacturer")]
    public string? Manufacturer { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>Fed By (Column: Fed By)</summary>
    [Category("General")]
    [Description("Fed By")]
    public string? FedBy { get; set; }

    /// <summary>Hdr Comment (Column: Hdr Comment)</summary>
    [Category("Metadata")]
    [Description("Hdr Comment")]
    public string? HdrComment { get; set; }

    /// <summary>Main Bus Rating (Column: Main Bus Rating (A))</summary>
    [Category("Physical")]
    [Description("Main Bus Rating")]
    [Units("A")]
    public string? MainBusRating { get; set; }

    /// <summary>Panel SC Rating (Column: Panel SC Rating (kA))</summary>
    [Category("Electrical")]
    [Description("Panel SC Rating")]
    [Units("kA")]
    public string? PanelSCRating { get; set; }

    /// <summary>Material (Column: Material)</summary>
    [Category("General")]
    [Description("Material")]
    public string? Material { get; set; }

    /// <summary>Mounting (Column: Mounting)</summary>
    [Category("General")]
    [Description("Mounting")]
    public string? Mounting { get; set; }

    /// <summary>Enclosure (Column: Enclosure)</summary>
    [Category("General")]
    [Description("Enclosure")]
    public string? Enclosure { get; set; }

    /// <summary>Incoming Device Type (Column: Incoming Device Type)</summary>
    [Category("Physical")]
    [Description("Incoming Device Type")]
    public string? IncomingDeviceType { get; set; }

    /// <summary>Conn Watts (Column: Conn Watts)</summary>
    [Category("General")]
    [Description("Conn Watts")]
    public string? ConnWatts { get; set; }

    /// <summary>Conn Vars (Column: Conn Vars)</summary>
    [Category("General")]
    [Description("Conn Vars")]
    public string? ConnVars { get; set; }

    /// <summary>Conn KVA (Column: Conn KVA)</summary>
    [Category("Electrical")]
    [Description("Conn KVA")]
    [Required]
    public string? ConnKVA { get; set; }

    /// <summary>Conn PF (Column: Conn PF)</summary>
    [Category("General")]
    [Description("Conn PF")]
    public string? ConnPF { get; set; }

    /// <summary>Conn FLA (Column: Conn FLA)</summary>
    [Category("General")]
    [Description("Conn FLA")]
    public string? ConnFLA { get; set; }

    /// <summary>Demand Watts (Column: Demand Watts)</summary>
    [Category("Demand")]
    [Description("Demand Watts")]
    public string? DemandWatts { get; set; }

    /// <summary>Demand Vars (Column: Demand Vars)</summary>
    [Category("Demand")]
    [Description("Demand Vars")]
    public string? DemandVars { get; set; }

    /// <summary>Demand KVA (Column: Demand KVA)</summary>
    [Category("Electrical")]
    [Description("Demand KVA")]
    public string? DemandKVA { get; set; }

    /// <summary>Demand PF (Column: Demand PF)</summary>
    [Category("Demand")]
    [Description("Demand PF")]
    public string? DemandPF { get; set; }

    /// <summary>Demand FLA (Column: Demand FLA)</summary>
    [Category("Demand")]
    [Description("Demand FLA")]
    public string? DemandFLA { get; set; }

    /// <summary>Dn Conn KVA (Column: Dn Conn KVA)</summary>
    [Category("Electrical")]
    [Description("Dn Conn KVA")]
    public string? DnConnKVA { get; set; }

    /// <summary>Dn Conn FLA (Column: Dn Conn FLA)</summary>
    [Category("General")]
    [Description("Dn Conn FLA")]
    public string? DnConnFLA { get; set; }

    /// <summary>Dn Demand KVA (Column: Dn Demand KVA)</summary>
    [Category("Electrical")]
    [Description("Dn Demand KVA")]
    public string? DnDemandKVA { get; set; }

    /// <summary>Dn Demand FLA (Column: Dn Demand FLA)</summary>
    [Category("Demand")]
    [Description("Dn Demand FLA")]
    public string? DnDemandFLA { get; set; }

    /// <summary>Dn Code KVA (Column: Dn Code KVA)</summary>
    [Category("Electrical")]
    [Description("Dn Code KVA")]
    public string? DnCodeKVA { get; set; }

    /// <summary>Dn Code FLA (Column: Dn Code FLA)</summary>
    [Category("General")]
    [Description("Dn Code FLA")]
    public string? DnCodeFLA { get; set; }

    /// <summary>Dn Design KVA (Column: Dn Design KVA)</summary>
    [Category("Electrical")]
    [Description("Dn Design KVA")]
    public string? DnDesignKVA { get; set; }

    /// <summary>Dn Design FLA (Column: Dn Design FLA)</summary>
    [Category("General")]
    [Description("Dn Design FLA")]
    public string? DnDesignFLA { get; set; }

    /// <summary>Load Model (Column: Load Model)</summary>
    [Category("Physical")]
    [Description("Load Model")]
    public string? LoadModel { get; set; }

    /// <summary>PF Load Type (Column: PF Load Type)</summary>
    [Category("Physical")]
    [Description("PF Load Type")]
    public string? PFLoadType { get; set; }

    /// <summary>Load Scaling (Column: Load Scaling)</summary>
    [Category("Demand")]
    [Description("Load Scaling")]
    public string? LoadScaling { get; set; }

    /// <summary>SCADA KW (Column: SCADA KW)</summary>
    [Category("Electrical")]
    [Description("SCADA KW")]
    public string? SCADAKW { get; set; }

    /// <summary>SCADA jKVAR (Column: SCADA jKVAR)</summary>
    [Category("Electrical")]
    [Description("SCADA jKVAR")]
    [Required]
    public string? SCADAjKVAR { get; set; }

    /// <summary>SCADA Load Type (Column: SCADA Load Type)</summary>
    [Category("Physical")]
    [Description("SCADA Load Type")]
    public string? SCADALoadType { get; set; }

    /// <summary>SCADA Scaling (Column: SCADA Scaling)</summary>
    [Category("General")]
    [Description("SCADA Scaling")]
    public string? SCADAScaling { get; set; }

    /// <summary>Conn Hp/KW (Column: Conn Hp/KW)</summary>
    [Category("Electrical")]
    [Description("Conn Hp/KW")]
    [Required]
    public string? ConnHpKW { get; set; }

    /// <summary>R1 pu (Column: R1 pu (<50))</summary>
    [Category("General")]
    [Description("R1 pu")]
    [Units("<50")]
    public string? R1pu { get; set; }

    /// <summary>X1 pu (Column: X1 pu (<50))</summary>
    [Category("General")]
    [Description("X1 pu")]
    [Units("<50")]
    public string? X1pu { get; set; }

    /// <summary>X/R (Column: X/R (<50))</summary>
    [Category("General")]
    [Description("X/R")]
    [Units("<50")]
    public string? XR { get; set; }

    /// <summary>Int MF (Column: Int MF (<50))</summary>
    [Category("General")]
    [Description("Int MF")]
    [Units("<50")]
    public string? IntMF { get; set; }

    /// <summary>R1 pu (Column: R1 pu (>50))</summary>
    [Category("General")]
    [Description("R1 pu")]
    [Units(">50")]
    public string? R1pu { get; set; }

    /// <summary>X1 pu (Column: X1 pu (>50))</summary>
    [Category("General")]
    [Description("X1 pu")]
    [Units(">50")]
    public string? X1pu { get; set; }

    /// <summary>X/R (Column: X/R (>50))</summary>
    [Category("General")]
    [Description("X/R")]
    [Units(">50")]
    public string? XR { get; set; }

    /// <summary>Int MF (Column: Int MF (>50))</summary>
    [Category("General")]
    [Description("Int MF")]
    [Units(">50")]
    public string? IntMF { get; set; }

    /// <summary>MVA pu (Column: MVA pu)</summary>
    [Category("General")]
    [Description("MVA pu")]
    [Required]
    public string? MVApu { get; set; }

    /// <summary>Hrm Load Type (Column: Hrm Load Type)</summary>
    [Category("Physical")]
    [Description("Hrm Load Type")]
    public string? HrmLoadType { get; set; }

    /// <summary>Hrm RC Value (Column: Hrm RC Value)</summary>
    [Category("General")]
    [Description("Hrm RC Value")]
    public string? HrmRCValue { get; set; }

    /// <summary>Hrm RC Factor (Column: Hrm RC Factor)</summary>
    [Category("General")]
    [Description("Hrm RC Factor")]
    public string? HrmRCFactor { get; set; }

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

    /// <summary>Stored AFB (Column: Stored AFB (in))</summary>
    [Category("General")]
    [Description("Stored AFB")]
    [Units("in")]
    public string? StoredAFB { get; set; }

    /// <summary>Stored AF IE (Column: Stored AF IE (cal/cm2))</summary>
    [Category("General")]
    [Description("Stored AF IE")]
    [Units("cal/cm²")]
    public string? StoredAFIE { get; set; }

    /// <summary>Stored AF PPE (Column: Stored AF PPE)</summary>
    [Category("General")]
    [Description("Stored AF PPE")]
    public string? StoredAFPPE { get; set; }

    /// <summary>SC Sym kA (Column: SC Sym kA (ANSI))</summary>
    [Category("Electrical")]
    [Description("SC Sym kA")]
    [Units("ANSI")]
    public string? SCSymkA { get; set; }

    /// <summary>SC Sym kA (Column: SC Sym kA (IEC))</summary>
    [Category("Electrical")]
    [Description("SC Sym kA")]
    [Units("°C")]
    public string? SCSymkA { get; set; }

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

    /// <summary>Alias for Panels (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Panels;
        set => Panels = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Panel"/> class.
    /// </summary>
    public Panel() { }

    /// <summary>
    /// Returns a string representation of the Panel.
    /// </summary>
    public override string ToString()
    {
        return $"Panel: {Panels}";
    }
}

