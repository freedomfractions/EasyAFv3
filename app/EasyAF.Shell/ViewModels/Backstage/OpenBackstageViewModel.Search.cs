using System;
using System.Threading;
using System.Windows;
using EasyAF.Shell.Models.Backstage;
using EasyAF.Shell.Helpers;
using EasyAF.Shell.Services; // added

namespace EasyAF.Shell.ViewModels.Backstage;

public partial class OpenBackstageViewModel
{
    private void ScheduleSearch()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;
        int delay = _settingsService.GetSetting("Search.DebounceDelayMs", 250);
        _ = RunDelayedSearchAsync(delay, token);
    }

    private async System.Threading.Tasks.Task RunDelayedSearchAsync(int delayMs, CancellationToken token)
    {
        try
        {
            await System.Threading.Tasks.Task.Delay(delayMs, token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return;
            Application.Current?.Dispatcher.Invoke(ApplySearchFilter);
        }
        catch (TaskCanceledException) { }
    }

    private void ApplySearchFilter()
    {
        var term = SearchText?.Trim() ?? string.Empty;
        var fuzzyThreshold = _settingsService.GetSetting("Search.FuzzyThreshold", 0.7);
        var fuzzyEnabled = _settingsService.GetSetting("Search.FuzzyEnabled", true);
        var threshold = fuzzyEnabled ? fuzzyThreshold : 0.0;

        RecentFiles.Clear();
        foreach (var f in _allRecentFiles)
        {
            if (SearchHelper.IsMatchAny(term, threshold, f.FileName, f.DirectoryPath, f.FilePath))
                RecentFiles.Add(f);
        }

        RecentFolders.Clear();
        foreach (var f in _allRecentFolders)
        {
            if (SearchHelper.IsMatchAny(term, threshold, f.FolderName, f.ParentPath, f.FolderPath))
                RecentFolders.Add(f);
        }

        if (Mode == OpenBackstageMode.QuickAccessFolder)
            ApplyBrowserEntryFilter();
    }
}
