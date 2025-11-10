using Serilog.Events;

namespace EasyAF.Core.Logging;

/// <summary>
/// Represents a single log entry for display in the UI.
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Gets or sets the timestamp of the log entry.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogEventLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the formatted log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exception information if present.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the source context (module/class name).
    /// </summary>
    public string? SourceContext { get; set; }

    /// <summary>
    /// Gets a display-friendly version of the log level.
    /// </summary>
    public string LevelDisplay => Level.ToString().ToUpperInvariant();

    /// <summary>
    /// Gets the theme-aware brush resource key for the log level color.
    /// </summary>
    /// <remarks>
    /// Returns a DynamicResource key that will automatically adapt to Light/Dark themes.
    /// The actual colors are defined in the theme resource dictionaries.
    /// </remarks>
    public string LevelBrushKey => Level switch
    {
        LogEventLevel.Verbose => "LogVerboseBrush",
        LogEventLevel.Debug => "LogDebugBrush",
        LogEventLevel.Information => "LogInformationBrush",
        LogEventLevel.Warning => "LogWarningBrush",
        LogEventLevel.Error => "LogErrorBrush",
        LogEventLevel.Fatal => "LogFatalBrush",
        _ => "LogInformationBrush"
    };
}
