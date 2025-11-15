using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasyAF.Shell.Models.Backstage;

/// <summary>
/// Represents a recent file entry in the Open backstage.
/// </summary>
public class RecentFileEntry : INotifyPropertyChanged
{
    private bool _isPinned;

    /// <summary>
    /// Full file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File name (extracted from FilePath).
    /// </summary>
    public string FileName => System.IO.Path.GetFileName(FilePath);

    /// <summary>
    /// Directory path (extracted from FilePath).
    /// </summary>
    public string DirectoryPath => System.IO.Path.GetDirectoryName(FilePath) ?? string.Empty;

    /// <summary>
    /// Last modified date/time.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Friendly display of last modified (e.g., "Today", "Yesterday", "2 days ago").
    /// </summary>
    public string LastModifiedDisplay
    {
        get
        {
            var now = DateTime.Now;
            var diff = now - LastModified;

            if (diff.TotalHours < 24 && now.Date == LastModified.Date)
                return "Today";
            else if (diff.TotalHours < 48 && now.Date.AddDays(-1) == LastModified.Date)
                return "Yesterday";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            else
                return LastModified.ToShortDateString();
        }
    }

    /// <summary>
    /// Whether this file is pinned (starred).
    /// </summary>
    public bool IsPinned
    {
        get => _isPinned;
        set
        {
            if (_isPinned != value)
            {
                _isPinned = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateCategory)); // Notify that category changed
                OnPropertyChanged(nameof(DateCategorySortPriority)); // Notify that sort priority changed
            }
        }
    }

    /// <summary>
    /// Category for grouping files by date (Pinned, Today, Yesterday, This Week, Last Week, Older).
    /// </summary>
    public string DateCategory
    {
        get
        {
            if (IsPinned)
                return "Pinned";

            var now = DateTime.Now;
            var diff = now - LastModified;

            if (now.Date == LastModified.Date)
                return "Today";

            if (now.Date.AddDays(-1) == LastModified.Date)
                return "Yesterday";

            if (diff.TotalDays < 7)
                return "This Week";

            if (diff.TotalDays < 14)
                return "Last Week";

            return "Older";
        }
    }

    /// <summary>
    /// Sort priority for the DateCategory to ensure proper group ordering.
    /// Lower numbers appear first.
    /// </summary>
    public int DateCategorySortPriority
    {
        get
        {
            return DateCategory switch
            {
                "Pinned" => 0,
                "Today" => 1,
                "Yesterday" => 2,
                "This Week" => 3,
                "Last Week" => 4,
                "Older" => 5,
                _ => 99
            };
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
