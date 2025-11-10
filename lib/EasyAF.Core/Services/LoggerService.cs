using EasyAF.Core.Contracts;
using Serilog;
using Serilog.Core;

namespace EasyAF.Core.Services;

/// <summary>
/// Implementation of ILoggerService wrapping Serilog.
/// </summary>
public class LoggerService : ILoggerService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerService"/> class.
    /// </summary>
    public LoggerService()
    {
        _logger = Log.Logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerService"/> class with a specific logger.
    /// </summary>
    /// <param name="logger">The Serilog logger instance to wrap.</param>
    internal LoggerService(ILogger logger)
    {
        _logger = logger ?? Log.Logger;
    }

    /// <inheritdoc/>
    public void Verbose(string messageTemplate, params object[] propertyValues)
    {
        _logger.Verbose(messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        _logger.Debug(messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public void Information(string messageTemplate, params object[] propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        _logger.Warning(messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public void Error(string messageTemplate, params object[] propertyValues)
    {
        _logger.Error(messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        _logger.Error(exception, messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        _logger.Fatal(messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        _logger.Fatal(exception, messageTemplate, propertyValues);
    }

    /// <inheritdoc/>
    public ILoggerService ForContext(string contextName)
    {
        var contextLogger = _logger.ForContext("SourceContext", contextName);
        return new LoggerService(contextLogger);
    }
}
