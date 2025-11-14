using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using EasyAF.Shell.Models.Backstage;

namespace EasyAF.Shell.ViewModels.Backstage;

public enum OpenBackstageMode { Recent, QuickAccessFolder }
public enum RecentTab { Files, Folders }

/// <summary>
/// ViewModel for Open Backstage with Quick Access folder support and sample data.
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

    public OpenBackstageViewModel()
    {
        QuickAccessFolders = new ObservableCollection<QuickAccessFolder>();
        RecentFiles = new ObservableCollection<RecentFileEntry>();
        RecentFolders = new ObservableCollection<RecentFolderEntry>();

        SelectRecentCommand = new DelegateCommand(ExecuteSelectRecent);
        SelectQuickAccessFolderCommand = new DelegateCommand<QuickAccessFolder>(ExecuteSelectQuickAccessFolder);
        TogglePinCommand = new DelegateCommand<RecentFileEntry>(ExecuteTogglePin);

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
        // In real implementation, persist to settings
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
