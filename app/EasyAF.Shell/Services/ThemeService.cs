using EasyAF.Core.Contracts;
using System.Windows;
using ControlzEx.Theming;
using Serilog;

namespace EasyAF.Shell.Services;

/// <summary>
/// Manages application theme switching and persistence.
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private string _currentTheme;
    // CROSS-MODULE EDIT: 2025-01-11 Task 2 (Supplemental Fix)
    // Modified for: Prevent stack overflow from recursive theme application & duplicate event firing
    // Related modules: Core (ISettingsService, SettingsManager)
    // Rollback instructions: Restore previous ApplyTheme implementation without re-entry guard and single CurrentTheme assignment
    private bool _isApplyingTheme;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service for persisting theme selection.</param>
    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _currentTheme = "Light";
        _isApplyingTheme = false;
    }

    /// <inheritdoc/>
    public string CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                ThemeChanged?.Invoke(this, value);
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> AvailableThemes => AvailableThemeDescriptors.Select(d => d.Name).ToList();

    /// <inheritdoc/>
    public IReadOnlyList<IThemeService.ThemeDescriptor> AvailableThemeDescriptors { get; } = new[]
    {
        new IThemeService.ThemeDescriptor(
            Name: "Light",
            DisplayName: "Light",
            BaseColor: "Light",
            Source: "/EasyAF.Shell;component/Theme/Light.xaml"),
        new IThemeService.ThemeDescriptor(
            Name: "Dark",
            DisplayName: "Dark",
            BaseColor: "Dark",
            Source: "/EasyAF.Shell;component/Theme/Dark.xaml")
    };

    /// <inheritdoc/>
    public event EventHandler<string>? ThemeChanged;

    /// <inheritdoc/>
    public void ApplyTheme(string themeName)
    {
        // Prevent re-entry
        if (_isApplyingTheme)
        {
            Log.Debug("Skipping recursive theme application for {Theme}", themeName);
            return;
        }

        if (string.IsNullOrWhiteSpace(themeName))
            themeName = "Light";

        var descriptor = AvailableThemeDescriptors.FirstOrDefault(d =>
            d.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase))
            ?? AvailableThemeDescriptors.First();

        // Skip if already applied
        if (CurrentTheme.Equals(descriptor.Name, StringComparison.OrdinalIgnoreCase))
        {
            Log.Debug("Theme {Theme} already applied, skipping", descriptor.Name);
            return;
        }

        try
        {
            _isApplyingTheme = true;

            // Remove existing theme dictionaries
            var resources = Application.Current.Resources.MergedDictionaries;
            var toRemove = resources
                .Where(d => d.Source != null && d.Source.OriginalString.Contains("/Theme/", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var dict in toRemove)
                resources.Remove(dict);

            // Add the new theme dictionary
            resources.Add(new ResourceDictionary { Source = new Uri(descriptor.Source, UriKind.RelativeOrAbsolute) });

            // Apply Fluent.Ribbon base color (safe fail)
            try
            {
                ThemeManager.Current.ChangeTheme(Application.Current, $"{descriptor.BaseColor}.Blue");
            }
            catch { /* ignore */ }

            // Persist only if changed value differs from stored setting to avoid unnecessary writes
            try
            {
                var stored = _settingsService.GetSetting("Theme", "Light");
                if (!string.Equals(stored, descriptor.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _settingsService.SetSetting("Theme", descriptor.Name);
                    Log.Information("Theme changed to {Theme} and persisted to settings", descriptor.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to persist theme selection to settings");
            }
        }
        finally
        {
            _isApplyingTheme = false;
        }

        // Fire change AFTER guard released
        CurrentTheme = descriptor.Name;
    }
}
