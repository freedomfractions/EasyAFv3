using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using EasyAF.Shell.Models.Backstage;

namespace EasyAF.Shell.ViewModels.Backstage;

public enum OpenBackstageMode { Recent, QuickAccessFolder }
public enum RecentTab { Files, Folders }

/// <summary>
/// ViewModel for Open Backstage with Quick Access folder support.
/// </summary>
public class OpenBackstageViewModel : BindableBase
{
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

    public DelegateCommand SelectRecentCommand { get; }
    public DelegateCommand<QuickAccessFolder> SelectQuickAccessFolderCommand { get; }

    public OpenBackstageViewModel()
    {
        QuickAccessFolders = new ObservableCollection<QuickAccessFolder>();

        SelectRecentCommand = new DelegateCommand(ExecuteSelectRecent);
        SelectQuickAccessFolderCommand = new DelegateCommand<QuickAccessFolder>(ExecuteSelectQuickAccessFolder);

        // Initialize with sample Quick Access folders for demonstration
        LoadSampleQuickAccessFolders();
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

    private void LoadSampleQuickAccessFolders()
    {
        // Sample data - in real implementation, load from ISettingsService
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
}
