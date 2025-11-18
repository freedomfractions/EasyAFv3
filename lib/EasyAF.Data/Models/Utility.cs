using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a utility connection point with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
[EasyPowerClass("Utility")]
public class Utility
{
    /// <summary>Utility identifier. (Column: Utility)</summary>
    [Category("Identity")]
    [Description("Utility connection identifier")]
    [Required]
    public string? UtilityId { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id { get => UtilityId; set => UtilityId = value; }
    
    [Category("Electrical")] public string? AcDc { get; set; }
    [Category("Identity")] public string? Status { get; set; }
    [Category("Electrical")] public string? NoOfPhases { get; set; }
    [Category("Electrical"), Required] public string? OnBus { get; set; }
    [Category("Electrical"), Units("kV"), Required] public string? BaseKV { get; set; }
    
    [Category("Electrical"), Units("MVA"), Required] public string? SCMVASymmetrical { get; set; }
    [Category("Electrical"), Units("MVA")] public string? SCMVAMomentary { get; set; }
    [Category("Electrical")] public string? XRRatio { get; set; }
    [Category("Electrical")] public string? X0R0Ratio { get; set; }
    
    [Category("Physical")] public string? UtilityName { get; set; }
    [Category("Physical")] public string? ContactInfo { get; set; }
    [Category("Physical")] public string? AccountNumber { get; set; }
    
    [Category("Electrical"), Units("%")] public string? ZPercent { get; set; }
    [Category("Electrical"), Units("%")] public string? Z0Percent { get; set; }
    
    [Category("Reliability"), Units("/year")] public string? FailureRatePerYear { get; set; }
    [Category("Reliability"), Units("h")] public string? RepairTimeHours { get; set; }
    [Category("Reliability")] public string? ReliabilitySource { get; set; }
    
    [Category("Metadata")] public string? DataStatus { get; set; }
    [Category("Metadata")] public string? Comments { get; set; }
    
    public Utility() { }
    public override string ToString() => $"Utility: {UtilityId}, OnBus: {OnBus}, SC MVA: {SCMVASymmetrical}";
}
