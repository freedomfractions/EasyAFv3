namespace EasyAF.Core.Services;

/// <summary>
/// Root settings container for the application.
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Gets or sets the global application settings.
    /// </summary>
    public Dictionary<string, object> Global { get; set; } = new();

    /// <summary>
    /// Gets or sets the module-specific settings.
    /// </summary>
    /// <remarks>
    /// Key: Module name, Value: Module settings dictionary.
    /// </remarks>
    public Dictionary<string, Dictionary<string, object>> Modules { get; set; } = new();
}
