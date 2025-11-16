using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Core.Services;

/// <summary>
/// Implementation of ISettingsService using JSON file storage with hot-reload support.
/// </summary>
public class SettingsManager : ISettingsService, IDisposable
{
    private readonly string _settingsFilePath;
    private readonly FileSystemWatcher _fileWatcher;
    private readonly object _lock = new();
    private ApplicationSettings _settings;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsManager"/> class.
    /// </summary>
    /// <param name="settingsFilePath">Path to the settings JSON file. If null, uses default location.</param>
    public SettingsManager(string? settingsFilePath = null)
    {
        _settingsFilePath = settingsFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasyAF",
            "settings.json");

        // Ensure directory exists
        var directory = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Load or create settings
        _settings = LoadSettings();

        // Setup file watcher for hot-reload
        _fileWatcher = new FileSystemWatcher
        {
            Path = directory ?? ".",
            Filter = Path.GetFileName(_settingsFilePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _fileWatcher.Changed += OnSettingsFileChanged;
        _fileWatcher.EnableRaisingEvents = true;

        Log.Information("Settings manager initialized with file: {SettingsPath}", _settingsFilePath);
    }

    /// <inheritdoc/>
    public event EventHandler? SettingsReloaded;

    /// <inheritdoc/>
    public T GetSetting<T>(string key, T defaultValue = default!)
    {
        if (string.IsNullOrWhiteSpace(key))
            return defaultValue;

        lock (_lock)
        {
            if (_settings.Global.TryGetValue(key, out var value))
            {
                try
                {
                    return ConvertValue<T>(value);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to convert setting {Key} to type {Type}", key, typeof(T).Name);
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public void SetSetting<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        lock (_lock)
        {
            _settings.Global[key] = value!;
            Save();
        }

        Log.Debug("Setting updated: {Key} = {Value}", key, value);
    }

    /// <inheritdoc/>
    public IDictionary<string, object> GetModuleSettings(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            return new Dictionary<string, object>();

        lock (_lock)
        {
            if (_settings.Modules.TryGetValue(moduleName, out var moduleSettings))
            {
                return new Dictionary<string, object>(moduleSettings);
            }

            return new Dictionary<string, object>();
        }
    }

    /// <inheritdoc/>
    public T GetModuleSetting<T>(string moduleName, string key, T defaultValue = default!)
    {
        if (string.IsNullOrWhiteSpace(moduleName) || string.IsNullOrWhiteSpace(key))
            return defaultValue;

        lock (_lock)
        {
            if (_settings.Modules.TryGetValue(moduleName, out var moduleSettings) &&
                moduleSettings.TryGetValue(key, out var value))
            {
                try
                {
                    return ConvertValue<T>(value);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to convert module setting {Module}.{Key} to type {Type}",
                        moduleName, key, typeof(T).Name);
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public void SetModuleSetting<T>(string moduleName, string key, T value)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentNullException(nameof(moduleName));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        lock (_lock)
        {
            if (!_settings.Modules.ContainsKey(moduleName))
            {
                _settings.Modules[moduleName] = new Dictionary<string, object>();
            }

            _settings.Modules[moduleName][key] = value!;
            Save();
        }

        Log.Debug("Module setting updated: {Module}.{Key} = {Value}", moduleName, key, value);
    }

    /// <inheritdoc/>
    public void Save()
    {
        lock (_lock)
        {
            try
            {
                // Temporarily disable file watcher to prevent reload on our own save
                _fileWatcher.EnableRaisingEvents = false;

                var json = JsonSerializer.Serialize(_settings, JsonOptions);
                File.WriteAllText(_settingsFilePath, json);

                Log.Debug("Settings saved to {Path}", _settingsFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save settings to {Path}", _settingsFilePath);
            }
            finally
            {
                // Re-enable file watcher
                _fileWatcher.EnableRaisingEvents = true;
            }
        }

        // Raise SettingsReloaded event to notify subscribers that settings have changed
        // This ensures open editors refresh immediately after user clicks OK/Apply
        SettingsReloaded?.Invoke(this, EventArgs.Empty);
        Log.Debug("SettingsReloaded event raised after save");
    }

    /// <inheritdoc/>
    public void Reload()
    {
        lock (_lock)
        {
            _settings = LoadSettings();
            Log.Information("Settings reloaded from {Path}", _settingsFilePath);
        }

        SettingsReloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Loads settings from the JSON file or creates a new instance if file doesn't exist.
    /// </summary>
    private ApplicationSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            Log.Information("Settings file not found, creating new settings: {Path}", _settingsFilePath);
            var newSettings = new ApplicationSettings();
            
            // Set some default settings
            newSettings.Global["Theme"] = "Light";
            newSettings.Global["LogLevel"] = "Information";
            
            return newSettings;
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<ApplicationSettings>(json, JsonOptions);
            return settings ?? new ApplicationSettings();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings from {Path}, using defaults", _settingsFilePath);
            return new ApplicationSettings();
        }
    }

    /// <summary>
    /// Handles file system watcher events for hot-reload.
    /// </summary>
    private void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce multiple rapid fire events
        Thread.Sleep(100);

        Log.Information("Settings file changed externally, reloading...");
        Reload();
    }

    /// <summary>
    /// Converts a setting value to the requested type.
    /// </summary>
    private static T ConvertValue<T>(object value)
    {
        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), JsonOptions)!;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        // Try to convert via JSON round-trip
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    /// <summary>
    /// Disposes the settings manager and file watcher.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _fileWatcher?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
