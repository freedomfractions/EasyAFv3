using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an UPS with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "UPSs" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("UPSs")]
public class UPS
{
    /// <summary>UPSs (Column: UPSs)</summary>
    [Category("General")]
    [Description("UPSs")]
    [Required]
    public string? UPSs { get; set; }

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

    /// <summary>Input Bus ID (Column: Input Bus ID)</summary>
    [Category("Identity")]
    [Description("Input Bus ID")]
    public string? InputBusID { get; set; }

    /// <summary>Input Device ID (Column: Input Device ID)</summary>
    [Category("General")]
    [Description("Input Device ID")]
    public string? InputDeviceID { get; set; }

    /// <summary>Input Device Type (Column: Input Device Type)</summary>
    [Category("Physical")]
    [Description("Input Device Type")]
    public string? InputDeviceType { get; set; }

    /// <summary>Output Bus ID (Column: Output Bus ID)</summary>
    [Category("Identity")]
    [Description("Output Bus ID")]
    public string? OutputBusID { get; set; }

    /// <summary>Output Device ID (Column: Output Device ID)</summary>
    [Category("General")]
    [Description("Output Device ID")]
    public string? OutputDeviceID { get; set; }

    /// <summary>Output Device Type (Column: Output Device Type)</summary>
    [Category("Physical")]
    [Description("Output Device Type")]
    public string? OutputDeviceType { get; set; }

    /// <summary>kVA (Column: kVA)</summary>
    [Category("Electrical")]
    [Description("kVA")]
    [Required]
    public string? KVA { get; set; }

    /// <summary>X/R (Column: X/R)</summary>
    [Category("General")]
    [Description("X/R")]
    public string? XR { get; set; }

    /// <summary>1/2 Cycle SC (Column: 1/2 Cycle SC)</summary>
    [Category("General")]
    [Description("1/2 Cycle SC")]
    public string? 12CycleSC { get; set; }

    /// <summary>Int SC (Column: Int SC)</summary>
    [Category("General")]
    [Description("Int SC")]
    public string? IntSC { get; set; }

    /// <summary>30 Cycle SC (Column: 30 Cycle SC)</summary>
    [Category("General")]
    [Description("30 Cycle SC")]
    public string? 30CycleSC { get; set; }

    /// <summary>Ctl kV pu (Column: Ctl kV pu)</summary>
    [Category("Electrical")]
    [Description("Ctl kV pu")]
    public string? CtlkVpu { get; set; }

    /// <summary>Ctl Angle (Column: Ctl Angle)</summary>
    [Category("General")]
    [Description("Ctl Angle")]
    public string? CtlAngle { get; set; }

    /// <summary>% Efficiency (Column: % Efficiency)</summary>
    [Category("General")]
    [Description("% Efficiency")]
    public string? %Efficiency { get; set; }

    /// <summary>% Battery (Column: % Battery)</summary>
    [Category("General")]
    [Description("% Battery")]
    public string? %Battery { get; set; }

    /// <summary>Input PF (Column: Input PF)</summary>
    [Category("General")]
    [Description("Input PF")]
    public string? InputPF { get; set; }

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

    /// <summary>Alias for UPSs (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => UPSs;
        set => UPSs = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UPS"/> class.
    /// </summary>
    public UPS() { }

    /// <summary>
    /// Returns a string representation of the UPS.
    /// </summary>
    public override string ToString()
    {
        return $"UPS: {UPSs}";
    }
}

