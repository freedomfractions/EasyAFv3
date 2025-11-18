using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents a Relay with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Relays" class in EasyPower CSV exports.
/// </para>
/// <para>
/// <strong>Auto-Generated:</strong> This file was automatically generated from CSV field definitions.
/// Do not manually edit property names - regenerate from source CSV if changes are needed.
/// </para>
/// </remarks>
[EasyPowerClass("Relays")]
public class Relay
{
    /// <summary>Relays (Column: Relays)</summary>
    [Category("General")]
    [Description("Relays")]
    [Required]
    public string? Relays { get; set; }

    /// <summary>Status (Column: Status)</summary>
    [Category("Identity")]
    [Description("Status")]
    public string? Status { get; set; }

    /// <summary>No of Phases (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("No of Phases")]
    public string? NoOfPhases { get; set; }

    /// <summary>Manufacturer (Column: Manufacturer)</summary>
    [Category("Physical")]
    [Description("Manufacturer")]
    public string? Manufacturer { get; set; }

    /// <summary>Type (Column: Type)</summary>
    [Category("Physical")]
    [Description("Type")]
    public string? Type { get; set; }

    /// <summary>TCC Clipping (Column: TCC Clipping)</summary>
    [Category("Protection")]
    [Description("TCC Clipping")]
    public string? TCCClipping { get; set; }

    /// <summary>Function ID (Column: Function ID)</summary>
    [Category("General")]
    [Description("Function ID")]
    public string? FunctionID { get; set; }

    /// <summary>Device Function (Column: Device Function)</summary>
    [Category("General")]
    [Description("Device Function")]
    public string? DeviceFunction { get; set; }

    /// <summary>CT Bus ID (Column: CT Bus ID)</summary>
    [Category("Identity")]
    [Description("CT Bus ID")]
    public string? CTBusID { get; set; }

    /// <summary>CT Ratio (Column: CT Ratio)</summary>
    [Category("General")]
    [Description("CT Ratio")]
    public string? CTRatio { get; set; }

    /// <summary>TCC Mom kA (Column: TCC Mom kA)</summary>
    [Category("Protection")]
    [Description("TCC Mom kA")]
    public string? TCCMomKA { get; set; }

    /// <summary>TCC Int kA (Column: TCC Int kA)</summary>
    [Category("Protection")]
    [Description("TCC Int kA")]
    public string? TCCIntKA { get; set; }

    /// <summary>TCC 30 Cyc kA (Column: TCC 30 Cyc kA)</summary>
    [Category("Protection")]
    [Description("TCC 30 Cyc kA")]
    public string? TCC30CyckA { get; set; }

    /// <summary>IEC TCC Initial kA (Column: IEC TCC Initial kA)</summary>
    [Category("Protection")]
    [Description("IEC TCC Initial kA")]
    public string? IECTCCInitialKA { get; set; }

    /// <summary>IEC TCC Breaking kA (Column: IEC TCC Breaking kA)</summary>
    [Category("Protection")]
    [Description("IEC TCC Breaking kA")]
    public string? IECTCCBreakingKA { get; set; }

    /// <summary>IEC TCC SS kA (Column: IEC TCC SS kA)</summary>
    [Category("Protection")]
    [Description("IEC TCC SS kA")]
    public string? IECTCCSSKA { get; set; }

    /// <summary>Tap Range (Column: Tap Range)</summary>
    [Category("Control")]
    [Description("Tap Range")]
    public string? TapRange { get; set; }

    /// <summary>Tap Setting (Column: Tap Setting)</summary>
    [Category("Control")]
    [Description("Tap Setting")]
    public string? TapSetting { get; set; }

    /// <summary>Tap (Column: Tap (PA))</summary>
    [Category("Control")]
    [Description("Tap")]
    [Units("PA")]
    public string? Tap { get; set; }

    /// <summary>Time Dial Curve Name (Column: Time Dial Curve Name)</summary>
    [Category("General")]
    [Description("Time Dial Curve Name")]
    public string? TimeDialCurveName { get; set; }

    /// <summary>Time Dial Shift Mult (Column: Time Dial Shift Mult)</summary>
    [Category("General")]
    [Description("Time Dial Shift Mult")]
    public string? TimeDialShiftMult { get; set; }

    /// <summary>Time Dial Range (Column: Time Dial Range)</summary>
    [Category("General")]
    [Description("Time Dial Range")]
    public string? TimeDialRange { get; set; }

    /// <summary>Time Dial Setting (Column: Time Dial Setting)</summary>
    [Category("Control")]
    [Description("Time Dial Setting")]
    public string? TimeDialSetting { get; set; }

    /// <summary>TOC Time Adder (Column: TOC Time Adder)</summary>
    [Category("General")]
    [Description("TOC Time Adder")]
    public string? TOCTimeAdder { get; set; }

    /// <summary>Time Adder Unit (Column: Time Adder Unit)</summary>
    [Category("General")]
    [Description("Time Adder Unit")]
    public string? TimeAdderUnit { get; set; }

