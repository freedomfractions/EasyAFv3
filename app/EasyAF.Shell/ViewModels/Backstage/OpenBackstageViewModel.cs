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
using EasyAF.Core.Contracts;
using Microsoft.Win32;
using EasyAF.Shell.Services;
using EasyAF.Shell.Helpers;

namespace EasyAF.Shell.ViewModels.Backstage;

public enum OpenBackstageMode { Recent, QuickAccessFolder }
public enum RecentTab { Files, Folders }

/// <summary>
/// ViewModel for Open Backstage with Quick Access folder support and comprehensive search.
/// </summary>
public partial class OpenBackstageViewModel : BindableBase, IDisposable
{
    private bool _disposed; // added for Dispose pattern

    private readonly IModuleLoader? _moduleLoader;
    private readonly ISettingsService _settingsService;
    private readonly IBackstageService? _backstageService;
    private readonly IRecentFilesService _recentFilesService;

    // Simplified search debounce
    private CancellationTokenSource? _searchCts;

    private OpenBackstageMode _mode = OpenBackstageMode.Recent;
    public OpenBackstageMode Mode { get => _mode; set => SetProperty(ref _mode, value); }

    private RecentTab _recentTab = RecentTab.Files;
    public RecentTab RecentTab { get => _recentTab; set => SetProperty(ref _recentTab, value); }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ScheduleSearch();
            }
        }
    }

    private QuickAccessFolder? _selectedQuickAccessFolder;
    public QuickAccessFolder? SelectedQuickAccessFolder { get => _selectedQuickAccessFolder; set => SetProperty(ref _selectedQuickAccessFolder, value); }

    public ObservableCollection<QuickAccessFolder> QuickAccessFolders { get; }
    private readonly ObservableCollection<RecentFileEntry> _allRecentFiles;
    private readonly ObservableCollection<RecentFolderEntry> _allRecentFolders;
    private readonly ObservableCollection<FolderFileEntry> _allFolderFiles;
    private readonly ObservableCollection<FolderBrowserEntry> _allBrowserEntries;
    public ObservableCollection<RecentFileEntry> RecentFiles { get; }
    public ObservableCollection<RecentFolderEntry> RecentFolders { get; }
    public ObservableCollection<FolderFileEntry> FolderFiles { get; }
    public ObservableCollection<FolderBrowserEntry> BrowserEntries { get; }

    private string _currentBrowsePath = string.Empty;
    public string CurrentBrowsePath
    {
        get => _currentBrowsePath;
        set { if (SetProperty(ref _currentBrowsePath, value)) RaisePropertyChanged(nameof(CanNavigateUp)); }
    }

    public bool CanNavigateUp
    {
        get
        {
            if (string.IsNullOrEmpty(CurrentBrowsePath)) return false;
            try { return new DirectoryInfo(CurrentBrowsePath).Parent != null; } catch { return false; }
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

    public event Action<string>? FileSelected;
    public event Action<string>? FolderSelected;
    public event EventHandler? FocusSearchRequested;
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

        LoadSampleQuickAccessFolders();
        _recentFilesService.RecentFiles.CollectionChanged += OnRecentFilesChanged;
        LoadRecentFilesFromService();
        LoadSampleRecentFolders();
        ApplySearchFilter();
    }

    // Command methods retained here; heavy logic for Quick Access, Browser & Search moved to partial files.

    private void ExecuteSelectRecent()
    {
        Mode = OpenBackstageMode.Recent;
        SelectedQuickAccessFolder = null;
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => ScrollToTopRequested?.Invoke(this, EventArgs.Empty), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void ExecuteSelectQuickAccessFolder(QuickAccessFolder folder)
    {
        if (folder == null) return;
        if (SelectedQuickAccessFolder != null) SelectedQuickAccessFolder.IsSelected = false;
        Mode = OpenBackstageMode.QuickAccessFolder;
        SelectedQuickAccessFolder = folder;
        folder.IsSelected = true;
        CurrentBrowsePath = folder.FolderPath;
        LoadBrowserEntries(CurrentBrowsePath);
        NavigateUpCommand.RaiseCanExecuteChanged();
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => ScrollToTopRequested?.Invoke(this, EventArgs.Empty), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void ExecuteTogglePin(RecentFileEntry file)
    {
        if (file == null) return;
        file.IsPinned = !file.IsPinned;
        // Refresh default view so grouping/regrouping occurs without collection mutation hack
        var view = System.Windows.Data.CollectionViewSource.GetDefaultView(RecentFiles);
        view?.Refresh();
        SavePinnedFiles();
    }

    private void ExecuteOpenFile(RecentFileEntry file)
    {
        if (file == null) return;
        FileSelected?.Invoke(file.FilePath);
        _backstageService?.RequestClose();
    }

    private void ExecuteOpenFolder(RecentFolderEntry folder)
    {
        if (folder == null) return;
        FolderSelected?.Invoke(folder.FolderPath);
        _backstageService?.RequestClose();
    }

    private void ExecuteOpenFolderFile(FolderFileEntry file)
    {
        if (file == null) return;
        FileSelected?.Invoke(file.FilePath);
        _backstageService?.RequestClose();
    }

    private void ExecuteOpenBrowserEntry(FolderBrowserEntry entry)
    {
        if (entry == null) return;
        if (entry.IsFolder)
        {
            CurrentBrowsePath = entry.FullPath;
            LoadBrowserEntries(CurrentBrowsePath);
            NavigateUpCommand.RaiseCanExecuteChanged();
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => ScrollToTopRequested?.Invoke(this, EventArgs.Empty), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        else
        {
            FileSelected?.Invoke(entry.FullPath);
            _backstageService?.RequestClose();
        }
    }

    private void ExecuteNavigateUp()
    {
        try
        {
            var dir = new DirectoryInfo(CurrentBrowsePath);
            if (dir.Parent != null)
            {
                CurrentBrowsePath = dir.Parent.FullName;
                LoadBrowserEntries(CurrentBrowsePath);
                NavigateUpCommand.RaiseCanExecuteChanged();
                System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => ScrollToTopRequested?.Invoke(this, EventArgs.Empty), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NavigateUp error: {ex.Message}");
        }
    }
    private bool CanExecuteNavigateUp() => CanNavigateUp;

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
        var mostRecentFolder = _allRecentFolders.OrderByDescending(f => f.LastAccessed).FirstOrDefault();
        if (mostRecentFolder != null && Directory.Exists(mostRecentFolder.FolderPath)) dialog.InitialDirectory = mostRecentFolder.FolderPath;
        if (dialog.ShowDialog() == true)
        {
            SaveDefaultFilterIndex(dialog.FilterIndex);
            FileSelected?.Invoke(dialog.FileName);
            _backstageService?.RequestClose();
        }
    }

    private void ExecuteCopyPath(RecentFileEntry file)
    {
        if (file == null) return;
        TryClipboard(() => System.Windows.Clipboard.SetText(file.FilePath), "copy file path", file.FilePath);
    }
    private void ExecuteCopyFolderPath(RecentFolderEntry folder)
    {
        if (folder == null) return;
        TryClipboard(() => System.Windows.Clipboard.SetText(folder.FolderPath), "copy folder path", folder.FolderPath);
    }
    private void ExecuteCopyBrowserPath(FolderBrowserEntry entry)
    {
        if (entry == null) return;
        TryClipboard(() => System.Windows.Clipboard.SetText(entry.FullPath), "copy browser path", entry.FullPath);
    }

    private static void TryClipboard(Action action, string context, string value)
    {
        try
        {
            action();
            System.Windows.MessageBox.Show($"Path copied to clipboard:\n\n{value}", "Path Copied", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Clipboard error ({context}): {ex.Message}");
        }
    }

    private void ExecuteRemoveFromList(RecentFileEntry file)
    {
        if (file == null) return;
        _allRecentFiles.Remove(file);
        RecentFiles.Remove(file);
        _recentFilesService.RemoveRecentFile(file.FilePath);
        if (file.IsPinned) SavePinnedFiles();
    }

    private void ExecuteRemoveFolderFromListCommand(RecentFolderEntry folder)
    {
        if (folder == null) return;
        _allRecentFolders.Remove(folder);
        RecentFolders.Remove(folder);
    }

    private void ExecuteOpenFileLocation(RecentFileEntry file)
    {
        if (file == null) return;
        try
        {
            var full = Path.GetFullPath(file.FilePath);
            if (File.Exists(full))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "explorer.exe", Arguments = $"/select,\"{full}\"", UseShellExecute = true });
            }
            else
            {
                var dir = Path.GetDirectoryName(full);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dir, UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenFileLocation error: {ex.Message}");
        }
    }

    private void ExecuteFocusSearch() => FocusSearchRequested?.Invoke(this, EventArgs.Empty);

    // Quick Access add/remove methods now in partial QuickAccess file.

    // Search + filtering now in partial Search file.

    // Browser loading/filtering now in partial Browser file.

    // File type filter + recent files integration retained.

    private string BuildFileTypeFilter()
    {
        var sb = new StringBuilder();
        var types = GetAvailableFileTypes();
        if (types.Count > 0)
        {
            var all = string.Join(";", types.Select(t => $"*.{t.Extension}"));
            sb.Append($"All Supported Files ({all})|{all}|");
            foreach (var t in types.OrderBy(t => t.Description))
                sb.Append($"{t.Description} (*.{t.Extension})|*.{t.Extension}|");
        }
        sb.Append("All Files (*.*)|*.*");
        return sb.ToString();
    }

    private System.Collections.Generic.List<FileTypeDefinition> GetAvailableFileTypes()
    {
        var list = new System.Collections.Generic.List<FileTypeDefinition>();
        if (_moduleLoader != null && _moduleLoader.LoadedModules.Any())
        {
            foreach (var m in _moduleLoader.LoadedModules)
            {
                if (m.SupportedFileTypes?.Count > 0) list.AddRange(m.SupportedFileTypes);
                else if (m.SupportedFileExtensions?.Length > 0)
                    foreach (var ext in m.SupportedFileExtensions)
                        list.Add(new FileTypeDefinition(ext, $"{ext.ToUpper()} Files"));
            }
        }
        if (list.Count == 0)
        {
            list.Add(new FileTypeDefinition("ezmap", "EasyAF Map Files"));
            list.Add(new FileTypeDefinition("ezaf", "EasyAF Project Files"));
            list.Add(new FileTypeDefinition("ezspec", "EasyAF Spec Files"));
        }
        return list;
    }

    private int GetDefaultFilterIndex() => _settingsService.GetSetting("OpenDialog.DefaultFilterIndex", 1);
    private void SaveDefaultFilterIndex(int idx) => _settingsService.SetSetting("OpenDialog.DefaultFilterIndex", idx);

    private void LoadRecentFilesFromService()
    {
        _allRecentFiles.Clear();
        foreach (var fp in _recentFilesService.RecentFiles.Take(_recentFilesService.MaxDisplayCount))
        {
            if (File.Exists(fp))
            {
                var info = new FileInfo(fp);
                _allRecentFiles.Add(new RecentFileEntry { FilePath = fp, LastModified = info.LastWriteTime, IsPinned = IsPinnedFile(fp) });
            }
        }
        ApplySearchFilter();
    }

    private void OnRecentFilesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => LoadRecentFilesFromService();
    private bool IsPinnedFile(string path) => _settingsService.GetSetting<System.Collections.Generic.List<string>>("OpenBackstage.PinnedFiles", new System.Collections.Generic.List<string>()).Contains(path, StringComparer.OrdinalIgnoreCase);
    private void SavePinnedFiles() => _settingsService.SetSetting("OpenBackstage.PinnedFiles", _allRecentFiles.Where(f => f.IsPinned).Select(f => f.FilePath).ToList());

    // Quick Access persistence in partial.

    private void LoadSampleRecentFolders() { /* placeholder */ }

    #region IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        if (disposing)
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _recentFilesService.RecentFiles.CollectionChanged -= OnRecentFilesChanged;
        }
    }
    ~OpenBackstageViewModel() => Dispose(false);
    #endregion
}
