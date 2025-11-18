using EasyAF.Data.Attributes;

namespace EasyAF.Data.Models;

/// <summary>
/// Represents an electrical cable/conductor with comprehensive properties from EasyPower exports.
/// All properties are strings to preserve source data fidelity without premature parsing.
/// </summary>
/// <remarks>
/// <para>
/// Cables connect electrical equipment and carry current between buses and devices.
/// This model includes physical properties, electrical characteristics, installation details, and reliability data.
/// </para>
/// <para>
/// <strong>EasyPower Correlation:</strong> Maps to "Cables" class in EasyPower CSV exports.
/// </para>
/// </remarks>
[EasyPowerClass("Cables")]
public class Cable
{
    // ========================================
    // IDENTITY & BASIC INFO
    // ========================================
    
    /// <summary>Cable identifier. (Column: Cables)</summary>
    [Category("Identity")]
    [Description("Cable identifier")]
    [Required]
    public string? Cables { get; set; }
    
    /// <summary>Alias for Cables (convenience property for dictionary indexing - not serialized).</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Id 
    { 
        get => Cables; 
        set => Cables = value; 
    }
    
    /// <summary>AC / DC designation. (Column: AC/DC)</summary>
    [Category("Electrical")]
    [Description("AC or DC power system designation")]
    public string? AcDc { get; set; }
    
    /// <summary>Status of the cable. (Column: Status)</summary>
    [Category("Identity")]
    [Description("Operational status")]
    public string? Status { get; set; }
    
    /// <summary>Number of phases. (Column: No of Phases)</summary>
    [Category("Electrical")]
    [Description("Number of phases (1, 2, or 3)")]
    public string? NoOfPhases { get; set; }

    // ========================================
    // FROM CONNECTION
    // ========================================
    
    /// <summary>From bus ID. (Column: From Bus ID)</summary>
    [Category("Electrical")]
    [Description("Source bus identifier")]
    public string? FromBusId { get; set; }
    
    /// <summary>From bus type. (Column: From Bus Type)</summary>
    [Category("Electrical")]
    [Description("Source bus type")]
    public string? FromBusType { get; set; }
    
    /// <summary>From device ID. (Column: From Device ID)</summary>
    [Category("Electrical")]
    [Description("Source device identifier")]
    public string? FromDeviceId { get; set; }
    
    /// <summary>From device type. (Column: From Device Type)</summary>
    [Category("Electrical")]
    [Description("Source device type")]
    public string? FromDeviceType { get; set; }
    
    /// <summary>From base kV. (Column: From Base kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("Source voltage level")]
    public string? FromBaseKV { get; set; }

    // ========================================
    // TO CONNECTION
    // ========================================
    
    /// <summary>To bus ID. (Column: To Bus ID)</summary>
    [Category("Electrical")]
    [Description("Destination bus identifier")]
    public string? ToBusId { get; set; }
    
    /// <summary>To bus type. (Column: To Bus Type)</summary>
    [Category("Electrical")]
    [Description("Destination bus type")]
    public string? ToBusType { get; set; }
    
    /// <summary>To device ID. (Column: To Device ID)</summary>
    [Category("Electrical")]
    [Description("Destination device identifier")]
    public string? ToDeviceId { get; set; }
    
    /// <summary>To device type. (Column: To Device Type)</summary>
    [Category("Electrical")]
    [Description("Destination device type")]
    public string? ToDeviceType { get; set; }
    
    /// <summary>To base kV. (Column: To Base kV)</summary>
    [Category("Electrical")]
    [Units("kV")]
    [Description("Destination voltage level")]
    public string? ToBaseKV { get; set; }

    // ========================================
    // CABLE CONFIGURATION
    // ========================================
    
    /// <summary>Cable unit designation. (Column: Unit)</summary>
    [Category("Physical")]
    [Description("Cable unit or section designation")]
    public string? Unit { get; set; }
    
