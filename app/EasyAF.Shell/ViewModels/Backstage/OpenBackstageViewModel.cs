using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyAF.Shell.Models.Backstage;
using EasyAF.Shell.Helpers;
using EasyAF.Shell.Services;
using EasyAF.Core.Contracts;
using Microsoft.Win32;

namespace EasyAF.Shell.ViewModels.Backstage;

public enum OpenBackstageMode { Recent, QuickAccessFolder }
public enum RecentTab { Files, Folders }

/// <summary>
/// ViewModel for Open Backstage with Quick Access folder support and comprehensive search.
/// </summary>
public class OpenBackstageViewModel : BindableBase
{
    private readonly IModuleLoader? _moduleLoader;
    private readonly ISettingsService _settingsService;
    private readonly IBackstageService? _backstageService;
    private readonly IRecentFilesService _recentFilesService;
    private CancellationTokenSource? _searchCancellation;
    private Timer? _searchDebounceTimer;

    private OpenBackstageMode _mode = OpenBackstageMode.Recent;
    public OpenBackstageMode Mode
    {
        get => _mode;
        set => SetProperty(ref _mode, value);
    }

    private RecentTab _recentTab = RecentTab.Files;
    public RecentTab RecentTab
    {
        get => _recentTab;
        set => SetProperty(ref _recentTab, value);
    }

    private string _searchText = string.Empty;
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

    private QuickAccessFolder? _selectedQuickAccessFolder;
    public QuickAccessFolder? SelectedQuickAccessFolder
    {
        get => _selectedQuickAccessFolder;
        set => SetProperty(ref _selectedQuickAccessFolder, value);
    }

    /// <summary>
    /// Collection of pinned Quick Access folders.
    /// </summary>
    public ObservableCollection<QuickAccessFolder> QuickAccessFolders { get; }

    /// <summary>
    /// Master collection of all recent files (unfiltered).
    /// </summary>
    private readonly ObservableCollection<RecentFileEntry> _allRecentFiles;

    /// <summary>
    /// Master collection of all recent folders (unfiltered).
    /// </summary>
    private readonly ObservableCollection<RecentFolderEntry> _allRecentFolders;

    /// <summary>
    /// Master collection of files in the selected Quick Access folder (unfiltered).
    /// </summary>
    private readonly ObservableCollection<FolderFileEntry> _allFolderFiles;

    /// <summary>
    /// Master collection of browser entries (files + folders) in the current path (unfiltered).
    /// </summary>
    private readonly ObservableCollection<FolderBrowserEntry> _allBrowserEntries;

    /// <summary>
    /// Filtered collection of recent files displayed in the Files tab.
    /// </summary>
    public ObservableCollection<RecentFileEntry> RecentFiles { get; }

    /// <summary>
    /// Filtered collection of recent folders displayed in the Folders tab.
    /// </summary>
    public ObservableCollection<RecentFolderEntry> RecentFolders { get; }

    /// <summary>
    /// Filtered collection of files in the selected Quick Access folder.
    /// </summary>
    public ObservableCollection<FolderFileEntry> FolderFiles { get; }

    /// <summary>
    /// Filtered collection of browser entries (files + folders) displayed in the browser.
    /// </summary>
    public ObservableCollection<FolderBrowserEntry> BrowserEntries { get; }

    private string _currentBrowsePath = string.Empty;
    /// <summary>
    /// Gets or sets the current directory being browsed.
    /// </summary>
    public string CurrentBrowsePath
    {
        get => _currentBrowsePath;
        set
        {
            if (SetProperty(ref _currentBrowsePath, value))
            {
                RaisePropertyChanged(nameof(CanNavigateUp));
            }
        }
    }

