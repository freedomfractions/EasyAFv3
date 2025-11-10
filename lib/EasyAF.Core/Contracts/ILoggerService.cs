namespace EasyAF.Core.Contracts;

/// <summary>
/// Service for application-wide logging with structured logging support.
/// </summary>
/// <remarks>
/// Provides a wrapper around Serilog for modules to use without direct dependency.
/// All log messages are enriched with context information automatically.
/// </remarks>
public interface ILoggerService
{
    /// <summary>
    /// Writes a verbose/trace message to the log.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Verbose(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Writes a debug message to the log.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Debug(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Information(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Writes a warning message to the log.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Warning(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Writes an error message to the log.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Error(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Writes an error message with exception details to the log.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Error(Exception exception, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Writes a fatal error message to the log.
    /// </summary>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Fatal(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Writes a fatal error message with exception details to the log.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">Message template with optional placeholders.</param>
    /// <param name="propertyValues">Values to fill into the message template.</param>
    void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Creates a context-enriched logger for a specific module or component.
    /// </summary>
    /// <param name="contextName">The name of the context (e.g., module name, class name).</param>
    /// <returns>A logger service enriched with the specified context.</returns>
    ILoggerService ForContext(string contextName);
}
