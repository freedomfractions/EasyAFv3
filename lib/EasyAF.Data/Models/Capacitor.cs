using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a capacitor bank for power factor correction from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
[EasyPowerClass("Capacitors")]
public class Capacitor
{
    /// <summary>Capacitor identifier. (Column: Capacitors)</summary>
    [Category("Identity")]
    [Description("Capacitor identifier")]
    [Required]
    public string? Capacitors { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id { get => Capacitors; set => Capacitors = value; }
    
    [Category("Electrical")] public string? AcDc { get; set; }
    [Category("Identity")] public string? Status { get; set; }
    [Category("Electrical")] public string? NoOfPhases { get; set; }
    [Category("Electrical"), Required] public string? OnBus { get; set; }
    [Category("Electrical"), Units("kV")] public string? BaseKV { get; set; }
    
    [Category("Physical"), Units("kVAR"), Required] public string? KVAR { get; set; }
    [Category("Physical"), Units("kV")] public string? RatedKV { get; set; }
    
    [Category("Physical")] public string? Type { get; set; }
    [Category("Physical")] public string? BankConfiguration { get; set; }
    [Category("Physical")] public string? Manufacturer { get; set; }
    [Category("Physical")] public string? Model { get; set; }
    
    [Category("Control")] public string? ControlType { get; set; }
    [Category("Control")] public string? SwitchingMode { get; set; }
    
    [Category("Protection")] public string? FuseSize { get; set; }
    [Category("Protection")] public string? FuseType { get; set; }
    
    [Category("Reliability"), Units("/year")] public string? FailureRatePerYear { get; set; }
    [Category("Reliability"), Units("h")] public string? RepairTimeHours { get; set; }
    [Category("Reliability"), Units("h")] public string? ReplaceTimeHours { get; set; }
    [Category("Reliability"), Units("$")] public string? RepairCost { get; set; }
    [Category("Reliability"), Units("$")] public string? ReplaceCost { get; set; }
    [Category("Reliability")] public string? ActionUponFailure { get; set; }
    [Category("Reliability")] public string? ReliabilitySource { get; set; }
    [Category("Reliability")] public string? ReliabilityCategory { get; set; }
    [Category("Reliability")] public string? ReliabilityClass { get; set; }
    
    [Category("Metadata")] public string? DataStatus { get; set; }
    [Category("Metadata")] public string? Comments { get; set; }
    
    public Capacitor() { }
    public override string ToString() => $"Capacitor: {Capacitors}, OnBus: {OnBus}, {KVAR} kVAR";
}
