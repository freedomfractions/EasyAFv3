using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for the main application window.
/// </summary>
public class MainWindowViewModel : BindableBase
{
    private readonly IThemeService _themeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="themeService">The theme service for managing application themes.</param>
    /// <param name="logViewerViewModel">The log viewer view model.</param>
    public MainWindowViewModel(IThemeService themeService, LogViewerViewModel logViewerViewModel)
    {
        _themeService = themeService;
        LogViewerViewModel = logViewerViewModel;

        Documents = new ObservableCollection<IDocument>();

        // Initialize commands
        SwitchToLightThemeCommand = new DelegateCommand(SwitchToLightTheme);
        SwitchToDarkThemeCommand = new DelegateCommand(SwitchToDarkTheme);
        ExitCommand = new DelegateCommand(Exit);
        CloseDocumentCommand = new DelegateCommand<IDocument?>(CloseDocument, CanCloseDocument);
    }

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
}