    /// <summary>Cable type. (Column: Type)</summary>
    [Category("Physical")]
    [Description("Cable type or construction")]
    public string? Type { get; set; }
    
    /// <summary>Number per phase. (Column: No/Ph)</summary>
    [Category("Physical")]
    [Description("Number of conductors per phase")]
    public string? NumberPerPhase { get; set; }
    
    /// <summary>Phase number. (Column: Phase Num)</summary>
    [Category("Physical")]
    [Description("Phase number designation")]
    public string? PhaseNum { get; set; }
    
    /// <summary>Conductor size. (Column: Size)</summary>
    [Category("Physical")]
    [Description("Conductor size (AWG, kcmil, mm²)")]
    public string? Size { get; set; }
    
    /// <summary>Cable length. (Column: Length)</summary>
    [Category("Physical")]
    [Units("ft")]
    [Description("Cable length")]
    public string? Length { get; set; }
    
    /// <summary>Operating temperature. (Column: Op Temp (C))</summary>
    [Category("Physical")]
    [Units("°C")]
    [Description("Operating temperature")]
    public string? OpTempC { get; set; }
    
    /// <summary>Ambient temperature. (Column: Ambient Temp (C))</summary>
    [Category("Physical")]
    [Units("°C")]
    [Description("Ambient temperature")]
    public string? AmbientTempC { get; set; }
    
    /// <summary>Insulation type. (Column: Insulation)</summary>
    [Category("Physical")]
    [Description("Insulation material type")]
    public string? Insulation { get; set; }
    
    /// <summary>Insulation level. (Column: Insulation Level)</summary>
    [Category("Physical")]
    [Description("Insulation voltage level or class")]
    public string? InsulationLevel { get; set; }
    
    /// <summary>Ampacity rating. (Column: Rating (A))</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Ampacity rating at operating temperature")]
    public string? RatingA { get; set; }
    
    /// <summary>75°C ampacity rating. (Column: 75 deg C Rating (A))</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Ampacity rating at 75°C")]
    public string? Rating75DegCA { get; set; }
    
    /// <summary>Conductor material. (Column: Material)</summary>
    [Category("Physical")]
    [Description("Conductor material (Copper, Aluminum)")]
    public string? Material { get; set; }

    // ========================================
    // RACEWAY / INSTALLATION
    // ========================================
    
    /// <summary>Raceway type. (Column: Raceway Type)</summary>
    [Category("Physical")]
    [Description("Raceway or conduit type")]
    public string? RacewayType { get; set; }
    
    /// <summary>Raceway material. (Column: Raceway Mtl)</summary>
    [Category("Physical")]
    [Description("Raceway material")]
    public string? RacewayMaterial { get; set; }
    
    /// <summary>Conduit size. (Column: Conduit Size)</summary>
    [Category("Physical")]
    [Description("Conduit size designation")]
    public string? ConduitSize { get; set; }
    
    /// <summary>Number of conduits. (Column: Conduit Num)</summary>
    [Category("Physical")]
    [Description("Number of conduits")]
    public string? ConduitNum { get; set; }

    // ========================================
    // ELECTRICAL IMPEDANCE (ANSI)
    // ========================================
    
    /// <summary>Positive sequence resistance. (Column: R1)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Positive sequence resistance")]
    public string? R1 { get; set; }
    
    /// <summary>Positive sequence reactance. (Column: X1)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Positive sequence reactance")]
    public string? X1 { get; set; }
    
    /// <summary>Zero sequence resistance. (Column: R0)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Zero sequence resistance")]
    public string? R0 { get; set; }
    
    /// <summary>Zero sequence reactance. (Column: X0)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Zero sequence reactance")]
    public string? X0 { get; set; }
    
    /// <summary>Positive sequence capacitive reactance. (Column: Xc)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Positive sequence capacitive reactance")]
    public string? Xc { get; set; }
    
    /// <summary>Zero sequence capacitive reactance. (Column: Xc0)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Zero sequence capacitive reactance")]
    public string? Xc0 { get; set; }

