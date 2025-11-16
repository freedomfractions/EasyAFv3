using System;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents a fuse with its electrical, protective, reliability and ancillary properties.
    /// NOTE: Many properties originate directly from import column headers; names normalized to PascalCase.
    /// </summary>
    public class Fuse
    {
        /// <summary>Unique identifier for the fuse. (Column: Fuses)</summary>
        public string? Id { get; set; }
        /// <summary>AC / DC indicator. (Column: AC/DC)</summary>
        public string? AcDc { get; set; }
        /// <summary>Status of the fuse/equipment. (Column: Status)</summary>
        public string? Status { get; set; }
        /// <summary>Number of phases. (Column: No of Phases)</summary>
        public string? Phases { get; set; }
        /// <summary>Bus to which the fuse is connected. (Column: On Bus)</summary>
        public string? Bus { get; set; }
        /// <summary>Base kV of the bus. (Column: Base kV)</summary>
        public string? Voltage { get; set; }
        /// <summary>Connection type. (Column: Conn Type)</summary>
        public string? ConnectionType { get; set; }
        /// <summary>Standards designation. (Column: Standard)</summary>
        public string? Standard { get; set; }
        /// <summary>Normal state (e.g., Open/Closed). (Column: Normal State)</summary>
        public string? NormalState { get; set; }
        /// <summary>Options. (Column: Options)</summary>
        public string? Options { get; set; }
        /// <summary>Manufacturer of the fuse. (Column: Fuse Mfr)</summary>
        public string? Manufacturer { get; set; }
        /// <summary>Type of the fuse. (Column: Fuse Type)</summary>
        public string? Type { get; set; }
        /// <summary>Style of the fuse. (Column: Fuse Style)</summary>
        public string? Style { get; set; }
        /// <summary>Model designation. (Column: Model)</summary>
        public string? Model { get; set; }
        /// <summary>Library kV value. (Column: Library kV)</summary>
        public string? LibraryKV { get; set; }
        /// <summary>Size of the fuse. (Column: Size)</summary>
        public string? Size { get; set; }
        /// <summary>Short circuit interrupting rating kA. (Column: SC Int kA)</summary>
        public string? AIC { get; set; }
        /// <summary>SC test X/R ratio. (Column: SC Test X/R)</summary>
        public string? ScTestXR { get; set; }
        /// <summary>SC test standard. (Column: SC Test Std)</summary>
        public string? ScTestStd { get; set; }
        /// <summary>TCC clipping value. (Column: TCC Clipping)</summary>
        public string? TccClipping { get; set; }
        /// <summary>TCC momentary kA. (Column: TCC Mom kA)</summary>
        public string? TccMomKA { get; set; }
        /// <summary>TCC interrupting kA. (Column: TCC Int kA)</summary>
        public string? TccIntKA { get; set; }
        /// <summary>TCC 30 cycle kA. (Column: TCC 30 Cyc kA)</summary>
        public string? Tcc30CycKA { get; set; }
        /// <summary>IEC breaking kA. (Column: IEC Breaking kA)</summary>
        public string? IecBreakingKA { get; set; }
        /// <summary>IEC TCC initial kA. (Column: IEC TCC Initial kA)</summary>
        public string? IecTccInitialKA { get; set; }
        /// <summary>IEC TCC breaking kA. (Column: IEC TCC Breaking kA)</summary>
        public string? IecTccBreakingKA { get; set; }
        /// <summary>IEC TCC breaking time. (Column: IEC TCC Breaking Time)</summary>
        public string? IecTccBreakingTime { get; set; }
        /// <summary>IEC TCC steady state kA. (Column: IEC TCC SS kA)</summary>
        public string? IecTccSsKA { get; set; }
        /// <summary>Switch manufacturer (if integral). (Column: Switch Manufacturer)</summary>
        public string? SwitchManufacturer { get; set; }
        /// <summary>Switch type. (Column: Switch Type)</summary>
        public string? SwitchType { get; set; }
        /// <summary>Switch style. (Column: Switch Style)</summary>
        public string? SwitchStyle { get; set; }
        /// <summary>Switch continuous current rating (A). (Column: Switch Cont A)</summary>
        public string? SwitchContA { get; set; }
        /// <summary>Switch momentary rating kA. (Column: Switch Mom kA)</summary>
        public string? SwitchMomKA { get; set; }
        /// <summary>Motor overload manufacturer. (Column: Mtr O/L Mfr)</summary>
        public string? MtrOlMfr { get; set; }
        /// <summary>Motor overload type. (Column: Mtr O/L Type)</summary>
        public string? MtrOlType { get; set; }
        /// <summary>Motor overload style. (Column: Mtr O/L Style)</summary>
        public string? MtrOlStyle { get; set; }
        /// <summary>Motor FLA. (Column: Motor FLA)</summary>
        public string? MotorFla { get; set; }
        /// <summary>Service factor. (Column: Service Factor)</summary>
        public string? ServiceFactor { get; set; }
        /// <summary>PCC kVA demand. (Column: PCC kVA Demand)</summary>
        public string? PccKvaDemand { get; set; }
        /// <summary>PCC Isc / ILoad ratio. (Column: PCC Isc/ILoad)</summary>
        public string? PccIscILoad { get; set; }
        /// <summary>Failure rate per year. (Column: Failure Rate (/year))</summary>
        public string? FailureRatePerYear { get; set; }
        /// <summary>Repair time (hours). (Column: Repair Time (h))</summary>
        public string? RepairTimeHours { get; set; }
        /// <summary>Replace time (hours). (Column: Replace Time (h))</summary>
        public string? ReplaceTimeHours { get; set; }
        /// <summary>Repair cost. (Column: Repair Cost)</summary>
        public string? RepairCost { get; set; }
        /// <summary>Replace cost. (Column: Replace Cost)</summary>
        public string? ReplaceCost { get; set; }
        /// <summary>Action upon failure. (Column: Action Upon Failure)</summary>
        public string? ActionUponFailure { get; set; }
        /// <summary>Reliability source. (Column: Reliability Source)</summary>
        public string? ReliabilitySource { get; set; }
        /// <summary>Reliability category. (Column: Reliability Category)</summary>
        public string? ReliabilityCategory { get; set; }
        /// <summary>Reliability class. (Column: Reliability Class)</summary>
        public string? ReliabilityClass { get; set; }
        /// <summary>SC failure mode percentage. (Column: SC Failure Mode %)</summary>
        public string? ScFailureModePercent { get; set; }
        /// <summary>Data status / quality indicator. (Column: Data Status)</summary>
        public string? DataStatus { get; set; }
        /// <summary>Additional comments or notes. (Column: Comment)</summary>
        public string? Comments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fuse"/> class.
        /// </summary>
        public Fuse() { }

        /// <summary>
        /// Legacy convenience constructor (subset of fields). Prefer setting properties directly for extended model.
        /// </summary>
        public Fuse(
            string? id,
            string? bus,
            string? manufacturer,
            string? type,
            string? style,
            string? aic,
            string? size,
            string? voltage,
            string? comments)
        {
            Id = id; Bus = bus; Manufacturer = manufacturer; Type = type; Style = style; AIC = aic; Size = size; Voltage = voltage; Comments = comments;
        }

        /// <summary>
        /// Returns a string describing the fuse size.
        /// </summary>
        public string SizeToString() => $"Size: {Size}";
        /// <summary>
        /// Returns a string describing the fuse interrupting capacity (AIC).
        /// </summary>
        public string AICToString() => $"AIC: {AIC}";
        /// <summary>
        /// Returns a string describing the fuse voltage rating.
        /// </summary>
        public string VoltageToString() => $"Voltage: {Voltage}";
        /// <summary>
        /// Returns a string representation of key fuse properties (truncated for readability).
        /// </summary>
        public override string ToString()
        {
            return $"Id: {Id}, Bus: {Bus}, Mfr: {Manufacturer}, Type: {Type}, Style: {Style}, Size: {Size}, AIC: {AIC}, kV: {Voltage}";
        }
    }
}
