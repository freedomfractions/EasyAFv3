using System.Collections.ObjectModel;
using EasyAF.Core.Contracts;
using Serilog;
using System.IO;

namespace EasyAF.Core.Services;

/// <summary>
/// Tracks recently accessed document file paths with persistence via ISettingsService.
/// </summary>
public class RecentFilesService : IRecentFilesService
{
    private readonly ISettingsService _settingsService;
    private const string SettingKeyList = "RecentFiles";
    private const string SettingKeyMax = "RecentFiles.MaxCount";
    private int _maxEntries;

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

    public ObservableCollection<string> RecentFiles { get; }

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

    private void TrimToMax()
    {
        while (RecentFiles.Count > _maxEntries)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }

    private void Persist()
    {
        _settingsService.SetSetting(SettingKeyList, RecentFiles.ToList());
        _settingsService.SetSetting(SettingKeyMax, _maxEntries);
    }

    private static int ClampMax(int value)
    {
        if (value < 1) return 1;
        if (value > 100) return 100;
        return value;
    }
}
