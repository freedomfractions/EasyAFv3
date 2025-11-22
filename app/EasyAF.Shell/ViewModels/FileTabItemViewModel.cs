using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// View model for an individual file tab in the vertical tab strip.
/// </summary>
/// <remarks>
/// <para>
/// Represents a single open document in the vertical file tab list.
/// Tracks file state (dirty/saved/clean) with visual indicators:
/// - Red bar: File has unsaved changes
/// - Green bar: File just saved (fades to blue after 5 seconds)
/// - Blue bar: File is clean and unmodified
/// </para>
/// <para>
/// Provides file metadata display:
/// - File name (short)
/// - Directory path (truncated with ellipses if needed)
/// - Last saved timestamp (relative, e.g., "3 minutes ago")
/// </para>
/// </remarks>
public class FileTabItemViewModel : BindableBase
{
    private readonly IDocument _document;
    private readonly DispatcherTimer _savedIndicatorTimer;
    private bool _isActive;
    private bool _isHovered;
    private DateTime _lastSaved;
    private string _statusColor = "#0078D4"; // Blue
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileTabItemViewModel"/> class.
    /// </summary>
    /// <param name="document">The document this tab represents.</param>
    /// <param name="closeCommand">Command to execute when close button is clicked.</param>
    public FileTabItemViewModel(IDocument document, ICommand closeCommand)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        CloseCommand = closeCommand;
        
        // Watch for dirty state changes
        if (_document is System.ComponentModel.INotifyPropertyChanged notifyDoc)
        {
            notifyDoc.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IDocument.IsDirty))
                {
                    UpdateStatusColor();
                    RaisePropertyChanged(nameof(StatusBarColor));
                }
            };
        }
        
        // Timer for "just saved" green indicator fade to blue
        _savedIndicatorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _savedIndicatorTimer.Tick += (_, __) =>
        {
            _savedIndicatorTimer.Stop();
            if (!_document.IsDirty)
            {
                StatusColor = "#0078D4"; // Fade back to blue
            }
        };
        
        UpdateStatusColor();
    }
    
    /// <summary>
    /// Gets the underlying document.
    /// </summary>
    public IDocument Document => _document;
    
    /// <summary>
    /// Gets the file name (without path).
    /// </summary>
    public string FileName =>
        !string.IsNullOrWhiteSpace(_document.FilePath)
            ? Path.GetFileName(_document.FilePath)
            : _document.Title;
    
    /// <summary>
    /// Gets the directory path, truncated with ellipses if too long.
    /// </summary>
    /// <remarks>
    /// Format: "C:\...\Projects\EasyPower\"
    /// Shows drive letter, ellipses, and last 2-3 folders.
    /// </remarks>
    public string DirectoryPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_document.FilePath))
                return "Not saved yet";
            
            try
            {
                var dir = Path.GetDirectoryName(_document.FilePath);
                if (string.IsNullOrWhiteSpace(dir))
                    return string.Empty;
                
                // Truncate if too long (e.g., > 40 characters)
                if (dir.Length > 40)
                {
                    var parts = dir.Split(Path.DirectorySeparatorChar);
                    if (parts.Length > 3)
                    {
                        // Show drive + "..." + last 2 folders
                        var drive = parts[0];
                        var last1 = parts[^1];
                        var last2 = parts[^2];
                        return $"{drive}{Path.DirectorySeparatorChar}...{Path.DirectorySeparatorChar}{last2}{Path.DirectorySeparatorChar}{last1}{Path.DirectorySeparatorChar}";
                    }
                }
                
                return dir + Path.DirectorySeparatorChar;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
    
    /// <summary>
    /// Gets the last saved indicator text.
    /// </summary>
    /// <remarks>
    /// Examples: "Just saved", "Saved 3 minutes ago", "Saved 2 hours ago"
    /// </remarks>
    public string LastSavedText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_document.FilePath))
                return "Never saved";
            
            if (_document.IsDirty)
                return "Unsaved changes";
            
            if (_lastSaved == default)
                return "Unknown";
            
            var elapsed = DateTime.Now - _lastSaved;
            
            if (elapsed.TotalSeconds < 30)
                return "Just saved";
            
            if (elapsed.TotalMinutes < 1)
                return "Saved moments ago";
            
            if (elapsed.TotalMinutes < 60)
                return $"Saved {(int)elapsed.TotalMinutes} minute{(elapsed.TotalMinutes >= 2 ? "s" : "")} ago";
            
            if (elapsed.TotalHours < 24)
                return $"Saved {(int)elapsed.TotalHours} hour{(elapsed.TotalHours >= 2 ? "s" : "")} ago";
            
            return $"Saved {(int)elapsed.TotalDays} day{(elapsed.TotalDays >= 2 ? "s" : "")} ago";
        }
    }
    
    /// <summary>
    /// Gets or sets whether this tab is the active (selected) tab.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
    
    /// <summary>
    /// Gets or sets whether the mouse is hovering over this tab.
    /// </summary>
    public bool IsHovered
    {
        get => _isHovered;
        set
        {
            SetProperty(ref _isHovered, value);
            RaisePropertyChanged(nameof(ShowCloseButton));
        }
    }
    
    /// <summary>
    /// Gets whether the close button should be visible.
    /// </summary>
    /// <remarks>
    /// Visible when hovered or active.
    /// </remarks>
    public bool ShowCloseButton => IsHovered || IsActive;
    
    /// <summary>
    /// Gets the status bar color based on file state.
    /// </summary>
    /// <remarks>
    /// - Red (#E81123): Dirty/unsaved
    /// - Green (#107C10): Just saved
    /// - Blue (#0078D4): Clean
    /// </remarks>
    public string StatusBarColor
    {
        get
        {
            if (_document.IsDirty)
                return "#E81123"; // Red
            
            return _statusColor; // Green (just saved) or Blue (clean)
        }
    }
    
    /// <summary>
    /// Gets or sets the internal status color (for green->blue fade).
    /// </summary>
    private string StatusColor
    {
        get => _statusColor;
        set
        {
            if (SetProperty(ref _statusColor, value))
            {
                RaisePropertyChanged(nameof(StatusBarColor));
            }
        }
    }
    
    /// <summary>
    /// Gets the module's file type icon.
    /// </summary>
    public System.Windows.Media.ImageSource? FileTypeIcon => _document.OwnerModule.ModuleIcon;
    
    /// <summary>
    /// Gets the command to close this tab.
    /// </summary>
    public ICommand CloseCommand { get; }
    
    /// <summary>
    /// Notifies that the file was just saved.
    /// </summary>
    /// <remarks>
    /// Triggers green indicator that fades to blue after 5 seconds.
    /// </remarks>
    public void NotifySaved()
    {
        _lastSaved = DateTime.Now;
        StatusColor = "#107C10"; // Green
        RaisePropertyChanged(nameof(LastSavedText));
        
        // Start fade timer
        _savedIndicatorTimer.Stop();
        _savedIndicatorTimer.Start();
    }
    
    /// <summary>
    /// Updates the last saved timestamp (for periodic refresh).
    /// </summary>
    public void RefreshTimestamp()
    {
        RaisePropertyChanged(nameof(LastSavedText));
    }
    
    private void UpdateStatusColor()
    {
        if (_document.IsDirty)
        {
            StatusColor = "#E81123"; // Red
            _savedIndicatorTimer.Stop();
        }
        else
        {
            // Don't override green if timer is running
            if (!_savedIndicatorTimer.IsEnabled)
            {
                StatusColor = "#0078D4"; // Blue
            }
        }
    }
}
