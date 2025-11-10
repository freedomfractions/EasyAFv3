using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Fluent;
using EasyAF.Shell.Services;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for the main application window.
/// </summary>
public class MainWindowViewModel : BindableBase
{
    private readonly IThemeService _themeService;
    private readonly IModuleRibbonService _ribbonService;
    private readonly IDocumentManager _documentManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="themeService">The theme service for managing application themes.</param>
    /// <param name="logViewerViewModel">The log viewer view model.</param>
    /// <param name="ribbonService">The module ribbon injection service.</param>
    /// <param name="documentManager">The document manager for managing open documents.</param>
    public MainWindowViewModel(IThemeService themeService, LogViewerViewModel logViewerViewModel, IModuleRibbonService ribbonService, IDocumentManager documentManager)
    {
        _themeService = themeService;
        _ribbonService = ribbonService;
        _documentManager = documentManager;
        LogViewerViewModel = logViewerViewModel;

        Documents = _documentManager.OpenDocuments;

        // Initialize commands
        SwitchToLightThemeCommand = new DelegateCommand(SwitchToLightTheme);
        SwitchToDarkThemeCommand = new DelegateCommand(SwitchToDarkTheme);
        ExitCommand = new DelegateCommand(Exit);
        CloseDocumentCommand = new DelegateCommand<IDocument?>(CloseDocument, CanCloseDocument);

        _documentManager.ActiveDocumentChanged += (_, doc) => SelectedDocument = doc;

        // Build Home tab
        BuildHomeTab();
    }

    private void BuildHomeTab()
    {
        var home = new RibbonTabItem { Header = "Home" };

        var fileGroup = new RibbonGroupBox { Header = "File" };
        fileGroup.Items.Add(new Button { Header = "New", SizeDefinition = "Large" });
        fileGroup.Items.Add(new Button { Header = "Open", SizeDefinition = "Large" });
        fileGroup.Items.Add(new Button { Header = "Save", SizeDefinition = "Large" });

        var viewGroup = new RibbonGroupBox { Header = "View" };
        var lightBtn = new Button { Header = "Light Theme" };
        lightBtn.SetBinding(Button.CommandProperty, new System.Windows.Data.Binding(nameof(SwitchToLightThemeCommand)));
        var darkBtn = new Button { Header = "Dark Theme" };
        darkBtn.SetBinding(Button.CommandProperty, new System.Windows.Data.Binding(nameof(SwitchToDarkThemeCommand)));
        viewGroup.Items.Add(lightBtn);
        viewGroup.Items.Add(darkBtn);

        home.Groups.Add(fileGroup);
        home.Groups.Add(viewGroup);

        _ribbonService.Tabs.Insert(0, home);
    }

    /// <summary>
    /// Collection of tabs shown in the ribbon, including Home and module tabs.
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

    private bool CanCloseDocument(IDocument? doc) => doc != null;

    private void CloseDocument(IDocument? doc)
    {
        if (doc == null) return;
        Documents.Remove(doc);
        if (SelectedDocument == doc)
            SelectedDocument = Documents.FirstOrDefault();
    }

    /// <summary>
    /// Gets the log viewer view model.
    /// </summary>
    public LogViewerViewModel LogViewerViewModel { get; }

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
        Application.Current.Shutdown();
    }

    public IDocumentManager DocumentManager => _documentManager;
}
