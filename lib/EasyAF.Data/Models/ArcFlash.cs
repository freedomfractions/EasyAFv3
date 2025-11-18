using System;
using System.Collections.Generic;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents arc flash data for a scenario, including calculated incident energy and device timings.
    /// Extended with full property set from import specifications.
    /// </summary>
    public class ArcFlash
    {
        /// <summary>Arc Fault Bus Name (equipment / bus identifier).</summary>
        public string? Id { get; set; }
        /// <summary>Worst Case flag (e.g., "X").</summary>
        public string? WorstCase { get; set; }
        /// <summary>Scenario or case for the arc flash calculation.</summary>
        public string? Scenario { get; set; }
        /// <summary>Bus kV value (Arc Fault Bus kV).</summary>
        public string? BusKV { get; set; }
        /// <summary>Upstream trip device name.</summary>
        public string? UpstreamDevice { get; set; }
        /// <summary>Upstream trip device function.</summary>
        public string? UpstreamTripFunction { get; set; }
        /// <summary>Type of equipment (e.g., panel, switchgear).</summary>
        public string? EquipmentType { get; set; }
        /// <summary>Electrode configuration.</summary>
        public string? ElectrodeConfiguration { get; set; }
        /// <summary>Electrode gap (mm).</summary>
        public string? ElectrodeGapMM { get; set; }
        /// <summary>Bolted fault current (kA).</summary>
        public string? BoltedFaultKA { get; set; }
        /// <summary>Arc fault current (kA).</summary>
        public string? ArcFaultKA { get; set; }
        /// <summary>Trip time of the upstream device (sec).</summary>
        public string? TripTime { get; set; }
        /// <summary>Opening time of the upstream device (sec).</summary>
        public string? OpeningTime { get; set; }
        /// <summary>Total arc time (sec).</summary>
        public string? ArcTime { get; set; }
        /// <summary>Estimated arc flash boundary (inches).</summary>
        public string? ArcFlashBoundaryInches { get; set; }
        /// <summary>Working distance (inches).</summary>
        public string? WorkingDistance { get; set; }
        /// <summary>Calculated incident energy (cal/cm^2).</summary>
        public string? IncidentEnergy { get; set; }
        /// <summary>Additional comments or notes.</summary>
        public string? Comments { get; set; }

        public ArcFlash() { }

        public ArcFlash(
            string? id,
            string? scenario,
            string? upstreamDevice,
            string? equipmentType,
            string? boltedFaultKA,
            string? arcFaultKA,
            string? tripTime,
            string? openingTime,
            string? arcTime,
            string? workingDistance,
            string? incidentEnergy,
            string? comments)
        {
            Id = id;
            Scenario = scenario;
            UpstreamDevice = upstreamDevice;
            EquipmentType = equipmentType;
            BoltedFaultKA = boltedFaultKA;
            ArcFaultKA = arcFaultKA;
            TripTime = tripTime;
            OpeningTime = openingTime;
            ArcTime = arcTime;
            WorkingDistance = workingDistance;
            IncidentEnergy = incidentEnergy;
            Comments = comments;
        }

        public override string ToString()
        {
            return $"Id: {Id}, WorstCase: {WorstCase}, Scenario: {Scenario}, BusKV: {BusKV}, UpstreamDevice: {UpstreamDevice}, Fn: {UpstreamTripFunction}, Equip: {EquipmentType}, ElecCfg: {ElectrodeConfiguration}, GapMM: {ElectrodeGapMM}, BoltedKA: {BoltedFaultKA}, ArcKA: {ArcFaultKA}, Trip: {TripTime}, Open: {OpeningTime}, Arc: {ArcTime}, BdryIn: {ArcFlashBoundaryInches}, WD: {WorkingDistance}, IE: {IncidentEnergy}, Comments: {Comments}";
        }

        /// <summary>
        /// Determines if the calculated incident energy exceeds 40 cal/cm^2.
        /// </summary>
        /// <returns>True if incident energy is over 40 cal/cm^2, otherwise false.</returns>
        public bool IsOver40Cal()
        {
            if (double.TryParse(IncidentEnergy, out double energy))
                return energy > 40.0;
            return false;
        }

        /// <summary>
        /// Produces a list of property-level changes between this instance (old) and the provided new instance.
        /// </summary>
        public List<PropertyChange> Diff(ArcFlash? newer)
        {
            var changes = DiffUtil.DiffObjects(this, newer);
            var oldEnergy = IncidentEnergy; var newEnergy = newer?.IncidentEnergy;
            if (!string.Equals(oldEnergy, newEnergy))
            {
                if (double.TryParse(oldEnergy, out double oldE) && double.TryParse(newEnergy, out double newE))
                {
                    var oldOver = oldE > 40.0; var newOver = newE > 40.0;
                    if (oldOver != newOver)
                    {
                        changes.Add(new PropertyChange
                        {
                            PropertyPath = "IncidentEnergy.Over40",
                            OldValue = oldOver ? "true" : "false",
                            NewValue = newOver ? "true" : "false",
                            ChangeType = ChangeType.Modified
                        });
                    }
                }
            }
            return changes;
        }
    }
}
