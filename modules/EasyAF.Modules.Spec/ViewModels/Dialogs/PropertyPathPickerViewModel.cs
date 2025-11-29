using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Engine;
using EasyAF.Modules.Spec.Models;
using EasyAF.Modules.Map.Services;
using EasyAF.Core.Contracts;
using EasyAF.Core.Services; // NEW: For DataTypeSettingsExtensions
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels.Dialogs
{
    /// <summary>
    /// View model for the PropertyPath picker dialog.
    /// </summary>
    /// <remarks>
    /// Allows users to select property paths from available data types.
    /// Multi-select with tree view and fuzzy search.
    /// Dynamically loads all data types from EasyAF.Data via reflection.
    /// </remarks>
    public class PropertyPathPickerViewModel : BindableBase
    {
        private string _searchText = string.Empty;
        private bool? _dialogResult;
        private bool _showActiveOnly = true; // NEW: Default to active (enabled) data types only
        private bool _allowMultiSelect = true; // NEW: Control multi-select behavior
        private bool _includeComputedProperties = true; // NEW: Default to include computed properties
        private readonly SpecDocument? _document;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly EasyAF.Core.Contracts.ISettingsService _settingsService; // NEW: For reading visibility settings

        public PropertyPathPickerViewModel(
            string[] currentPaths, 
            SpecDocument? document, 
            IPropertyDiscoveryService propertyDiscovery, 
            EasyAF.Core.Contracts.ISettingsService settingsService,
            bool allowMultiSelect = true,
            bool includeComputedProperties = true)
        {
            _document = document;
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _allowMultiSelect = allowMultiSelect;
            _includeComputedProperties = includeComputedProperties;

            // Initialize collections
            DataTypes = new ObservableCollection<DataTypeNode>();
            SelectedPaths = new HashSet<string>(currentPaths ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            // Setup collection view for filtering
            DataTypesView = CollectionViewSource.GetDefaultView(DataTypes);
            DataTypesView.Filter = FilterDataType;

            // Commands
            OkCommand = new DelegateCommand(ExecuteOk);
            CancelCommand = new DelegateCommand(ExecuteCancel);
            ClearSearchCommand = new DelegateCommand(ExecuteClearSearch, () => !string.IsNullOrWhiteSpace(SearchText))
                .ObservesProperty(() => SearchText);

            // Load data types dynamically from EasyAF.Data
            LoadDataTypes();

            Log.Debug("PropertyPathPickerViewModel initialized with {Count} selected paths (IncludeComputed={Include})", 
                SelectedPaths.Count, _includeComputedProperties);
        }

        #region Properties

        public ObservableCollection<DataTypeNode> DataTypes { get; }
        public ICollectionView DataTypesView { get; }
        public HashSet<string> SelectedPaths { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    DataTypesView.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show only active (enabled) data types.
        /// </summary>
        /// <remarks>
        /// When true, only shows data types that are enabled in Options > Data Types.
        /// When false, shows ALL data types discovered via reflection.
        /// </remarks>
        public bool ShowActiveOnly
        {
            get => _showActiveOnly;
            set
            {
                if (SetProperty(ref _showActiveOnly, value))
                {
                    // Reload data types with new filter
                    ReloadDataTypes();
                    Log.Information("PropertyPath picker filter changed: {Mode}", value ? "Active Only" : "All Data Types");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to include computed properties (like IsAdjustable).
        /// </summary>
        /// <remarks>
        /// When true (default), includes properties marked with [Category("Computed")].
        /// When false, only shows properties that map to CSV/import columns.
        /// Computed properties are useful for filtering/sorting but are not part of data imports.
        /// </remarks>
        public bool IncludeComputedProperties
        {
            get => _includeComputedProperties;
            set
            {
                if (SetProperty(ref _includeComputedProperties, value))
                {
                    // Reload properties with new filter
                    ReloadDataTypes();
                    Log.Information("PropertyPath picker: IncludeComputedProperties = {Include}", value);
                }
            }
        }

        /// <summary>
        /// Gets the count of data types currently displayed.
        /// </summary>
        public int DisplayedTypeCount => DataTypes.Count;

        /// <summary>
        /// Gets the total count of properties currently displayed.
        /// </summary>
        public int DisplayedPropertyCount => DataTypes.Sum(dt => dt.AllProperties.Count);

        public bool? DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        public string[] ResultPaths => SelectedPaths
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .OrderBy(p => p)
            .ToArray();

        /// <summary>
        /// Gets whether multi-select is allowed.
        /// </summary>
        public bool AllowMultiSelect => _allowMultiSelect;

        #endregion

        #region Commands

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ClearSearchCommand { get; }

        #endregion

        #region Methods

        private void LoadDataTypes()
        {
            // CROSS-MODULE EDIT: 2025-11-28 Global Data Type Filtering
            // Modified for: Filter by enabled data types when ShowActiveOnly is true
            // Related modules: Core (DataTypeSettingsExtensions), Map (MapModuleSettingsViewModel)
            // Rollback instructions: Remove ShowActiveOnly logic, always load all types
            
            // Get data types (filtered or all based on toggle)
            List<string> dataTypeNames;
            if (_showActiveOnly)
            {
                // Only enabled data types
                dataTypeNames = _propertyDiscovery.GetAvailableDataTypes()
                    .Where(dt => _settingsService.IsDataTypeEnabled(dt))
                    .ToList();
                Log.Debug("Loading {Count} ACTIVE data types (filtered)", dataTypeNames.Count);
            }
            else
            {
                // All data types discovered via reflection
                dataTypeNames = _propertyDiscovery.GetAvailableDataTypes();
                Log.Debug("Loading {Count} data types (ALL - unfiltered)", dataTypeNames.Count);
            }
            
            // Calculate usage counts across entire spec document
            var usageCounts = CalculateUsageCounts();

            Log.Information("Loading {Count} data types from EasyAF.Data (ShowActiveOnly={ShowActive})", 
                dataTypeNames.Count, _showActiveOnly);

            foreach (var typeName in dataTypeNames)
            {
                // CROSS-MODULE EDIT: 2025-11-28 Property-Level Filtering
                // Modified for: Filter properties by enabled state when ShowActiveOnly is true
                // Related modules: Core (DataTypeSettingsExtensions)
                // Rollback instructions: Use GetAllPropertiesForType unconditionally
                
                // Get properties (filtered or all based on toggle)
                List<EasyAF.Modules.Map.Models.PropertyInfo> properties;
                if (_showActiveOnly)
                {
                    // Only enabled properties (respects property visibility settings)
                    properties = _propertyDiscovery.GetPropertiesForType(typeName);
                    Log.Debug("Data type {TypeName}: {Count} active properties (filtered)", typeName, properties.Count);
                }
                else
                {
                    // All properties (ignores property visibility settings)
                    properties = _propertyDiscovery.GetAllPropertiesForType(typeName);
                    Log.Debug("Data type {TypeName}: {Count} total properties (unfiltered)", typeName, properties.Count);
                }

                // CROSS-MODULE EDIT: 2025-01-19 Computed Properties Filter
                // Modified for: Filter out computed properties when IncludeComputedProperties is false
                // Related modules: Data (LVBreaker.Computed.cs and future computed properties), Map (PropertyInfo.IsComputed)
                // Rollback instructions: Remove this filtering logic
                if (!_includeComputedProperties)
                {
                    var beforeCount = properties.Count;
                    properties = properties.Where(p => !p.IsComputed).ToList();
                    
                    if (beforeCount != properties.Count)
                    {
                        Log.Debug("Filtered {Count} computed properties from {TypeName}", 
                            beforeCount - properties.Count, typeName);
                    }
                }
                
                if (properties.Count == 0)
                {
                    Log.Debug("Skipping data type {TypeName} - no {Mode} properties found", 
                        typeName, _showActiveOnly ? "active" : "total");
                    continue;
                }

                var node = new DataTypeNode
                {
                    TypeName = typeName,
                    FriendlyName = _propertyDiscovery.GetDataTypeDescription(typeName), // Use friendly name from [EasyPowerClass] attribute
                    IsExpanded = false
                };

                foreach (var prop in properties)
                {
                    var fullPath = $"{typeName}.{prop.PropertyName}";
                    var propertyNode = new PropertyNode
                    {
                        PropertyName = prop.PropertyName,
                        FullPath = fullPath,
                        IsSelected = SelectedPaths.Contains(fullPath),
                        UsageCount = usageCounts.GetValueOrDefault(fullPath, 0)
                    };

                    propertyNode.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(PropertyNode.IsSelected) && s is PropertyNode pNode)
                        {
                            if (pNode.IsSelected)
                            {
                                // If single-select mode, deselect all others
                                if (!_allowMultiSelect)
                                {
                                    foreach (var dt in DataTypes)
                                    {
                                        foreach (var p in dt.AllProperties)
                                        {
                                            if (p != pNode && p.IsSelected)
                                            {
                                                p.IsSelected = false; // This will trigger another event
                                            }
                                        }
                                    }
                                }
                                SelectedPaths.Add(pNode.FullPath);
                            }
                            else
                            {
                                SelectedPaths.Remove(pNode.FullPath);
                            }

                            Log.Debug("Property {Path} {Action}", pNode.FullPath, pNode.IsSelected ? "selected" : "deselected");
                        }
                    };

                    node.AllProperties.Add(propertyNode);
                }

                DataTypes.Add(node);
            }

            var totalComputed = DataTypes.Sum(dt => dt.AllProperties.Count(p => 
                _propertyDiscovery.GetAllPropertiesForType(dt.TypeName)
                    .FirstOrDefault(pi => pi.PropertyName == p.PropertyName)?.IsComputed ?? false));

            Log.Information("Loaded {Count} data types with {PropertyCount} total properties ({ComputedCount} computed, ShowActiveOnly={ShowActive}, IncludeComputed={IncludeComputed})", 
                DataTypes.Count, 
                DataTypes.Sum(dt => dt.AllProperties.Count),
                totalComputed,
                _showActiveOnly,
                _includeComputedProperties);
            
            // Notify UI of count changes
            RaisePropertyChanged(nameof(DisplayedTypeCount));
            RaisePropertyChanged(nameof(DisplayedPropertyCount));
        }

        /// <summary>
        /// Reloads data types (called when ShowActiveOnly toggle changes).
        /// </summary>
        private void ReloadDataTypes()
        {
            // Preserve current selections before clearing
            var currentSelections = new HashSet<string>(SelectedPaths);
            
            // Clear and reload
            DataTypes.Clear();
            LoadDataTypes();
            
            // No need to restore selections - they're preserved in SelectedPaths HashSet
            // The LoadDataTypes method reads from SelectedPaths when creating PropertyNodes
        }

        /// <summary>
        /// Calculates how many times each property path is used across all tables in the spec.
        /// </summary>
        private Dictionary<string, int> CalculateUsageCounts()
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (_document?.Spec?.Tables == null)
                return counts;

            foreach (var table in _document.Spec.Tables)
            {
                if (table.Columns == null) continue;

                foreach (var column in table.Columns)
                {
                    if (column.PropertyPaths == null) continue;

                    foreach (var path in column.PropertyPaths)
                    {
                        if (string.IsNullOrWhiteSpace(path)) continue;

                        if (counts.ContainsKey(path))
                            counts[path]++;
                        else
                            counts[path] = 1;
                    }
                }
            }

            Log.Debug("Calculated usage counts for {Count} unique property paths", counts.Count);
            return counts;
        }

        private bool FilterDataType(object obj)
        {
            if (obj is not DataTypeNode node) return false;
            
            // When search is cleared, show all properties
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                // Reset all properties to visible
                foreach (var prop in node.AllProperties)
                {
                    prop.IsVisible = true;
                }
                node.RefreshPropertyFilter();
                return true;
            }

            var search = _searchText.ToLowerInvariant();

            // CROSS-MODULE EDIT: 2025-11-28 Enhanced Search with Property Filtering
            // Modified for: Smart filtering - show all properties for good data type matches, filter properties otherwise
            // Related modules: None
            // Rollback instructions: Restore simple contains() logic
            
            // Score the data type name match (0-100)
            var typeScore = CalculateTypeMatchScore(node, search);
            
            // If data type name is a strong match (?70), show the entire data type with all properties
            if (typeScore >= 70)
            {
                // Clear any property filtering - show all properties
                foreach (var prop in node.AllProperties)
                {
                    prop.IsVisible = true;
                }
                node.RefreshPropertyFilter();
                return true; // Show this data type
            }
            
            // Data type name is not a strong match - check if any properties match
            var hasMatchingProperties = false;
            foreach (var prop in node.AllProperties)
            {
                // Check if property name matches search
                var propertyMatches = prop.PropertyName.ToLowerInvariant().Contains(search);
                
                // Set visibility for this property
                prop.IsVisible = propertyMatches;
                
                if (propertyMatches)
                {
                    hasMatchingProperties = true;
                }
            }
            
            // Refresh the property filter to consolidate results
            node.RefreshPropertyFilter();
            
            // Show this data type only if it has matching properties
            return hasMatchingProperties;
        }

        /// <summary>
        /// Calculates a match score for data type name (0-100).
        /// Higher score = better match.
        /// </summary>
        private int CalculateTypeMatchScore(DataTypeNode node, string search)
        {
            var friendlyLower = node.FriendlyName.ToLowerInvariant();
            var typeLower = node.TypeName.ToLowerInvariant();
            
            // Exact match = 100
            if (friendlyLower == search || typeLower == search)
                return 100;
            
            // Starts with match = 90
            if (friendlyLower.StartsWith(search) || typeLower.StartsWith(search))
                return 90;
            
            // Contains match = 70 (threshold for showing all properties)
            if (friendlyLower.Contains(search) || typeLower.Contains(search))
                return 70;
            
            // No match = 0
            return 0;
        }

        private void ExecuteOk()
        {
            DialogResult = true;
            Log.Information("PropertyPath picker: OK - {Count} paths selected", SelectedPaths.Count);
        }

        private void ExecuteCancel()
        {
            DialogResult = false;
            Log.Debug("PropertyPath picker: Canceled");
        }

        private void ExecuteClearSearch()
        {
            SearchText = string.Empty;
        }

        #endregion
    }

    /// <summary>
    /// Represents a data type node in the tree (e.g., "Bus", "LVCB").
    /// </summary>
    public class DataTypeNode : BindableBase
    {
        private string _typeName = string.Empty;
        private string _friendlyName = string.Empty; // NEW: User-friendly display name
        private bool _isExpanded;

        public DataTypeNode()
        {
            // Create filtered view of properties
            PropertiesView = CollectionViewSource.GetDefaultView(AllProperties);
            PropertiesView.Filter = obj => obj is PropertyNode node && node.IsVisible;
        }

        public string TypeName
        {
            get => _typeName;
            set => SetProperty(ref _typeName, value);
        }

        /// <summary>
        /// Gets or sets the user-friendly display name (e.g., "Buses", "LV Breakers").
        /// </summary>
        public string FriendlyName
        {
            get => string.IsNullOrEmpty(_friendlyName) ? _typeName : _friendlyName;
            set => SetProperty(ref _friendlyName, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Gets the raw collection of all properties (for manipulation).
        /// </summary>
        public ObservableCollection<PropertyNode> AllProperties { get; } = new();

        /// <summary>
        /// Gets the filtered view of properties (for display in TreeView).
        /// </summary>
        /// <remarks>
        /// This automatically filters based on PropertyNode.IsVisible.
        /// Call RefreshPropertyFilter() after changing IsVisible values.
        /// </remarks>
        public ICollectionView PropertiesView { get; }

        /// <summary>
        /// Refreshes the property filter to apply visibility changes.
        /// </summary>
        public void RefreshPropertyFilter()
        {
            PropertiesView.Refresh();
        }
    }

    /// <summary>
    /// Represents a property node in the tree (e.g., "Bus.Name").
    /// </summary>
    public class PropertyNode : BindableBase
    {
        private string _propertyName = string.Empty;
        private string _fullPath = string.Empty;
        private bool _isSelected;
        private bool _isExpanded = false; // FIX: Add IsExpanded property for TreeView binding
        private int _usageCount = 0; // NEW: Track how many times this property is used
        private bool _isVisible = true; // NEW: Track visibility for search filtering

        public string PropertyName
        {
            get => _propertyName;
            set => SetProperty(ref _propertyName, value);
        }

        public string FullPath
        {
            get => _fullPath;
            set => SetProperty(ref _fullPath, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Gets or sets whether the property node is expanded in the tree view.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Gets or sets the number of times this property is used in the current spec.
        /// </summary>
        public int UsageCount
        {
            get => _usageCount;
            set => SetProperty(ref _usageCount, value);
        }

        /// <summary>
        /// Gets the display text for the usage count (e.g., "(3)" or empty if 0).
        /// </summary>
        public string UsageCountDisplay => UsageCount > 0 ? $"({UsageCount})" : string.Empty;

        /// <summary>
        /// Gets or sets whether this property is visible in the tree (for search filtering).
        /// </summary>
        /// <remarks>
        /// When true, property is shown in the tree.
        /// When false, property is hidden (filtered out by search).
        /// </remarks>
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
    }
}
