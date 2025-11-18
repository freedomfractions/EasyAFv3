using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Bus with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Buses" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Buses")]
public class Bus
{
    /// <summary>Buses (Column: Buses)</summary>
    [Category("Identity")]
    [Description("Buses")]
    [Required]
    public string? Buss { get; set; }

    /// <summary>AC/DC (Column: AC/DC)</summary>
    [Category("General")]
    [Description("AC/DC")]
    public string? AcDc { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

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

    /// <summary>Device Code (Column: Device Code)</summary>
    [Category("General")]
    [Description("Device Code")]
    public string? DeviceCode { get; set; }

    /// <summary>Manufacturer (Column: Manufacturer)</summary>
    [Category("Physical")]
    [Description("Manufacturer")]
    public string? Manufacturer { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

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

    /// <summary>Test Standard (Column: Test Standard)</summary>
    [Category("General")]
    [Description("Test Standard")]
    public string? TestStandard { get; set; }

    /// <summary>Material (Column: Material)</summary>
    [Category("Physical")]
    [Description("Material")]
    public string? Material { get; set; }

    /// <summary>Mounting (Column: Mounting)</summary>
    [Category("General")]
    [Description("Mounting")]
    public string? Mounting { get; set; }

    /// <summary>Dn Conn kVA (Column: Dn Conn kVA)</summary>
    [Category("Electrical")]
    [Description("Dn Conn kVA")]
    public string? DnConnKVA { get; set; }

    /// <summary>Dn Conn FLA (Column: Dn Conn FLA)</summary>
    [Category("General")]
    [Description("Dn Conn FLA")]
    public string? DnConnFLA { get; set; }

    /// <summary>Dn Demand kVA (Column: Dn Demand kVA)</summary>
    [Category("Electrical")]
    [Description("Dn Demand kVA")]
    public string? DnDemandKVA { get; set; }

    /// <summary>Dn Demand FLA (Column: Dn Demand FLA)</summary>
    [Category("Demand")]
    [Description("Dn Demand FLA")]
    public string? DnDemandFLA { get; set; }

    /// <summary>Dn Code kVA (Column: Dn Code kVA)</summary>
    [Category("Electrical")]
    [Description("Dn Code kVA")]
    public string? DnCodeKVA { get; set; }

    /// <summary>Dn Code FLA (Column: Dn Code FLA)</summary>
    [Category("General")]
    [Description("Dn Code FLA")]
    public string? DnCodeFLA { get; set; }

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

    /// <summary>Stored AFB (in) (Column: Stored AFB (in))</summary>
    [Category("General")]
    [Description("Stored AFB (in)")]
    [Units("in")]
    public string? StoredAfbInches { get; set; }

    /// <summary>Stored AF IE (cal/cm2) (Column: Stored AF IE (cal/cm2))</summary>
    [Category("General")]
    [Description("Stored AF IE (cal/cm2)")]
    [Units("calcm²")]
    public string? StoredAFIeCalPerCm2 { get; set; }

    /// <summary>Stored AF PPE (Column: Stored AF PPE)</summary>
    [Category("General")]
    [Description("Stored AF PPE")]
    public string? StoredAFPpe { get; set; }

    /// <summary>SC Sym kA (ANSI) (Column: SC Sym kA (ANSI))</summary>
    [Category("General")]
    [Description("SC Sym kA (ANSI)")]
    [Units("ANSI")]
    public string? SCSymKAANSI { get; set; }

    /// <summary>SC Sym kA (IEC) (Column: SC Sym kA (IEC))</summary>
    [Category("General")]
    [Description("SC Sym kA (IEC)")]
    [Units("IEC")]
    public string? SCSymKAIEC { get; set; }

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

    /// <summary>Alias for Bus (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Buss;
        set => Buss = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bus"/> class.
    /// </summary>
    public Bus() { }

    /// <summary>
    /// Returns a string representation of the Bus.
    /// </summary>
    public override string ToString()
    {
        return $"Bus: {Buss}";
    }
}

