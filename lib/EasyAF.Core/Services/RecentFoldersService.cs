using System.Collections.ObjectModel;
using EasyAF.Core.Contracts;
using Serilog;
using System.IO;

namespace EasyAF.Core.Services;

/// <summary>
/// Tracks recently accessed folder paths with automatic persistence via ISettingsService.
/// </summary>
/// <remarks>
/// <para>
/// Maintains an ordered list of folder paths (most recent first) with automatic persistence.
/// Folders are typically tracked when files are opened, allowing users to quickly navigate
/// back to frequently-used directories.
/// </para>
/// <para>
/// Storage vs Display: Stores up to 100 folders to preserve history, but displays fewer
/// based on user preferences (3-50, default 10).
/// </para>
/// </remarks>
public class RecentFoldersService : IRecentFoldersService
{
    private readonly ISettingsService _settingsService;
    private const string SettingKeyList = "RecentFolders";
    private const string SettingKeyMax = "RecentFolders.MaxCount";
    private const int AbsoluteMaximum = 100; // Folders need less storage than files
    private int _maxEntries;

    public RecentFoldersService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _maxEntries = ClampMax(_settingsService.GetSetting<int>(SettingKeyMax, 10));
        _settingsService.SetSetting(SettingKeyMax, _maxEntries);

        var existing = _settingsService.GetSetting<List<string>>(SettingKeyList, new List<string>());
        RecentFolders = new ObservableCollection<string>(existing.Take(AbsoluteMaximum));

        _settingsService.SettingsReloaded += (_, __) =>
        {
            _maxEntries = ClampMax(_settingsService.GetSetting<int>(SettingKeyMax, _maxEntries));
            Log.Debug("Recent folders display limit changed to: {MaxCount}", _maxEntries);
        };
    }

    public ObservableCollection<string> RecentFolders { get; }

    public int MaxDisplayCount => _maxEntries;

    public void AddRecentFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        
        // Normalize and validate
        try
        {
            var normalized = Path.GetFullPath(path);
            
            // Ensure it's actually a directory
            if (!Directory.Exists(normalized))
            {
                Log.Debug("Skipping non-existent folder: {Path}", normalized);
                return;
            }
            
            if (RecentFolders.Contains(normalized))
            {
                // Move to top
                RecentFolders.Remove(normalized);
            }
            
            RecentFolders.Insert(0, normalized);
            TrimToAbsoluteMax();
            Persist();
            Log.Debug("Recent folder added: {Path}", normalized);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to add recent folder: {Path}", path);
        }
    }

    public void RemoveRecentFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        
        try
        {
            var normalized = Path.GetFullPath(path);
            if (RecentFolders.Contains(normalized))
            {
                RecentFolders.Remove(normalized);
                Persist();
                Log.Debug("Recent folder removed: {Path}", normalized);
            }
        }
        catch
        {
            // Silently ignore invalid paths
        }
    }

    public void Clear()
    {
        RecentFolders.Clear();
        Persist();
        Log.Debug("Recent folders cleared");
    }

    private void TrimToAbsoluteMax()
    {
        while (RecentFolders.Count > AbsoluteMaximum)
        {
            RecentFolders.RemoveAt(RecentFolders.Count - 1);
        }
    }

    private void Persist()
    {
        _settingsService.SetSetting(SettingKeyList, RecentFolders.ToList());
        _settingsService.SetSetting(SettingKeyMax, _maxEntries);
    }

    private static int ClampMax(int value)
    {
        if (value < 3) return 3;
        if (value > 50) return 50;
        return value;
    }
}
