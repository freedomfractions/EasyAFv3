using System.Collections.ObjectModel;
using EasyAF.Core.Contracts;
using Serilog;
using System.IO;

namespace EasyAF.Core.Services;

/// <summary>
/// Tracks recently accessed document file paths with automatic persistence via ISettingsService.
/// </summary>
/// <remarks>
/// <para>
/// This service maintains an ordered list of file paths (most recent first) and automatically
/// persists changes to settings storage. The list is bounded by a configurable maximum count.
/// </para>
/// <para>
/// Settings keys used:
/// - "RecentFiles": List of file paths
/// - "RecentFiles.MaxCount": Maximum number of entries (clamped to 1-100, default 15)
/// </para>
/// <para>
/// The service responds to settings hot-reload events and will trim the list if MaxCount is reduced.
/// </para>
/// </remarks>
public class RecentFilesService : IRecentFilesService
{
    private readonly ISettingsService _settingsService;
    private const string SettingKeyList = "RecentFiles";
    private const string SettingKeyMax = "RecentFiles.MaxCount";
    private int _maxEntries;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecentFilesService"/> class.
    /// </summary>
    /// <param name="settingsService">Settings service for persistence.</param>
    /// <remarks>
    /// Loads existing recent files from settings on construction and subscribes to settings reload events.
    /// </remarks>
    public RecentFilesService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _maxEntries = ClampMax(_settingsService.GetSetting<int>(SettingKeyMax, 15));
        // Write back clamped default so it persists
        _settingsService.SetSetting(SettingKeyMax, _maxEntries);

        var existing = _settingsService.GetSetting<List<string>>(SettingKeyList, new List<string>());
        RecentFiles = new ObservableCollection<string>(existing.Take(_maxEntries));

        // React to settings reloads
        _settingsService.SettingsReloaded += (_, __) =>
        {
            _maxEntries = ClampMax(_settingsService.GetSetting<int>(SettingKeyMax, _maxEntries));
            TrimToMax();
        };
    }

    /// <inheritdoc/>
    public ObservableCollection<string> RecentFiles { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// File paths are normalized via <see cref="Path.GetFullPath"/> before storage.
    /// If the path already exists in the list, it is moved to the top (position 0).
    /// </para>
    /// <para>
    /// The list is automatically trimmed to MaxCount after insertion and persisted to settings.
    /// </para>
    /// </remarks>
    public void AddRecentFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        var normalized = Path.GetFullPath(path);
        if (RecentFiles.Contains(normalized))
        {
            // Move to top
            RecentFiles.Remove(normalized);
        }
        RecentFiles.Insert(0, normalized);
        TrimToMax();
        Persist();
        Log.Debug("Recent file added: {Path}", normalized);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// File path is normalized before removal. If the path doesn't exist, no action is taken.
    /// Changes are persisted immediately.
    /// </remarks>
    public void RemoveRecentFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var normalized = Path.GetFullPath(path);
            if (RecentFiles.Contains(normalized))
            {
                RecentFiles.Remove(normalized);
                Persist();
                Log.Debug("Recent file removed: {Path}", normalized);
            }
        }
        catch
        {
            // Silently ignore invalid paths
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        RecentFiles.Clear();
        Persist();
        Log.Debug("Recent files cleared");
    }

    /// <summary>
    /// Trims the recent files list to the configured maximum count.
    /// </summary>
    /// <remarks>
    /// Removes entries from the end of the list (oldest files) until the count is within bounds.
    /// </remarks>
    private void TrimToMax()
    {
        while (RecentFiles.Count > _maxEntries)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }

    /// <summary>
    /// Persists the current recent files list and maximum count to settings storage.
    /// </summary>
    private void Persist()
    {
        _settingsService.SetSetting(SettingKeyList, RecentFiles.ToList());
        _settingsService.SetSetting(SettingKeyMax, _maxEntries);
    }

    /// <summary>
    /// Clamps the maximum entry count to valid range (1-100).
    /// </summary>
    /// <param name="value">Desired maximum count.</param>
    /// <returns>Clamped value between 1 and 100 inclusive.</returns>
    private static int ClampMax(int value)
    {
        if (value < 1) return 1;
        if (value > 100) return 100;
        return value;
    }
}
