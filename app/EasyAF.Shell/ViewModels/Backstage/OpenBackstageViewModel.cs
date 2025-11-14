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

    public DelegateCommand SelectRecentCommand { get; }
    public DelegateCommand<QuickAccessFolder> SelectQuickAccessFolderCommand { get; }
    public DelegateCommand<RecentFileEntry> TogglePinCommand { get; }
    public DelegateCommand BrowseCommand { get; }
    public DelegateCommand<RecentFileEntry> OpenFileCommand { get; }
    public DelegateCommand<RecentFolderEntry> OpenFolderCommand { get; }
    public DelegateCommand<FolderFileEntry> OpenFolderFileCommand { get; }

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

    public OpenBackstageViewModel(IModuleLoader? moduleLoader, ISettingsService settingsService)
    {
        _moduleLoader = moduleLoader;
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        QuickAccessFolders = new ObservableCollection<QuickAccessFolder>();
        _allRecentFiles = new ObservableCollection<RecentFileEntry>();
        _allRecentFolders = new ObservableCollection<RecentFolderEntry>();
        _allFolderFiles = new ObservableCollection<FolderFileEntry>();
        RecentFiles = new ObservableCollection<RecentFileEntry>();
        RecentFolders = new ObservableCollection<RecentFolderEntry>();
        FolderFiles = new ObservableCollection<FolderFileEntry>();

        SelectRecentCommand = new DelegateCommand(ExecuteSelectRecent);
        SelectQuickAccessFolderCommand = new DelegateCommand<QuickAccessFolder>(ExecuteSelectQuickAccessFolder);
        TogglePinCommand = new DelegateCommand<RecentFileEntry>(ExecuteTogglePin);
        BrowseCommand = new DelegateCommand(ExecuteBrowse);
        OpenFileCommand = new DelegateCommand<RecentFileEntry>(ExecuteOpenFile);
        OpenFolderCommand = new DelegateCommand<RecentFolderEntry>(ExecuteOpenFolder);
        OpenFolderFileCommand = new DelegateCommand<FolderFileEntry>(ExecuteOpenFolderFile);

        // Initialize with sample data
        LoadSampleQuickAccessFolders();
        LoadSampleRecentFiles();
        LoadSampleRecentFolders();

        // Initial filter (shows all)
        ApplySearchFilter();
    }

    private void ExecuteSelectRecent()
    {
        Mode = OpenBackstageMode.Recent;
        SelectedQuickAccessFolder = null;
    }

    private void ExecuteSelectQuickAccessFolder(QuickAccessFolder folder)
    {
        if (folder == null) return;
        Mode = OpenBackstageMode.QuickAccessFolder;
        SelectedQuickAccessFolder = folder;
        
        // Load files from the selected folder
        LoadFolderFiles(folder.FolderPath);
    }

    private void ExecuteTogglePin(RecentFileEntry file)
    {
        if (file == null) return;
        file.IsPinned = !file.IsPinned;
        // TODO: Persist to settings via ISettingsService
    }

    private void ExecuteOpenFile(RecentFileEntry file)
    {
        if (file == null) return;
        
        // Visual feedback for demonstration (remove when integrated with real file system)
        System.Windows.MessageBox.Show(
            $"Would open file:\n\n{file.FilePath}\n\nThis event will be handled by MainWindowViewModel to:\n• Close backstage\n• Load file via DocumentManager\n• Update recent files list",
            "File Double-Click (Demo)",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
        
        // Fire the event for parent integration
        FileSelected?.Invoke(file.FilePath);
    }

    private void ExecuteOpenFolder(RecentFolderEntry folder)
    {
        if (folder == null) return;
        
        // Visual feedback for demonstration (remove when integrated with real file system)
        System.Windows.MessageBox.Show(
            $"Would open folder:\n\n{folder.FolderPath}\n\nThis event will be handled by MainWindowViewModel to:\n• Navigate to folder in Quick Access view\n• Or open in file explorer",
            "Folder Double-Click (Demo)",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
        
        // Fire the event for parent integration
        FolderSelected?.Invoke(folder.FolderPath);
    }

    private void ExecuteOpenFolderFile(FolderFileEntry file)
    {
        if (file == null) return;
        
        // Visual feedback for demonstration (remove when integrated with real file system)
        System.Windows.MessageBox.Show(
            $"Would open file:\n\n{file.FilePath}\n\nThis event will be handled by MainWindowViewModel to:\n• Close backstage\n• Load file via DocumentManager\n• Update recent files list",
            "Folder File Double-Click (Demo)",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
        
        // Fire the event for parent integration
        FileSelected?.Invoke(file.FilePath);
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
        }
    }

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
            ApplyFolderFileFilter();
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

    #region Sample Data Loading

    private void LoadSampleQuickAccessFolders()
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
    }

    private void LoadSampleRecentFiles()
    {
        _allRecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Projects\Proposal_2024.docx",
            LastModified = DateTime.Now.AddHours(-2),
            IsPinned = true
        });

        _allRecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Reports\Q4_Analysis.xlsx",
            LastModified = DateTime.Now.AddHours(-5),
            IsPinned = false
        });

        _allRecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Meeting_Notes.txt",
            LastModified = DateTime.Now.AddDays(-1),
            IsPinned = false
        });

        _allRecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Downloads\Invoice_12345.pdf",
            LastModified = DateTime.Now.AddDays(-2),
            IsPinned = true
        });

        _allRecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Projects\Client_Presentation.pptx",
            LastModified = DateTime.Now.AddDays(-3),
            IsPinned = false
        });
    }

    private void LoadSampleRecentFolders()
    {
        _allRecentFolders.Add(new RecentFolderEntry
        {
            FolderPath = @"C:\Users\Documents\Projects",
            LastAccessed = DateTime.Now.AddHours(-1)
        });

        _allRecentFolders.Add(new RecentFolderEntry
        {
            FolderPath = @"C:\Users\Documents\Downloads",
            LastAccessed = DateTime.Now.AddDays(-1)
        });

        _allRecentFolders.Add(new RecentFolderEntry
        {
            FolderPath = @"C:\Users\Documents\Reports",
            LastAccessed = DateTime.Now.AddDays(-4)
        });
    }

    #endregion
}
