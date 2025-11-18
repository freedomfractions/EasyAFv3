using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services;
using EasyAF.Modules.Map.Views;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// ViewModel for the Map Module settings page in the Options dialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides UI for configuring:
    /// - Which data types are enabled (appear as tabs)
    /// - Which properties are enabled for each data type
    /// </para>
    /// <para>
    /// Changes are saved to ISettingsService when user clicks OK in Options dialog.
    /// </para>
    /// </remarks>
    public class MapModuleSettingsViewModel : BindableBase
    {
        private readonly ISettingsService _settingsService;
        private readonly Services.IPropertyDiscoveryService _propertyDiscovery;
        private readonly IUserDialogService _dialogService; // NEW: For confirmation dialogs
        private DataTypeVisibilitySettings _settings;

        /// <summary>
        /// Initializes a new instance of the MapModuleSettingsViewModel.
        /// </summary>
        /// <param name="settingsService">Service for accessing application settings.</param>
        /// <param name="propertyDiscovery">Service for discovering data type properties.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        public MapModuleSettingsViewModel(
            ISettingsService settingsService,
            Services.IPropertyDiscoveryService propertyDiscovery,
            IUserDialogService dialogService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Load current settings
            _settings = _settingsService.GetMapVisibilitySettings();

            // Create data type items
            DataTypes = new ObservableCollection<DataTypeItem>();
            InitializeDataTypes();

            // Commands
            ConfigurePropertiesCommand = new DelegateCommand<DataTypeItem>(ExecuteConfigureProperties);
            ResetToDefaultsCommand = new DelegateCommand(ExecuteResetToDefaults);

            Log.Debug("MapModuleSettingsViewModel initialized with {Count} data types", DataTypes.Count);
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
            var allDataTypes = _propertyDiscovery.GetAvailableDataTypes();

            foreach (var dataType in allDataTypes)
            {
                var config = _settings.GetOrCreateConfig(dataType);
                var allProperties = _propertyDiscovery.GetAllPropertiesForType(dataType);
                var enabledProperties = config.EnabledProperties;

                int enabledCount;
                if (enabledProperties.Contains("*"))
                {
                    enabledCount = allProperties.Count;
                }
                else
                {
                    enabledCount = allProperties.Count(p => enabledProperties.Contains(p.PropertyName));
                }

                var item = new DataTypeItem
                {
                    DataTypeName = dataType,
                    DataTypeDisplayName = _propertyDiscovery.GetDataTypeDescription(dataType), // NEW: Get friendly name
                    IsEnabled = config.Enabled,
                    EnabledPropertiesCount = enabledCount,
                    TotalPropertiesCount = allProperties.Count,
                    AllProperties = allProperties.Select(p => p.PropertyName).ToList(),
                    EnabledProperties = new List<string>(enabledProperties)
                };

                // Subscribe to property changes
                item.PropertyChanged += OnDataTypeItemChanged;

                DataTypes.Add(item);
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
                var defaultProperties = new List<string> { "*" }; // Default: all properties enabled

                // Get all properties with descriptions
                var allPropertiesWithInfo = _propertyDiscovery.GetAllPropertiesForType(item.DataTypeName);

                // CROSS-MODULE EDIT: 2025-01-18 Property Selector Friendly Names
                // Modified for: Pass friendly display name and correct parameter order
                // Related modules: Map (PropertySelectorViewModel constructor now takes dataTypeName AND dataTypeDisplayName)
                // Rollback instructions: Revert to old constructor signature (3 parameters instead of 4)
                
                var viewModel = new PropertySelectorViewModel(
                    item.DataTypeName,           // Internal name (e.g., "LVBreaker")
                    item.DataTypeDisplayName,    // Friendly name (e.g., "LV Breakers")
                    allPropertiesWithInfo,       // Full PropertyInfo objects with descriptions
                    item.EnabledProperties,      // Currently enabled property names
                    defaultProperties);          // Default properties (wildcard)

                var dialog = new PropertySelectorDialog
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    // Update enabled properties
                    item.EnabledProperties = viewModel.GetEnabledProperties();
                    item.EnabledPropertiesCount = viewModel.EnabledCount;

                    Log.Information("Updated {DataType} properties: {Enabled} of {Total} enabled", 
                        item.DataTypeName, item.EnabledPropertiesCount, item.TotalPropertiesCount);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error opening property selector for {DataType}", item.DataTypeName);
            }
        }

        /// <summary>
        /// Executes the reset to defaults command.
        /// </summary>
        /// <remarks>
        /// Prompts the user for confirmation before resetting all data types to wildcard mode.
        /// This ensures accidental resets don't lose custom property configurations.
        /// </remarks>
        private void ExecuteResetToDefaults()
        {
            // CROSS-MODULE EDIT: 2025-01-17 Reset to Defaults Confirmation
            // Modified for: Add confirmation dialog to prevent accidental resets
            // Related modules: Core (IUserDialogService)
            // Rollback instructions: Remove confirmation dialog logic
            
            // Count how many data types will be affected
            var disabledCount = DataTypes.Count(dt => !dt.IsEnabled);
            var customPropertiesCount = DataTypes.Count(dt => 
                dt.EnabledProperties != null && 
                dt.EnabledProperties.Count > 0 && 
                !dt.EnabledProperties.Contains("*"));

            // Build confirmation message
            var message = "Reset all data types to default configuration?\n\n" +
                         "This will:\n" +
                         $"• Enable all {TotalCount} data types\n" +
                         $"• Reset all properties to wildcard mode (*)\n";

            if (customPropertiesCount > 0)
            {
                message += $"\nThis will affect {customPropertiesCount} data type(s) with custom property selections.\n";
            }

            message += "\nThis action cannot be undone.";

            // Show confirmation dialog
            var confirmed = _dialogService.Confirm(message, "Reset to Defaults?");
            if (!confirmed)
            {
                Log.Debug("User canceled reset to defaults");
                return; // User canceled
            }

            try
            {
                Log.Information("Resetting {Count} data types to defaults", DataTypes.Count);

                // Reset all data types
                foreach (var dataType in DataTypes)
                {
                    dataType.IsEnabled = true;
                    dataType.EnabledProperties = new List<string> { "*" };
                    dataType.EnabledPropertiesCount = dataType.AllProperties?.Count ?? 0;
                }

                // Update counts
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

        /// <summary>
        /// Handles property changes on individual DataTypeItem instances.
        /// </summary>
        /// <param name="sender">The DataTypeItem that changed.</param>
        /// <param name="e">Event args containing the property name.</param>
        private void OnDataTypeItemChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataTypeItem.IsEnabled))
            {
                // Enabled state changed - update counts
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
        /// <remarks>
        /// Called by the Options dialog when user clicks OK.
        /// </remarks>
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

                _settingsService.SetMapVisibilitySettings(settings);
                
                Log.Information("Map module settings saved: {Count} data types configured", DataTypes.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save Map module settings");
                throw;
            }
        }

        /// <summary>
        /// Reloads settings from ISettingsService (for Cancel operation).
        /// </summary>
        public void ReloadSettings()
        {
            _settings = _settingsService.GetMapVisibilitySettings();
            DataTypes.Clear();
            InitializeDataTypes();
            
            Log.Debug("Map module settings reloaded");
        }

        #endregion
    }

    /// <summary>
    /// Represents a data type in the settings grid.
    /// </summary>
    public class DataTypeItem : BindableBase
    {
        private string _dataTypeName = string.Empty;
        private string _dataTypeDisplayName = string.Empty; // NEW: User-friendly name
        private bool _isEnabled;
        private int _enabledPropertiesCount;
        private int _totalPropertiesCount;
        private List<string> _enabledProperties = new();

        /// <summary>
        /// Gets or sets the data type name (e.g., "Bus", "LVBreaker") - used internally.
        /// </summary>
        public string DataTypeName
        {
            get => _dataTypeName;
            set => SetProperty(ref _dataTypeName, value);
        }

        /// <summary>
        /// Gets or sets the user-friendly display name (e.g., "Electrical buses/switchgear", "Low Voltage Breakers").
        /// </summary>
        /// <remarks>
        /// This is what appears in the UI grid. The raw class name is stored in <see cref="DataTypeName"/>.
        /// </remarks>
        public string DataTypeDisplayName
        {
            get => _dataTypeDisplayName;
            set => SetProperty(ref _dataTypeDisplayName, value);
        }

        /// <summary>
        /// Gets or sets whether this data type is enabled (tab appears in Map Editor).
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// Gets or sets the count of enabled properties.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the total count of available properties.
        /// </summary>
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

        /// <summary>
        /// Gets the display string for the properties button ("X of Y Properties...").
        /// </summary>
        public string PropertiesDisplay => $"{EnabledPropertiesCount} of {TotalPropertiesCount} Properties...";

        /// <summary>
        /// Gets or sets the list of enabled property names.
        /// </summary>
        public List<string> EnabledProperties
        {
            get => _enabledProperties;
            set => SetProperty(ref _enabledProperties, value);
        }

        /// <summary>
        /// Gets or sets the list of all available property names.
        /// </summary>
        public List<string> AllProperties { get; set; } = new();
    }
}
