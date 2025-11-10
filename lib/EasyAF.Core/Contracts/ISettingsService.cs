namespace EasyAF.Core.Contracts;

/// <summary>
/// Service for managing application and module settings with persistence.
/// </summary>
/// <remarks>
/// Settings are stored in JSON format and support hot-reload via file watcher.
/// Module settings are isolated in their own sections.
/// </remarks>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
    /// <returns>The setting value or the default value if not found.</returns>
    T GetSetting<T>(string key, T defaultValue = default!);

    /// <summary>
    /// Sets a setting value by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to set.</param>
    /// <remarks>
    /// Changes are immediately persisted to the settings file.
    /// </remarks>
    void SetSetting<T>(string key, T value);

    /// <summary>
    /// Gets all settings for a specific module.
    /// </summary>
    /// <param name="moduleName">The name of the module.</param>
    /// <returns>A dictionary of module-specific settings.</returns>
    IDictionary<string, object> GetModuleSettings(string moduleName);

    /// <summary>
    /// Sets a module-specific setting.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to set.</param>
    void SetModuleSetting<T>(string moduleName, string key, T value);

    /// <summary>
    /// Gets a module-specific setting value.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
    /// <returns>The setting value or the default value if not found.</returns>
    T GetModuleSetting<T>(string moduleName, string key, T defaultValue = default!);

    /// <summary>
    /// Saves all settings to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Reloads settings from disk.
    /// </summary>
    void Reload();

    /// <summary>
    /// Event raised when settings are changed externally and reloaded.
    /// </summary>
    event EventHandler? SettingsReloaded;
}
