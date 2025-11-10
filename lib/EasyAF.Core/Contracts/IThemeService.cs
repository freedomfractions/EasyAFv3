namespace EasyAF.Core.Contracts;

/// <summary>
/// Abstraction for runtime theme management in the UI layer.
/// </summary>
/// <remarks>
/// Implementations are responsible for merging the appropriate resource dictionaries
/// into the application and persisting the current selection.
/// </remarks>
public interface IThemeService
{
    /// <summary>
    /// Describes a theme that can be applied, including display metadata and base color mapping for Fluent.Ribbon.
    /// </summary>
    /// <param name="Name">The canonical theme name (e.g., "Light", "Dark").</param>
    /// <param name="DisplayName">The user-friendly display name.</param>
    /// <param name="BaseColor">The base color scheme ("Light" or "Dark") for Fluent.Ribbon.</param>
    /// <param name="Source">The URI to the theme resource dictionary.</param>
    public sealed record ThemeDescriptor(string Name, string DisplayName, string BaseColor, string Source);

    /// <summary>
    /// Gets the name of the currently applied theme.
    /// </summary>
    /// <value>
    /// A canonical theme name matching a resource dictionary (e.g., "Light", "Dark").
    /// </value>
    string CurrentTheme { get; }

    /// <summary>
    /// Gets the list of available theme names presented to the user.
    /// </summary>
    /// <remarks>
    /// Names correspond to XAML dictionaries in the Theme folder.
    /// </remarks>
    IReadOnlyList<string> AvailableThemes { get; }

    /// <summary>
    /// Gets the list of available theme descriptors including display names and base color mapping.
    /// </summary>
    IReadOnlyList<ThemeDescriptor> AvailableThemeDescriptors { get; }

    /// <summary>
    /// Applies the specified theme at runtime.
    /// </summary>
    /// <param name="themeName">The theme name to apply (case-insensitive).</param>
    void ApplyTheme(string themeName);

    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    event EventHandler<string>? ThemeChanged;
}