    // ========================================
    // GROUND CONDUCTOR
    // ========================================
    
    /// <summary>Number of ground conductors. (Column: Gnd Num)</summary>
    [Category("Physical")]
    [Description("Number of ground conductors")]
    public string? GroundNum { get; set; }
    
    /// <summary>Ground conductor size. (Column: Gnd Size)</summary>
    [Category("Physical")]
    [Description("Ground conductor size")]
    public string? GroundSize { get; set; }
    
    /// <summary>Ground conductor material. (Column: Gnd Mtl)</summary>
    [Category("Physical")]
    [Description("Ground conductor material")]
    public string? GroundMaterial { get; set; }
    
    /// <summary>Ground conductor type. (Column: Gnd Type)</summary>
    [Category("Physical")]
    [Description("Ground conductor type")]
    public string? GroundType { get; set; }
    
    /// <summary>Ground conductor insulation. (Column: Gnd Insul)</summary>
    [Category("Physical")]
    [Description("Ground conductor insulation")]
    public string? GroundInsulation { get; set; }

    // ========================================
    // NEUTRAL CONDUCTOR
    // ========================================
    
    /// <summary>Number of neutral conductors. (Column: Neutral Num)</summary>
    [Category("Physical")]
    [Description("Number of neutral conductors")]
    public string? NeutralNum { get; set; }
    
    /// <summary>Neutral insulation. (Column: Neutral Insul)</summary>
    [Category("Physical")]
    [Description("Neutral conductor insulation")]
    public string? NeutralInsulation { get; set; }
    
    /// <summary>Neutral conductor size. (Column: Neutral Size)</summary>
    [Category("Physical")]
    [Description("Neutral conductor size")]
    public string? NeutralSize { get; set; }
    
    /// <summary>Neutral conductor material. (Column: Neutral Mtl)</summary>
    [Category("Physical")]
    [Description("Neutral conductor material")]
    public string? NeutralMaterial { get; set; }
    
    /// <summary>Neutral insulation level. (Column: Neutral Insulation Level)</summary>
    [Category("Physical")]
    [Description("Neutral insulation voltage level")]
    public string? NeutralInsulationLevel { get; set; }
    
    /// <summary>Neutral resistance. (Column: Neutral R)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Neutral conductor resistance")]
    public string? NeutralR { get; set; }
    
    /// <summary>Neutral reactance. (Column: Neutral X)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Neutral conductor reactance")]
    public string? NeutralX { get; set; }
    
    /// <summary>Neutral capacitive reactance. (Column: Neutral Xc)</summary>
    [Category("Electrical")]
    [Units("?")]
    [Description("Neutral capacitive reactance")]
    public string? NeutralXc { get; set; }
    
    /// <summary>Neutral conductor rating. (Column: Neutral Rating)</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Neutral conductor ampacity")]
    public string? NeutralRating { get; set; }

    // ========================================
    // CONDUCTOR CONSTRUCTION
    // ========================================
    
    /// <summary>Conductor lay type. (Column: Conductor Lay)</summary>
    [Category("Physical")]
    [Description("Conductor lay or stranding type")]
    public string? ConductorLay { get; set; }
    
    /// <summary>Conductor form. (Column: Conductor Form)</summary>
    [Category("Physical")]
    [Description("Conductor form or shape")]
    public string? ConductorForm { get; set; }
    
    /// <summary>Duct configuration. (Column: Duct Config)</summary>
    [Category("Physical")]
    [Description("Duct bank configuration")]
    public string? DuctConfig { get; set; }
    
    /// <summary>Cable spacing. (Column: Spacing)</summary>
    [Category("Physical")]
    [Description("Cable spacing arrangement")]
    public string? Spacing { get; set; }

    // ========================================
    // PER-UNIT IMPEDANCE (ANSI)
    // ========================================
    
