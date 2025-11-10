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

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service for persisting theme selection.</param>
    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _currentTheme = "Light";
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
        if (string.IsNullOrWhiteSpace(themeName))
            themeName = "Light";

        var descriptor = AvailableThemeDescriptors.FirstOrDefault(d =>
            d.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase))
            ?? AvailableThemeDescriptors.First();

        // Remove existing theme dictionaries
        var resources = Application.Current.Resources.MergedDictionaries;
        var toRemove = resources
            .Where(d => d.Source != null && d.Source.OriginalString.Contains("/Theme/", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var dict in toRemove)
        {
            resources.Remove(dict);
        }

        // Add the new theme dictionary
        var themeUri = new Uri(descriptor.Source, UriKind.RelativeOrAbsolute);
        resources.Add(new ResourceDictionary { Source = themeUri });

        // Apply Fluent.Ribbon theme using ControlzEx
        try
        {
            var baseColor = descriptor.BaseColor;
            ThemeManager.Current.ChangeTheme(Application.Current, $"{baseColor}.Blue");
        }
        catch
        {
            // Fluent theme change failed, continue with custom theme
        }

        CurrentTheme = descriptor.Name;

        // Persist theme selection to settings
        try
        {
            _settingsService.SetSetting("Theme", descriptor.Name);
            Log.Information("Theme changed to {Theme} and persisted to settings", descriptor.Name);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to persist theme selection to settings");
        }
    }
}
