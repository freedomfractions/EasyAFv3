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
/// ViewModel for the main application window (EasyAF Shell).
/// </summary>
/// <remarks>
/// <para>
/// This is the root ViewModel for the entire application shell, responsible for coordinating:
/// - Document management (open/close/switch)
/// - Ribbon customization from modules
/// - File operations (New/Open/Save/SaveAs)
/// - Settings and Help dialogs
/// - Application lifecycle (exit, unsaved changes)
/// </para>
/// <para>
/// The ViewModel aggregates several sub-ViewModels:
/// - <see cref="FileCommands"/>: New/Open/Save operations
/// - <see cref="OpenBackstage"/>: Open file backstage UI
/// - <see cref="LogViewerViewModel"/>: Status bar log viewer
/// </para>
/// <para>
/// Document content display is handled via DataTemplates that modules register.
/// The shell provides the chrome (ribbon, tabs, status bar) while modules provide the content.
/// </para>
/// </remarks>
public class MainWindowViewModel : BindableBase
{
    private readonly IThemeService _themeService;
    private readonly IModuleRibbonService _ribbonService;
    private readonly IDocumentManager _documentManager;
    private readonly IUserDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly IContainerProvider _container;
    private bool _isFileTabStripVisible = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="themeService">Theme management service.</param>
    /// <param name="logViewerViewModel">Log viewer for status bar.</param>
    /// <param name="ribbonService">Service for dynamic ribbon tab management.</param>
    /// <param name="documentManager">Document lifecycle manager.</param>
    /// <param name="fileCommands">File operation commands (New/Open/Save).</param>
    /// <param name="dialogService">Service for showing dialogs.</param>
    /// <param name="settingsService">Application settings service.</param>
    /// <param name="container">DI container for resolving dependencies.</param>
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
        
        // CROSS-MODULE EDIT: 2025-01-20 Vertical File Tab System
        // Modified for: Add FileTabManager for left-side vertical file tabs
        // Related modules: Shell (FileTabManagerViewModel, FileTabItemViewModel, FileTabGroupViewModel)
        // Rollback instructions: Remove FileTabManager property and initialization
        
        // Initialize file tab manager for vertical file list
        FileTabManager = new FileTabManagerViewModel(documentManager);

        ExitCommand = new DelegateCommand(Exit);
        CloseDocumentCommand = new DelegateCommand<IDocument?>(CloseDocument, CanCloseDocument);
        OpenSettingsCommand = new DelegateCommand(OpenSettings);
        OpenHelpCommand = new DelegateCommand(OpenHelp);
        OpenAboutCommand = new DelegateCommand(OpenAbout);
        
        // File tab strip toggle command
        ToggleFileTabStripCommand = new DelegateCommand(ToggleFileTabStrip);

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
    /// </summary>
    /// <param name="filePath">Full path to the selected file.</param>
    /// <remarks>
    /// <para>
    /// This method attempts to open the file via DocumentManager and handles various error cases:
    /// - File not found: Shows error dialog
    /// - No module available: Shows informative message about installing modules
    /// - General errors: Shows error dialog with exception details
    /// </para>
    /// <para>
    /// On success, the file is added to the recent files list.
    /// </para>
    /// </remarks>
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
            
            // Add to recent files
            var recentFilesService = _container.Resolve<IRecentFilesService>();
            recentFilesService.AddRecentFile(filePath);
            
