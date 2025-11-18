using System;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents a cable with its electrical, physical, and installation properties.
    /// </summary>
    public class Cable
    {
        /// <summary>Unique identifier for the cable.</summary>
        public string? Id { get; set; }
        /// <summary>Source connection of the cable.</summary>
        public string? Source { get; set; }
        /// <summary>Destination connection of the cable.</summary>
        public string? Destination { get; set; }
        /// <summary>Length of the cable.</summary>
        public string? Length { get; set; }
        /// <summary>Voltage rating of the cable.</summary>
        public string? Voltage { get; set; }
        /// <summary>Ampacity of the cable.</summary>
        public string? Ampacity { get; set; }
        /// <summary>Size of the phase conductor.</summary>
        public string? PhaseConductorSize { get; set; }
        /// <summary>Count of phase conductors.</summary>
        public string? PhaseConductorCount { get; set; }
        /// <summary>Size of the neutral conductor.</summary>
        public string? NeutralConductorSize { get; set; }
        /// <summary>Count of neutral conductors.</summary>
        public string? NeutralConductorCount { get; set; }
        /// <summary>Size of the ground conductor.</summary>
        public string? GroundConductorSize { get; set; }
        /// <summary>Count of ground conductors.</summary>
        public string? GroundConductorCount { get; set; }
        /// <summary>Type of insulation used.</summary>
        public string? Insulation { get; set; }
        /// <summary>Material of the conductor.</summary>
        public string? ConductorMaterial { get; set; }
        /// <summary>Material of the raceway.</summary>
        public string? RacewayMaterial { get; set; }
        /// <summary>Type of the raceway.</summary>
        public string? RacewayType { get; set; }
        /// <summary>Size of the conduit.</summary>
        public string? ConduitSize { get; set; }
        /// <summary>Count of conduits.</summary>
        public string? ConduitCount { get; set; }
        /// <summary>Additional comments or notes.</summary>
        public string? Comments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cable"/> class.
        /// </summary>
        public Cable() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cable"/> class with all properties.
        /// </summary>
        public Cable(
            string? id,
            string? source,
            string? destination,
            string? length,
            string? voltage,
            string? ampacity,
            string? phaseConductorSize,
            string? phaseConductorCount,
            string? neutralConductorSize,
            string? neutralConductorCount,
            string? groundConductorSize,
            string? groundConductorCount,
            string? insulation,
            string? conductorMaterial,
            string? racewayMaterial,
            string? racewayType,
            string? conduitSize,
            string? conduitCount,
            string? comments
        )
        {
            Id = id;
            Source = source;
            Destination = destination;
            Length = length;
            Voltage = voltage;
            Ampacity = ampacity;
            PhaseConductorSize = phaseConductorSize;
            PhaseConductorCount = phaseConductorCount;
            NeutralConductorSize = neutralConductorSize;
            NeutralConductorCount = neutralConductorCount;
            GroundConductorSize = groundConductorSize;
            GroundConductorCount = groundConductorCount;
            Insulation = insulation;
            ConductorMaterial = conductorMaterial;
            RacewayMaterial = racewayMaterial;
            RacewayType = racewayType;
            ConduitSize = conduitSize;
            ConduitCount = conduitCount;
            Comments = comments;
        }

        /// <summary>
        /// Returns a summary string of the cable's main properties.
        /// </summary>
        public override string ToString()
        {
            return $"Id: {Id}, Source: {Source}, Destination: {Destination}, Length: {Length}, " +
                   $"Voltage: {Voltage}, Ampacity: {Ampacity}, PhaseConductorSize: {PhaseConductorSize}, " +
                   $"PhaseConductorCount: {PhaseConductorCount}, NeutralConductorSize: {NeutralConductorSize}, " +
                   $"NeutralConductorCount: {NeutralConductorCount}, GroundConductorSize: {GroundConductorSize}, GroundConductorCount: {GroundConductorCount}, Insulation: {Insulation}, " +
                   $"ConductorMaterial: {ConductorMaterial}, RacewayMaterial: {RacewayMaterial}, " +
                   $"RacewayType: {RacewayType}, ConduitSize: {ConduitSize}, ConduitCount: {ConduitCount}, " +
                   $"Comments: {Comments}";
        }
    }
}
