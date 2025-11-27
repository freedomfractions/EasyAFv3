using System.ComponentModel;
using System.IO;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Serilog;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for the Settings Dialog.
/// </summary>
public class SettingsDialogViewModel : BindableBase
{
    private readonly IThemeService _themeService;
    private readonly ISettingsService _settingsService;
    private readonly string _originalTheme;
    private readonly int _originalRecentFilesLimit;
    private IThemeService.ThemeDescriptor? _selectedThemeDescriptor;
    private int _recentFilesLimit;
    private string _recentFilesLimitText = string.Empty;
    private bool? _dialogResult;
    private string _mapsDirectory = string.Empty;
    private string _specsDirectory = string.Empty;
    private string _templatesDirectory = string.Empty;
    private string _outputDirectory = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsDialogViewModel"/> class.
    /// </summary>
    /// <param name="themeService">The theme service.</param>
    /// <param name="settingsService">The settings service.</param>
    /// <param name="mapSettingsViewModel">The Map module settings view model (optional).</param>
    /// <param name="projectSettingsViewModel">The Project module settings view model (optional).</param>
    public SettingsDialogViewModel(
        IThemeService themeService,
        ISettingsService settingsService,
        object? mapSettingsViewModel = null,
        object? projectSettingsViewModel = null)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        _originalTheme = _themeService.CurrentTheme;

        // Load recent files limit setting (default 25, range 3-250)
        _originalRecentFilesLimit = _settingsService.GetSetting("RecentFiles.MaxCount", 25);
        _recentFilesLimit = _originalRecentFilesLimit;
        _recentFilesLimitText = _recentFilesLimit.ToString();

        // Load directory paths
        _mapsDirectory = _settingsService.GetSetting("Directories.Maps", GetDefaultMapsDirectory());
        _specsDirectory = _settingsService.GetSetting("Directories.Specs", GetDefaultSpecsDirectory());
        _templatesDirectory = _settingsService.GetSetting("Directories.Templates", GetDefaultTemplatesDirectory());
        _outputDirectory = _settingsService.GetSetting("Directories.Output", GetDefaultOutputDirectory());

        // Store Map module settings VM if provided
        MapSettingsViewModel = mapSettingsViewModel;
        
        // CROSS-MODULE EDIT: 2025-01-27 Project Module Settings
        // Modified for: Store Project module settings ViewModel
        // Related modules: Project (ProjectModuleSettingsViewModel)
        // Rollback instructions: Remove ProjectSettingsViewModel property and assignment
        
        // Store Project module settings VM if provided
        ProjectSettingsViewModel = projectSettingsViewModel;

        // Initialize commands
        ApplyCommand = new DelegateCommand(Apply);
        OkCommand = new DelegateCommand(Ok);
        CancelCommand = new DelegateCommand(Cancel);
        BrowseMapsDirectoryCommand = new DelegateCommand(BrowseMapsDirectory);
        BrowseSpecsDirectoryCommand = new DelegateCommand(BrowseSpecsDirectory);
        BrowseTemplatesDirectoryCommand = new DelegateCommand(BrowseTemplatesDirectory);
        BrowseOutputDirectoryCommand = new DelegateCommand(BrowseOutputDirectory);

        // Populate theme descriptors
        AvailableThemes = new ObservableCollection<IThemeService.ThemeDescriptor>(_themeService.AvailableThemeDescriptors);

        // Set selected theme
        _selectedThemeDescriptor = AvailableThemes.FirstOrDefault(t => t.Name.Equals(_originalTheme, StringComparison.OrdinalIgnoreCase));