    /// <summary>TOC Min Time (Column: TOC Min Time)</summary>
    [Category("General")]
    [Description("TOC Min Time")]
    public string? TOCMinTime { get; set; }

    /// <summary>Min Time Unit (Column: Min Time Unit)</summary>
    [Category("General")]
    [Description("Min Time Unit")]
    public string? MinTimeUnit { get; set; }

    /// <summary>ST Pickup Range (Column: ST Pickup Range)</summary>
    [Category("Protection")]
    [Description("ST Pickup Range")]
    public string? STPickupRange { get; set; }

    /// <summary>ST Pickup Setting (Column: ST Pickup Setting)</summary>
    [Category("Control")]
    [Description("ST Pickup Setting")]
    public string? STPickupSetting { get; set; }

    /// <summary>STPU Amps (Column: STPU Amps (PA))</summary>
    [Category("Electrical")]
    [Description("STPU Amps")]
    [Units("PA")]
    public string? STPUAmps { get; set; }

    /// <summary>I2T (Column: I2T)</summary>
    [Category("General")]
    [Description("I2T")]
    public string? I2T { get; set; }

    /// <summary>ST Delay Setting (Column: ST Delay Setting)</summary>
    [Category("Control")]
    [Description("ST Delay Setting")]
    public string? STDelaySetting { get; set; }

    /// <summary>ST Delay Unit (Column: ST Delay Unit)</summary>
    [Category("Protection")]
    [Description("ST Delay Unit")]
    public string? STDelayUnit { get; set; }

    /// <summary>Inst Range (Column: Inst Range)</summary>
    [Category("Protection")]
    [Description("Inst Range")]
    public string? InstRange { get; set; }

    /// <summary>Inst Setting (Column: Inst Setting)</summary>
    [Category("Control")]
    [Description("Inst Setting")]
    public string? InstSetting { get; set; }

    /// <summary>Inst (Column: Inst (PA))</summary>
    [Category("Protection")]
    [Description("Inst")]
    [Units("PA")]
    public string? Inst { get; set; }

    /// <summary>Inst Delay (Column: Inst Delay)</summary>
    [Category("Protection")]
    [Description("Inst Delay")]
    public string? InstDelay { get; set; }

    /// <summary>Inst Delay Unit (Column: Inst Delay Unit)</summary>
    [Category("Protection")]
    [Description("Inst Delay Unit")]
    public string? InstDelayUnit { get; set; }

    /// <summary>Maint Mode (Column: Maint Mode)</summary>
    [Category("General")]
    [Description("Maint Mode")]
    public string? MaintMode { get; set; }

    /// <summary>Maint Pickup (Column: Maint Pickup)</summary>
    [Category("Protection")]
    [Description("Maint Pickup")]
    public string? MaintPickup { get; set; }

    /// <summary>2 X Pickup (Column: 2 X Pickup (A))</summary>
    [Category("Protection")]
    [Description("2 X Pickup")]
    [Units("A")]
    public string? 2XPickup { get; set; }

    /// <summary>2 X Pickup (Column: 2 X Pickup (s))</summary>
    [Category("Protection")]
    [Description("2 X Pickup")]
    [Units("s")]
    public string? 2XPickup { get; set; }

    /// <summary>5 X Pickup (Column: 5 X Pickup (A))</summary>
    [Category("Protection")]
    [Description("5 X Pickup")]
    [Units("A")]
    public string? 5XPickup { get; set; }

    /// <summary>5 X Pickup (Column: 5 X Pickup (s))</summary>
    [Category("Protection")]
    [Description("5 X Pickup")]
    [Units("s")]
    public string? 5XPickup { get; set; }

    /// <summary>7 X Pickup (Column: 7 X Pickup (A))</summary>
    [Category("Protection")]
    [Description("7 X Pickup")]
    [Units("A")]
    public string? 7XPickup { get; set; }

    /// <summary>7 X Pickup (Column: 7 X Pickup (s))</summary>
    [Category("Protection")]
    [Description("7 X Pickup")]
    [Units("s")]
    public string? 7XPickup { get; set; }

    /// <summary>Breaker Type (Column: Breaker Type)</summary>
    [Category("Physical")]
    [Description("Breaker Type")]
    public string? BreakerType { get; set; }

    /// <summary>Breaker ID (Column: Breaker ID)</summary>
    [Category("General")]
    [Description("Breaker ID")]
    public string? BreakerID { get; set; }

    /// <summary>Aux Time (Column: Aux Time (cyc))</summary>
    [Category("General")]
    [Description("Aux Time")]
    [Units("cyc")]
    public string? AuxTime { get; set; }

    /// <summary>Data Status (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data Status")]
    public string? DataStatus { get; set; }

    /// <summary>Comment (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("Comment")]
    public string? Comment { get; set; }

    /// <summary>Alias for Relays (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id
    {
        get => Relays;
        set => Relays = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Relay"/> class.
    /// </summary>
    public Relay() { }

    /// <summary>
    /// Returns a string representation of the Relay.
    /// </summary>
    public override string ToString()
    {
        return $"Relay: {Relays}";
    }
}

