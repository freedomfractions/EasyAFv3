using System;

namespace EasyAF.Shell.Models.Backstage;

/// <summary>
/// Represents a recent file entry in the Open backstage.
/// </summary>
public class RecentFileEntry
{
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
    public bool IsPinned { get; set; }
}
