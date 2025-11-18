using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a general electrical load from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
[EasyPowerClass("Loads")]
public class Load
{
    /// <summary>Load identifier. (Column: Loads)</summary>
    [Category("Identity")]
    [Description("Load identifier")]
    [Required]
    public string? Loads { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id { get => Loads; set => Loads = value; }
    
    [Category("Electrical")] public string? AcDc { get; set; }
    [Category("Identity")] public string? Status { get; set; }
    [Category("Electrical")] public string? NoOfPhases { get; set; }
    [Category("Electrical"), Required] public string? OnBus { get; set; }
    [Category("Electrical"), Units("kV")] public string? BaseKV { get; set; }
    
    [Category("Electrical"), Units("kW")] public string? KW { get; set; }
    [Category("Electrical"), Units("kVAR")] public string? KVAR { get; set; }
    [Category("Electrical"), Units("kVA"), Required] public string? KVA { get; set; }
    [Category("Electrical"), Units("A")] public string? FLA { get; set; }
    [Category("Electrical"), Units("%")] public string? PFPercent { get; set; }
    
    [Category("Physical")] public string? LoadType { get; set; }
    [Category("Physical")] public string? LoadCharacteristics { get; set; }
    
    [Category("Demand")] public string? DemandFactor { get; set; }
    [Category("Demand")] public string? DiversityFactor { get; set; }
    
    [Category("Protection")] public string? BranchCircuit { get; set; }
    [Category("Protection")] public string? ProtectiveDevice { get; set; }
    
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
    
    public Load() { }
    public override string ToString() => $"Load: {Loads}, OnBus: {OnBus}, {KVA} kVA, PF: {PFPercent}%";
}
