using System.Collections.ObjectModel;
using Serilog.Core;
using Serilog.Events;

namespace EasyAF.Core.Logging;

/// <summary>
/// Serilog sink that captures log events into an observable collection for UI display.
/// </summary>
public class InMemoryLogSink : ILogEventSink
{
    private readonly ObservableCollection<LogEntry> _logEntries;
    private readonly int _maxEntries;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryLogSink"/> class.
    /// </summary>
    /// <param name="logEntries">The observable collection to populate with log entries.</param>
    /// <param name="maxEntries">Maximum number of entries to keep in memory (default: 1000).</param>
    public InMemoryLogSink(ObservableCollection<LogEntry> logEntries, int maxEntries = 1000)
    {
        _logEntries = logEntries ?? throw new ArgumentNullException(nameof(logEntries));
        _maxEntries = maxEntries;
    }

    /// <inheritdoc/>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null)
            return;

        var entry = new LogEntry
        {
            Timestamp = logEvent.Timestamp,
            Level = logEvent.Level,
            Message = logEvent.RenderMessage(),
            Exception = logEvent.Exception
        };

        // Extract source context if available
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            entry.SourceContext = sourceContext.ToString().Trim('"');
        }

        // Add to collection on UI thread if needed
        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == false)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => AddEntry(entry));
        }
        else
        {
            AddEntry(entry);
        }
    }

    private void AddEntry(LogEntry entry)
    {
        lock (_lock)
        {
            _logEntries.Add(entry);

            // Trim old entries if we exceed max
            while (_logEntries.Count > _maxEntries)
            {
                _logEntries.RemoveAt(0);
            }
        }
    }
}
