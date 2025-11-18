using System;
using System.Collections.Generic;
using System.Globalization;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents a circuit breaker trip unit with adjustable and fixed settings.
    /// Stores manufacturer, style, and all trip-related parameters.
    /// </summary>
    public class TripUnit
    {
        public string? Manufacturer { get; set; }
        public bool Adjustable { get; set; }
        public string? Type { get; set; }
        public string? Style { get; set; }
        public string? TripPlug { get; set; }
        public string? SensorFrame { get; set; }
        public string? Ltpu { get; set; }
        public string? LtpuMult { get; set; }
        public string? LtpuAmps { get; set; }
        public string? LtdBand { get; set; }
        public string? LtdCurve { get; set; }
        public string? TripAdjust { get; set; }
        public string? TripPickup { get; set; }
        public string? Stpu { get; set; }
        public string? StpuAmps { get; set; }
        public string? StdBand { get; set; }
        public string? StpuI2t { get; set; }
        public string? StdI2t { get; set; }
        public string? Inst { get; set; }
        public string? InstOverride { get; set; }
        public string? InstAmps { get; set; }
        public string? InstOverridePickupAmps { get; set; }
        public string? Maint { get; set; }
        public string? MaintSetting { get; set; }
        public string? MaintAmps { get; set; }
        public string? GndSensor { get; set; }
        public string? Gfpu { get; set; }
        public string? GfpuAmps { get; set; }
        public string? Gfd { get; set; }
        public string? GfdI2t { get; set; }
        public string? GndMaintPickup { get; set; }
        public string? GndMaintAmps { get; set; }
        public string? Comments { get; set; }

        public TripUnit() { }

        public TripUnit(
            string? manufacturer,
            bool adjustable,
            string? type,
            string? style,
            string? tripPlug,
            string? ltpu,
            string? ltpuAmps,
            string? ltdBand,
            string? ltdCurve,
            string? stpu,
            string? stpuAmps,
            string? stdBand,
            string? stdI2t,
            string? inst,
            string? instAmps,
            string? maint,
            string? maintAmps,
            string? gfpu,
            string? gfpuAmps,
            string? gfd,
            string? gfdI2t,
            string? comments)
        {
            Manufacturer = manufacturer;
            Adjustable = adjustable;
            Type = type;
            Style = style;
            TripPlug = tripPlug;
            Ltpu = ltpu;
            LtpuAmps = ltpuAmps;
            LtdBand = ltdBand;
            LtdCurve = ltdCurve;
            Stpu = stpu;
            StpuAmps = stpuAmps;
            StdBand = stdBand;
            StdI2t = stdI2t;
            Inst = inst;
            InstAmps = instAmps;
            Maint = maint;
            MaintAmps = maintAmps;
            Gfpu = gfpu;
            GfpuAmps = gfpuAmps;
            Gfd = gfd;
            GfdI2t = gfdI2t;
            Comments = comments;
        }

        public override string ToString()
            => $"Manufacturer: {Manufacturer}, Adjustable: {Adjustable}, Type: {Type}, Style: {Style}, TripPlug: {TripPlug}, Ltpu: {Ltpu}, Stpu: {Stpu}, Inst: {Inst}, Maint: {Maint}, Gfpu: {Gfpu}, Gfd: {Gfd}";

        public string LtpuToString() => $"Ltpu: {Ltpu}, LtpuAmps: {LtpuAmps}, LtdBand: {LtdBand}, LtdCurve: {LtdCurve}";
        public string StpuToString() => $"Stpu: {Stpu}, StpuAmps: {StpuAmps}";
        public string StdToString() => $"StdBand: {StdBand}, StdI2t: {StdI2t}, StpuI2t: {StpuI2t}";
        public string InstToString() => $"Inst: {Inst}, InstAmps: {InstAmps}, InstOvr: {InstOverride}, InstOvrAmps: {InstOverridePickupAmps}";
        public string MaintToString() => $"Maint: {Maint}, MaintSetting: {MaintSetting}, MaintAmps: {MaintAmps}";
        public string GfpuToString() => $"Gfpu: {Gfpu}, GfpuAmps: {GfpuAmps}, GndMaintPickup: {GndMaintPickup}, GndMaintAmps: {GndMaintAmps}";
        public string GfdToString() => $"Gfd: {Gfd}, GfdI2t: {GfdI2t}";

        /// <summary>
        /// Heuristic adjustable inference (pre-legacy logic): looks for populated adjustable setting fields (excluding always-present / fixed indicators).
        /// Ignores LtpuAmps as it is typically always present.
        /// </summary>
        public void InferAdjustableIfUnset()
        {
            if (Adjustable) return;
            bool HasAdj(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return false;
                var t = s.Trim();
                if (t.Length == 0) return false;
                if (string.Equals(t, "fixed", StringComparison.OrdinalIgnoreCase)) return false;
                return true;
            }
            bool adjustableSignal =
                HasAdj(LtpuMult) || HasAdj(LtdBand) || HasAdj(LtdCurve) ||
                HasAdj(Stpu) || HasAdj(StpuAmps) || HasAdj(StdBand) || HasAdj(StdI2t) || HasAdj(StpuI2t) ||
                (HasAdj(Inst) && !string.Equals(Inst?.Trim(), "Fixed", StringComparison.OrdinalIgnoreCase)) || HasAdj(InstAmps) ||
                HasAdj(Maint) || HasAdj(MaintAmps) || HasAdj(MaintSetting) ||
                HasAdj(Gfpu) || HasAdj(GfpuAmps) || HasAdj(Gfd) || HasAdj(GfdI2t) ||
                HasAdj(TripAdjust);
            if (adjustableSignal) Adjustable = true;
        }

        public List<PropertyChange> Diff(TripUnit? newer)
        {
            var changes = new List<PropertyChange>();
            if (!string.Equals(Manufacturer, newer?.Manufacturer, StringComparison.Ordinal)) changes.Add(new PropertyChange { PropertyPath = "Manufacturer", OldValue = Manufacturer, NewValue = newer?.Manufacturer, ChangeType = ChangeType.Modified });
            if (Adjustable != (newer?.Adjustable ?? false)) changes.Add(new PropertyChange { PropertyPath = "Adjustable", OldValue = Adjustable.ToString(), NewValue = (newer?.Adjustable ?? false).ToString(), ChangeType = ChangeType.Modified });
            SimplePropDiff(changes, "Type", Type, newer?.Type);
            SimplePropDiff(changes, "Style", Style, newer?.Style);
            SimplePropDiff(changes, "TripPlug", TripPlug, newer?.TripPlug);
            SimplePropDiff(changes, "SensorFrame", SensorFrame, newer?.SensorFrame);
            SimplePropDiff(changes, "Ltpu", Ltpu, newer?.Ltpu);
            SimplePropDiff(changes, "LtpuMult", LtpuMult, newer?.LtpuMult);
            SimplePropDiff(changes, "LtpuAmps", LtpuAmps, newer?.LtpuAmps);
            SimplePropDiff(changes, "LtdBand", LtdBand, newer?.LtdBand);
            SimplePropDiff(changes, "LtdCurve", LtdCurve, newer?.LtdCurve);
            SimplePropDiff(changes, "TripAdjust", TripAdjust, newer?.TripAdjust);
            SimplePropDiff(changes, "TripPickup", TripPickup, newer?.TripPickup);
            SimplePropDiff(changes, "Stpu", Stpu, newer?.Stpu);
            SimplePropDiff(changes, "StpuAmps", StpuAmps, newer?.StpuAmps);
            SimplePropDiff(changes, "StdBand", StdBand, newer?.StdBand);
            SimplePropDiff(changes, "StdI2t", StdI2t, newer?.StdI2t);
            SimplePropDiff(changes, "StpuI2t", StpuI2t, newer?.StpuI2t);
            SimplePropDiff(changes, "Inst", Inst, newer?.Inst);
            SimplePropDiff(changes, "InstOverride", InstOverride, newer?.InstOverride);
            SimplePropDiff(changes, "InstAmps", InstAmps, newer?.InstAmps);
            SimplePropDiff(changes, "InstOverridePickupAmps", InstOverridePickupAmps, newer?.InstOverridePickupAmps);
            SimplePropDiff(changes, "Maint", Maint, newer?.Maint);
            SimplePropDiff(changes, "MaintSetting", MaintSetting, newer?.MaintSetting);
            SimplePropDiff(changes, "MaintAmps", MaintAmps, newer?.MaintAmps);
            SimplePropDiff(changes, "GndSensor", GndSensor, newer?.GndSensor);
            SimplePropDiff(changes, "Gfpu", Gfpu, newer?.Gfpu);
            SimplePropDiff(changes, "GfpuAmps", GfpuAmps, newer?.GfpuAmps);
            SimplePropDiff(changes, "Gfd", Gfd, newer?.Gfd);
            SimplePropDiff(changes, "GfdI2t", GfdI2t, newer?.GfdI2t);
            SimplePropDiff(changes, "GndMaintPickup", GndMaintPickup, newer?.GndMaintPickup);
            SimplePropDiff(changes, "GndMaintAmps", GndMaintAmps, newer?.GndMaintAmps);
            SimplePropDiff(changes, "Comments", Comments, newer?.Comments);
            return changes;
        }

        private void SimplePropDiff(List<PropertyChange> list, string path, string? oldVal, string? newVal)
        {
            if (!string.Equals(oldVal, newVal, StringComparison.Ordinal))
                list.Add(new PropertyChange { PropertyPath = path, OldValue = oldVal, NewValue = newVal, ChangeType = ChangeType.Modified });
        }

        private bool TryParseDouble(string? s, out double val)
        {
            val = 0.0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var cleaned = s.Replace("A", "").Replace("kA", "").Replace("%", "").Trim();
            return double.TryParse(cleaned, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out val);
        }
    }
}
