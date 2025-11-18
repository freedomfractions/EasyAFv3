using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a generator with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
[EasyPowerClass("Generators")]
public class Generator
{
    /// <summary>Generator identifier. (Column: Generators)</summary>
    [Category("Identity")]
    [Description("Generator identifier")]
    [Required]
    public string? Generators { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id { get => Generators; set => Generators = value; }
    
    [Category("Electrical")] public string? AcDc { get; set; }
    [Category("Identity")] public string? Status { get; set; }
    [Category("Electrical")] public string? NoOfPhases { get; set; }
    [Category("Electrical"), Required] public string? OnBus { get; set; }
    [Category("Electrical"), Units("kV")] public string? BaseKV { get; set; }
    
    [Category("Physical"), Units("kW"), Required] public string? KW { get; set; }
    [Category("Physical"), Units("kVA")] public string? KVA { get; set; }
    [Category("Physical"), Units("%")] public string? PFPercent { get; set; }
    [Category("Physical"), Units("%")] public string? EfficiencyPercent { get; set; }
    
    [Category("Physical")] public string? Type { get; set; }
    [Category("Physical")] public string? FuelType { get; set; }
    [Category("Physical")] public string? Manufacturer { get; set; }
    [Category("Physical")] public string? Model { get; set; }
    
    [Category("Electrical"), Units("%")] public string? XdPercent { get; set; }
    [Category("Electrical"), Units("%")] public string? XdPrimePercent { get; set; }
    [Category("Electrical"), Units("%")] public string? XdDoublePrimePercent { get; set; }
    [Category("Electrical")] public string? XRRatio { get; set; }
    
    [Category("Control")] public string? VoltageRegulation { get; set; }
    [Category("Control"), Units("V")] public string? Setpoint { get; set; }
    
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
    
    public Generator() { }
    public override string ToString() => $"Generator: {Generators}, OnBus: {OnBus}, {KW} kW / {KVA} kVA";
}