    /// <summary>
    /// Gets whether the user can navigate up to the parent directory.
    /// </summary>
    public bool CanNavigateUp
    {
        get
        {
            if (string.IsNullOrEmpty(CurrentBrowsePath))
                return false;

            try
            {
                var dirInfo = new DirectoryInfo(CurrentBrowsePath);
                return dirInfo.Parent != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public DelegateCommand SelectRecentCommand { get; }
    public DelegateCommand<QuickAccessFolder> SelectQuickAccessFolderCommand { get; }
    public DelegateCommand<RecentFileEntry> TogglePinCommand { get; }
    public DelegateCommand BrowseCommand { get; }
    public DelegateCommand<RecentFileEntry> OpenFileCommand { get; }
    public DelegateCommand<RecentFolderEntry> OpenFolderCommand { get; }
    public DelegateCommand<FolderFileEntry> OpenFolderFileCommand { get; }
    public DelegateCommand<FolderBrowserEntry> OpenBrowserEntryCommand { get; }
    public DelegateCommand NavigateUpCommand { get; }
    public DelegateCommand<RecentFileEntry> CopyPathCommand { get; }
    public DelegateCommand<RecentFolderEntry> CopyFolderPathCommand { get; }
    public DelegateCommand<RecentFileEntry> RemoveFromListCommand { get; }
    public DelegateCommand<RecentFolderEntry> RemoveFolderFromListCommand { get; }
    public DelegateCommand<FolderBrowserEntry> CopyBrowserPathCommand { get; }
    public DelegateCommand<RecentFileEntry> OpenFileLocationCommand { get; }
    public DelegateCommand FocusSearchCommand { get; }
    public DelegateCommand<object> AddToQuickAccessCommand { get; }
    public DelegateCommand<QuickAccessFolder> RemoveFromQuickAccessCommand { get; }

    /// <summary>
    /// Event raised when a file is selected (via Browse, double-click, etc.)
    /// The string parameter is the selected file path.
    /// </summary>
    public event Action<string>? FileSelected;

    /// <summary>
    /// Event raised when a folder is selected for opening (via double-click).
    /// The string parameter is the selected folder path.
    /// </summary>
    public event Action<string>? FolderSelected;
    
    /// <summary>
    /// Event raised when Ctrl+F is pressed to focus the search box.
    /// The view will handle this to actually move focus.
    /// </summary>
    public event EventHandler? FocusSearchRequested;
    
    /// <summary>
    /// Event raised when the data source changes (mode switch or folder navigation).
    /// The view should reset the scroll position to the top.
    /// </summary>
    public event EventHandler? ScrollToTopRequested;

    public OpenBackstageViewModel(IModuleLoader? moduleLoader, ISettingsService settingsService, IRecentFilesService recentFilesService, IBackstageService? backstageService = null)
    {
        _moduleLoader = moduleLoader;
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _recentFilesService = recentFilesService ?? throw new ArgumentNullException(nameof(recentFilesService));
        _backstageService = backstageService;

        QuickAccessFolders = new ObservableCollection<QuickAccessFolder>();
        _allRecentFiles = new ObservableCollection<RecentFileEntry>();
        _allRecentFolders = new ObservableCollection<RecentFolderEntry>();
        _allFolderFiles = new ObservableCollection<FolderFileEntry>();
        _allBrowserEntries = new ObservableCollection<FolderBrowserEntry>();
        RecentFiles = new ObservableCollection<RecentFileEntry>();
        RecentFolders = new ObservableCollection<RecentFolderEntry>();
        FolderFiles = new ObservableCollection<FolderFileEntry>();
        BrowserEntries = new ObservableCollection<FolderBrowserEntry>();

        SelectRecentCommand = new DelegateCommand(ExecuteSelectRecent);
        SelectQuickAccessFolderCommand = new DelegateCommand<QuickAccessFolder>(ExecuteSelectQuickAccessFolder);
        TogglePinCommand = new DelegateCommand<RecentFileEntry>(ExecuteTogglePin);
        BrowseCommand = new DelegateCommand(ExecuteBrowse);
        OpenFileCommand = new DelegateCommand<RecentFileEntry>(ExecuteOpenFile);
        OpenFolderCommand = new DelegateCommand<RecentFolderEntry>(ExecuteOpenFolder);
        OpenFolderFileCommand = new DelegateCommand<FolderFileEntry>(ExecuteOpenFolderFile);
        OpenBrowserEntryCommand = new DelegateCommand<FolderBrowserEntry>(ExecuteOpenBrowserEntry);
        NavigateUpCommand = new DelegateCommand(ExecuteNavigateUp, CanExecuteNavigateUp);
        CopyPathCommand = new DelegateCommand<RecentFileEntry>(ExecuteCopyPath);
        CopyFolderPathCommand = new DelegateCommand<RecentFolderEntry>(ExecuteCopyFolderPath);
        RemoveFromListCommand = new DelegateCommand<RecentFileEntry>(ExecuteRemoveFromList);
        RemoveFolderFromListCommand = new DelegateCommand<RecentFolderEntry>(ExecuteRemoveFolderFromListCommand);
        CopyBrowserPathCommand = new DelegateCommand<FolderBrowserEntry>(ExecuteCopyBrowserPath);
        OpenFileLocationCommand = new DelegateCommand<RecentFileEntry>(ExecuteOpenFileLocation);
        FocusSearchCommand = new DelegateCommand(ExecuteFocusSearch);
        AddToQuickAccessCommand = new DelegateCommand<object>(ExecuteAddToQuickAccess);
        RemoveFromQuickAccessCommand = new DelegateCommand<QuickAccessFolder>(ExecuteRemoveFromQuickAccess);

        // Initialize Quick Access folders and load data
        LoadSampleQuickAccessFolders();
        
        // Subscribe to RecentFiles changes to keep our list in sync
        _recentFilesService.RecentFiles.CollectionChanged += OnRecentFilesChanged;
        
        // Initial load from service
        LoadRecentFilesFromService();
        LoadSampleRecentFolders(); // TODO: Replace with real folder tracking service

        // Initial filter (shows all)
        ApplySearchFilter();
    }

    #region Command Execution Methods

    private void ExecuteSelectRecent()
    {
        Mode = OpenBackstageMode.Recent;
        SelectedQuickAccessFolder = null;
        
        // Delay scroll reset until ContentControl finishes switching content
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            ScrollToTopRequested?.Invoke(this, EventArgs.Empty);
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void ExecuteSelectQuickAccessFolder(QuickAccessFolder folder)
    {
        if (folder == null) return;
        Mode = OpenBackstageMode.QuickAccessFolder;
        SelectedQuickAccessFolder = folder;
        
        // Set the current browse path and load browser entries
        CurrentBrowsePath = folder.FolderPath;
        LoadBrowserEntries(CurrentBrowsePath);
        NavigateUpCommand.RaiseCanExecuteChanged();
        
        // Delay scroll reset until ContentControl finishes switching content
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            ScrollToTopRequested?.Invoke(this, EventArgs.Empty);
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void ExecuteTogglePin(RecentFileEntry file)
    {
        if (file == null) return;
        file.IsPinned = !file.IsPinned;
        
        // Force the CollectionViewSource to refresh its grouping
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            // Trigger a refresh by removing and re-adding the item
            var index = RecentFiles.IndexOf(file);
            if (index >= 0)
            {
                RecentFiles.RemoveAt(index);
                RecentFiles.Insert(index, file);
            }
        });
        
        SavePinnedFiles();
    }

    private void ExecuteOpenFile(RecentFileEntry file)
    {
        if (file == null) return;
        
        // Fire the event for parent integration
        FileSelected?.Invoke(file.FilePath);
        
        // Request backstage close
        _backstageService?.RequestClose();
    }

    private void ExecuteOpenFolder(RecentFolderEntry folder)
    {
        if (folder == null) return;
        
        // Fire the event for parent integration
        FolderSelected?.Invoke(folder.FolderPath);
        
        // Request backstage close
        _backstageService?.RequestClose();
    }

    private void ExecuteOpenFolderFile(FolderFileEntry file)
    {
        if (file == null) return;
        
        // Fire the event for parent integration
        FileSelected?.Invoke(file.FilePath);
        
        // Request backstage close
        _backstageService?.RequestClose();
    }

    private void ExecuteOpenBrowserEntry(FolderBrowserEntry entry)
    {
        if (entry == null) return;

        if (entry.IsFolder)
        {
            // Navigate into the folder
            CurrentBrowsePath = entry.FullPath;
            LoadBrowserEntries(CurrentBrowsePath);
            NavigateUpCommand.RaiseCanExecuteChanged();
            
            // Delay scroll reset until new content is loaded and rendered
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                ScrollToTopRequested?.Invoke(this, EventArgs.Empty);
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }
        else
        {
            // Open the file
            FileSelected?.Invoke(entry.FullPath);
            
            // Request backstage close
            _backstageService?.RequestClose();
        }
    }

    private void ExecuteNavigateUp()
    {
        try
        {
            var currentDir = new DirectoryInfo(CurrentBrowsePath);
            if (currentDir.Parent != null)
            {
                CurrentBrowsePath = currentDir.Parent.FullName;
                LoadBrowserEntries(CurrentBrowsePath);
                NavigateUpCommand.RaiseCanExecuteChanged();
                
                // Delay scroll reset until new content is loaded and rendered
                System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                {
                    ScrollToTopRequested?.Invoke(this, EventArgs.Empty);
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating up: {ex.Message}");
        }
    }

    private bool CanExecuteNavigateUp()
    {
        return CanNavigateUp;
    }

    private void ExecuteBrowse()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open Document",
            Filter = BuildFileTypeFilter(),
            FilterIndex = GetDefaultFilterIndex(),
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true
        };

        // Set initial directory to most recent folder if available
        var mostRecentFolder = _allRecentFolders.OrderByDescending(f => f.LastAccessed).FirstOrDefault();
        if (mostRecentFolder != null && System.IO.Directory.Exists(mostRecentFolder.FolderPath))
        {
            dialog.InitialDirectory = mostRecentFolder.FolderPath;
        }

        if (dialog.ShowDialog() == true)
        {
            // Save user's filter selection as preference
            SaveDefaultFilterIndex(dialog.FilterIndex);

            // Raise event for parent to handle (close backstage, open file)
            FileSelected?.Invoke(dialog.FileName);
            
            // Request backstage close
            _backstageService?.RequestClose();
        }
    }

    private void ExecuteCopyPath(RecentFileEntry file)
    {
        if (file == null) return;

        try
        {
            // Copy the file path to clipboard
            System.Windows.Clipboard.SetText(file.FilePath);

            // Show a messagebox indicating the path has been copied
            System.Windows.MessageBox.Show(
                $"File path copied to clipboard:\n\n{file.FilePath}",
                "Path Copied",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying file path: {ex.Message}");
        }
    }

    private void ExecuteCopyFolderPath(RecentFolderEntry folder)
    {
        if (folder == null) return;

        try
        {
            // Copy the folder path to clipboard
            System.Windows.Clipboard.SetText(folder.FolderPath);

            // Show a messagebox indicating the path has been copied
            System.Windows.MessageBox.Show(
                $"Folder path copied to clipboard:\n\n{folder.FolderPath}",
                "Path Copied",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying folder path: {ex.Message}");
        }
    }

    private void ExecuteCopyBrowserPath(FolderBrowserEntry entry)
    {
        if (entry == null) return;

        try
        {
            // Copy the browser entry path to clipboard
            System.Windows.Clipboard.SetText(entry.FullPath);

            // Show a messagebox indicating the path has been copied
            System.Windows.MessageBox.Show(
                $"Path copied to clipboard:\n\n{entry.FullPath}",
                "Path Copied",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying browser entry path: {ex.Message}");
        }
    }

    private void ExecuteRemoveFromList(RecentFileEntry file)
    {
        if (file == null) return;

        // Remove from recent files collection
        _allRecentFiles.Remove(file);
        RecentFiles.Remove(file);

        _recentFilesService.RemoveRecentFile(file.FilePath);
        
        // Also remove from pinned files if it was pinned
        if (file.IsPinned)
        {
            SavePinnedFiles();
        }
    }

    private void ExecuteRemoveFolderFromListCommand(RecentFolderEntry folder)
    {
        if (folder == null) return;

        // Remove from recent folders collection
        _allRecentFolders.Remove(folder);
        RecentFolders.Remove(folder);

        // TODO: Persist removal to settings via ISettingsService
    }

    private void ExecuteOpenFileLocation(RecentFileEntry file)
    {
        if (file == null) return;

        try
        {
            var fullPath = Path.GetFullPath(file.FilePath);
            if (File.Exists(fullPath))
            {
                // Open Explorer and select the file
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{fullPath}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                // File doesn't exist, just open the directory
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = directory,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening file location: {ex.Message}");
        }
    }

    private void ExecuteFocusSearch()
    {
        // Raise the FocusSearchRequested event
        FocusSearchRequested?.Invoke(this, EventArgs.Empty);
    }
    
    private void ExecuteAddToQuickAccess(object? parameter)
    {
        string? folderPath = null;
        int insertionIndex = -1; // -1 means append to end
        
        // Handle both string (from context menu) and tuple (from drag & drop)
        if (parameter is string str)
        {
            folderPath = str;
        }
        else if (parameter is ValueTuple<string, int> tuple)
        {
            (folderPath, insertionIndex) = tuple;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Invalid parameter type for AddToQuickAccess: {parameter?.GetType()}");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(folderPath)) return;
        
        try
        {
            // Normalize the path
            var fullPath = Path.GetFullPath(folderPath);
            
            // If it's a file, get its directory
            if (File.Exists(fullPath))
            {
                var dir = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrWhiteSpace(dir))
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot extract directory from file path: {fullPath}");
                    return;
                }
                fullPath = dir;
            }
            
            // Verify it's a directory
            if (!Directory.Exists(fullPath))
            {
                System.Diagnostics.Debug.WriteLine($"Directory does not exist: {fullPath}");
                return;
            }
            
            // Check if already in Quick Access
            if (QuickAccessFolders.Any(f => string.Equals(f.FolderPath, fullPath, StringComparison.OrdinalIgnoreCase)))
            {
                System.Diagnostics.Debug.WriteLine($"Folder already in Quick Access: {fullPath}");
                return;
            }
            
            // Create the folder entry
            var folderName = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(folderName))
            {
                folderName = fullPath; // Use full path for root drives
            }
            
            var newFolder = new QuickAccessFolder
            {
                FolderName = folderName,
                FolderPath = fullPath,
                IconGlyph = "\uE8B7" // Folder icon
            };
            
            // Insert at specified index or append to end
            if (insertionIndex >= 0 && insertionIndex < QuickAccessFolders.Count)
            {
                QuickAccessFolders.Insert(insertionIndex, newFolder);
                System.Diagnostics.Debug.WriteLine($"Inserted to Quick Access at index {insertionIndex}: {fullPath}");
            }
            else
            {
                QuickAccessFolders.Add(newFolder);
                System.Diagnostics.Debug.WriteLine($"Added to Quick Access (end of list): {fullPath}");
            }
            
            SaveQuickAccessFolders();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding to Quick Access: {ex.Message}");
        }
    }
    
    private void ExecuteRemoveFromQuickAccess(QuickAccessFolder? folder)
    {
        if (folder == null) return;
        
        try
        {
            // Remember the index and selection state before removal
            var currentIndex = QuickAccessFolders.IndexOf(folder);
            var wasCurrentlySelected = folder == SelectedQuickAccessFolder;
            
            // Remove the folder
            QuickAccessFolders.Remove(folder);
            SaveQuickAccessFolders();
            
            System.Diagnostics.Debug.WriteLine($"Removed from Quick Access: {folder.FolderPath}");
            
            // If we removed the currently selected folder, auto-select next/previous
            if (wasCurrentlySelected)
            {
                if (QuickAccessFolders.Count > 0)
                {
                    // Select next item (same index), or previous if we removed the last item
                    var newIndex = Math.Min(currentIndex, QuickAccessFolders.Count - 1);
                    ExecuteSelectQuickAccessFolder(QuickAccessFolders[newIndex]);
                    System.Diagnostics.Debug.WriteLine($"Auto-selected Quick Access folder at index {newIndex}: {QuickAccessFolders[newIndex].FolderPath}");
                }
                else
                {
                    // No Quick Access folders left - switch to Recent Files
                    ExecuteSelectRecent();
                    System.Diagnostics.Debug.WriteLine("No Quick Access folders remaining - switched to Recent Files");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing from Quick Access: {ex.Message}");
        }
    }

    #endregion

    #region Search Implementation

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
    /// Applies the search filter to RecentFiles, RecentFolders, and FolderFiles collections.
    /// </summary>
    private void ApplySearchFilter()
    {
        var searchTerm = SearchText?.Trim() ?? string.Empty;
        var fuzzyThreshold = _settingsService.GetSetting("Search.FuzzyThreshold", 0.7);
        var fuzzyEnabled = _settingsService.GetSetting("Search.FuzzyEnabled", true);

        // Use threshold of 0 if fuzzy is disabled (forces exact/wildcard only)
        var threshold = fuzzyEnabled ? fuzzyThreshold : 0.0;

        // Filter files
        RecentFiles.Clear();
        foreach (var file in _allRecentFiles)
        {
            if (MatchesFileSearch(file, searchTerm, threshold))
            {
                RecentFiles.Add(file);
            }
        }

        // Filter folders
        RecentFolders.Clear();
        foreach (var folder in _allRecentFolders)
        {
            if (MatchesFolderSearch(folder, searchTerm, threshold))
            {
                RecentFolders.Add(folder);
            }
        }

        // Filter Quick Access folder files (if in that mode)
        if (Mode == OpenBackstageMode.QuickAccessFolder)
        {
            ApplyBrowserEntryFilter();
        }
    }

    /// <summary>
    /// Checks if a file entry matches the search criteria.
    /// Searches: FileName and DirectoryPath.
    /// </summary>
    private bool MatchesFileSearch(RecentFileEntry file, string searchTerm, double fuzzyThreshold)
    {
        return SearchHelper.IsMatchAny(
            searchTerm,
            fuzzyThreshold,
            file.FileName,
            file.DirectoryPath,
            file.FilePath  // Also search full path
        );
    }

    /// <summary>
    /// Checks if a folder entry matches the search criteria.
    /// Searches: FolderName and ParentPath.
    /// </summary>
    private bool MatchesFolderSearch(RecentFolderEntry folder, string searchTerm, double fuzzyThreshold)
    {
        return SearchHelper.IsMatchAny(
            searchTerm,
            fuzzyThreshold,
            folder.FolderName,
            folder.ParentPath,
            folder.FolderPath  // Also search full path
        );
    }

    #endregion

    #region Quick Access Folder File Loading

    /// <summary>
    /// Loads files from the specified folder path.
    /// </summary>
    private void LoadFolderFiles(string folderPath)
    {
        _allFolderFiles.Clear();
        FolderFiles.Clear();

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            System.Diagnostics.Debug.WriteLine("LoadFolderFiles: folderPath is null or empty");
            return;
        }

        if (!System.IO.Directory.Exists(folderPath))
        {
            System.Diagnostics.Debug.WriteLine($"LoadFolderFiles: Directory does not exist: {folderPath}");
            return;
        }

        try
        {
            var directory = new System.IO.DirectoryInfo(folderPath);
            var files = directory.GetFiles();

            System.Diagnostics.Debug.WriteLine($"LoadFolderFiles: Found {files.Length} files in {folderPath}");

            foreach (var fileInfo in files)
            {
                _allFolderFiles.Add(new FolderFileEntry
                {
                    FilePath = fileInfo.FullName,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                });
            }

            System.Diagnostics.Debug.WriteLine($"LoadFolderFiles: Added {_allFolderFiles.Count} files to _allFolderFiles");

            // Apply initial filter
            ApplyFolderFileFilter();

            System.Diagnostics.Debug.WriteLine($"LoadFolderFiles: After filter, FolderFiles.Count = {FolderFiles.Count}");
        }
        catch (Exception ex)
        {
            // Log error or show message to user
            System.Diagnostics.Debug.WriteLine($"Error loading folder files: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads files and folders from the specified directory into BrowserEntries.
    /// </summary>
    private void LoadBrowserEntries(string path)
    {
        _allBrowserEntries.Clear();
        BrowserEntries.Clear();

        if (string.IsNullOrWhiteSpace(path))
        {
            System.Diagnostics.Debug.WriteLine("LoadBrowserEntries: path is null or empty");
            return;
        }

        if (!Directory.Exists(path))
        {
            System.Diagnostics.Debug.WriteLine($"LoadBrowserEntries: Directory does not exist: {path}");
            return;
        }

        try
        {
            var directory = new DirectoryInfo(path);
            
            // Load subdirectories first
            var directories = directory.GetDirectories();
            System.Diagnostics.Debug.WriteLine($"LoadBrowserEntries: Found {directories.Length} folders in {path}");
            
            foreach (var dirInfo in directories)
            {
                _allBrowserEntries.Add(new FolderBrowserEntry
                {
                    FullPath = dirInfo.FullName,
                    IsFolder = true,
                    LastModified = dirInfo.LastWriteTime
                });
            }

            // Then load files
            var files = directory.GetFiles();
            System.Diagnostics.Debug.WriteLine($"LoadBrowserEntries: Found {files.Length} files in {path}");
            
            foreach (var fileInfo in files)
            {
                _allBrowserEntries.Add(new FolderBrowserEntry
                {
                    FullPath = fileInfo.FullName,
                    IsFolder = false,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime
                });
            }

            // Apply filter
            ApplyBrowserEntryFilter();

            System.Diagnostics.Debug.WriteLine($"LoadBrowserEntries: After filter, BrowserEntries.Count = {BrowserEntries.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading browser entries: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies search filter to browser entries.
    /// </summary>
    private void ApplyBrowserEntryFilter()
    {
        var searchTerm = SearchText?.Trim() ?? string.Empty;
        var fuzzyThreshold = _settingsService.GetSetting("Search.FuzzyThreshold", 0.7);
        var fuzzyEnabled = _settingsService.GetSetting("Search.FuzzyEnabled", true);
        var threshold = fuzzyEnabled ? fuzzyThreshold : 0.0;

        BrowserEntries.Clear();
        foreach (var entry in _allBrowserEntries)
        {
            if (MatchesBrowserEntrySearch(entry, searchTerm, threshold))
            {
                BrowserEntries.Add(entry);
            }
        }
    }

    /// <summary>
    /// Checks if a browser entry matches the search criteria.
    /// </summary>
    private bool MatchesBrowserEntrySearch(FolderBrowserEntry entry, string searchTerm, double fuzzyThreshold)
    {
        return SearchHelper.IsMatchAny(
            searchTerm,
            fuzzyThreshold,
            entry.Name,
            entry.Extension,
            entry.FullPath
        );
    }

    /// <summary>
    /// Applies search filter to folder files.
    /// </summary>
    private void ApplyFolderFileFilter()
    {
        var searchTerm = SearchText?.Trim() ?? string.Empty;
        var fuzzyThreshold = _settingsService.GetSetting("Search.FuzzyThreshold", 0.7);
        var fuzzyEnabled = _settingsService.GetSetting("Search.FuzzyEnabled", true);
        var threshold = fuzzyEnabled ? fuzzyThreshold : 0.0;

        FolderFiles.Clear();
        foreach (var file in _allFolderFiles)
        {
            if (MatchesFolderFileSearch(file, searchTerm, threshold))
            {
                FolderFiles.Add(file);
            }
        }
    }

    /// <summary>
    /// Checks if a folder file entry matches the search criteria.
    /// </summary>
    private bool MatchesFolderFileSearch(FolderFileEntry file, string searchTerm, double fuzzyThreshold)
    {
        return SearchHelper.IsMatchAny(
            searchTerm,
            fuzzyThreshold,
            file.FileName,
            file.Extension,
            file.FilePath
        );
    }

    #endregion

    #region File Type Filter Building

    /// <summary>
    /// Builds the file type filter string for OpenFileDialog.
    /// Uses loaded modules if available, otherwise uses placeholder file types.
    /// Format: "Description (*.ext)|*.ext|All Files (*.*)|*.*"
    /// </summary>
    private string BuildFileTypeFilter()
    {
        var filterBuilder = new StringBuilder();
        var fileTypes = GetAvailableFileTypes();

        if (fileTypes.Count > 0)
        {
            // Add "All Supported Files" filter if we have any file types
            var allExtensions = string.Join(";", fileTypes.Select(ft => $"*.{ft.Extension}"));
            filterBuilder.Append($"All Supported Files ({allExtensions})|{allExtensions}|");

            // Add individual file type filters
            foreach (var fileType in fileTypes.OrderBy(ft => ft.Description))
            {
                filterBuilder.Append($"{fileType.Description} (*.{fileType.Extension})|*.{fileType.Extension}|");
            }
        }

        // Always append "All Files" as the last option
        filterBuilder.Append("All Files (*.*)|*.*");

        return filterBuilder.ToString();
    }

    /// <summary>
    /// Gets available file types from loaded modules, or placeholder types if no modules loaded.
    /// </summary>
    private List<FileTypeDefinition> GetAvailableFileTypes()
    {
        var fileTypes = new List<FileTypeDefinition>();

        // Try to get file types from loaded modules first
        if (_moduleLoader != null && _moduleLoader.LoadedModules.Any())
        {
            foreach (var module in _moduleLoader.LoadedModules)
            {
                if (module.SupportedFileTypes != null && module.SupportedFileTypes.Count > 0)
                {
                    fileTypes.AddRange(module.SupportedFileTypes);
                }
                else if (module.SupportedFileExtensions != null && module.SupportedFileExtensions.Length > 0)
                {
                    // Fallback to SupportedFileExtensions with generic description
                    foreach (var ext in module.SupportedFileExtensions)
                    {
                        fileTypes.Add(new FileTypeDefinition(ext, $"{ext.ToUpper()} Files"));
                    }
                }
            }
        }

        // If no modules loaded, use placeholder file types for demonstration
        // TODO: Remove these placeholders once real modules are implemented
        if (fileTypes.Count == 0)
        {
            fileTypes.Add(new FileTypeDefinition("ezmap", "EasyAF Map Files (PLACEHOLDER)"));
            fileTypes.Add(new FileTypeDefinition("ezproj", "EasyAF Project Files (PLACEHOLDER)"));
            fileTypes.Add(new FileTypeDefinition("ezspec", "EasyAF Spec Files (PLACEHOLDER)"));
        }

        return fileTypes;
    }

    /// <summary>
    /// Gets the user's preferred default filter index from settings.
    /// Returns 1 (first filter) if not set.
    /// </summary>
    private int GetDefaultFilterIndex()
    {
        return _settingsService.GetSetting("OpenDialog.DefaultFilterIndex", 1);
    }

    /// <summary>
    /// Saves the user's filter selection as the default for next time.
    /// </summary>
    private void SaveDefaultFilterIndex(int filterIndex)
    {
        _settingsService.SetSetting("OpenDialog.DefaultFilterIndex", filterIndex);
    }

    #endregion

    #region Recent Files Service Integration

    /// <summary>
    /// Loads recent files from IRecentFilesService and converts them to RecentFileEntry objects.
    /// </summary>
    /// <remarks>
    /// Takes only MaxDisplayCount items from the service's full list to respect the user's display limit setting.
    /// </remarks>
    private void LoadRecentFilesFromService()
    {
        _allRecentFiles.Clear();
        
        // Take only the number of items the user wants to display
        var itemsToLoad = _recentFilesService.RecentFiles
            .Take(_recentFilesService.MaxDisplayCount);
        
        foreach (var filePath in itemsToLoad)
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                var entry = new RecentFileEntry
                {
                    FilePath = filePath,
                    LastModified = fileInfo.LastWriteTime,
                    IsPinned = IsPinnedFile(filePath) // Check if pinned in settings
                };
                _allRecentFiles.Add(entry);
            }
        }
        
        ApplySearchFilter();
    }

    /// <summary>
    /// Handles changes to the RecentFiles collection from IRecentFilesService.
    /// </summary>
    private void OnRecentFilesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Reload when the service's list changes
        LoadRecentFilesFromService();
    }

    /// <summary>
    /// Checks if a file path is pinned (stored in settings).
    /// </summary>
    private bool IsPinnedFile(string filePath)
    {
        var pinnedFiles = _settingsService.GetSetting<List<string>>("OpenBackstage.PinnedFiles", new List<string>());
        return pinnedFiles.Contains(filePath, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Saves pinned file paths to settings.
    /// </summary>
    private void SavePinnedFiles()
    {
        var pinnedFiles = _allRecentFiles
            .Where(f => f.IsPinned)
            .Select(f => f.FilePath)
            .ToList();
        
        _settingsService.SetSetting("OpenBackstage.PinnedFiles", pinnedFiles);
    }
    
    /// <summary>
    /// Saves Quick Access folders to settings.
    /// </summary>
    public void SaveQuickAccessFolders()
    {
        var folders = QuickAccessFolders
            .Select(f => f.FolderPath)
            .ToList();
        
        _settingsService.SetSetting("OpenBackstage.QuickAccessFolders", folders);
    }
    
    /// <summary>
    /// Loads Quick Access folders from settings.
    /// </summary>
    private void LoadQuickAccessFolders()
    {
        var savedFolders = _settingsService.GetSetting<List<string>>("OpenBackstage.QuickAccessFolders", new List<string>());
        
        QuickAccessFolders.Clear();
        
        // If no saved folders, use defaults
        if (savedFolders.Count == 0)
        {
            LoadDefaultQuickAccessFolders();
            return;
        }
        
        // Load saved folders
        foreach (var folderPath in savedFolders)
        {
            if (Directory.Exists(folderPath))
            {
                var folderName = Path.GetFileName(folderPath);
                if (string.IsNullOrEmpty(folderName))
                {
                    folderName = folderPath; // Use full path for root drives
                }
                
                QuickAccessFolders.Add(new QuickAccessFolder
                {
                    FolderName = folderName,
                    FolderPath = folderPath,
                    IconGlyph = "\uE8B7"
                });
            }
        }
    }
    
    /// <summary>
    /// Loads default Quick Access folders (Documents, Desktop, Downloads).
    /// </summary>
    private void LoadDefaultQuickAccessFolders()
    {
        // Use real paths that exist on the system
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        QuickAccessFolders.Add(new QuickAccessFolder
        {
            FolderName = "Documents",
            FolderPath = documentsPath,
            IconGlyph = "\uE8B7"
        });

        QuickAccessFolders.Add(new QuickAccessFolder
        {
            FolderName = "Desktop",
            FolderPath = desktopPath,
            IconGlyph = "\uE8B7"
        });

        if (Directory.Exists(downloadsPath))
        {
            QuickAccessFolders.Add(new QuickAccessFolder
            {
                FolderName = "Downloads",
                FolderPath = downloadsPath,
                IconGlyph = "\uE8B7"
            });
        }
        
        // Save defaults to settings
        SaveQuickAccessFolders();
    }

    #endregion

    #region Sample Data Loading

    private void LoadSampleQuickAccessFolders()
    {
        LoadQuickAccessFolders();
    }

    private void LoadSampleRecentFolders()
    {
        // TODO: Replace with real IRecentFoldersService when implemented
        // For now, Recent Folders tab will be empty until service exists
    }

    #endregion
}
