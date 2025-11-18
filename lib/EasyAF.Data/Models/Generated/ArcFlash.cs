using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an ArcFlash with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Arc Flash Scenario Report" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Arc Flash Scenario Report")]
public class ArcFlash
{
    /// <summary>Arc Fault Bus Name (Column: Arc Fault Bus Name)</summary>
    [Category("Electrical")]
    [Description("Arc Fault Bus Name")]
    [Required]
    public string? ArcFaultBusName { get; set; }

    /// <summary>Worst Case (Column: Worst Case)</summary>
    [Category("Protection")]
    [Description("Worst Case")]
    public string? WorstCase { get; set; }

    /// <summary>Scenario (Column: Scenario)</summary>
    [Category("General")]
    [Description("Scenario")]
    [Required]
    public string? Scenario { get; set; }

    /// <summary>Arc Fault Bus kV (Column: Arc Fault Bus kV)</summary>
    [Category("Electrical")]
    [Description("Arc Fault Bus kV")]
    public string? ArcFaultBuskV { get; set; }

    /// <summary>Upstream Trip Device Name (Column: Upstream Trip Device Name)</summary>
    [Category("Protection")]
    [Description("Upstream Trip Device Name")]
    public string? UpstreamTripDeviceName { get; set; }

    /// <summary>Upstream Trip Device Function (Column: Upstream Trip Device Function)</summary>
    [Category("Protection")]
    [Description("Upstream Trip Device Function")]
    public string? UpstreamTripDeviceFunction { get; set; }

    /// <summary>Equip Type (Column: Equip Type)</summary>
    [Category("Physical")]
    [Description("Equip Type")]
    public string? EquipType { get; set; }

    /// <summary>Electrode Configuration (Column: Electrode Configuration)</summary>
    [Category("General")]
    [Description("Electrode Configuration")]
    public string? ElectrodeConfiguration { get; set; }

    /// <summary>Electrode Gap (Column: Electrode Gap (mm))</summary>
    [Category("General")]
    [Description("Electrode Gap")]
    [Units("mm")]
    public string? ElectrodeGap { get; set; }

    /// <summary>Bus Bolted Fault (Column: Bus Bolted Fault (kA))</summary>
    [Category("Electrical")]
    [Description("Bus Bolted Fault")]
    [Units("kA")]
    public string? BusBoltedFault { get; set; }

    /// <summary>Bus Arc Fault (Column: Bus Arc Fault (kA))</summary>
    [Category("Electrical")]
    [Description("Bus Arc Fault")]
    [Units("kA")]
    public string? BusArcFault { get; set; }

    /// <summary>Trip Time (Column: Trip Time (sec))</summary>
    [Category("Protection")]
    [Description("Trip Time")]
    [Units("s")]
    public string? TripTime { get; set; }

    /// <summary>Opening Time (Column: Opening Time (sec))</summary>
    [Category("General")]
    [Description("Opening Time")]
    [Units("s")]
    public string? OpeningTime { get; set; }

    /// <summary>Arc Time (Column: Arc Time (sec))</summary>
    [Category("General")]
    [Description("Arc Time")]
    [Units("s")]
    public string? ArcTime { get; set; }

    /// <summary>Est Arc Flash Boundary (Column: Est Arc Flash Boundary (inches))</summary>
    [Category("Protection")]
    [Description("Est Arc Flash Boundary")]
    [Units("in")]
    public string? EstArcFlashBoundary { get; set; }

    /// <summary>Working Distance (Column: Working Distance (inches))</summary>
    [Category("General")]
    [Description("Working Distance")]
    [Units("in")]
    public string? WorkingDistance { get; set; }

    /// <summary>Incident Energy (Column: Incident Energy (cal/cm2))</summary>
    [Category("General")]
    [Description("Incident Energy")]
    [Units("cal/cm²")]
    public string? IncidentEnergy { get; set; }

    /// <summary>Comments (Column: Comments)</summary>
    [Category("Metadata")]
    [Description("Comments")]
    public string? Comments { get; set; }

    /// <summary>Alias for ArcFaultBusName (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => ArcFaultBusName;
        set => ArcFaultBusName = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArcFlash"/> class.
    /// </summary>
    public ArcFlash() { }

    /// <summary>
    /// Returns a string representation of the ArcFlash.
    /// </summary>
    public override string ToString()
    {
        return $"ArcFlash: {ArcFaultBusName}";
    }
}
