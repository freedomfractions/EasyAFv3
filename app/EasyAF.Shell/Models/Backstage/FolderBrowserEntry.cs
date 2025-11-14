using System;
using System.IO;
using Prism.Mvvm;

namespace EasyAF.Shell.Models.Backstage;

/// <summary>
/// Represents a file or folder in the folder browser.
/// Unified model that can represent both files and subdirectories.
/// </summary>
public class FolderBrowserEntry : BindableBase
{
    private string _fullPath = string.Empty;
    private bool _isFolder;
    private long _fileSize;
    private DateTime _lastModified;

    /// <summary>
    /// Gets or sets the full path to the file or folder.
    /// </summary>
    public string FullPath
    {
        get => _fullPath;
        set => SetProperty(ref _fullPath, value);
    }

    /// <summary>
    /// Gets or sets whether this entry is a folder (true) or file (false).
    /// </summary>
    public bool IsFolder
    {
        get => _isFolder;
        set
        {
            if (SetProperty(ref _isFolder, value))
            {
                RaisePropertyChanged(nameof(Icon));
                RaisePropertyChanged(nameof(FileSizeDisplay));
            }
        }
    }

    /// <summary>
    /// Gets the name of the file or folder (without path).
    /// </summary>
    public string Name => IsFolder 
        ? new DirectoryInfo(FullPath).Name 
        : Path.GetFileName(FullPath);

    /// <summary>
    /// Gets the file extension (empty for folders).
    /// </summary>
    public string Extension => IsFolder ? string.Empty : Path.GetExtension(FullPath);

    /// <summary>
    /// Gets the directory path containing this file or folder.
    /// For files: returns the parent directory path
    /// For folders: returns the parent directory path
    /// </summary>
    public string DirectoryPath
    {
        get
        {
            try
            {
                if (IsFolder)
                {
                    var dirInfo = new DirectoryInfo(FullPath);
                    return dirInfo.Parent?.FullName ?? string.Empty;
                }
                else
                {
                    return Path.GetDirectoryName(FullPath) ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets the appropriate Segoe MDL2 icon for this entry.
    /// </summary>
    public string Icon => IsFolder ? "\uE8B7" : "\uE8A5"; // Folder or Document icon

    /// <summary>
    /// Gets or sets the file size in bytes (0 for folders).
    /// </summary>
    public long FileSize
    {
        get => _fileSize;
        set
        {
            if (SetProperty(ref _fileSize, value))
            {
                RaisePropertyChanged(nameof(FileSizeDisplay));
            }
        }
    }

    /// <summary>
    /// Gets a friendly display string for file size (empty for folders).
    /// </summary>
    public string FileSizeDisplay
    {
        get
        {
            if (IsFolder)
                return string.Empty;

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
                return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";
            if (LastModified.Year == now.Year)
                return LastModified.ToString("MMM d");
            
            return LastModified.ToString("MMM d, yyyy");
        }
    }
}
