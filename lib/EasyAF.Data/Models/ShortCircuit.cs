using System;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents short circuit data for a bus or device, including ratings and duty calculations.
    /// Extended to include all provided import properties.
    /// </summary>
    public class ShortCircuit
    {
        /// <summary>Equipment Name (unique identifier for the entry).</summary>
        public string? Id { get; set; }
        /// <summary>Bus Name associated with the short circuit entry.</summary>
        public string? Bus { get; set; }
        /// <summary>Indicates worst case flag (e.g., "X").</summary>
        public string? WorstCase { get; set; }
        /// <summary>Scenario or case for the short circuit calculation.</summary>
        public string? Scenario { get; set; }
        /// <summary>Type of fault (e.g., 3-phase, single-line-to-ground).</summary>
        public string? FaultType { get; set; }
        /// <summary>Per unit voltage (Vpu).</summary>
        public string? Vpu { get; set; }
        /// <summary>Bus Base kV (base voltage).</summary>
        public string? Voltage { get; set; }
        /// <summary>Bus phases (Bus Ph / No. of Phases).</summary>
        public string? Phase { get; set; }
        /// <summary>Manufacturer of the device.</summary>
        public string? Manufacturer { get; set; }
        /// <summary>Style / model of the device.</summary>
        public string? Style { get; set; }
        /// <summary>Test Standard used (e.g., ANSI, IEC).</summary>
        public string? TestStandard { get; set; }
        /// <summary>Rated short circuit current (kA) (1/2 Cycle Rating (kA)).</summary>
        public string? RatingKA { get; set; }
        /// <summary>Calculated duty short circuit current (kA) (1/2 Cycle Duty (kA)).</summary>
        public string? DutyKA { get; set; }
        /// <summary>Duty as a percent of rating (1/2 Cycle Duty (%)).</summary>
        public string? DutyPercent { get; set; }
        /// <summary>Additional comments or notes.</summary>
        public string? Comments { get; set; }

        public ShortCircuit() { }

        public ShortCircuit(
            string? id,
            string? bus,
            string? scenario,
            string? faultType,
            string? voltage,
            string? phase,
            string? manufacturer,
            string? ratingKA,
            string? dutyKA,
            string? dutyPercent,
            string? comments)
        {
            Id = id;
            Bus = bus;
            Scenario = scenario;
            FaultType = faultType;
            Voltage = voltage;
            Phase = phase;
            Manufacturer = manufacturer;
            RatingKA = ratingKA;
            DutyKA = dutyKA;
            DutyPercent = dutyPercent;
            Comments = comments;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Bus: {Bus}, WorstCase: {WorstCase}, Scenario: {Scenario}, FaultType: {FaultType}, Vpu: {Vpu}, Voltage: {Voltage}, Phase: {Phase}, Mfr: {Manufacturer}, Style: {Style}, TestStd: {TestStandard}, RatingKA: {RatingKA}, DutyKA: {DutyKA}, DutyPercent: {DutyPercent}, Comments: {Comments}";
        }

        /// <summary>Determines if the device is over-dutied (duty exceeds rating).</summary>
        public bool IsOverDutied()
        {
            if (double.TryParse(DutyKA, out double duty) && double.TryParse(RatingKA, out double rating))
                return duty > rating;
            return false;
        }

        public string RatingToString() => $"RatingKA: {RatingKA}";
        public string DutyToString() => $"DutyKA: {DutyKA}, DutyPercent: {DutyPercent}";
    }
}
