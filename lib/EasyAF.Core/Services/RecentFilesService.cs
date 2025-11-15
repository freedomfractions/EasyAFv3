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
/// persists changes to settings storage. The list is bounded by an absolute maximum (1000 items)
/// but the display limit is controlled separately via settings.
/// </para>
/// <para>
/// <strong>Storage vs Display Behavior:</strong>
/// The service stores up to 1000 recent file paths to preserve user history.
/// The RecentFiles.MaxCount setting controls how many items are DISPLAYED in the UI,
/// not how many are stored. This allows users to temporarily reduce the limit without
/// losing their history.
/// </para>
/// <para>
/// Settings keys used:
/// - "RecentFiles": List of file paths (up to 1000 stored)
/// - "RecentFiles.MaxCount": Maximum number of entries to DISPLAY (clamped to 3-250, default 25)
/// </para>
/// <para>
/// The service responds to settings hot-reload events. When MaxCount changes, the displayed
/// list adjusts immediately without deleting any stored items.
/// </para>
/// </remarks>
public class RecentFilesService : IRecentFilesService
{
    private readonly ISettingsService _settingsService;
    private const string SettingKeyList = "RecentFiles";
    private const string SettingKeyMax = "RecentFiles.MaxCount";
    private const int AbsoluteMaximum = 1000; // Maximum items to store (memory limit)
    private int _maxEntries;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecentFilesService"/> class.
    /// </summary>
    /// <param name="settingsService">Settings service for persistence.</param>
    /// <remarks>
    /// Loads existing recent files from settings on construction and subscribes to settings reload events.
    /// The service will store up to 1000 items, but only expose MaxCount items via RecentFiles collection.
    /// </remarks>
    public RecentFilesService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _maxEntries = ClampMax(_settingsService.GetSetting<int>(SettingKeyMax, 25));
        // Write back clamped default so it persists
        _settingsService.SetSetting(SettingKeyMax, _maxEntries);

        var existing = _settingsService.GetSetting<List<string>>(SettingKeyList, new List<string>());
        
        // Load all stored items (up to absolute maximum)
        RecentFiles = new ObservableCollection<string>(existing.Take(AbsoluteMaximum));

        // React to settings reloads (e.g., user changed MaxCount in Options dialog)
        _settingsService.SettingsReloaded += (_, __) =>
        {
            _maxEntries = ClampMax(_settingsService.GetSetting<int>(SettingKeyMax, _maxEntries));
            // Note: We do NOT trim the collection here - just update the display limit
            // UI components should bind to a filtered view if they want to respect MaxCount
            Log.Debug("Recent files display limit changed to: {MaxCount}", _maxEntries);
        };
    }

    /// <summary>
    /// Gets the ordered collection of recent file paths (most recent first).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This collection contains ALL stored recent files (up to 1000).
    /// To respect the user's display limit (RecentFiles.MaxCount setting),
    /// UI components should take only the first MaxCount items when displaying.
    /// </para>
    /// <para>
    /// Example in ViewModel:
    /// <code>
    /// public IEnumerable&lt;string&gt; DisplayedRecentFiles => 
    ///     _recentFilesService.RecentFiles.Take(_maxDisplayCount);
    /// </code>
    /// </para>
    /// </remarks>
    public ObservableCollection<string> RecentFiles { get; }

    /// <summary>
    /// Gets the current display limit for recent files (user-configurable, 3-99).
    /// </summary>
    /// <remarks>
    /// This value comes from the "RecentFiles.MaxCount" setting.
    /// UI components should use this to limit how many items they display from RecentFiles.
    /// </remarks>
    public int MaxDisplayCount => _maxEntries;

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// File paths are normalized via <see cref="Path.GetFullPath"/> before storage.
    /// If the path already exists in the list, it is moved to the top (position 0).
    /// </para>
    /// <para>
    /// The list is automatically trimmed to <see cref="AbsoluteMaximum"/> (1000 items) 
    /// after insertion and persisted to settings. The user's display limit (MaxCount) 
    /// does NOT affect what is stored, only what is shown in the UI.
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
        TrimToAbsoluteMax(); // Only trim to storage limit, not display limit
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
    /// Trims the recent files list to the absolute storage maximum (1000 items).
    /// </summary>
    /// <remarks>
    /// This is different from the user's display limit (MaxCount).
    /// We always store up to 1000 items to preserve history, even if the user
    /// has set a lower display limit.
    /// </remarks>
    private void TrimToAbsoluteMax()
    {
        while (RecentFiles.Count > AbsoluteMaximum)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }

    /// <summary>
    /// Persists the current recent files list and maximum count to settings storage.
    /// </summary>
    /// <remarks>
    /// Saves ALL items in RecentFiles (up to 1000), not just the first MaxCount items.
    /// This ensures the full history is preserved.
    /// </remarks>
    private void Persist()
    {
        _settingsService.SetSetting(SettingKeyList, RecentFiles.ToList());
        _settingsService.SetSetting(SettingKeyMax, _maxEntries);
    }

    /// <summary>
    /// Clamps the maximum entry count to valid range (3-250).
    /// </summary>
    /// <param name="value">Desired maximum count.</param>
    /// <returns>Clamped value between 3 and 250 inclusive.</returns>
    /// <remarks>
    /// This controls the DISPLAY limit, not the storage limit.
    /// Storage is always capped at 1000 items regardless of this setting.
    /// </remarks>
    private static int ClampMax(int value)
    {
        if (value < 3) return 3;
        if (value > 250) return 250;
        return value;
    }
}
