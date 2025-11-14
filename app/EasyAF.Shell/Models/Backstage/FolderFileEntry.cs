using System;
using System.IO;
using Prism.Mvvm;

namespace EasyAF.Shell.Models.Backstage;

/// <summary>
/// Represents a file in a Quick Access folder for display in the file browser.
/// </summary>
public class FolderFileEntry : BindableBase
{
    private string _filePath = string.Empty;
    private long _fileSize;
    private DateTime _lastModified;

    /// <summary>
    /// Gets or sets the full file path.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    /// <summary>
    /// Gets the file name (without directory).
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public string Extension => Path.GetExtension(FilePath);

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize
    {
        get => _fileSize;
        set => SetProperty(ref _fileSize, value);
    }

    /// <summary>
    /// Gets a friendly display string for file size (e.g., "1.5 MB", "234 KB").
    /// </summary>
    public string FileSizeDisplay
    {
        get
        {
            if (FileSize < 1024)
                return $"{FileSize} bytes";
            else if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024.0:F1} KB";
            else if (FileSize < 1024 * 1024 * 1024)
                return $"{FileSize / (1024.0 * 1024.0):F1} MB";
            else
                return $"{FileSize / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime LastModified
    {
        get => _lastModified;
        set
        {
            if (SetProperty(ref _lastModified, value))
            {
                RaisePropertyChanged(nameof(LastModifiedDisplay));
            }
        }
    }

    /// <summary>
    /// Gets a friendly display string for the last modified date.
    /// </summary>
    public string LastModifiedDisplay
    {
        get
        {
            var now = DateTime.Now;
            var diff = now - LastModified;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} minutes ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            if (LastModified.Year == now.Year)
                return LastModified.ToString("MMM d");
            
            return LastModified.ToString("MMM d, yyyy");
        }
    }
}
