using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Fluent;
using EasyAF.Shell.Services;
using Serilog;
using Prism.Ioc;
using System.Diagnostics;
using System.IO;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for the main application window.
/// </summary>
public class MainWindowViewModel : BindableBase
{
    private readonly IThemeService _themeService;
    private readonly IModuleRibbonService _ribbonService;
    private readonly IDocumentManager _documentManager;
    private readonly IUserDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly IContainerProvider _container;

    public MainWindowViewModel(IThemeService themeService,
                               LogViewerViewModel logViewerViewModel,
                               IModuleRibbonService ribbonService,
                               IDocumentManager documentManager,
                               FileCommandsViewModel fileCommands,
                               IUserDialogService dialogService,
                               ISettingsService settingsService,
                               IContainerProvider container)
    {
        _themeService = themeService;
        _ribbonService = ribbonService;
        _documentManager = documentManager;
        _dialogService = dialogService;
        _settingsService = settingsService;
        _container = container;
        FileCommands = fileCommands;
        LogViewerViewModel = logViewerViewModel;

        Documents = _documentManager.OpenDocuments;

        ExitCommand = new DelegateCommand(Exit);
        CloseDocumentCommand = new DelegateCommand<IDocument?>(CloseDocument, CanCloseDocument);
        OpenSettingsCommand = new DelegateCommand(OpenSettings);
        OpenHelpCommand = new DelegateCommand(OpenHelp);
        OpenAboutCommand = new DelegateCommand(OpenAbout);

        // Shell-level document commands
        CloseActiveCommand = new DelegateCommand(() => CloseDocument(SelectedDocument), () => SelectedDocument != null)
            .ObservesProperty(() => SelectedDocument);
        CloseAllCommand = new DelegateCommand(CloseAll, () => Documents.Count > 0)
            .ObservesProperty(() => Documents.Count);
        CloseOthersCommand = new DelegateCommand(CloseOthers, () => Documents.Count > 1 && SelectedDocument != null)
            .ObservesProperty(() => Documents.Count)
            .ObservesProperty(() => SelectedDocument);
        OpenContainingFolderCommand = new DelegateCommand(OpenContainingFolder, () => !string.IsNullOrWhiteSpace(SelectedDocument?.FilePath))
            .ObservesProperty(() => SelectedDocument);

        // System/Settings commands (Help tab)
        OpenLogsFolderCommand = new DelegateCommand(OpenLogsFolder);
        OpenAppDataFolderCommand = new DelegateCommand(OpenAppDataFolder);
        ExportSettingsCommand = new DelegateCommand(ExportSettings);
        ImportSettingsCommand = new DelegateCommand(ImportSettings);

        _documentManager.ActiveDocumentChanged += (_, doc) => SelectedDocument = doc;

        // Initialize Open Backstage ViewModel
        OpenBackstage = _container.Resolve<EasyAF.Shell.ViewModels.Backstage.OpenBackstageViewModel>();
        
        // CROSS-MODULE EDIT: 2025-01-15 Option A Step 3
        // Modified for: Wire OpenBackstage FileSelected event to DocumentManager
        // Related modules: Core (IDocumentManager, IRecentFilesService), Shell (OpenBackstageViewModel)
        // Rollback instructions: Remove FileSelected event subscription below
        
        // Subscribe to backstage file selection to open documents
        OpenBackstage.FileSelected += OnBackstageFileSelected;
    }

    /// <summary>
    /// Handles file selection from the Open backstage.
    /// Attempts to open the file via DocumentManager and adds to recent files.
    /// </summary>
    private void OnBackstageFileSelected(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Log.Warning("BackstageFileSelected received null/empty path");
            return;
        }

