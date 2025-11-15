using System;
using System.IO;
using EasyAF.Shell.Models.Backstage;
using EasyAF.Shell.Helpers; // added

namespace EasyAF.Shell.ViewModels.Backstage;

public partial class OpenBackstageViewModel
{
    private void LoadBrowserEntries(string path)
    {
        _allBrowserEntries.Clear();
        BrowserEntries.Clear();
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
        try
        {
            var dir = new DirectoryInfo(path);
            foreach (var d in dir.GetDirectories())
                _allBrowserEntries.Add(new FolderBrowserEntry { FullPath = d.FullName, IsFolder = true, LastModified = d.LastWriteTime });
            foreach (var f in dir.GetFiles())
                _allBrowserEntries.Add(new FolderBrowserEntry { FullPath = f.FullName, IsFolder = false, FileSize = f.Length, LastModified = f.LastWriteTime });
            ApplyBrowserEntryFilter();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Browser load error: {ex.Message}");
        }
    }

    private void ApplyBrowserEntryFilter()
    {
        var term = SearchText?.Trim() ?? string.Empty;
        var fuzzyThreshold = _settingsService.GetSetting("Search.FuzzyThreshold", 0.7);
        var fuzzyEnabled = _settingsService.GetSetting("Search.FuzzyEnabled", true);
        var threshold = fuzzyEnabled ? fuzzyThreshold : 0.0;
        BrowserEntries.Clear();
        foreach (var e in _allBrowserEntries)
        {
            if (SearchHelper.IsMatchAny(term, threshold, e.Name, e.Extension, e.FullPath))
                BrowserEntries.Add(e);
        }
    }
}
