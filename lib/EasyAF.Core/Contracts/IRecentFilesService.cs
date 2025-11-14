using System.Collections.ObjectModel;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Provides tracking for recently accessed document file paths with persistence.
/// </summary>
public interface IRecentFilesService
{
    /// <summary>
    /// Gets the ordered collection of recent file paths (most recent first).
    /// </summary>
    ObservableCollection<string> RecentFiles { get; }

    /// <summary>
    /// Adds or moves a file path to the top of the recent list.
    /// </summary>
    /// <param name="path">Full file path.</param>
    void AddRecentFile(string path);

    // CROSS-MODULE EDIT: 2025-11-14 Backstage Open UX
    // Modified for: Allow removing an individual recent file from the list via Backstage Open context/actions
    // Related modules: Shell (FileCommandsViewModel), Core (RecentFilesService)
    // Rollback instructions: Remove RemoveRecentFile method signature; update RecentFilesService and any Shell bindings to stop calling it
    /// <summary>
    /// Removes a file path from the recent list if present.
    /// </summary>
    /// <param name="path">Full file path.</param>
    void RemoveRecentFile(string path);

    // CROSS-MODULE EDIT: 2025-11-14 Backstage Open UX
    // Modified for: Support clearing the entire recent files list from Open backstage
    // Related modules: Shell (FileCommandsViewModel), Core (RecentFilesService)
    // Rollback instructions: Remove Clear method signature and associated calls
    /// <summary>
    /// Clears all recent files and persists the empty list.
    /// </summary>
    void Clear();
}