        try
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                _dialogService.ShowError($"File not found:\n\n{filePath}", "Open File");
                Log.Warning("Attempted to open non-existent file: {Path}", filePath);
                return;
            }

            // Attempt to open via DocumentManager (will throw if no module can handle it)
            var document = _documentManager.OpenDocument(filePath);
            
            // DocumentManager already adds to recent files via DocumentOpened event handler in FileCommandsViewModel
            // But we'll add it again here to ensure it's at the top of the list
            var recentFilesService = _container.Resolve<IRecentFilesService>();
            recentFilesService.AddRecentFile(filePath);
            
            Log.Information("Opened document from backstage: {Path}", filePath);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No document module"))
        {
            // No module can handle this file type
            var extension = Path.GetExtension(filePath);
            _dialogService.ShowMessage(
                $"No module is available to open '{extension}' files.\n\n" +
                $"File: {Path.GetFileName(filePath)}\n\n" +
                $"Please install a module that supports this file type.",
                "Cannot Open File");
            Log.Warning("No module found for file: {Path}", filePath);
        }
        catch (Exception ex)
        {
            // General error opening file
            _dialogService.ShowError(
                $"Failed to open file:\n\n{Path.GetFileName(filePath)}\n\nError: {ex.Message}",
                "Open File Error");
            Log.Error(ex, "Failed to open file from backstage: {Path}", filePath);
        }
    }

    private void OpenHelp()
    {
        var vm = _container.Resolve<HelpDialogViewModel>();
        var dlg = new Views.HelpDialog { DataContext = vm, Owner = Application.Current.MainWindow };
        dlg.ShowDialog();
    }

    private void OpenAbout()
    {
        var vm = _container.Resolve<AboutDialogViewModel>();
        var dlg = new Views.AboutDialog { DataContext = vm, Owner = Application.Current.MainWindow };
        dlg.ShowDialog();
    }

    /// <summary>
    /// Exposes Open Backstage ViewModel for the Open tab content.
    /// </summary>
    public EasyAF.Shell.ViewModels.Backstage.OpenBackstageViewModel OpenBackstage { get; }

    /// <summary>
    /// Collection of tabs shown in the ribbon contributed by modules (XAML declares Home & Help).
    /// </summary>
    public ObservableCollection<RibbonTabItem> RibbonTabs => _ribbonService.Tabs;

    /// <summary>
    /// Collection of open documents (placeholder until DocumentManager implemented).
    /// </summary>
    public ObservableCollection<IDocument> Documents { get; }

    private IDocument? _selectedDocument;
    /// <summary>
    /// Currently selected document.
    /// </summary>
    public IDocument? SelectedDocument
    {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
    }

    /// <summary>
    /// Command to close a document.
    /// </summary>
    public ICommand CloseDocumentCommand { get; }

    /// <summary>
    /// Gets the command to open the settings dialog.
    /// </summary>
    public ICommand OpenSettingsCommand { get; }

    /// <summary>
    /// Gets the command to open the help dialog.
    /// </summary>
    public ICommand OpenHelpCommand { get; }

    /// <summary>
    /// Gets the command to open the about dialog.
    /// </summary>
    public ICommand OpenAboutCommand { get; }

    /// <summary>
    /// Gets the command to exit the application.
    /// </summary>
    public ICommand ExitCommand { get; }

    // Newly added shell-level commands
    public ICommand CloseActiveCommand { get; }
    public ICommand CloseAllCommand { get; }
    public ICommand CloseOthersCommand { get; }
    public ICommand OpenContainingFolderCommand { get; }
    public ICommand OpenLogsFolderCommand { get; }
    public ICommand OpenAppDataFolderCommand { get; }
    public ICommand ExportSettingsCommand { get; }
    public ICommand ImportSettingsCommand { get; }

    // DEPRECATED (A4): Theme switching now exclusively handled via Options dialog
    public ICommand? SwitchToLightThemeCommand => null;
    public ICommand? SwitchToDarkThemeCommand => null;

    private bool CanCloseDocument(IDocument? doc) => doc != null;

    private void CloseDocument(IDocument? doc)
    {
        if (doc == null) return;
        
        // Use DocumentManager with dirty confirmation callback
        _documentManager.CloseDocument(doc, ConfirmCloseDocument);
    }

    private void CloseAll()
    {
        foreach (var doc in Documents.ToList())
        {
            if (!_documentManager.CloseDocument(doc, ConfirmCloseDocument))
                break; // user canceled
        }
    }

    private void CloseOthers()
    {
        var keep = SelectedDocument;
        if (keep == null) return;
        foreach (var doc in Documents.Where(d => d != keep).ToList())
        {
            if (!_documentManager.CloseDocument(doc, ConfirmCloseDocument))
                break; // user canceled
        }
    }

    private void OpenContainingFolder()
    {
        var path = SelectedDocument?.FilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _dialogService.ShowMessage("No file on disk for this document yet.", "Open Containing Folder");
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open containing folder for {Path}", path);
            _dialogService.ShowError("Unable to open containing folder.");
        }
    }

    private void OpenLogsFolder()
    {
        try
        {
            var logs = Path.Combine(AppContext.BaseDirectory, "logs");
            if (!Directory.Exists(logs)) Directory.CreateDirectory(logs);
            Process.Start(new ProcessStartInfo { FileName = logs, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open logs folder");
            _dialogService.ShowError("Unable to open logs folder.");
        }
    }

    private void OpenAppDataFolder()
    {
        try
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyAF");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            Process.Start(new ProcessStartInfo { FileName = appData, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open app data folder");
            _dialogService.ShowError("Unable to open app data folder.");
        }
    }

    private void ExportSettings()
    {
        try
        {
            // Ensure latest settings saved
            _settingsService.Save();
            var source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyAF", "settings.json");
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Settings",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = "easyaf-settings.json",
                AddExtension = true,
                OverwritePrompt = true
            };
            if (dlg.ShowDialog() == true)
            {
                File.Copy(source, dlg.FileName, true);
                _dialogService.ShowMessage("Settings exported successfully.", "Export Settings");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Export settings failed");
            _dialogService.ShowError("Failed to export settings.");
        }
    }

    private void ImportSettings()
    {
        try
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import Settings",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                var targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyAF");
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                var target = Path.Combine(targetDir, "settings.json");
                File.Copy(dlg.FileName, target, true);
                _settingsService.Reload();
                _dialogService.ShowMessage("Settings imported. Some changes may apply after restart.", "Import Settings");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Import settings failed");
            _dialogService.ShowError("Failed to import settings.");
        }
    }

    /// <summary>
    /// Confirmation callback for closing dirty documents.
    /// </summary>
    private DocumentCloseDecision ConfirmCloseDocument(IDocument doc)
    {
        var result = _dialogService.ConfirmWithCancel(
            $"'{doc.Title}' has unsaved changes. Do you want to save before closing?",
            "Unsaved Changes");

        return result switch
        {
            MessageBoxResult.Yes => DocumentCloseDecision.Save,
            MessageBoxResult.No => DocumentCloseDecision.Discard,
            _ => DocumentCloseDecision.Cancel
        };
    }

    /// <summary>
    /// Gets the log viewer view model.
    /// </summary>
    public LogViewerViewModel LogViewerViewModel { get; }

    private void Exit()
    {
        // Check for dirty documents before exiting
        var dirtyDocs = Documents.Where(d => d.IsDirty).ToList();
        if (dirtyDocs.Count > 0)
        {
            var message = dirtyDocs.Count == 1
                ? $"'{dirtyDocs[0].Title}' has unsaved changes. Save before exiting?"
                : $"{dirtyDocs.Count} documents have unsaved changes. Save all before exiting?";

            var result = _dialogService.ConfirmWithCancel(message, "Unsaved Changes");
            
            if (result == MessageBoxResult.Cancel)
                return;
                
            if (result == MessageBoxResult.Yes)
            {
                // Try to save all dirty documents
                foreach (var doc in dirtyDocs)
                {
                    if (!_documentManager.SaveDocument(doc))
                    {
                        _dialogService.ShowError($"Failed to save '{doc.Title}'.", "Save Error");
                        return; // Abort exit
                    }
                }
            }
        }
        
        Application.Current.Shutdown();
    }

    private void OpenSettings()
    {
        var viewModel = new SettingsDialogViewModel(_themeService, _settingsService);
        var dialog = new Views.SettingsDialog
        {
            DataContext = viewModel,
            Owner = Application.Current.MainWindow
        };

        dialog.ShowDialog();
        Log.Information("Settings dialog opened");
    }

    public IDocumentManager DocumentManager => _documentManager;

    /// <summary>
    /// Gets the file commands view model.
    /// </summary>
    public FileCommandsViewModel FileCommands { get; }
}
