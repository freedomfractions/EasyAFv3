using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Mvvm;
using Prism.Commands;
using EasyAF.Import;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services;
using EasyAF.Core.Contracts;
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// View model for a single data type mapping tab (e.g., "Bus", "LVCB").
    /// </summary>
    /// <remarks>
    /// This VM manages the mapping interface for one data type, providing:
    /// - Source column list (from sample files)
    /// - Target property list (from reflection)
    /// - Mapping operations (Map, Unmap, Auto-Map)
    /// - Filtering and search
    /// - Validation
    /// </remarks>
    public class DataTypeMappingViewModel : BindableBase
    {
        private readonly MapDocument _document;
        private readonly string _dataType;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly MapDocumentViewModel _parentViewModel;
        private readonly ColumnExtractionService _columnExtraction;
        private readonly ISettingsService _settingsService;

        private string _sourceFilter = string.Empty;
        private string _targetFilter = string.Empty;
        private ColumnInfo? _selectedSourceColumn;
        private MapPropertyInfo? _selectedTargetProperty;
        private TableReference _selectedTable;

        /// <summary>
        /// Initializes a new instance of the DataTypeMappingViewModel.
        /// </summary>
        public DataTypeMappingViewModel(
            MapDocument document,
            string dataType,
            IPropertyDiscoveryService propertyDiscovery,
            MapDocumentViewModel parentViewModel,
            ISettingsService settingsService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _columnExtraction = new ColumnExtractionService();

            // Initialize collections
            SourceColumns = new ObservableCollection<ColumnInfo>();
            TargetProperties = new ObservableCollection<MapPropertyInfo>();
            AvailableTables = new ObservableCollection<TableReference>();
            ComboBoxItems = new ObservableCollection<ComboBoxItemBase>();

            // Setup collection views for filtering
            SourceColumnsView = CollectionViewSource.GetDefaultView(SourceColumns);
            SourceColumnsView.Filter = FilterSourceColumn;

            TargetPropertiesView = CollectionViewSource.GetDefaultView(TargetProperties);
            TargetPropertiesView.Filter = FilterTargetProperty;

            // Initialize commands
            MapSelectedCommand = new DelegateCommand(ExecuteMapSelected, CanExecuteMapSelected);
            UnmapSelectedCommand = new DelegateCommand(ExecuteUnmapSelected, CanExecuteUnmapSelected);
            AutoMapCommand = new DelegateCommand(ExecuteAutoMap);
            ClearMappingsCommand = new DelegateCommand(ExecuteClearMappings);
            ManageFieldsCommand = new DelegateCommand(ExecuteManageFields);

            // Load initial data
            LoadTargetProperties();
            LoadAvailableTables();

            Log.Debug("DataTypeMappingViewModel initialized for {DataType}", _dataType);
        }

        #region Properties

        /// <summary>
        /// Gets the data type this VM represents.
        /// </summary>
        public string DataType => _dataType;

        /// <summary>
        /// Gets the collection of source columns.
        /// </summary>
        public ObservableCollection<ColumnInfo> SourceColumns { get; }

        /// <summary>
        /// Gets the filtered view of source columns.
        /// </summary>
        public ICollectionView SourceColumnsView { get; }

        /// <summary>
        /// Gets the collection of target properties.
        /// </summary>
        public ObservableCollection<MapPropertyInfo> TargetProperties { get; }

        /// <summary>
        /// Gets the filtered view of target properties.
        /// </summary>
        public ICollectionView TargetPropertiesView { get; }

        /// <summary>
        /// Gets the collection of available table references.
        /// </summary>
        public ObservableCollection<TableReference> AvailableTables { get; }

        /// <summary>
        /// Gets the flat collection of ComboBox items (headers + tables).
        /// </summary>
        public ObservableCollection<ComboBoxItemBase> ComboBoxItems { get; }

        /// <summary>
        /// Gets or sets the selected ComboBox item (will be a TableItem when selected).
        /// </summary>
        private ComboBoxItemBase? _selectedComboBoxItem;
        public ComboBoxItemBase? SelectedComboBoxItem
        {
            get => _selectedComboBoxItem;
            set
            {
                if (SetProperty(ref _selectedComboBoxItem, value))
                {
                    // Extract the TableReference if it's a TableItem
                    if (value is TableItem tableItem)
                    {
                        SelectedTable = tableItem.TableReference;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the source column filter text.
        /// </summary>
        public string SourceFilter
        {
            get => _sourceFilter;
            set
            {
                if (SetProperty(ref _sourceFilter, value))
                {
                    SourceColumnsView.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the target property filter text.
        /// </summary>
        public string TargetFilter
        {
            get => _targetFilter;
            set
            {
                if (SetProperty(ref _targetFilter, value))
                {
                    TargetPropertiesView.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected source column.
        /// </summary>
        public ColumnInfo? SelectedSourceColumn
        {
            get => _selectedSourceColumn;
            set
            {
                if (SetProperty(ref _selectedSourceColumn, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected target property.
        /// </summary>
        public MapPropertyInfo? SelectedTargetProperty
        {
            get => _selectedTargetProperty;
            set
            {
                if (SetProperty(ref _selectedTargetProperty, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected table reference.
        /// </summary>
        public TableReference SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    OnTableChanged(value);
                }
            }
        }

        /// <summary>
        /// Gets the count of mapped properties.
        /// </summary>
        public int MappedCount =>
            _document.MappingsByDataType.TryGetValue(_dataType, out var mappings)
                ? mappings.Count
                : 0;

        /// <summary>
        /// Gets the count of available properties.
        /// </summary>
        public int AvailableCount => TargetProperties.Count;

        #endregion

        #region Commands

        /// <summary>
        /// Command to map the selected source column to the selected target property.
        /// </summary>
        public ICommand MapSelectedCommand { get; }

        /// <summary>
        /// Command to unmap the selected target property.
        /// </summary>
        public ICommand UnmapSelectedCommand { get; }

        /// <summary>
        /// Command to automatically map columns using intelligent matching.
        /// </summary>
        public ICommand AutoMapCommand { get; }

        /// <summary>
        /// Command to clear all mappings for this data type.
        /// </summary>
        public ICommand ClearMappingsCommand { get; }

        /// <summary>
        /// Command to open the property selector dialog.
        /// </summary>
        public ICommand ManageFieldsCommand { get; }

        #endregion

        #region Public Methods for Refreshing

        /// <summary>
        /// Refreshes the available tables from the document's referenced files.
        /// </summary>
        /// <remarks>
        /// This should be called whenever files are added/removed from the document.
        /// </remarks>
        public void RefreshAvailableTables()
        {
            LoadAvailableTables();
        }

        /// <summary>
        /// Refreshes the property list for this data type (called when settings change).
        /// </summary>
        public void RefreshTargetProperties()
        {
            // Get the current list of visible properties (after settings filter)
            var visibleProperties = _propertyDiscovery.GetPropertiesForType(_dataType);
            var visiblePropertyNames = new HashSet<string>(visibleProperties.Select(p => p.PropertyName));

            // Remove mappings to properties that are no longer visible
            if (_document.MappingsByDataType.TryGetValue(_dataType, out var existingMappings))
            {
                var mappingsToRemove = existingMappings
                    .Where(m => !visiblePropertyNames.Contains(m.PropertyName))
                    .ToList();

                foreach (var mapping in mappingsToRemove)
                {
                    _document.RemoveMapping(_dataType, mapping.PropertyName);
                    Log.Information("Removed mapping to hidden property: {DataType}.{Property} (was mapped to {Column})", 
                        _dataType, mapping.PropertyName, mapping.ColumnHeader);
                }

                if (mappingsToRemove.Any())
                {
                    // Update source columns to clear their mapping indicators
                    foreach (var mapping in mappingsToRemove)
                    {
                        var sourceCol = SourceColumns.FirstOrDefault(c => c.ColumnName == mapping.ColumnHeader);
                        if (sourceCol != null)
                        {
                            sourceCol.IsMapped = false;
                            sourceCol.MappedTo = null;
                        }
                    }
                }
            }

            // Now reload the properties list (only visible properties)
            LoadTargetProperties();

            // Refresh views to update UI
            SourceColumnsView.Refresh();
            TargetPropertiesView.Refresh();
            RaisePropertyChanged(nameof(MappedCount));
            RaisePropertyChanged(nameof(AvailableCount));

            // Update parent tab status
            UpdateTabStatus();

            Log.Information("Refreshed properties for {DataType} based on updated settings ({Visible} visible, {Mapped} mapped)", 
                _dataType, visibleProperties.Count, MappedCount);
        }

        #endregion

        #region Data Loading

        /// <summary>
        /// Loads target properties for this data type.
        /// </summary>
        private void LoadTargetProperties()
        {
            var properties = _propertyDiscovery.GetPropertiesForType(_dataType);
            
            TargetProperties.Clear();
            foreach (var prop in properties)
            {
                // Check if property is already mapped
                var isMapped = _document.MappingsByDataType.TryGetValue(_dataType, out var mappings) &&
                               mappings.Any(m => m.PropertyName == prop.PropertyName);

                prop.IsMapped = isMapped;
                if (isMapped)
                {
                    var mapping = mappings!.First(m => m.PropertyName == prop.PropertyName);
                    prop.MappedColumn = mapping.ColumnHeader;
                }

                TargetProperties.Add(prop);
            }

            Log.Debug("Loaded {Count} properties for {DataType}", properties.Count, _dataType);
        }

        /// <summary>
        /// Loads available tables from referenced files.
        /// </summary>
        private void LoadAvailableTables()
        {
            AvailableTables.Clear();
            ComboBoxItems.Clear();

            // First pass: count tables per file
            var tablesByFile = new Dictionary<string, List<TableReference>>();
            
            foreach (var file in _document.ReferencedFiles)
            {
                try
                {
                    var columns = _columnExtraction.ExtractColumns(file.FilePath);
                    
                    if (!tablesByFile.ContainsKey(file.FilePath))
                    {
                        tablesByFile[file.FilePath] = new List<TableReference>();
                    }
                    
                    foreach (var tableName in columns.Keys)
                    {
                        tablesByFile[file.FilePath].Add(new TableReference 
                        { 
                            TableName = tableName, 
                            FilePath = file.FilePath 
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to extract columns from {FilePath}", file.FilePath);
                }
            }

            // Second pass: build flat list with headers and items
            foreach (var fileGroup in tablesByFile.OrderBy(kvp => System.IO.Path.GetFileName(kvp.Key)))
            {
                bool isMultiTable = fileGroup.Value.Count > 1;
                string fileName = System.IO.Path.GetFileName(fileGroup.Key);
                
                Log.Debug("File {FileName} has {Count} tables (IsMultiTable={IsMultiTable})", 
                    fileName, fileGroup.Value.Count, isMultiTable);
                
                // Add file header ONLY for multi-table files
                if (isMultiTable)
                {
                    ComboBoxItems.Add(new FileHeaderItem { FileName = fileName });
                    Log.Debug("  Added header for multi-table file: {FileName}", fileName);
                }
                
                foreach (var tableRef in fileGroup.Value)
                {
                    tableRef.IsMultiTableFile = isMultiTable;
                    AvailableTables.Add(tableRef);
                    
                    // Add table item to ComboBox
                    ComboBoxItems.Add(new TableItem(tableRef));

                    Log.Debug("  Added table: {TableName} (IsMultiTable={IsMultiTable}, indented={Indented})", 
                        tableRef.TableName, tableRef.IsMultiTableFile, isMultiTable);
                }
            }

            // DO NOT auto-select any table - require explicit user selection
            // This prevents accidental mappings to the wrong table
            SelectedTable = null;
            SelectedComboBoxItem = null;
            
            // Clear source columns since no table is selected
            SourceColumns.Clear();

            Log.Information("Loaded {Count} available tables from {FileCount} files ({ItemCount} ComboBox items) - No table pre-selected", 
                AvailableTables.Count, tablesByFile.Count, ComboBoxItems.Count);
        }

        /// <summary>
        /// Handles table selection change.
        /// </summary>
        private void OnTableChanged(TableReference? tableRef)
        {
            if (tableRef == null || string.IsNullOrEmpty(tableRef.TableName)) return;

            try
            {
                var columns = _columnExtraction.ExtractColumns(tableRef.FilePath);
                if (columns.TryGetValue(tableRef.TableName, out var columnList))
                {
                    LoadSourceColumns(columnList);
                    Log.Information("Loaded {Count} columns from table '{Table}' in file '{File}'", 
                        columnList.Count, tableRef.TableName, tableRef.FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error loading table {Table} from {File}", tableRef.TableName, tableRef.FileName);
            }
        }

        /// <summary>
        /// Loads source columns into the UI.
        /// </summary>
        private void LoadSourceColumns(System.Collections.Generic.List<ColumnInfo> columns)
        {
            SourceColumns.Clear();
            
            foreach (var column in columns)
            {
                // Check if column is already mapped
                column.IsMapped = _document.MappingsByDataType.TryGetValue(_dataType, out var mappings) &&
                                  mappings.Any(m => m.ColumnHeader == column.ColumnName);

                if (column.IsMapped)
                {
                    var mapping = mappings!.First(m => m.ColumnHeader == column.ColumnName);
                    column.MappedTo = $"{_dataType}.{mapping.PropertyName}";
                }

                SourceColumns.Add(column);
            }
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Filters source columns based on search text.
        /// </summary>
        private bool FilterSourceColumn(object obj)
        {
            if (obj is not ColumnInfo column) return false;
            if (string.IsNullOrWhiteSpace(_sourceFilter)) return true;

            return column.ColumnName.Contains(_sourceFilter, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Filters target properties based on search text.
        /// </summary>
        private bool FilterTargetProperty(object obj)
        {
            if (obj is not MapPropertyInfo property) return false;
            if (string.IsNullOrWhiteSpace(_targetFilter)) return true;

            return property.PropertyName.Contains(_targetFilter, StringComparison.OrdinalIgnoreCase) ||
                   (property.Description?.Contains(_targetFilter, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        #endregion

        #region Command Implementations

        private bool CanExecuteMapSelected()
        {
            return SelectedSourceColumn != null && SelectedTargetProperty != null;
        }

        private void ExecuteMapSelected()
        {
            if (SelectedSourceColumn == null || SelectedTargetProperty == null) return;

            try
            {
                // Update document mapping
                _document.UpdateMapping(_dataType, SelectedTargetProperty.PropertyName, SelectedSourceColumn.ColumnName);

                // Update UI state
                SelectedTargetProperty.IsMapped = true;
                SelectedTargetProperty.MappedColumn = SelectedSourceColumn.ColumnName;
                SelectedSourceColumn.IsMapped = true;
                SelectedSourceColumn.MappedTo = $"{_dataType}.{SelectedTargetProperty.PropertyName}";

                // Refresh views
                SourceColumnsView.Refresh();
                TargetPropertiesView.Refresh();
                RaisePropertyChanged(nameof(MappedCount));

                // Update command states immediately
                UpdateCommandStates();

                // Update parent tab status
                UpdateTabStatus();

                Log.Information("Mapped {Column} -> {DataType}.{Property}",
                    SelectedSourceColumn.ColumnName, _dataType, SelectedTargetProperty.PropertyName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create mapping");
            }
        }

        private bool CanExecuteUnmapSelected()
        {
            return SelectedTargetProperty?.IsMapped == true;
        }

        private void ExecuteUnmapSelected()
        {
            if (SelectedTargetProperty == null) return;

            try
            {
                // Find and remove from source columns
                var mappedColumn = SelectedTargetProperty.MappedColumn;
                if (mappedColumn != null)
                {
                    var sourceCol = SourceColumns.FirstOrDefault(c => c.ColumnName == mappedColumn);
                    if (sourceCol != null)
                    {
                        sourceCol.IsMapped = false;
                        sourceCol.MappedTo = null;
                    }
                }

                // Update document
                _document.RemoveMapping(_dataType, SelectedTargetProperty.PropertyName);

                // Update UI state
                SelectedTargetProperty.IsMapped = false;
                SelectedTargetProperty.MappedColumn = null;

                // Refresh views
                SourceColumnsView.Refresh();
                TargetPropertiesView.Refresh();
                RaisePropertyChanged(nameof(MappedCount));

                // Update parent tab status
                UpdateTabStatus();

                Log.Information("Unmapped {DataType}.{Property}", _dataType, SelectedTargetProperty.PropertyName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to remove mapping");
            }
        }

        private void ExecuteAutoMap()
        {
            // TODO: Implement auto-mapping logic using AutoMappingService
            Log.Information("Auto-mapping requested for {DataType} - Not yet implemented", _dataType);
        }

        private void ExecuteClearMappings()
        {
            try
            {
                // Clear document mappings
                _document.ClearMappings(_dataType);

                // Reset UI state
                foreach (var prop in TargetProperties)
                {
                    prop.IsMapped = false;
                    prop.MappedColumn = null;
                }

                foreach (var col in SourceColumns)
                {
                    col.IsMapped = false;
                    col.MappedTo = null;
                }

                // Refresh views
                SourceColumnsView.Refresh();
                TargetPropertiesView.Refresh();
                RaisePropertyChanged(nameof(MappedCount));

                // Update parent tab status
                UpdateTabStatus();

                Log.Information("Cleared all mappings for {DataType}", _dataType);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to clear mappings");
            }
        }

        /// <summary>
        /// Executes the manage fields command to open the property selector dialog.
        /// </summary>
        private void ExecuteManageFields()
        {
            try
            {
                // Get ALL properties (including hidden ones) for this data type
                var allProperties = _propertyDiscovery.GetAllPropertiesForType(_dataType);
                
                // Get current enabled properties from settings
                var enabledProperties = _settingsService.GetEnabledProperties(_dataType);
                
                // Get default properties (use wildcard "*" as default)
                var defaultProperties = new List<string> { "*" };

                // Create and show dialog
                var viewModel = new PropertySelectorViewModel(
                    _dataType,
                    allProperties,
                    enabledProperties,
                    defaultProperties);

                var dialog = new Views.PropertySelectorDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                // Subscribe to DialogResult changes
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(PropertySelectorViewModel.DialogResult))
                    {
                        dialog.DialogResult = viewModel.DialogResult;
                    }
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    // Save the updated property visibility
                    var newEnabledProperties = viewModel.GetEnabledProperties();
                    _settingsService.SetEnabledProperties(_dataType, newEnabledProperties);

                    // Refresh the target properties list
                    RefreshTargetProperties();

                    Log.Information("Updated field visibility for {DataType}: {EnabledCount} of {TotalCount} properties enabled",
                        _dataType, viewModel.EnabledCount, viewModel.TotalCount);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open property selector for {DataType}", _dataType);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates command execution states.
        /// </summary>
        private void UpdateCommandStates()
        {
            (MapSelectedCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (UnmapSelectedCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Updates the parent tab's status indicator.
        /// </summary>
        private void UpdateTabStatus()
        {
            var status = MappedCount == 0 ? MappingStatus.Unmapped :
                         MappedCount >= AvailableCount ? MappingStatus.Complete :
                         MappingStatus.Partial;

            _parentViewModel.UpdateTabStatus(_dataType, status);
        }

        #endregion
    }
}
