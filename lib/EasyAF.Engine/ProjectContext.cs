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

        // === Original 6 Equipment Types ===
        public IEnumerable<LVBreaker> LVCBs() => NewData.LVBreakerEntries?.Values ?? Enumerable.Empty<LVBreaker>();
        public IEnumerable<LVBreaker> OldLVCBs() => OldData?.LVBreakerEntries?.Values ?? Enumerable.Empty<LVBreaker>();
        public IEnumerable<Fuse> Fuses() => NewData.FuseEntries?.Values ?? Enumerable.Empty<Fuse>();
        public IEnumerable<Fuse> OldFuses() => OldData?.FuseEntries?.Values ?? Enumerable.Empty<Fuse>();
        public IEnumerable<Cable> Cables() => NewData.CableEntries?.Values ?? Enumerable.Empty<Cable>();
        public IEnumerable<Cable> OldCables() => OldData?.CableEntries?.Values ?? Enumerable.Empty<Cable>();
        public IEnumerable<ArcFlash> ArcFlashEntries() => NewData.ArcFlashEntries?.Values ?? Enumerable.Empty<ArcFlash>();
        public IEnumerable<ShortCircuit> ShortCircuitEntries() => NewData.ShortCircuitEntries?.Values ?? Enumerable.Empty<ShortCircuit>();
        public IEnumerable<ShortCircuit> OldShortCircuitEntries() => OldData?.ShortCircuitEntries?.Values ?? Enumerable.Empty<ShortCircuit>();
        public IEnumerable<Bus> Buses() => NewData.BusEntries?.Values ?? Enumerable.Empty<Bus>();
        public IEnumerable<Bus> OldBuses() => OldData?.BusEntries?.Values ?? Enumerable.Empty<Bus>();

        // === Extended Equipment Types (Alphabetically) ===
        public IEnumerable<AFD> AFDs() => NewData.AFDEntries?.Values ?? Enumerable.Empty<AFD>();
        public IEnumerable<AFD> OldAFDs() => OldData?.AFDEntries?.Values ?? Enumerable.Empty<AFD>();
        
        public IEnumerable<ATS> ATSs() => NewData.ATSEntries?.Values ?? Enumerable.Empty<ATS>();
        public IEnumerable<ATS> OldATSs() => OldData?.ATSEntries?.Values ?? Enumerable.Empty<ATS>();
        
        public IEnumerable<Battery> Batteries() => NewData.BatteryEntries?.Values ?? Enumerable.Empty<Battery>();
        public IEnumerable<Battery> OldBatteries() => OldData?.BatteryEntries?.Values ?? Enumerable.Empty<Battery>();
        
        public IEnumerable<Busway> Busways() => NewData.BuswayEntries?.Values ?? Enumerable.Empty<Busway>();
        public IEnumerable<Busway> OldBusways() => OldData?.BuswayEntries?.Values ?? Enumerable.Empty<Busway>();
        
        public IEnumerable<Capacitor> Capacitors() => NewData.CapacitorEntries?.Values ?? Enumerable.Empty<Capacitor>();
        public IEnumerable<Capacitor> OldCapacitors() => OldData?.CapacitorEntries?.Values ?? Enumerable.Empty<Capacitor>();
        
        public IEnumerable<CLReactor> CLReactors() => NewData.CLReactorEntries?.Values ?? Enumerable.Empty<CLReactor>();
        public IEnumerable<CLReactor> OldCLReactors() => OldData?.CLReactorEntries?.Values ?? Enumerable.Empty<CLReactor>();
        
        public IEnumerable<CT> CTs() => NewData.CTEntries?.Values ?? Enumerable.Empty<CT>();
        public IEnumerable<CT> OldCTs() => OldData?.CTEntries?.Values ?? Enumerable.Empty<CT>();
        
        public IEnumerable<Filter> Filters() => NewData.FilterEntries?.Values ?? Enumerable.Empty<Filter>();
        public IEnumerable<Filter> OldFilters() => OldData?.FilterEntries?.Values ?? Enumerable.Empty<Filter>();
        
        public IEnumerable<Generator> Generators() => NewData.GeneratorEntries?.Values ?? Enumerable.Empty<Generator>();
        public IEnumerable<Generator> OldGenerators() => OldData?.GeneratorEntries?.Values ?? Enumerable.Empty<Generator>();
        
        public IEnumerable<HVBreaker> HVBreakers() => NewData.HVBreakerEntries?.Values ?? Enumerable.Empty<HVBreaker>();
        public IEnumerable<HVBreaker> OldHVBreakers() => OldData?.HVBreakerEntries?.Values ?? Enumerable.Empty<HVBreaker>();
        
        public IEnumerable<Inverter> Inverters() => NewData.InverterEntries?.Values ?? Enumerable.Empty<Inverter>();
        public IEnumerable<Inverter> OldInverters() => OldData?.InverterEntries?.Values ?? Enumerable.Empty<Inverter>();
        
        public IEnumerable<Load> Loads() => NewData.LoadEntries?.Values ?? Enumerable.Empty<Load>();
        public IEnumerable<Load> OldLoads() => OldData?.LoadEntries?.Values ?? Enumerable.Empty<Load>();
        
        public IEnumerable<MCC> MCCs() => NewData.MCCEntries?.Values ?? Enumerable.Empty<MCC>();
        public IEnumerable<MCC> OldMCCs() => OldData?.MCCEntries?.Values ?? Enumerable.Empty<MCC>();
        
        public IEnumerable<Meter> Meters() => NewData.MeterEntries?.Values ?? Enumerable.Empty<Meter>();
        public IEnumerable<Meter> OldMeters() => OldData?.MeterEntries?.Values ?? Enumerable.Empty<Meter>();
        
        public IEnumerable<Motor> Motors() => NewData.MotorEntries?.Values ?? Enumerable.Empty<Motor>();
        public IEnumerable<Motor> OldMotors() => OldData?.MotorEntries?.Values ?? Enumerable.Empty<Motor>();
        
        public IEnumerable<Panel> Panels() => NewData.PanelEntries?.Values ?? Enumerable.Empty<Panel>();
        public IEnumerable<Panel> OldPanels() => OldData?.PanelEntries?.Values ?? Enumerable.Empty<Panel>();
        
        public IEnumerable<Photovoltaic> Photovoltaics() => NewData.PhotovoltaicEntries?.Values ?? Enumerable.Empty<Photovoltaic>();
        public IEnumerable<Photovoltaic> OldPhotovoltaics() => OldData?.PhotovoltaicEntries?.Values ?? Enumerable.Empty<Photovoltaic>();
        
        public IEnumerable<POC> POCs() => NewData.POCEntries?.Values ?? Enumerable.Empty<POC>();
        public IEnumerable<POC> OldPOCs() => OldData?.POCEntries?.Values ?? Enumerable.Empty<POC>();
        
        public IEnumerable<Rectifier> Rectifiers() => NewData.RectifierEntries?.Values ?? Enumerable.Empty<Rectifier>();
        public IEnumerable<Rectifier> OldRectifiers() => OldData?.RectifierEntries?.Values ?? Enumerable.Empty<Rectifier>();
        
        public IEnumerable<Relay> Relays() => NewData.RelayEntries?.Values ?? Enumerable.Empty<Relay>();
        public IEnumerable<Relay> OldRelays() => OldData?.RelayEntries?.Values ?? Enumerable.Empty<Relay>();
        
        public IEnumerable<Shunt> Shunts() => NewData.ShuntEntries?.Values ?? Enumerable.Empty<Shunt>();
        public IEnumerable<Shunt> OldShunts() => OldData?.ShuntEntries?.Values ?? Enumerable.Empty<Shunt>();
        
        public IEnumerable<Switch> Switches() => NewData.SwitchEntries?.Values ?? Enumerable.Empty<Switch>();
        public IEnumerable<Switch> OldSwitches() => OldData?.SwitchEntries?.Values ?? Enumerable.Empty<Switch>();
        
        public IEnumerable<Transformer2W> Transformers2W() => NewData.Transformer2WEntries?.Values ?? Enumerable.Empty<Transformer2W>();
        public IEnumerable<Transformer2W> OldTransformers2W() => OldData?.Transformer2WEntries?.Values ?? Enumerable.Empty<Transformer2W>();
        
        public IEnumerable<Transformer3W> Transformers3W() => NewData.Transformer3WEntries?.Values ?? Enumerable.Empty<Transformer3W>();
        public IEnumerable<Transformer3W> OldTransformers3W() => OldData?.Transformer3WEntries?.Values ?? Enumerable.Empty<Transformer3W>();
        
        public IEnumerable<TransmissionLine> TransmissionLines() => NewData.TransmissionLineEntries?.Values ?? Enumerable.Empty<TransmissionLine>();
        public IEnumerable<TransmissionLine> OldTransmissionLines() => OldData?.TransmissionLineEntries?.Values ?? Enumerable.Empty<TransmissionLine>();
        
        public IEnumerable<UPS> UPSs() => NewData.UPSEntries?.Values ?? Enumerable.Empty<UPS>();
        public IEnumerable<UPS> OldUPSs() => OldData?.UPSEntries?.Values ?? Enumerable.Empty<UPS>();
        
        public IEnumerable<Utility> Utilities() => NewData.UtilityEntries?.Values ?? Enumerable.Empty<Utility>();
        public IEnumerable<Utility> OldUtilities() => OldData?.UtilityEntries?.Values ?? Enumerable.Empty<Utility>();
        
        public IEnumerable<ZigzagTransformer> ZigzagTransformers() => NewData.ZigzagTransformerEntries?.Values ?? Enumerable.Empty<ZigzagTransformer>();
        public IEnumerable<ZigzagTransformer> OldZigzagTransformers() => OldData?.ZigzagTransformerEntries?.Values ?? Enumerable.Empty<ZigzagTransformer>();

        // Helper formatting for breaker column described by user
        public string FormatBreakerSummary(LVBreaker lvBreaker)
        {
            if (lvBreaker == null) return string.Empty;
            var a = string.Join(' ', new[] { lvBreaker.BreakerMfr, lvBreaker.BreakerStyle }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var b = string.Join(' ', new[] { lvBreaker.TripType, lvBreaker.TripStyle }.Where(s => !string.IsNullOrWhiteSpace(s)));
            return string.IsNullOrWhiteSpace(b) ? a : a + "\n" + b;
        }
    }
}

