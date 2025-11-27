using System.Collections.ObjectModel;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Provides tracking for recently accessed folder paths with persistence.
/// </summary>
/// <remarks>
/// <para>
/// Similar to IRecentFilesService, this service maintains an ordered list of
/// folder paths (most recent first) and automatically persists changes via settings.
/// </para>
/// <para>
/// Folders are tracked whenever files are opened, ensuring users can quickly
/// navigate back to frequently-used directories.
/// </para>
/// <para>
/// Settings keys used:
/// - "RecentFolders": List of folder paths (up to 100 stored)
/// - "RecentFolders.MaxCount": Maximum number of entries to DISPLAY (clamped to 3-50, default 10)
/// </para>
/// </remarks>
public interface IRecentFoldersService
{
    /// <summary>
    /// Gets the ordered collection of recent folder paths (most recent first).
    /// </summary>
    /// <remarks>
    /// This collection contains ALL stored recent folders (up to 100).
    /// Use <see cref="MaxDisplayCount"/> to determine how many items to display in the UI.
    /// </remarks>
    ObservableCollection<string> RecentFolders { get; }
    
    /// <summary>
    /// Gets the maximum number of items to display in the UI (user-configurable, 3-50).
    /// </summary>
    int MaxDisplayCount { get; }

    /// <summary>
    /// Adds or moves a folder path to the top of the recent list.
    /// </summary>
    /// <param name="path">Full folder path.</param>
    void AddRecentFolder(string path);

    /// <summary>
    /// Removes a folder path from the recent list if present.
    /// </summary>
    /// <param name="path">Full folder path.</param>
    void RemoveRecentFolder(string path);

    /// <summary>
    /// Clears all recent folders and persists the empty list.
    /// </summary>
    void Clear();
}