    /// <summary>R1 per unit. (Column: R1 pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Positive sequence resistance (per unit)")]
    public string? R1Pu { get; set; }
    
    /// <summary>X1 per unit. (Column: X1 pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Positive sequence reactance (per unit)")]
    public string? X1Pu { get; set; }
    
    /// <summary>R0 per unit. (Column: R0 pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Zero sequence resistance (per unit)")]
    public string? R0Pu { get; set; }
    
    /// <summary>X0 per unit. (Column: X0 pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Zero sequence reactance (per unit)")]
    public string? X0Pu { get; set; }
    
    /// <summary>B1 per unit. (Column: B1 pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Positive sequence susceptance (per unit)")]
    public string? B1Pu { get; set; }
    
    /// <summary>B0 per unit. (Column: B0 pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Zero sequence susceptance (per unit)")]
    public string? B0Pu { get; set; }
    
    /// <summary>Neutral R per unit. (Column: Neutral R Pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Neutral resistance (per unit)")]
    public string? NeutralRPu { get; set; }
    
    /// <summary>Neutral X per unit. (Column: Neutral X Pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Neutral reactance (per unit)")]
    public string? NeutralXPu { get; set; }
    
    /// <summary>Neutral B per unit. (Column: Neutral B Pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("Neutral susceptance (per unit)")]
    public string? NeutralBPu { get; set; }

    // ========================================
    // IEC TEMPERATURE-CORRECTED IMPEDANCE
    // ========================================
    
    /// <summary>IEC field temperature. (Column: IEC Field Temp (C))</summary>
    [Category("Electrical")]
    [Units("°C")]
    [Description("IEC field operating temperature")]
    public string? IecFieldTempC { get; set; }
    
    /// <summary>IEC short-circuit temperature. (Column: IEC SC Temp (C))</summary>
    [Category("Electrical")]
    [Units("°C")]
    [Description("IEC short-circuit temperature")]
    public string? IecScTempC { get; set; }
    
    /// <summary>IEC R1 at Cmax. (Column: IEC R1 Cmax pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("IEC positive sequence resistance at max temp")]
    public string? IecR1CmaxPu { get; set; }
    
    /// <summary>IEC R0 at Cmax. (Column: IEC R0 Cmax pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("IEC zero sequence resistance at max temp")]
    public string? IecR0CmaxPu { get; set; }
    
    /// <summary>IEC R1 at Cmin. (Column: IEC R1 Cmin pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("IEC positive sequence resistance at min temp")]
    public string? IecR1CminPu { get; set; }
    
    /// <summary>IEC R0 at Cmin. (Column: IEC R0 Cmin pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("IEC zero sequence resistance at min temp")]
    public string? IecR0CminPu { get; set; }
    
    /// <summary>IEC neutral R at Cmax. (Column: IEC Neutral R Cmax pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("IEC neutral resistance at max temp")]
    public string? IecNeutralRCmaxPu { get; set; }
    
    /// <summary>IEC neutral R at Cmin. (Column: IEC Neutral R Cmin pu)</summary>
    [Category("Electrical")]
    [Units("pu")]
    [Description("IEC neutral resistance at min temp")]
    public string? IecNeutralRCminPu { get; set; }

    // ========================================
    // HARMONICS & DERATING
    // ========================================
    
    /// <summary>Rating at max temperature. (Column: Rating at Max Temp)</summary>
    [Category("Physical")]
    [Units("A")]
    [Description("Ampacity at maximum temperature")]
    public string? RatingAtMaxTemp { get; set; }
    
    /// <summary>Harmonic RC factor. (Column: Hrm RC Factor)</summary>
    [Category("Electrical")]
    [Description("Harmonic resistance correction factor")]
    public string? HrmRcFactor { get; set; }
    
    /// <summary>Harmonic RC value. (Column: Hrm RC Value)</summary>
    [Category("Electrical")]
    [Description("Harmonic resistance correction value")]
    public string? HrmRcValue { get; set; }
    
