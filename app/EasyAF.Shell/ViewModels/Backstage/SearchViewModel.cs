using Prism.Mvvm;
using System;
using System.Threading;
using EasyAF.Core.Contracts;

namespace EasyAF.Shell.ViewModels.Backstage;

/// <summary>
/// ViewModel responsible for search and filtering logic in the Open Backstage.
/// Handles debounced search across Recent Files, Recent Folders, and Browser entries.
/// </summary>
public class SearchViewModel : BindableBase
{
    private readonly ISettingsService _settingsService;
    private CancellationTokenSource? _searchCancellation;
    private Timer? _searchDebounceTimer;

    private string _searchText = string.Empty;
    
    /// <summary>
    /// Gets or sets the current search text.
    /// Triggers debounced search when changed.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                DebounceSearch();
            }
        }
    }

    /// <summary>
    /// Event raised when search filter should be applied.
    /// Subscribers should filter their collections based on the current SearchText.
    /// </summary>
    public event EventHandler? SearchFilterRequested;

    public SearchViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    /// <summary>
    /// Debounces the search to avoid filtering on every keystroke.
    /// Waits for user to stop typing for the configured delay.
    /// </summary>
    private void DebounceSearch()
    {
        // Cancel any pending search
        _searchCancellation?.Cancel();
        _searchDebounceTimer?.Dispose();

        // Get debounce delay from settings (default 250ms)
        int delayMs = _settingsService.GetSetting("Search.DebounceDelayMs", 250);

        // Create new cancellation token
        _searchCancellation = new CancellationTokenSource();
        var token = _searchCancellation.Token;

        // Start debounce timer
        _searchDebounceTimer = new Timer(_ =>
        {
            if (!token.IsCancellationRequested)
            {
                // Execute search on UI thread
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ApplySearchFilter();
                });
            }
        }, null, delayMs, Timeout.Infinite);
    }

    /// <summary>
    /// Raises the SearchFilterRequested event to notify subscribers that filtering should occur.
    /// </summary>
    private void ApplySearchFilter()
    {
        SearchFilterRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the fuzzy search threshold from settings.
    /// </summary>
    /// <returns>Fuzzy threshold value (0.0 to 1.0), or 0.0 if fuzzy search is disabled.</returns>
    public double GetFuzzyThreshold()
    {
        var fuzzyThreshold = _settingsService.GetSetting("Search.FuzzyThreshold", 0.7);
        var fuzzyEnabled = _settingsService.GetSetting("Search.FuzzyEnabled", true);
        
        return fuzzyEnabled ? fuzzyThreshold : 0.0;
    }

    /// <summary>
    /// Gets the current search term, trimmed and ready for use.
    /// </summary>
    public string GetSearchTerm()
    {
        return SearchText?.Trim() ?? string.Empty;
    }
}
