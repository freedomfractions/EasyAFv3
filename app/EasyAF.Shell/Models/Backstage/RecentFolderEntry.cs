using System;

namespace EasyAF.Shell.Models.Backstage;

/// <summary>
/// Represents a recent folder entry in the Open backstage.
/// </summary>
public class RecentFolderEntry
{
    /// <summary>
    /// Full folder path.
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Folder name (last segment of path).
    /// </summary>
    public string FolderName => System.IO.Path.GetFileName(FolderPath.TrimEnd('\\', '/')) ?? string.Empty;

    /// <summary>
    /// Parent directory path.
    /// </summary>
    public string ParentPath => System.IO.Path.GetDirectoryName(FolderPath) ?? string.Empty;

    /// <summary>
    /// Last accessed date/time.
    /// </summary>
    public DateTime LastAccessed { get; set; }

    /// <summary>
    /// Friendly display of last accessed.
    /// </summary>
    public string LastAccessedDisplay
    {
        get
        {
            var now = DateTime.Now;
            var diff = now - LastAccessed;

            if (diff.TotalHours < 24 && now.Date == LastAccessed.Date)
                return "Today";
            else if (diff.TotalHours < 48 && now.Date.AddDays(-1) == LastAccessed.Date)
                return "Yesterday";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            else
                return LastAccessed.ToShortDateString();
        }
    }
}
