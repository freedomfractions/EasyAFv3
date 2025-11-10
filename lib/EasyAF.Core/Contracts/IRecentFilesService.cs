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
}