            // Track the folder containing this file
            var recentFoldersService = _container.Resolve<IRecentFoldersService>();
            var folderPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                recentFoldersService.AddRecentFolder(folderPath);
            }
            
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

    /// <summary>
    /// Opens the Help dialog showing aggregated help content from all loaded modules.
    /// </summary>
    private void OpenHelp()
    {
        var vm = _container.Resolve<HelpDialogViewModel>();
        var dlg = new Views.HelpDialog { DataContext = vm, Owner = Application.Current.MainWindow };
        dlg.ShowDialog();
    }

    /// <summary>
    /// Opens the About dialog showing application version and loaded modules.
    /// </summary>
    private void OpenAbout()
    {
        var vm = _container.Resolve<AboutDialogViewModel>();
        var dlg = new Views.AboutDialog { DataContext = vm, Owner = Application.Current.MainWindow };
        dlg.ShowDialog();
    }

    /// <summary>
    /// Gets the Open Backstage ViewModel for the backstage Open tab content.
    /// </summary>
    public EasyAF.Shell.ViewModels.Backstage.OpenBackstageViewModel OpenBackstage { get; }

    /// <summary>
    /// Gets the collection of ribbon tabs contributed by loaded modules.
    /// </summary>
    /// <remarks>
    /// Static tabs (Home, Help) are defined in XAML. This collection holds dynamically
    /// injected tabs from modules via <see cref="IModuleRibbonService"/>.
    /// </remarks>
    public ObservableCollection<RibbonTabItem> RibbonTabs => _ribbonService.Tabs;

    /// <summary>
    /// Gets the collection of currently open documents.
    /// </summary>
    /// <remarks>
    /// This collection is bound to the document tab strip on the left side of the window.
    /// It automatically updates when documents are opened or closed via <see cref="IDocumentManager"/>.
    /// </remarks>
    public ObservableCollection<IDocument> Documents { get; }
    
    /// <summary>
    /// Gets the file tab manager for the vertical file tab list.
    /// </summary>
    /// <remarks>
    /// Manages the vertical file tab UI on the left side, including Welcome tab,
    /// file groups by module type, and tab selection synchronization.
    /// </remarks>
    public FileTabManagerViewModel FileTabManager { get; }

    private IDocument? _selectedDocument;
    
    /// <summary>
    /// Gets or sets the currently selected (active) document.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property is synchronized with <see cref="IDocumentManager.ActiveDocument"/>.
    /// When changed, it triggers ribbon tab updates to show the selected document's module tabs.
    /// </para>
    /// <para>
    /// The document's content is displayed in the main content area via DataTemplates.
    /// </para>
    /// </remarks>
    public IDocument? SelectedDocument
    {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
    }

    /// <summary>
    /// Gets the command to close a specific document.
    /// </summary>
    /// <remarks>
    /// If the document has unsaved changes, prompts the user via <see cref="ConfirmCloseDocument"/>.
    /// </remarks>
    public ICommand CloseDocumentCommand { get; }

    /// <summary>
    /// Gets the command to open the application settings dialog.
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
    /// <remarks>
    /// Checks for unsaved changes in all documents before exiting.
    /// If dirty documents exist, prompts user to save/discard/cancel.
    /// </remarks>
    public ICommand ExitCommand { get; }

    /// <summary>
    /// Gets the command to close the currently active document.
    /// </summary>
    public ICommand CloseActiveCommand { get; }
    
    /// <summary>
    /// Gets the command to close all open documents.
    /// </summary>
    /// <remarks>
    /// Prompts for unsaved changes on each dirty document.
    /// If any prompt is canceled, the operation stops.
    /// </remarks>
    public ICommand CloseAllCommand { get; }
    
    /// <summary>
    /// Gets the command to close all documents except the active one.
    /// </summary>
    public ICommand CloseOthersCommand { get; }
    
    /// <summary>
    /// Gets the command to open Windows Explorer to the active document's containing folder.
    /// </summary>
    /// <remarks>
    /// Only enabled when <see cref="SelectedDocument"/> has a <see cref="IDocument.FilePath"/>.
    /// </remarks>
    public ICommand OpenContainingFolderCommand { get; }
    
    /// <summary>
    /// Gets the command to open the application logs folder in Windows Explorer.
    /// </summary>
    public ICommand OpenLogsFolderCommand { get; }
    
    /// <summary>
    /// Gets the command to open the application data folder in Windows Explorer.
    /// </summary>
    /// <remarks>
    /// Application data folder is typically: %AppData%\EasyAF
    /// Contains settings.json and other persisted application state.
    /// </remarks>
    public ICommand OpenAppDataFolderCommand { get; }
    
    /// <summary>
    /// Gets the command to export application settings to a JSON file.
    /// </summary>
    public ICommand ExportSettingsCommand { get; }
    
    /// <summary>
    /// Gets the command to import application settings from a JSON file.
    /// </summary>
    /// <remarks>
    /// Imported settings take effect immediately via hot-reload mechanism.
    /// Some settings may require application restart.
    /// </remarks>
    public ICommand ImportSettingsCommand { get; }

    // DEPRECATED (A4): Theme switching now exclusively handled via Options dialog
    
    /// <summary>
    /// Gets the command to switch to light theme (deprecated).
    /// </summary>
    /// <remarks>
    /// Returns null. Theme switching is now handled exclusively via the Options dialog.
    /// Retained for potential future ribbon UI if theme switching is re-added to ribbon.
    /// </remarks>
    public ICommand? SwitchToLightThemeCommand => null;
    
    /// <summary>
    /// Gets the command to switch to dark theme (deprecated).
    /// </summary>
    /// <remarks>
    /// Returns null. Theme switching is now handled exclusively via the Options dialog.
    /// Retained for potential future ribbon UI if theme switching is re-added to ribbon.
    /// </remarks>
    public ICommand? SwitchToDarkThemeCommand => null;

    /// <summary>
    /// Determines whether a document can be closed.
    /// </summary>
    /// <param name="doc">The document to check.</param>
    /// <returns>True if the document is not null; otherwise false.</returns>
    private bool CanCloseDocument(IDocument? doc) => doc != null;

    /// <summary>
    /// Closes a document, prompting for unsaved changes if necessary.
    /// </summary>
    /// <param name="doc">The document to close.</param>
    private void CloseDocument(IDocument? doc)
    {
        if (doc == null) return;
        
        // Use DocumentManager with dirty confirmation callback
        _documentManager.CloseDocument(doc, ConfirmCloseDocument);
    }

    /// <summary>
    /// Closes all open documents, prompting for unsaved changes on each dirty document.
    /// </summary>
    /// <remarks>
    /// If any save prompt is canceled, the operation stops immediately.
    /// </remarks>
    private void CloseAll()
    {
        foreach (var doc in Documents.ToList())
        {
            if (!_documentManager.CloseDocument(doc, ConfirmCloseDocument))
                break; // user canceled
        }
    }

    /// <summary>
    /// Closes all documents except the currently selected one.
    /// </summary>
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

    /// <summary>
    /// Opens Windows Explorer to the selected document's containing folder.
    /// </summary>
    /// <remarks>
    /// If the file exists, Explorer opens with the file selected.
    /// If the document hasn't been saved yet, shows an informative message.
    /// </remarks>
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

    /// <summary>
    /// Opens the application logs folder in Windows Explorer.
    /// </summary>
    /// <remarks>
    /// Creates the folder if it doesn't exist.
    /// Logs folder is typically in the application base directory.
    /// </remarks>
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

    /// <summary>
    /// Opens the application data folder in Windows Explorer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Application data folder: %AppData%\EasyAF
    /// Contains settings.json and other persisted state.
    /// </para>
    /// <para>
    /// Creates the folder if it doesn't exist.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Exports application settings to a user-selected JSON file.
    /// </summary>
    /// <remarks>
    /// Copies the current settings.json file to the chosen location.
    /// Settings are saved before export to ensure latest state.
    /// </remarks>
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

    /// <summary>
    /// Imports application settings from a user-selected JSON file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Copies the selected file to the application data folder as settings.json.
    /// Settings are reloaded immediately via <see cref="ISettingsService.Reload"/>.
    /// </para>
    /// <para>
    /// Some settings may require application restart to take full effect.
    /// </para>
    /// </remarks>
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
    /// Callback for confirming close of documents with unsaved changes.
    /// </summary>
    /// <param name="doc">The document being closed.</param>
    /// <returns>User's decision: Save, Discard, or Cancel.</returns>
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
    /// Gets the log viewer ViewModel for the status bar.
    /// </summary>
    public LogViewerViewModel LogViewerViewModel { get; }

    /// <summary>
    /// Exits the application, prompting to save unsaved changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If any documents have unsaved changes:
    /// - Shows confirmation dialog with Save/Don't Save/Cancel options
    /// - On Save: Attempts to save all dirty documents, aborts exit if any save fails
    /// - On Don't Save: Closes without saving
    /// - On Cancel: Aborts the exit operation
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Opens the application settings dialog.
    /// </summary>
    /// <remarks>
    /// Settings changes are persisted via <see cref="ISettingsService"/> when the user clicks OK.
    /// Theme changes apply immediately via live preview.
    /// </remarks>
    private void OpenSettings()
    {
        // CROSS-MODULE EDIT: 2025-01-16 Map Module Settings Feature - Step 8
        // Modified for: Pass Map module settings ViewModel to SettingsDialog
        // Related modules: Map (MapModuleSettingsViewModel)
        // Rollback instructions: Remove mapSettingsVm resolution and parameter passing
        
        // Try to resolve Map module settings ViewModel (may be null if Map module not loaded)
        object? mapSettingsVm = null;
        try
        {
            mapSettingsVm = _container.Resolve<EasyAF.Modules.Map.ViewModels.MapModuleSettingsViewModel>();
        }
        catch
        {
            // Map module not loaded or registered - no problem, dialog will hide the Map tab
            Log.Debug("Map module settings not available (module not loaded)");
        }
        
        // CROSS-MODULE EDIT: 2025-01-27 Project Module Settings
        // Modified for: Pass Project module settings ViewModel to SettingsDialog
        // Related modules: Project (ProjectModuleSettingsViewModel)
        // Rollback instructions: Remove projectSettingsVm resolution and parameter passing
        
        // Try to resolve Project module settings ViewModel (may be null if Project module not loaded)
        object? projectSettingsVm = null;
        try
        {
            projectSettingsVm = _container.Resolve<EasyAF.Modules.Project.ViewModels.ProjectModuleSettingsViewModel>();
        }
        catch
        {
            // Project module not loaded or registered - no problem, dialog will hide the Project tab
            Log.Debug("Project module settings not available (module not loaded)");
        }
        
        var viewModel = new SettingsDialogViewModel(_themeService, _settingsService, mapSettingsVm, projectSettingsVm);
        var dialog = new Views.SettingsDialog
        {
            DataContext = viewModel,
            Owner = Application.Current.MainWindow
        };

        dialog.ShowDialog();
        Log.Information("Settings dialog opened");
    }

    /// <summary>
    /// Gets the document manager for direct access to document lifecycle operations.
    /// </summary>
    public IDocumentManager DocumentManager => _documentManager;

    /// <summary>
    /// Gets the file commands ViewModel exposing New/Open/Save operations.
    /// </summary>
    /// <remarks>
    /// This property is bound to the Backstage "New" tab and provides file operation commands.
    /// </remarks>
    public FileCommandsViewModel FileCommands { get; }
    
    /// <summary>
    /// Gets or sets whether the file tab strip is visible.
    /// </summary>
    public bool IsFileTabStripVisible
    {
        get => _isFileTabStripVisible;
        set
        {
            if (SetProperty(ref _isFileTabStripVisible, value))
            {
                RaisePropertyChanged(nameof(FileTabStripChevronGlyph));
                RaisePropertyChanged(nameof(FileTabStripToggleTooltip));
            }
        }
    }
    
    /// <summary>
    /// Gets the chevron glyph for the file tab strip toggle button.
    /// </summary>
    public string FileTabStripChevronGlyph => IsFileTabStripVisible ? "\uE76B" : "\uE76C"; // ChevronLeft : ChevronRight
    
    /// <summary>
    /// Gets the tooltip text for the file tab strip toggle button.
    /// </summary>
    public string FileTabStripToggleTooltip => IsFileTabStripVisible ? "Hide file list" : "Show file list";
    
    /// <summary>
    /// Gets the command to toggle the file tab strip visibility.
    /// </summary>
    public ICommand ToggleFileTabStripCommand { get; }
    
    /// <summary>
    /// Toggles the file tab strip visibility.
    /// </summary>
    private void ToggleFileTabStrip()
    {
        IsFileTabStripVisible = !IsFileTabStripVisible;
        Log.Debug("File tab strip visibility toggled: {Visible}", IsFileTabStripVisible);
    }
}
