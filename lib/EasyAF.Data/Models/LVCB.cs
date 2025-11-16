using System;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents a low voltage circuit breaker (LVCB) with protection, ratings, reliability data, and trip unit settings.
    /// Trip unit settings are flattened onto this class as TripUnit* properties to simplify data processing.
    /// </summary>
    public class LVCB
    {
        /// <summary>LV Breakers (unique identifier).</summary>
        public string? Id { get; set; }
        /// <summary>AC / DC indicator.</summary>
        public string? AcDc { get; set; }
        /// <summary>Status.</summary>
        public string? Status { get; set; }
        /// <summary>No of Phases.</summary>
        public string? Phases { get; set; }
        /// <summary>On Bus.</summary>
        public string? Bus { get; set; }
        /// <summary>Base kV.</summary>
        public string? Voltage { get; set; }
        /// <summary>Conn Type.</summary>
        public string? ConnectionType { get; set; }
        /// <summary>Class.</summary>
        public string? Class { get; set; }
        /// <summary>Options.</summary>
        public string? Options { get; set; }
        /// <summary>Breaker Mfr.</summary>
        public string? Manufacturer { get; set; }
        /// <summary>Breaker Type.</summary>
        public string? BreakerType { get; set; }
        /// <summary>Breaker Style.</summary>
        public string? Style { get; set; }
        /// <summary>Continuous Current (A).</summary>
        public string? ContCurrentA { get; set; }
        /// <summary>Frame (A).</summary>
        public string? FrameSize { get; set; }
        /// <summary>ST ZSI.</summary>
        public string? StZsi { get; set; }
        /// <summary>ST ZSI I2T.</summary>
        public string? StZsiI2t { get; set; }
        /// <summary>ST ZSI Delay.</summary>
        public string? StZsiDelay { get; set; }
        /// <summary>Inst ZSI.</summary>
        public string? InstZsi { get; set; }
        /// <summary>Gnd ZSI.</summary>
        public string? GndZsi { get; set; }
        /// <summary>Gnd ZSI I2T.</summary>
        public string? GndZsiI2t { get; set; }
        /// <summary>Gnd ZSI Delay.</summary>
        public string? GndZsiDelay { get; set; }
        /// <summary>Self Restrain.</summary>
        public string? SelfRestrain { get; set; }
        /// <summary>T-ZSI.</summary>
        public string? TZsi { get; set; }
        /// <summary>Fuse Mfr.</summary>
        public string? FuseMfr { get; set; }
        /// <summary>Fuse Type.</summary>
        public string? FuseType { get; set; }
        /// <summary>Fuse Style.</summary>
        public string? FuseStyle { get; set; }
        /// <summary>Fuse Size.</summary>
        public string? FuseSize { get; set; }
        /// <summary>Motor O/L Mfr.</summary>
        public string? MtrOlMfr { get; set; }
        /// <summary>Motor O/L Type.</summary>
        public string? MtrOlType { get; set; }
        /// <summary>Motor O/L Style.</summary>
        public string? MtrOlStyle { get; set; }
        /// <summary>Motor FLA.</summary>
        public string? MotorFla { get; set; }
        /// <summary>Service Factor.</summary>
        public string? ServiceFactor { get; set; }
        /// <summary>Standard.</summary>
        public string? Standard { get; set; }
        /// <summary>SC Rating Based On.</summary>
        public string? ScRatingBasedOn { get; set; }
        /// <summary>SC Int kA (ANSI interrupting rating).</summary>
        public string? AIC { get; set; }
        /// <summary>IEC Breaking kA.</summary>
        public string? IecBreakingKA { get; set; }
        /// <summary>SC Test Std.</summary>
        public string? ScTestStd { get; set; }
        /// <summary>TCC Clipping.</summary>
        public string? TccClipping { get; set; }
        /// <summary>TCC Mom kA.</summary>
        public string? TccMomKA { get; set; }
        /// <summary>TCC Int kA.</summary>
        public string? TccIntKA { get; set; }
        /// <summary>TCC 30 Cyc kA.</summary>
        public string? Tcc30CycKA { get; set; }
        /// <summary>TCC Gnd Mom kA.</summary>
        public string? TccGndMomKA { get; set; }
        /// <summary>TCC Gnd Int kA.</summary>
        public string? TccGndIntKA { get; set; }
        /// <summary>TCC Gnd 30 Cyc kA.</summary>
        public string? TccGnd30CycKA { get; set; }
        /// <summary>IEC TCC Initial kA.</summary>
        public string? IecTccInitialKA { get; set; }
        /// <summary>IEC TCC Breaking kA.</summary>
        public string? IecTccBreakingKA { get; set; }
        /// <summary>IEC TCC Breaking Time.</summary>
        public string? IecTccBreakingTime { get; set; }
        /// <summary>IEC TCC SS kA.</summary>
        public string? IecTccSsKA { get; set; }
        /// <summary>IEC TCC Gnd Initial kA.</summary>
        public string? IecTccGndInitialKA { get; set; }
        /// <summary>IEC TCC Gnd Breaking kA.</summary>
        public string? IecTccGndBreakingKA { get; set; }
        /// <summary>IEC TCC Gnd Breaking Time.</summary>
        public string? IecTccGndBreakingTime { get; set; }
        /// <summary>IEC TCC Gnd SS kA.</summary>
        public string? IecTccGndSsKA { get; set; }
        /// <summary>Normal State.</summary>
        public string? NormalState { get; set; }
        /// <summary>PCC kVA Demand.</summary>
        public string? PccKvaDemand { get; set; }
        /// <summary>PCC Isc / ILoad.</summary>
        public string? PccIscILoad { get; set; }
        /// <summary>Failure Rate (/year).</summary>
        public string? FailureRatePerYear { get; set; }
        /// <summary>Repair Time (h).</summary>
        public string? RepairTimeHours { get; set; }
        /// <summary>Replace Time (h).</summary>
        public string? ReplaceTimeHours { get; set; }
        /// <summary>Repair Cost.</summary>
        public string? RepairCost { get; set; }
        /// <summary>Replace Cost.</summary>
        public string? ReplaceCost { get; set; }
        /// <summary>Action Upon Failure.</summary>
        public string? ActionUponFailure { get; set; }
        /// <summary>Reliability Source.</summary>
        public string? ReliabilitySource { get; set; }
        /// <summary>Reliability Category.</summary>
        public string? ReliabilityCategory { get; set; }
        /// <summary>Reliability Class.</summary>
        public string? ReliabilityClass { get; set; }
        /// <summary>SC Failure Mode %.</summary>
        public string? ScFailureModePercent { get; set; }
        /// <summary>Data Status.</summary>
        public string? DataStatus { get; set; }

        // FLATTENED TRIP UNIT PROPERTIES (prefixed with TripUnit*)
        /// <summary>Trip unit manufacturer.</summary>
        public string? TripUnitManufacturer { get; set; }
        /// <summary>Trip unit adjustable flag.</summary>
        public bool TripUnitAdjustable { get; set; }
        /// <summary>Trip unit type.</summary>
        public string? TripUnitType { get; set; }
        /// <summary>Trip unit style.</summary>
        public string? TripUnitStyle { get; set; }
        /// <summary>Trip plug.</summary>
        public string? TripUnitTripPlug { get; set; }
        /// <summary>Sensor frame.</summary>
        public string? TripUnitSensorFrame { get; set; }
        /// <summary>Long time pickup.</summary>
        public string? TripUnitLtpu { get; set; }
        /// <summary>Long time pickup multiplier.</summary>
        public string? TripUnitLtpuMult { get; set; }
        /// <summary>Long time pickup (amps).</summary>
        public string? TripUnitLtpuAmps { get; set; }
        /// <summary>Long time delay band.</summary>
        public string? TripUnitLtdBand { get; set; }
        /// <summary>Long time delay curve.</summary>
        public string? TripUnitLtdCurve { get; set; }
        /// <summary>Trip adjust.</summary>
        public string? TripUnitTripAdjust { get; set; }
        /// <summary>Trip pickup.</summary>
        public string? TripUnitTripPickup { get; set; }
        /// <summary>Short time pickup.</summary>
        public string? TripUnitStpu { get; set; }
        /// <summary>Short time pickup (amps).</summary>
        public string? TripUnitStpuAmps { get; set; }
        /// <summary>Short time delay band.</summary>
        public string? TripUnitStdBand { get; set; }
        /// <summary>Short time I2t.</summary>
        public string? TripUnitStpuI2t { get; set; }
        /// <summary>Short time delay I2t.</summary>
        public string? TripUnitStdI2t { get; set; }
        /// <summary>Instantaneous.</summary>
        public string? TripUnitInst { get; set; }
        /// <summary>Instantaneous override.</summary>
        public string? TripUnitInstOverride { get; set; }
        /// <summary>Instantaneous (amps).</summary>
        public string? TripUnitInstAmps { get; set; }
        /// <summary>Instantaneous override pickup (amps).</summary>
        public string? TripUnitInstOverridePickupAmps { get; set; }
        /// <summary>Maintenance mode.</summary>
        public string? TripUnitMaint { get; set; }
        /// <summary>Maintenance setting.</summary>
        public string? TripUnitMaintSetting { get; set; }
        /// <summary>Maintenance (amps).</summary>
        public string? TripUnitMaintAmps { get; set; }
        /// <summary>Ground sensor.</summary>
        public string? TripUnitGndSensor { get; set; }
        /// <summary>Ground fault pickup.</summary>
        public string? TripUnitGfpu { get; set; }
        /// <summary>Ground fault pickup (amps).</summary>
        public string? TripUnitGfpuAmps { get; set; }
        /// <summary>Ground fault delay.</summary>
        public string? TripUnitGfd { get; set; }
        /// <summary>Ground fault delay I2t.</summary>
        public string? TripUnitGfdI2t { get; set; }
        /// <summary>Ground maintenance pickup.</summary>
        public string? TripUnitGndMaintPickup { get; set; }
        /// <summary>Ground maintenance (amps).</summary>
        public string? TripUnitGndMaintAmps { get; set; }
        /// <summary>Trip unit comments.</summary>
        public string? TripUnitComments { get; set; }

        /// <summary>Additional comments or notes. (Comment)</summary>
        public string? Comments { get; set; }

        /// <summary>
        /// Legacy nested trip unit property. Prefer using flattened TripUnit* properties.
        /// </summary>
        [Obsolete("Use flattened TripUnit* properties on LVCB instead of nested TripUnit.")]
        public TripUnit? TripUnit 
        { 
            get
            {
                if (_tripUnit == null)
                {
                    _tripUnit = new TripUnit
                    {
                        Manufacturer = TripUnitManufacturer,
                        Adjustable = TripUnitAdjustable,
                        Type = TripUnitType,
                        Style = TripUnitStyle,
                        TripPlug = TripUnitTripPlug,
                        SensorFrame = TripUnitSensorFrame,
                        Ltpu = TripUnitLtpu,
                        LtpuMult = TripUnitLtpuMult,
                        LtpuAmps = TripUnitLtpuAmps,
                        LtdBand = TripUnitLtdBand,
                        LtdCurve = TripUnitLtdCurve,
                        TripAdjust = TripUnitTripAdjust,
                        TripPickup = TripUnitTripPickup,
                        Stpu = TripUnitStpu,
                        StpuAmps = TripUnitStpuAmps,
                        StdBand = TripUnitStdBand,
                        StpuI2t = TripUnitStpuI2t,
                        StdI2t = TripUnitStdI2t,
                        Inst = TripUnitInst,
                        InstOverride = TripUnitInstOverride,
                        InstAmps = TripUnitInstAmps,
                        InstOverridePickupAmps = TripUnitInstOverridePickupAmps,
                        Maint = TripUnitMaint,
                        MaintSetting = TripUnitMaintSetting,
                        MaintAmps = TripUnitMaintAmps,
                        GndSensor = TripUnitGndSensor,
                        Gfpu = TripUnitGfpu,
                        GfpuAmps = TripUnitGfpuAmps,
                        Gfd = TripUnitGfd,
                        GfdI2t = TripUnitGfdI2t,
                        GndMaintPickup = TripUnitGndMaintPickup,
                        GndMaintAmps = TripUnitGndMaintAmps,
                        Comments = TripUnitComments
                    };
                }
                return _tripUnit;
            }
            set
            {
                _tripUnit = value;
                if (value != null)
                {
                    TripUnitManufacturer = value.Manufacturer;
                    TripUnitAdjustable = value.Adjustable;
                    TripUnitType = value.Type;
                    TripUnitStyle = value.Style;
                    TripUnitTripPlug = value.TripPlug;
                    TripUnitSensorFrame = value.SensorFrame;
                    TripUnitLtpu = value.Ltpu;
                    TripUnitLtpuMult = value.LtpuMult;
                    TripUnitLtpuAmps = value.LtpuAmps;
                    TripUnitLtdBand = value.LtdBand;
                    TripUnitLtdCurve = value.LtdCurve;
                    TripUnitTripAdjust = value.TripAdjust;
                    TripUnitTripPickup = value.TripPickup;
                    TripUnitStpu = value.Stpu;
                    TripUnitStpuAmps = value.StpuAmps;
                    TripUnitStdBand = value.StdBand;
                    TripUnitStpuI2t = value.StpuI2t;
                    TripUnitStdI2t = value.StdI2t;
                    TripUnitInst = value.Inst;
                    TripUnitInstOverride = value.InstOverride;
                    TripUnitInstAmps = value.InstAmps;
                    TripUnitInstOverridePickupAmps = value.InstOverridePickupAmps;
                    TripUnitMaint = value.Maint;
                    TripUnitMaintSetting = value.MaintSetting;
                    TripUnitMaintAmps = value.MaintAmps;
                    TripUnitGndSensor = value.GndSensor;
                    TripUnitGfpu = value.Gfpu;
                    TripUnitGfpuAmps = value.GfpuAmps;
                    TripUnitGfd = value.Gfd;
                    TripUnitGfdI2t = value.GfdI2t;
                    TripUnitGndMaintPickup = value.GndMaintPickup;
                    TripUnitGndMaintAmps = value.GndMaintAmps;
                    TripUnitComments = value.Comments;
                    _tripUnit?.InferAdjustableIfUnset();
                }
            }
        }
        private TripUnit? _tripUnit;

        /// <summary>
        /// Initializes a new instance of the <see cref="LVCB"/> class.
        /// </summary>
        public LVCB() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LVCB"/> class with select properties (legacy ctor).
        /// </summary>
        public LVCB(
            string? id,
            string? bus,
            string? manufacturer,
            string? style,
            TripUnit? tripUnit,
            string? aic,
            string? frameSize,
            string? comments)
        {
            Id = id; Bus = bus; Manufacturer = manufacturer; Style = style; TripUnit = tripUnit; AIC = aic; FrameSize = frameSize; Comments = comments;
        }

        /// <summary>
        /// Returns a summary string of the breaker's main properties.
        /// </summary>
        public override string ToString()
        {
            return $"Id: {Id}, Bus: {Bus}, Mfr: {Manufacturer}, Type: {BreakerType}, Style: {Style}, Frame: {FrameSize}, AIC: {AIC}";
        }

        /// <summary>
        /// Returns a string describing the breaker's interrupting capacity (AIC).
        /// </summary>
        public string AICToString() => $"AIC: {AIC}";
        /// <summary>
        /// Returns a string describing the breaker's frame size.
        /// </summary>
        public string FrameSizeToString() => $"FrameSize: {FrameSize}";

        private static bool ComputeAdjustable(TripUnit tu)
        {
            bool HasDynamic(string? s) => !string.IsNullOrWhiteSpace(s) && !string.Equals(s.Trim(), "fixed", StringComparison.OrdinalIgnoreCase);
            if (HasDynamic(tu.Ltpu) || HasDynamic(tu.LtpuAmps) || HasDynamic(tu.LtdBand) || HasDynamic(tu.LtdCurve) ||
                HasDynamic(tu.Stpu) || HasDynamic(tu.StpuAmps) || HasDynamic(tu.StdBand) || HasDynamic(tu.StdI2t) ||
                HasDynamic(tu.Inst) || HasDynamic(tu.InstAmps) || HasDynamic(tu.Maint) || HasDynamic(tu.MaintAmps) ||
                HasDynamic(tu.Gfpu) || HasDynamic(tu.GfpuAmps) || HasDynamic(tu.Gfd) || HasDynamic(tu.GfdI2t) ||
                HasDynamic(tu.TripPlug) || HasDynamic(tu.SensorFrame) || HasDynamic(tu.TripAdjust) || HasDynamic(tu.TripPickup))
                return true;
            return false;
        }
    }
}
