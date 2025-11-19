namespace EasyAF.Modules.Map.Models;

/// <summary>
/// Represents the mapping completion status for a data type.
/// </summary>
public enum MappingStatus
{
    /// <summary>
    /// No properties have been mapped yet.
    /// </summary>
    Unmapped,
    
    /// <summary>
    /// Some properties are mapped, but not all visible properties.
    /// </summary>
    Partial,
    
    /// <summary>
    /// All visible properties have been mapped.
    /// </summary>
    Complete
}
