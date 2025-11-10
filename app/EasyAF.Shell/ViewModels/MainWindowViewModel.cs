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

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for the main application window.
/// </summary>
public class MainWindowViewModel : BindableBase
{
    private readonly IThemeService _themeService;
    private readonly IModuleRibbonService _ribbonService;
    private readonly IDocumentManager _documentManager;
    // CROSS-MODULE EDIT: 2025-01-11 Task 10
    // Modified for: Add dirty-close confirmation support
    // Related modules: Core (IUserDialogService), Shell (UserDialogService)
    // Rollback instructions: Remove _dialogService field and constructor parameter
    private readonly IUserDialogService _dialogService;
    // CROSS-MODULE EDIT: 2025-01-11 Task 11
    // Modified for: Add settings dialog support
    // Related modules: Core (ISettingsService)
    // Rollback instructions: Remove _settingsService field and constructor parameter
    private readonly ISettingsService _settingsService;
    private readonly IContainerProvider _container;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="themeService">The theme service for managing application themes.</param>
    /// <param name="logViewerViewModel">The log viewer view model.</param>
    /// <param name="ribbonService">The module ribbon injection service.</param>
    /// <param name="documentManager">The document manager for managing open documents.</param>
    /// <param name="fileCommands">The file commands view model.</param>
    /// <param name="dialogService">The dialog service for user confirmations.</param>
    /// <param name="settingsService">The settings service for application settings.</param>
    /// <param name="container">The container provider for resolving services.</param>
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

        // Initialize commands
        SwitchToLightThemeCommand = new DelegateCommand(SwitchToLightTheme);
        SwitchToDarkThemeCommand = new DelegateCommand(SwitchToDarkTheme);
        ExitCommand = new DelegateCommand(Exit);
        CloseDocumentCommand = new DelegateCommand<IDocument?>(CloseDocument, CanCloseDocument);
        // CROSS-MODULE EDIT: 2025-01-11 Task 11
        // Modified for: Add settings dialog support
        // Related modules: Shell (SettingsDialog, SettingsDialogViewModel)
        // Rollback instructions: Remove OpenSettingsCommand
        OpenSettingsCommand = new DelegateCommand(OpenSettings);
        // CROSS-MODULE EDIT: 2025-01-11 SANITY CHECK
        // Modified for: Add Help/About commands for Help ribbon tab
        // Related modules: Shell (HelpDialog, AboutDialog), Core (IHelpProvider via catalog)
        // Rollback instructions: Remove OpenHelpCommand/OpenAboutCommand and Help tab XAML
        OpenHelpCommand = new DelegateCommand(OpenHelp);
        OpenAboutCommand = new DelegateCommand(OpenAbout);

        _documentManager.ActiveDocumentChanged += (_, doc) => SelectedDocument = doc;
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
    /// Gets the command to switch to light theme.
    /// </summary>
    public ICommand SwitchToLightThemeCommand { get; }

    /// <summary>
    /// Gets the command to switch to dark theme.
    /// </summary>
    public ICommand SwitchToDarkThemeCommand { get; }

    /// <summary>
    /// Gets the command to exit the application.
    /// </summary>
    public ICommand ExitCommand { get; }

    private bool CanCloseDocument(IDocument? doc) => doc != null;

    private void CloseDocument(IDocument? doc)
    {
        if (doc == null) return;
        
        // Use DocumentManager with dirty confirmation callback
        _documentManager.CloseDocument(doc, ConfirmCloseDocument);
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

    private void SwitchToLightTheme()
    {
        _themeService.ApplyTheme("Light");
    }

    private void SwitchToDarkTheme()
    {
        _themeService.ApplyTheme("Dark");
    }

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