        Log.Debug("SettingsDialogViewModel initialized with theme: {Theme}, RecentFilesLimit: {Limit}", _originalTheme, _recentFilesLimit);
    }

    /// <summary>
    /// Gets the collection of available theme descriptors.
    /// </summary>
    public ObservableCollection<IThemeService.ThemeDescriptor> AvailableThemes { get; }

    /// <summary>
    /// Gets or sets the selected theme descriptor.
    /// </summary>
    public IThemeService.ThemeDescriptor? SelectedThemeDescriptor
    {
        get => _selectedThemeDescriptor;
        set
        {
            if (SetProperty(ref _selectedThemeDescriptor, value) && value != null)
            {
                // Apply theme immediately for live preview
                _themeService.ApplyTheme(value.Name);
                Log.Debug("Theme preview changed to: {Theme}", value.Name);
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of recent files to display (3-250) as text.
    /// </summary>
    /// <remarks>
    /// This property allows free-form text entry. Validation and clamping occurs
    /// when focus is lost or when OK/Apply is clicked, not during typing.
    /// Valid range is 3-250.
    /// </remarks>
    public string RecentFilesLimitText
    {
        get => _recentFilesLimitText;
        set
        {
            if (SetProperty(ref _recentFilesLimitText, value))
            {
                // Try to parse, but don't clamp yet - allow user to type
                if (int.TryParse(value, out var parsed))
                {
                    _recentFilesLimit = parsed;
                }
            }
        }
    }

    /// <summary>
    /// Validates and clamps the recent files limit text input.
    /// Called when the TextBox loses focus or when OK/Apply is clicked.
    /// </summary>
    public void ValidateRecentFilesLimit()
    {
        // Parse the text input
        if (!int.TryParse(_recentFilesLimitText, out var value))
        {
            // Invalid input - revert to last valid value
            value = _recentFilesLimit;
        }

        // Clamp to valid range
        var clamped = ClampRecentFilesLimit(value);
        
        // Update both the value and text
        _recentFilesLimit = clamped;
        RecentFilesLimitText = clamped.ToString();
        
        Log.Debug("Recent files limit validated and clamped to: {Limit}", clamped);
    }

    /// <summary>
    /// Gets the Map module settings view model (null if Map module not loaded).
    /// </summary>
    public object? MapSettingsViewModel { get; }

    /// <summary>
    /// Gets the Project module settings view model (null if Project module not loaded).
    /// </summary>
    /// <remarks>
    /// CROSS-MODULE EDIT: 2025-01-27 Project Module Settings
    /// Modified for: Expose Project module settings to Options dialog
    /// Related modules: Project (ProjectModuleSettingsViewModel)
    /// Rollback instructions: Remove this property
    /// </remarks>
    public object? ProjectSettingsViewModel { get; }

    /// <summary>
    /// Gets or sets the default maps directory path.
    /// </summary>
    public string MapsDirectory
    {
        get => _mapsDirectory;
        set => SetProperty(ref _mapsDirectory, value);
    }

    /// <summary>
    /// Gets or sets the default specs directory path.
    /// </summary>
    public string SpecsDirectory
    {
        get => _specsDirectory;
        set => SetProperty(ref _specsDirectory, value);
    }

    /// <summary>
    /// Gets or sets the default templates directory path.
    /// </summary>
    public string TemplatesDirectory
    {
        get => _templatesDirectory;
        set => SetProperty(ref _templatesDirectory, value);
    }

    /// <summary>
    /// Gets or sets the default output directory path.
    /// </summary>
    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    /// <summary>
    /// Gets the application directory (read-only).
    /// </summary>
    public string ApplicationDirectory => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Gets the settings directory (read-only).
    /// </summary>
    public string SettingsDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasyAF");

    /// <summary>
    /// Gets the Apply command (applies settings without closing dialog).
    /// </summary>
    public ICommand ApplyCommand { get; }

    /// <summary>
    /// Gets the OK command (applies settings and closes dialog).
    /// </summary>
    public ICommand OkCommand { get; }

    /// <summary>
    /// Gets the Cancel command (closes dialog without applying changes).
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets the command to browse for maps directory.
    /// </summary>
    public ICommand BrowseMapsDirectoryCommand { get; }

    /// <summary>
    /// Gets the command to browse for specs directory.
    /// </summary>
    public ICommand BrowseSpecsDirectoryCommand { get; }

    /// <summary>
    /// Gets the command to browse for templates directory.
    /// </summary>
    public ICommand BrowseTemplatesDirectoryCommand { get; }

    /// <summary>
    /// Gets the command to browse for output directory.
    /// </summary>
    public ICommand BrowseOutputDirectoryCommand { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog should close.
    /// </summary>
    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    /// <summary>
    /// Applies current settings to services and persists them.
    /// </summary>
    private void Apply()
    {
        // Validate recent files limit before applying
        ValidateRecentFilesLimit();

        if (SelectedThemeDescriptor != null)
        {
            _themeService.ApplyTheme(SelectedThemeDescriptor.Name);
            _settingsService.SetSetting("Application.Theme", SelectedThemeDescriptor.Name);
        }

        // Save recent files limit
        _settingsService.SetSetting("RecentFiles.MaxCount", _recentFilesLimit);

        // Save directory paths
        _settingsService.SetSetting("Directories.Maps", _mapsDirectory);
        _settingsService.SetSetting("Directories.Specs", _specsDirectory);
        _settingsService.SetSetting("Directories.Templates", _templatesDirectory);
        _settingsService.SetSetting("Directories.Output", _outputDirectory);

        // CROSS-MODULE EDIT: 2025-01-16 Map Module Settings Feature - Step 8
        // Modified for: Save Map module settings when user clicks OK/Apply
        // Related modules: Map (MapModuleSettingsViewModel)
        // Rollback instructions: Remove this block and MapSettingsViewModel property

        // Save Map module settings if available
        if (MapSettingsViewModel is EasyAF.Modules.Map.ViewModels.MapModuleSettingsViewModel mapVm)
        {
            mapVm.SaveSettings();
            Log.Debug("Map module settings saved");
        }

        // CROSS-MODULE EDIT: 2025-01-27 Project Module Settings
        // Modified for: Save Project module settings when user clicks OK/Apply
        // Related modules: Project (ProjectModuleSettingsViewModel)
        // Rollback instructions: Remove this block

        // Save Project module settings if available
        if (ProjectSettingsViewModel is EasyAF.Modules.Project.ViewModels.ProjectModuleSettingsViewModel projectVm)
        {
            projectVm.SaveSettings();
            Log.Debug("Project module settings saved");
        }

        Log.Information("Settings applied: Theme={Theme}, RecentFilesLimit={Limit}", SelectedThemeDescriptor?.Name, _recentFilesLimit);
    }

    private void Ok()
    {
        Apply();
        DialogResult = true;
        Log.Debug("Settings dialog closed with OK");
    }

    private void Cancel()
    {
        // Revert to original theme
        _themeService.ApplyTheme(_originalTheme);

        // Revert recent files limit (no need to persist, just local state)
        _recentFilesLimit = _originalRecentFilesLimit;
        RecentFilesLimitText = _originalRecentFilesLimit.ToString();

        // Reload Map module settings if available (discard changes)
        if (MapSettingsViewModel is EasyAF.Modules.Map.ViewModels.MapModuleSettingsViewModel mapVm)
        {
            mapVm.ReloadSettings();
            Log.Debug("Map module settings reloaded (changes discarded)");
        }

        // CROSS-MODULE EDIT: 2025-01-27 Project Module Settings
        // Modified for: Reload Project module settings on cancel
        // Related modules: Project (ProjectModuleSettingsViewModel)
        // Rollback instructions: Remove this block

        // Reload Project module settings if available (discard changes)
        if (ProjectSettingsViewModel is EasyAF.Modules.Project.ViewModels.ProjectModuleSettingsViewModel projectVm)
        {
            projectVm.ReloadSettings();
            Log.Debug("Project module settings reloaded (changes discarded)");
        }

        DialogResult = false;
        Log.Debug("Settings dialog closed with Cancel, theme reverted to: {Theme}", _originalTheme);
    }

    /// <summary>
    /// Clamps the recent files limit to valid range (3-250).
    /// </summary>
    /// <param name="value">The desired limit.</param>
    /// <returns>Clamped value between 3 and 250.</returns>
    private static int ClampRecentFilesLimit(int value)
    {
        if (value < 3) return 3;
        if (value > 250) return 250;
        return value;
    }

    /// <summary>
    /// Opens a folder browser dialog for selecting the maps directory.
    /// </summary>
    private void BrowseMapsDirectory()
    {
        var folder = BrowseForFolder("Select Maps Directory", _mapsDirectory);
        if (folder != null)
            MapsDirectory = folder;
    }

    /// <summary>
    /// Opens a folder browser dialog for selecting the specs directory.
    /// </summary>
    private void BrowseSpecsDirectory()
    {
        var folder = BrowseForFolder("Select Specs Directory", _specsDirectory);
        if (folder != null)
            SpecsDirectory = folder;
    }

    /// <summary>
    /// Opens a folder browser dialog for selecting the templates directory.
    /// </summary>
    private void BrowseTemplatesDirectory()
    {
        var folder = BrowseForFolder("Select Templates Directory", _templatesDirectory);
        if (folder != null)
            TemplatesDirectory = folder;
    }

    /// <summary>
    /// Opens a folder browser dialog for selecting the output directory.
    /// </summary>
    private void BrowseOutputDirectory()
    {
        var folder = BrowseForFolder("Select Output Directory", _outputDirectory);
        if (folder != null)
            OutputDirectory = folder;
    }

    /// <summary>
    /// Opens a folder browser dialog and returns the selected path.
    /// </summary>
    /// <param name="description">The dialog description text.</param>
    /// <param name="selectedPath">The initially selected path.</param>
    /// <returns>The selected folder path, or null if cancelled.</returns>
    private static string? BrowseForFolder(string description, string selectedPath)
    {
        // Use OpenFolderDialog (available in .NET 8+)
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = description,
            InitialDirectory = Directory.Exists(selectedPath) ? selectedPath : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        return dialog.ShowDialog() == true 
            ? dialog.FolderName 
            : null;
    }

    /// <summary>
    /// Gets the default maps directory path.
    /// </summary>
    private static string GetDefaultMapsDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(appData, "EasyAF", "Maps");
    }

    /// <summary>
    /// Gets the default specs directory path.
    /// </summary>
    private static string GetDefaultSpecsDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(appData, "EasyAF", "Specs");
    }

    /// <summary>
    /// Gets the default templates directory path.
    /// </summary>
    private static string GetDefaultTemplatesDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(appData, "EasyAF", "Templates");
    }

    /// <summary>
    /// Gets the default output directory path.
    /// </summary>
    private static string GetDefaultOutputDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(appData, "EasyAF", "Output");
    }
}
