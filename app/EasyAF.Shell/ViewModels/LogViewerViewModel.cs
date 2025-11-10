using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Logging;
using System.Windows.Input;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for the log viewer control.
/// </summary>
public class LogViewerViewModel : BindableBase
{
    /// <summary>
    /// Gets the observable collection of log entries.
    /// </summary>
    public ObservableCollection<LogEntry> LogEntries { get; }

    /// <summary>
    /// Gets the command to clear the log.
    /// </summary>
    public ICommand ClearLogCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerViewModel"/> class.
    /// </summary>
    /// <param name="logEntries">The shared log entries collection.</param>
    public LogViewerViewModel(ObservableCollection<LogEntry> logEntries)
    {
        LogEntries = logEntries ?? throw new ArgumentNullException(nameof(logEntries));
        ClearLogCommand = new DelegateCommand(ClearLog);
    }

    private void ClearLog()
    {
        LogEntries.Clear();
    }
}
