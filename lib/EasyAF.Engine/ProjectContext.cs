using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Data.Models;

namespace EasyAF.Engine
{
    public class ProjectContext
    {
        public DataSet NewData { get; }
        public DataSet? OldData { get; }
        public ProjectContext(DataSet newData, DataSet? oldData = null) { NewData = newData; OldData = oldData; }

        public IEnumerable<LVCB> LVCBs() => NewData.LVCBEntries?.Values ?? Enumerable.Empty<LVCB>();
        public IEnumerable<LVCB> OldLVCBs() => OldData?.LVCBEntries?.Values ?? Enumerable.Empty<LVCB>();
        public IEnumerable<Fuse> Fuses() => NewData.FuseEntries?.Values ?? Enumerable.Empty<Fuse>();
        public IEnumerable<Fuse> OldFuses() => OldData?.FuseEntries?.Values ?? Enumerable.Empty<Fuse>();
        public IEnumerable<Cable> Cables() => NewData.CableEntries?.Values ?? Enumerable.Empty<Cable>();
        public IEnumerable<Cable> OldCables() => OldData?.CableEntries?.Values ?? Enumerable.Empty<Cable>();
        public IEnumerable<ArcFlash> ArcFlashEntries() => NewData.ArcFlashEntries?.Values ?? Enumerable.Empty<ArcFlash>();
        public IEnumerable<ShortCircuit> ShortCircuitEntries() => NewData.ShortCircuitEntries?.Values ?? Enumerable.Empty<ShortCircuit>();
        public IEnumerable<ShortCircuit> OldShortCircuitEntries() => OldData?.ShortCircuitEntries?.Values ?? Enumerable.Empty<ShortCircuit>();
        public IEnumerable<Bus> Buses() => NewData.BusEntries?.Values ?? Enumerable.Empty<Bus>();
        public IEnumerable<Bus> OldBuses() => OldData?.BusEntries?.Values ?? Enumerable.Empty<Bus>();

        // Helper formatting for breaker column described by user
        public string FormatBreakerSummary(LVCB lvcb)
        {
            if (lvcb == null) return string.Empty;
            var a = string.Join(' ', new[] { lvcb.Manufacturer, lvcb.Style }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var b = lvcb.TripUnit == null ? string.Empty : string.Join(' ', new[] { lvcb.TripUnit.Type, lvcb.TripUnit.Style }.Where(s => !string.IsNullOrWhiteSpace(s)));
            return string.IsNullOrWhiteSpace(b) ? a : a + "\n" + b;
        }
    }
}
