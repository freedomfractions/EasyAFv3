using System.ComponentModel;
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
    private bool? _dialogResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsDialogViewModel"/> class.
    /// </summary>
    /// <param name="themeService">The theme service.</param>
    /// <param name="settingsService">The settings service.</param>
    public SettingsDialogViewModel(IThemeService themeService, ISettingsService settingsService)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        _originalTheme = _themeService.CurrentTheme;
        
        // Load recent files limit setting (default 15, range 3-99)
        _originalRecentFilesLimit = _settingsService.GetSetting("RecentFiles.MaxCount", 15);
        _recentFilesLimit = ClampRecentFilesLimit(_originalRecentFilesLimit);

        // Initialize commands
        ApplyCommand = new DelegateCommand(Apply);
        OkCommand = new DelegateCommand(Ok);
        CancelCommand = new DelegateCommand(Cancel);

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
    /// Gets or sets the maximum number of recent files to track (3-99).
    /// </summary>
    /// <remarks>
    /// This setting controls how many recent files are shown in the Open backstage.
    /// Valid range is 3-99. Values outside this range are automatically clamped.
    /// </remarks>
    public int RecentFilesLimit
    {
        get => _recentFilesLimit;
        set => SetProperty(ref _recentFilesLimit, ClampRecentFilesLimit(value));
    }

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
        if (SelectedThemeDescriptor != null)
        {
            _themeService.ApplyTheme(SelectedThemeDescriptor.Name);
            _settingsService.SetSetting("Application.Theme", SelectedThemeDescriptor.Name);
        }
        
        // Save recent files limit
        _settingsService.SetSetting("RecentFiles.MaxCount", _recentFilesLimit);
        
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
        
        DialogResult = false;
        Log.Debug("Settings dialog closed with Cancel, theme reverted to: {Theme}", _originalTheme);
    }

    /// <summary>
    /// Clamps the recent files limit to valid range (3-99).
    /// </summary>
    /// <param name="value">The desired limit.</param>
    /// <returns>Clamped value between 3 and 99.</returns>
    private static int ClampRecentFilesLimit(int value)
    {
        if (value < 3) return 3;
        if (value > 99) return 99;
        return value;
    }
}
