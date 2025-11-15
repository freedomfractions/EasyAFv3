using System.Collections.ObjectModel;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Provides tracking for recently accessed document file paths with persistence.
/// </summary>
/// <remarks>
/// <para>
/// The service maintains two separate concepts:
/// - **Storage limit**: Maximum number of items to store (1000 items, hardcoded)
/// - **Display limit**: Maximum number of items to show in UI (user-configurable, 3-250)
/// </para>
/// <para>
/// This separation allows users to temporarily reduce the display limit without
/// losing their history. For example, setting display limit to 5 will show only
/// 5 items in the UI, but all items remain stored. Changing back to 100 will
/// immediately show 100 items from the stored history.
/// </para>
/// </remarks>
public interface IRecentFilesService
{
    /// <summary>
    /// Gets the ordered collection of recent file paths (most recent first).
    /// </summary>
    /// <remarks>
    /// This collection contains ALL stored recent files (up to 1000).
    /// Use <see cref="MaxDisplayCount"/> to determine how many items to display in the UI.
    /// </remarks>
    ObservableCollection<string> RecentFiles { get; }
    
    /// <summary>
    /// Gets the maximum number of items to display in the UI (user-configurable, 3-250).
    /// </summary>
    /// <remarks>
    /// This is controlled by the "RecentFiles.MaxCount" setting.
    /// UI components should use this value to limit displayed items:
    /// <code>
    /// var displayedItems = recentFilesService.RecentFiles.Take(recentFilesService.MaxDisplayCount);
    /// </code>
    /// </remarks>
    int MaxDisplayCount { get; }

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