    /// <summary>Harmonic current rating setting. (Column: I Hrm Rating Setting)</summary>
    [Category("Electrical")]
    [Description("Harmonic current rating setting method")]
    public string? IHrmRatingSetting { get; set; }
    
    /// <summary>Harmonic current rating. (Column: I Hrm Rating)</summary>
    [Category("Electrical")]
    [Units("A")]
    [Description("Derated ampacity for harmonic currents")]
    public string? IHrmRating { get; set; }

    // ========================================
    // RELIABILITY - COMPONENT
    // ========================================
    
    /// <summary>Component failure rate. (Column: Comp Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Units("/year")]
    [Description("Cable component failure rate")]
    public string? CompFailureRatePerYear { get; set; }
    
    /// <summary>Component repair time. (Column: Comp Repair Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Component mean time to repair")]
    public string? CompRepairTimeHours { get; set; }
    
    /// <summary>Component replacement time. (Column: Comp Replace Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Component mean time to replace")]
    public string? CompReplaceTimeHours { get; set; }
    
    /// <summary>Component repair cost. (Column: Comp Repair Cost)</summary>
    [Category("Reliability")]
    [Units("$")]
    [Description("Component repair cost")]
    public string? CompRepairCost { get; set; }
    
    /// <summary>Component replacement cost. (Column: Comp Replace Cost)</summary>
    [Category("Reliability")]
    [Units("$")]
    [Description("Component replacement cost")]
    public string? CompReplaceCost { get; set; }
    
    /// <summary>Component action upon failure. (Column: Comp Action Upon Failure)</summary>
    [Category("Reliability")]
    [Description("Action when cable component fails")]
    public string? CompActionUponFailure { get; set; }

    // ========================================
    // RELIABILITY - CONNECTION
    // ========================================
    
    /// <summary>Connection failure rate. (Column: Conn Failure Rate (/year))</summary>
    [Category("Reliability")]
    [Units("/year")]
    [Description("Connection/termination failure rate")]
    public string? ConnFailureRatePerYear { get; set; }
    
    /// <summary>Connection repair time. (Column: Conn Repair Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Connection mean time to repair")]
    public string? ConnRepairTimeHours { get; set; }
    
    /// <summary>Connection replacement time. (Column: Conn Replace Time (h))</summary>
    [Category("Reliability")]
    [Units("h")]
    [Description("Connection mean time to replace")]
    public string? ConnReplaceTimeHours { get; set; }
    
    /// <summary>Reliability data source. (Column: Reliability Source)</summary>
    [Category("Reliability")]
    [Description("Source of reliability data")]
    public string? ReliabilitySource { get; set; }
    
    /// <summary>Reliability category. (Column: Reliability Category)</summary>
    [Category("Reliability")]
    [Description("Reliability classification category")]
    public string? ReliabilityCategory { get; set; }
    
    /// <summary>Reliability class. (Column: Reliability Class)</summary>
    [Category("Reliability")]
    [Description("Reliability class designation")]
    public string? ReliabilityClass { get; set; }

    // ========================================
    // METADATA
    // ========================================
    
    /// <summary>Data status. (Column: Data Status)</summary>
    [Category("Metadata")]
    [Description("Data quality or completion status")]
    public string? DataStatus { get; set; }
    
    /// <summary>Comments. (Column: Comment)</summary>
    [Category("Metadata")]
    [Description("General comments or notes")]
    public string? Comments { get; set; }

    // ========================================
    // METHODS
    // ========================================

    /// <summary>
    /// Initializes a new instance of the <see cref="Cable"/> class.
    /// </summary>
    public Cable() { }

    /// <summary>
    /// Returns a string representation of the cable.
    /// </summary>
    public override string ToString()
    {
        return $"Cables: {Cables}, From: {FromBusId ?? FromDeviceId}, To: {ToBusId ?? ToDeviceId}, Size: {Size}, Length: {Length}";
    }
}
