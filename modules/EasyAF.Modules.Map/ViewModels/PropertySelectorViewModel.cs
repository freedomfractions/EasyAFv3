using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// ViewModel for the Property Selector dialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allows users to select which properties of a data type should be available
    /// for mapping in the Map Editor. Properties can be enabled/disabled via checkboxes.
    /// </para>
    /// <para>
    /// Features:
    /// - Searchable property list
    /// - Select All / Select None / Reset to Defaults
    /// - Shows property count (X of Y enabled)
    /// - Displays property descriptions from XML documentation
    /// </para>
    /// </remarks>
    public class PropertySelectorViewModel : BindableBase
    {
        private string _dataTypeName;
        private string _dataTypeDisplayName;
        private string _searchText = string.Empty;
        private readonly List<string> _originalEnabledProperties;
        private readonly List<string> _defaultEnabledProperties;

        /// <summary>
        /// Initializes a new instance of the PropertySelectorViewModel.
        /// </summary>
        /// <param name="dataTypeName">The name of the data type being configured.</param>
        /// <param name="dataTypeDisplayName">The friendly display name for the data type.</param>
        /// <param name="allProperties">All available properties for this data type (with descriptions).</param>
        /// <param name="enabledPropertyNames">Currently enabled property names.</param>
        /// <param name="defaultPropertyNames">Default enabled property names (for reset).</param>
        public PropertySelectorViewModel(
            string dataTypeName,
            string dataTypeDisplayName,
            IEnumerable<MapPropertyInfo> allProperties,
            IEnumerable<string> enabledPropertyNames,
            IEnumerable<string>? defaultPropertyNames = null)
        {
            _dataTypeName = dataTypeName ?? throw new ArgumentNullException(nameof(dataTypeName));
            _dataTypeDisplayName = dataTypeDisplayName ?? dataTypeName;
            
            // Store original state for cancel operation
            _originalEnabledProperties = enabledPropertyNames?.ToList() ?? new List<string>();
            _defaultEnabledProperties = defaultPropertyNames?.ToList() ?? new List<string> { "*" };

            // Create property items with descriptions
            Properties = new ObservableCollection<PropertyItem>();
            
            var allPropsList = allProperties?.ToList() ?? new List<MapPropertyInfo>();
            var enabledSet = new HashSet<string>(_originalEnabledProperties);
            var isWildcard = enabledSet.Contains("*");

            foreach (var propInfo in allPropsList.OrderBy(p => p.PropertyName))
            {
                var item = new PropertyItem
                {
                    PropertyName = propInfo.PropertyName,
                    Description = propInfo.Description,
                    PropertyType = propInfo.PropertyType,
                    IsEnabled = isWildcard || enabledSet.Contains(propInfo.PropertyName)
                };

                // Subscribe to property changes to update EnabledCount
                item.PropertyChanged += OnPropertyItemChanged;

                Properties.Add(item);
            }

            // Setup filtered view
            PropertiesView = CollectionViewSource.GetDefaultView(Properties);
            PropertiesView.Filter = FilterProperty;

            // Commands
            SelectAllCommand = new DelegateCommand(ExecuteSelectAll);
            SelectNoneCommand = new DelegateCommand(ExecuteSelectNone);
            ResetToDefaultsCommand = new DelegateCommand(ExecuteResetToDefaults);
            OkCommand = new DelegateCommand(ExecuteOk);
            CancelCommand = new DelegateCommand(ExecuteCancel);

            Log.Debug("PropertySelectorViewModel initialized for {DataType} with {Count} properties", 
                dataTypeName, Properties.Count);
        }

        #region Properties

        /// <summary>
        /// Gets the data type name being configured (internal class name).
        /// </summary>
        public string DataTypeName
        {
            get => _dataTypeName;
            set => SetProperty(ref _dataTypeName, value);
        }

        /// <summary>
        /// Gets the friendly display name for the data type (e.g., "LV Breakers" instead of "LVBreaker").
        /// </summary>
        public string DataTypeDisplayName
        {
            get => _dataTypeDisplayName;
            set => SetProperty(ref _dataTypeDisplayName, value);
        }

        /// <summary>
        /// Gets or sets the search filter text.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    PropertiesView.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets the collection of property items.
        /// </summary>
        public ObservableCollection<PropertyItem> Properties { get; }

        /// <summary>
        /// Gets the filtered view of properties (for search).
        /// </summary>
        public ICollectionView PropertiesView { get; }

        /// <summary>
        /// Gets the count of enabled properties.
        /// </summary>
        public int EnabledCount => Properties.Count(p => p.IsEnabled);

        /// <summary>
        /// Gets the total count of properties.
        /// </summary>
        public int TotalCount => Properties.Count;

        /// <summary>
        /// Gets the dialog result (true = OK, false = Cancel).
        /// </summary>
        public bool? DialogResult { get; private set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to select all properties.
        /// </summary>
        public ICommand SelectAllCommand { get; }

        /// <summary>
        /// Gets the command to deselect all properties.
        /// </summary>
        public ICommand SelectNoneCommand { get; }

        /// <summary>
        /// Gets the command to reset to default property selection.
        /// </summary>
        public ICommand ResetToDefaultsCommand { get; }

        /// <summary>
        /// Gets the OK command.
        /// </summary>
        public ICommand OkCommand { get; }

        /// <summary>
        /// Gets the Cancel command.
        /// </summary>
        public ICommand CancelCommand { get; }

        #endregion

        #region Command Handlers

        private void ExecuteSelectAll()
        {
            foreach (var prop in Properties)
            {
                prop.IsEnabled = true;
            }
            RaisePropertyChanged(nameof(EnabledCount));
            Log.Debug("Selected all properties for {DataType}", DataTypeName);
        }

        private void ExecuteSelectNone()
        {
            foreach (var prop in Properties)
            {
                prop.IsEnabled = false;
            }
            RaisePropertyChanged(nameof(EnabledCount));
            Log.Debug("Deselected all properties for {DataType}", DataTypeName);
        }

        private void ExecuteResetToDefaults()
        {
            var defaultSet = new HashSet<string>(_defaultEnabledProperties);
            var isWildcard = defaultSet.Contains("*");

            foreach (var prop in Properties)
            {
                prop.IsEnabled = isWildcard || defaultSet.Contains(prop.PropertyName);
            }
            RaisePropertyChanged(nameof(EnabledCount));
            Log.Debug("Reset properties to defaults for {DataType}", DataTypeName);
        }

        private void ExecuteOk()
        {
            DialogResult = true;
            RaisePropertyChanged(nameof(DialogResult));
            Log.Debug("PropertySelectorViewModel OK: {Enabled} of {Total} properties enabled for {DataType}", 
                EnabledCount, TotalCount, DataTypeName);
        }

        private void ExecuteCancel()
        {
            DialogResult = false;
            RaisePropertyChanged(nameof(DialogResult));
            Log.Debug("PropertySelectorViewModel cancelled for {DataType}", DataTypeName);
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Filters properties based on search text.
        /// </summary>
        /// <remarks>
        /// Searches both property name and description (case-insensitive).
        /// TODO: Centralize search logic to EasyAF.Core.Utilities.SearchUtility
        /// </remarks>
        private bool FilterProperty(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            if (obj is PropertyItem item)
            {
                // Search in property name
                if (item.PropertyName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    return true;

                // Search in description
                if (!string.IsNullOrWhiteSpace(item.Description) && 
                    item.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles property changes on individual PropertyItem instances.
        /// </summary>
        private void OnPropertyItemChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Result Retrieval

        /// <summary>
        /// Gets the list of enabled property names.
        /// </summary>
        /// <returns>List of property names that are enabled.</returns>
        public List<string> GetEnabledProperties()
        {
            var enabled = Properties.Where(p => p.IsEnabled).Select(p => p.PropertyName).ToList();
            
            // If all properties are enabled, return wildcard
            if (enabled.Count == Properties.Count)
            {
                return new List<string> { "*" };
            }

            return enabled;
        }

        #endregion
    }

    /// <summary>
    /// Represents a property item in the selector dialog.
    /// </summary>
    public class PropertyItem : BindableBase
    {
        private string _propertyName = string.Empty;
        private string? _description;
        private string? _propertyType;
        private bool _isEnabled;

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName
        {
            get => _propertyName;
            set => SetProperty(ref _propertyName, value);
        }

        /// <summary>
        /// Gets or sets the property description (from XML documentation).
        /// </summary>
        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Gets or sets the property type (e.g., "String", "Double?").
        /// </summary>
        public string? PropertyType
        {
            get => _propertyType;
            set => SetProperty(ref _propertyType, value);
        }

        /// <summary>
        /// Gets or sets whether this property is enabled for mapping.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// Gets whether this property has a description.
        /// </summary>
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    }
}
