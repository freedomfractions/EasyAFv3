using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EasyAF.Shell.Models.Backstage;
using EasyAF.Core.Contracts;
using Microsoft.Win32;

namespace EasyAF.Shell.ViewModels.Backstage;

public enum OpenBackstageMode { Recent, QuickAccessFolder }
public enum RecentTab { Files, Folders }

/// <summary>
/// ViewModel for Open Backstage with Quick Access folder support and sample data.
/// </summary>
public class OpenBackstageViewModel : BindableBase
{
    private readonly IModuleLoader? _moduleLoader;
    private readonly ISettingsService _settingsService;

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
        set => SetProperty(ref _searchText, value);
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
    /// Collection of recent files for the Files tab.
    /// </summary>
    public ObservableCollection<RecentFileEntry> RecentFiles { get; }

    /// <summary>
    /// Collection of recent folders for the Folders tab.
    /// </summary>
    public ObservableCollection<RecentFolderEntry> RecentFolders { get; }

    public DelegateCommand SelectRecentCommand { get; }
    public DelegateCommand<QuickAccessFolder> SelectQuickAccessFolderCommand { get; }
    public DelegateCommand<RecentFileEntry> TogglePinCommand { get; }
    public DelegateCommand BrowseCommand { get; }

    /// <summary>
    /// Event raised when a file is selected (via Browse, double-click, etc.)
    /// The string parameter is the selected file path.
    /// </summary>
    public event Action<string>? FileSelected;

    public OpenBackstageViewModel(IModuleLoader? moduleLoader, ISettingsService settingsService)
    {
        _moduleLoader = moduleLoader; // Nullable - modules may not be loaded yet
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        QuickAccessFolders = new ObservableCollection<QuickAccessFolder>();
        RecentFiles = new ObservableCollection<RecentFileEntry>();
        RecentFolders = new ObservableCollection<RecentFolderEntry>();

        SelectRecentCommand = new DelegateCommand(ExecuteSelectRecent);
        SelectQuickAccessFolderCommand = new DelegateCommand<QuickAccessFolder>(ExecuteSelectQuickAccessFolder);
        TogglePinCommand = new DelegateCommand<RecentFileEntry>(ExecuteTogglePin);
        BrowseCommand = new DelegateCommand(ExecuteBrowse);

        // Initialize with sample data
        LoadSampleQuickAccessFolders();
        LoadSampleRecentFiles();
        LoadSampleRecentFolders();
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
    }

    private void ExecuteTogglePin(RecentFileEntry file)
    {
        if (file == null) return;
        file.IsPinned = !file.IsPinned;
        // TODO: Persist to settings via ISettingsService
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
        var mostRecentFolder = RecentFolders.OrderByDescending(f => f.LastAccessed).FirstOrDefault();
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

    private void LoadSampleQuickAccessFolders()
    {
        QuickAccessFolders.Add(new QuickAccessFolder
        {
            FolderName = "Projects",
            FolderPath = @"C:\Users\Documents\Projects",
            IconGlyph = "\uE8B7"
        });

        QuickAccessFolders.Add(new QuickAccessFolder
        {
            FolderName = "Templates",
            FolderPath = @"C:\Users\Documents\Templates",
            IconGlyph = "\uE8B7"
        });

        QuickAccessFolders.Add(new QuickAccessFolder
        {
            FolderName = "Archives",
            FolderPath = @"C:\Users\Documents\Archives",
            IconGlyph = "\uE8B7"
        });
    }

    private void LoadSampleRecentFiles()
    {
        RecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Projects\Proposal_2024.docx",
            LastModified = DateTime.Now.AddHours(-2),
            IsPinned = true
        });

        RecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Reports\Q4_Analysis.xlsx",
            LastModified = DateTime.Now.AddHours(-5),
            IsPinned = false
        });

        RecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Meeting_Notes.txt",
            LastModified = DateTime.Now.AddDays(-1),
            IsPinned = false
        });

        RecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Downloads\Invoice_12345.pdf",
            LastModified = DateTime.Now.AddDays(-2),
            IsPinned = true
        });

        RecentFiles.Add(new RecentFileEntry
        {
            FilePath = @"C:\Users\Documents\Projects\Client_Presentation.pptx",
            LastModified = DateTime.Now.AddDays(-3),
            IsPinned = false
        });
    }

    private void LoadSampleRecentFolders()
    {
        RecentFolders.Add(new RecentFolderEntry
        {
            FolderPath = @"C:\Users\Documents\Projects",
            LastAccessed = DateTime.Now.AddHours(-1)
        });

        RecentFolders.Add(new RecentFolderEntry
        {
            FolderPath = @"C:\Users\Downloads",
            LastAccessed = DateTime.Now.AddDays(-1)
        });

        RecentFolders.Add(new RecentFolderEntry
        {
            FolderPath = @"C:\Users\Documents\Reports",
            LastAccessed = DateTime.Now.AddDays(-4)
        });
    }
}
