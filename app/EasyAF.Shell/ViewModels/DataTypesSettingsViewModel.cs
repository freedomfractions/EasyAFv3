using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EasyAF.Core.Contracts;
using EasyAF.Core.Models;
using EasyAF.Core.Services;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;

namespace EasyAF.Shell.ViewModels
{
    /// <summary>
    /// ViewModel for the global Data Types settings tab in the Options dialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides global UI for configuring:
    /// - Which data types are enabled (appear as tabs in Map/Spec modules)
    /// - Which properties are enabled for each data type
    /// </para>
    /// <para>
    /// This was moved from Map module to Shell/Core as part of global data type filtering.
    /// Changes are saved to ISettingsService when user clicks OK in Options dialog.
    /// </para>
    /// </remarks>
    public class DataTypesSettingsViewModel : BindableBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IUserDialogService _dialogService;
        private DataTypeVisibilitySettings _settings;

        /// <summary>
        /// Initializes a new instance of the DataTypesSettingsViewModel.
        /// </summary>
        /// <param name="settingsService">Service for accessing application settings.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        public DataTypesSettingsViewModel(ISettingsService settingsService, IUserDialogService dialogService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Load settings
            _settings = _settingsService.GetDataTypeVisibilitySettings();

            // Create data type items
            DataTypes = new ObservableCollection<DataTypeItem>();
            InitializeDataTypes();

            // Commands
            ConfigurePropertiesCommand = new DelegateCommand<DataTypeItem>(ExecuteConfigureProperties);
            ResetToDefaultsCommand = new DelegateCommand(ExecuteResetToDefaults);

            Log.Debug("DataTypesSettingsViewModel initialized with {Count} data types", DataTypes.Count);
        }

        #region Properties

        /// <summary>
        /// Gets the collection of data type configuration items.
        /// </summary>
        public ObservableCollection<DataTypeItem> DataTypes { get; }

        /// <summary>
        /// Gets the count of enabled data types.
        /// </summary>
        public int EnabledCount => DataTypes.Count(dt => dt.IsEnabled);

        /// <summary>
        /// Gets the total count of data types.
        /// </summary>
        public int TotalCount => DataTypes.Count;

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to configure properties for a data type.
        /// </summary>
        public ICommand ConfigurePropertiesCommand { get; }

        /// <summary>
        /// Gets the command to reset all settings to defaults.
        /// </summary>
        public ICommand ResetToDefaultsCommand { get; }

        #endregion

        #region Initialization

        private void InitializeDataTypes()
        {
            // Get all data types from the DataSet model
            var dataTypes = GetAvailableDataTypes();

            foreach (var dataType in dataTypes)
            {
                var config = _settings.GetOrCreateConfig(dataType);
                var allProperties = GetAllPropertiesForType(dataType);
                var enabledProperties = config.EnabledProperties;

                int enabledCount;
                if (enabledProperties.Contains("*"))
                {
                    enabledCount = allProperties.Count;
                }
                else
                {
                    enabledCount = allProperties.Count(p => enabledProperties.Contains(p));
                }

                var item = new DataTypeItem
                {
                    DataTypeName = dataType,
                    DataTypeDisplayName = GetDataTypeDescription(dataType),
                    IsEnabled = config.Enabled,
                    EnabledPropertiesCount = enabledCount,
                    TotalPropertiesCount = allProperties.Count,
                    AllProperties = allProperties,
                    EnabledProperties = new List<string>(enabledProperties)
                };

                // Subscribe to property changes
                item.PropertyChanged += OnDataTypeItemChanged;

                DataTypes.Add(item);
            }
        }

        /// <summary>
        /// Gets all available data types from the DataSet model.
        /// </summary>
        private List<string> GetAvailableDataTypes()
        {
            try
            {
                // Use reflection on DataSet to find all "*Entries" properties
                var dataSetType = typeof(EasyAF.Data.Models.DataSet);
                var dataModelTypes = dataSetType.GetProperties()
                    .Where(p => p.Name.EndsWith("Entries") && p.PropertyType.IsGenericType)
                    .Select(p =>
                    {
                        var genericArgs = p.PropertyType.GetGenericArguments();
                        return genericArgs.Length >= 2 ? genericArgs[1].Name : null;
                    })
                    .Where(name => !string.IsNullOrEmpty(name))
                    .OrderBy(name => name)
                    .ToList();

                Log.Debug("Discovered {Count} data types", dataModelTypes.Count);
                return dataModelTypes!;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to discover data types");
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets the user-friendly description for a data type.
        /// </summary>
        private string GetDataTypeDescription(string dataTypeName)
        {
            try
            {
                var type = typeof(EasyAF.Data.Models.Bus).Assembly
                    .GetTypes()
                    .FirstOrDefault(t => t.Name == dataTypeName && t.Namespace == "EasyAF.Data.Models");

                if (type != null)
                {
                    var attribute = type.GetCustomAttributes(typeof(EasyAF.Data.Attributes.EasyPowerClassAttribute), false)
                        .FirstOrDefault() as EasyAF.Data.Attributes.EasyPowerClassAttribute;

                    if (attribute != null && !string.IsNullOrWhiteSpace(attribute.EasyPowerClassName))
                    {
                        return attribute.EasyPowerClassName;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get description for {DataType}", dataTypeName);
            }

            return dataTypeName; // Fallback to class name
        }

        /// <summary>
        /// Gets all properties for a data type.
        /// </summary>
        private List<string> GetAllPropertiesForType(string dataTypeName)
        {
            try
            {
                var type = typeof(EasyAF.Data.Models.Bus).Assembly
                    .GetTypes()
                    .FirstOrDefault(t => t.Name == dataTypeName && t.Namespace == "EasyAF.Data.Models");

                if (type == null)
                    return new List<string>();

                var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite)
                    .Where(p => !p.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), false).Any()
                             && !p.GetCustomAttributes(typeof(Newtonsoft.Json.JsonIgnoreAttribute), false).Any())
                    .Select(p => p.Name)
                    .OrderBy(name => name)
                    .ToList();

                return properties;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get properties for {DataType}", dataTypeName);
                return new List<string>();
            }
        }

        #endregion

        #region Command Handlers

        private void ExecuteConfigureProperties(DataTypeItem? item)
        {
            if (item == null)
                return;

            try
            {
                // For now, show a simple message - property selector dialog needs to be moved to Shell too
                _dialogService.ShowMessage(
                    $"Property configuration for {item.DataTypeDisplayName} will be implemented in the property selector dialog.\n\n" +
                    $"Currently: {item.EnabledPropertiesCount} of {item.TotalPropertiesCount} properties enabled.",
                    "Configure Properties");

                // TODO: Move PropertySelectorDialog from Map module to Shell
                // Then use it here to configure properties
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error opening property selector for {DataType}", item.DataTypeName);
            }
        }

        private void ExecuteResetToDefaults()
        {
            var customPropertiesCount = DataTypes.Count(dt => 
                dt.EnabledProperties != null && 
                dt.EnabledProperties.Count > 0 && 
                !dt.EnabledProperties.Contains("*"));

            var message = "Reset all data types to default configuration?\n\n" +
                         "This will:\n" +
                         $"• Enable all {TotalCount} data types\n" +
                         $"• Reset all properties to wildcard mode (*)\n";

            if (customPropertiesCount > 0)
            {
                message += $"\nThis will affect {customPropertiesCount} data type(s) with custom property selections.\n";
            }

            message += "\nThis action cannot be undone.";

            var confirmed = _dialogService.Confirm(message, "Reset to Defaults?");
            if (!confirmed)
            {
                Log.Debug("User canceled reset to defaults");
                return;
            }

            try
            {
                Log.Information("Resetting {Count} data types to defaults", DataTypes.Count);

                foreach (var dataType in DataTypes)
                {
                    dataType.IsEnabled = true;
                    dataType.EnabledProperties = new List<string> { "*" };
                    dataType.EnabledPropertiesCount = dataType.AllProperties?.Count ?? 0;
                }

                RaisePropertyChanged(nameof(EnabledCount));
                RaisePropertyChanged(nameof(TotalCount));

                Log.Information("Reset to defaults complete");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error resetting to defaults");
            }
        }

        #endregion

        #region Event Handlers

        private void OnDataTypeItemChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataTypeItem.IsEnabled))
            {
                RaisePropertyChanged(nameof(EnabledCount));
                RaisePropertyChanged(nameof(TotalCount));
                
                Log.Debug("Data type enabled state changed: {EnabledCount}/{TotalCount} enabled", 
                    EnabledCount, TotalCount);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves the current settings to ISettingsService.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var settings = new DataTypeVisibilitySettings();

                foreach (var item in DataTypes)
                {
                    settings.DataTypes[item.DataTypeName] = new DataTypeConfig
                    {
                        Enabled = item.IsEnabled,
                        EnabledProperties = new List<string>(item.EnabledProperties)
                    };
                }

                _settingsService.SetDataTypeVisibilitySettings(settings);

                Log.Information("Data types settings saved: {Count} data types configured", DataTypes.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save data types settings");
                throw;
            }
        }

        /// <summary>
        /// Reloads settings from ISettingsService (for Cancel operation).
        /// </summary>
        public void ReloadSettings()
        {
            _settings = _settingsService.GetDataTypeVisibilitySettings();
            DataTypes.Clear();
            InitializeDataTypes();
            
            Log.Debug("Data types settings reloaded");
        }

        #endregion
    }

    /// <summary>
    /// Represents a data type in the settings grid.
    /// </summary>
    public class DataTypeItem : BindableBase
    {
        private string _dataTypeName = string.Empty;
        private string _dataTypeDisplayName = string.Empty;
        private bool _isEnabled;
        private int _enabledPropertiesCount;
        private int _totalPropertiesCount;
        private List<string> _enabledProperties = new();

        public string DataTypeName
        {
            get => _dataTypeName;
            set => SetProperty(ref _dataTypeName, value);
        }

        public string DataTypeDisplayName
        {
            get => _dataTypeDisplayName;
            set => SetProperty(ref _dataTypeDisplayName, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public int EnabledPropertiesCount
        {
            get => _enabledPropertiesCount;
            set
            {
                if (SetProperty(ref _enabledPropertiesCount, value))
                {
                    RaisePropertyChanged(nameof(PropertiesDisplay));
                }
            }
        }

        public int TotalPropertiesCount
        {
            get => _totalPropertiesCount;
            set
            {
                if (SetProperty(ref _totalPropertiesCount, value))
                {
                    RaisePropertyChanged(nameof(PropertiesDisplay));
                }
            }
        }

        public string PropertiesDisplay => $"{EnabledPropertiesCount} of {TotalPropertiesCount} Properties...";

        public List<string> EnabledProperties
        {
            get => _enabledProperties;
            set => SetProperty(ref _enabledProperties, value);
        }

        public List<string> AllProperties { get; set; } = new();
    }
}
