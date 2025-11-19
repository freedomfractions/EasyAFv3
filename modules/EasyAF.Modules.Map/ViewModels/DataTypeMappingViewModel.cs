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
        private readonly ISettingsService _settingsService;
        private readonly IUserDialogService _dialogService;
        private readonly IFuzzyMatcher _fuzzyMatcher; // Fuzzy search for Auto-Map
        private readonly ColumnExtractionService _columnExtraction; // Extract columns from files

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
            ISettingsService settingsService,
            IUserDialogService dialogService,
            IFuzzyMatcher fuzzyMatcher)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _fuzzyMatcher = fuzzyMatcher ?? throw new ArgumentNullException(nameof(fuzzyMatcher));

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
            ResetTableCommand = new DelegateCommand(ExecuteResetTable, CanExecuteResetTable);

            // Load initial data
            LoadTargetProperties();
            LoadAvailableTables();

            Log.Debug("DataTypeMappingViewModel initialized for {DataType} (display: {DisplayName})", 
                _dataType, _propertyDiscovery.GetDataTypeDescription(_dataType));
        }

        #region Properties

        /// <summary>
        /// Gets the data type this VM represents (raw class name, e.g., "Bus", "LVBreaker").
        /// </summary>
        public string DataType => _dataType;

        /// <summary>
        /// Gets the user-friendly display name for this data type (e.g., "Buses", "LV Breakers").
        /// </summary>
        /// <remarks>
        /// This is retrieved from the [EasyPowerClass] attribute on the model class.
        /// Used for UI display in the GroupBox header.
        /// </remarks>
        public string DataTypeDisplayName => _propertyDiscovery.GetDataTypeDescription(_dataType);

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
                    // CROSS-MODULE EDIT: 2025-01-17 Table Reference Persistence
                    // Modified for: Save table selection to document for round-trip UX
                    // Related modules: Map (MapDocument, MapDocumentSerializer)
                    // Rollback instructions: Remove this persistence block
                    
                    // Save the table reference to the document
                    if (value != null && !string.IsNullOrEmpty(value.DisplayName))
                    {
                        _document.TableReferencesByDataType[_dataType] = value.DisplayName;
                        _document.MarkDirty();
                        Log.Debug("Saved table reference for {DataType}: {TableRef}", _dataType, value.DisplayName);
                    }
                    
                    // Notify that table selection state changed
                    RaisePropertyChanged(nameof(HasTableSelected));
                    RaisePropertyChanged(nameof(NoTableMessage));
                    UpdateCommandStates();
                    
                    // Update Reset button state
                    (ResetTableCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    
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

        /// <summary>
        /// Gets whether a table is currently selected.
        /// </summary>
        /// <remarks>
        /// Used to enable/disable mapping buttons and show helpful messages.
        /// </remarks>
        public bool HasTableSelected => SelectedTable != null && !string.IsNullOrEmpty(SelectedTable.TableName);

        /// <summary>
        /// Gets the message to display when no table is selected.
        /// </summary>
        public string NoTableMessage => "Please select a source table from the dropdown above to begin mapping.";

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

        /// <summary>
        /// Command to reset the table selection and return to the welcome overlay.
        /// </summary>
        /// <remarks>
        /// This command clears the selected table, which triggers:
        /// - Source columns list is cleared
        /// - Welcome overlay becomes visible
        /// - Glow effect appears on table dropdown
        /// Useful when user wants to select a different table or start over.
        /// </remarks>
        public ICommand ResetTableCommand { get; }

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

        /// <summary>
        /// Restores the previously selected table from the document (called after loading a saved map).
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-17 Table Selection Restoration
        /// Modified for: Restore table selection persistence after document load
        /// Related modules: Map (MapDocument, MapDocumentSerializer, TableReference)
        /// Rollback instructions: Remove this method
        /// 
        /// The saved reference is the DisplayName from TableReference, which uses format:
        /// "FileName | TableName" (e.g., "sample.csv | BusData")
        /// 
        /// This must match EXACTLY what TableReference.DisplayName returns.
        /// Uses simple ASCII pipe (|) character for cross-system compatibility.
        /// </remarks>
        public void RestoreTableSelection()
        {
            // Check if we have a saved table reference for this data type
            if (!_document.TableReferencesByDataType.TryGetValue(_dataType, out var savedTableRef))
            {
                Log.Debug("No saved table reference for {DataType}", _dataType);
                return;
            }

            if (string.IsNullOrEmpty(savedTableRef))
            {
                Log.Debug("Empty table reference for {DataType}", _dataType);
                return;
            }

            Log.Debug("Attempting to restore table selection for {DataType}: '{SavedRef}'", _dataType, savedTableRef);
            
            // Try exact DisplayName match
            var matchingItem = ComboBoxItems.OfType<TableItem>()
                .FirstOrDefault(item => item.TableReference.DisplayName == savedTableRef);
            
            if (matchingItem != null)
            {
                SelectedComboBoxItem = matchingItem;
                Log.Debug("Restored table selection for {DataType}: '{TableRef}'", _dataType, savedTableRef);
                return;
            }

            Log.Warning("Could not restore table selection for {DataType}: '{TableRef}' - table may have been removed or file renamed", _dataType, savedTableRef);
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
            // Remember current selection (by stable DisplayName)
            var previouslySelectedDisplayName = SelectedTable?.DisplayName;

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

            // CROSS-MODULE EDIT: 2025-01-16 Empty Files Placeholder
            // Modified for: Show helpful placeholder when no files are loaded
            // Related modules: Map (ComboBoxItemBase, FileHeaderItem)
            // Rollback instructions: Remove this empty check and placeholder logic
            
            // Check if no files were loaded
            if (tablesByFile.Count == 0)
            {
                // Add placeholder item instructing user to load files
                ComboBoxItems.Add(new FileHeaderItem 
                { 
                    FileName = "?? No sample files loaded - Add files in Summary tab" 
                });
                
                // Explicitly clear selection and columns when nothing is available
                SelectedTable = null;
                SelectedComboBoxItem = null;
                SourceColumns.Clear();
                
                Log.Information("No sample files loaded - showing placeholder in table dropdown");
                return; // Early exit since there are no tables to show
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

            // Selection preservation / restoration logic
            // 1) If there was a selection before refresh, try to re-select it
            // 2) Else, try to restore from document's saved TableReferences
            // 3) Else, leave selection null (user must pick)

            TableItem? itemToSelect = null;

            if (!string.IsNullOrEmpty(previouslySelectedDisplayName))
            {
                itemToSelect = ComboBoxItems.OfType<TableItem>()
                    .FirstOrDefault(i => string.Equals(i.TableReference.DisplayName, previouslySelectedDisplayName, StringComparison.Ordinal));

                if (itemToSelect != null)
                {
                    Log.Debug("Preserved previous table selection: {DisplayName}", previouslySelectedDisplayName);
                }
            }

            if (itemToSelect == null && _document.TableReferencesByDataType.TryGetValue(_dataType, out var savedRef) && !string.IsNullOrEmpty(savedRef))
            {
                itemToSelect = ComboBoxItems.OfType<TableItem>()
                    .FirstOrDefault(i => string.Equals(i.TableReference.DisplayName, savedRef, StringComparison.Ordinal));

                if (itemToSelect != null)
                {
                    Log.Debug("Restored saved table selection for {DataType}: {DisplayName}", _dataType, savedRef);
                }
            }

            if (itemToSelect != null)
            {
                // This will route through SelectedComboBoxItem setter and set SelectedTable
                SelectedComboBoxItem = itemToSelect;
            }
            else
            {
                // Do not force-clear selection here; leave as-is to avoid wiping a restored selection elsewhere
                Log.Debug("No table auto-selected for {DataType} (previous/saved selection not found)", _dataType);
            }

            Log.Information("Loaded {Count} available tables from {FileCount} files ({ItemCount} ComboBox items)", 
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
            // CROSS-MODULE EDIT: 2025-01-17 Table Selection Validation
            // Modified for: Require table selection before allowing mapping
            // Related modules: Map (DataTypeMappingView)
            // Rollback instructions: Remove HasTableSelected check
            
            return HasTableSelected && 
                   SelectedSourceColumn != null && 
                   SelectedTargetProperty != null;
        }

        private void ExecuteMapSelected()
        {
            if (SelectedSourceColumn == null || SelectedTargetProperty == null) return;

            // CROSS-MODULE EDIT: 2025-01-17 Duplicate Mapping Prevention (Bidirectional)
            // Modified for: Detect and warn for BOTH directions of duplicate mappings (property?column AND column?property)
            // Related modules: Core (IUserDialogService)
            // Rollback instructions: Remove bidirectional duplicate detection logic below
            
            // === CHECK 1: Is the selected PROPERTY already mapped to a DIFFERENT COLUMN? ===
            if (SelectedTargetProperty.IsMapped && SelectedTargetProperty.MappedColumn != SelectedSourceColumn.ColumnName)
            {
                var confirmed = _dialogService.Confirm(
                    $"Property '{SelectedTargetProperty.PropertyName}' is already mapped to column '{SelectedTargetProperty.MappedColumn}'.\n\n" +
                    $"Do you want to replace the existing mapping with '{SelectedSourceColumn.ColumnName}'?",
                    "Replace Existing Mapping?");
                
                if (!confirmed)
                {
                    Log.Debug("User canceled replacing property mapping for {Property}", SelectedTargetProperty.PropertyName);
                    return; // User canceled
                }
                
                // Clear the old mapping from the source column
                var oldColumn = SourceColumns.FirstOrDefault(c => c.ColumnName == SelectedTargetProperty.MappedColumn);
                if (oldColumn != null)
                {
                    oldColumn.IsMapped = false;
                    oldColumn.MappedTo = null;
                }
                
                Log.Information("Replacing property mapping for {Property}: {OldColumn} ? {NewColumn}",
                    SelectedTargetProperty.PropertyName, SelectedTargetProperty.MappedColumn, SelectedSourceColumn.ColumnName);
            }

            // === CHECK 2: Is the selected COLUMN already mapped to a DIFFERENT PROPERTY? ===
            if (SelectedSourceColumn.IsMapped && SelectedSourceColumn.MappedTo != $"{_dataType}.{SelectedTargetProperty.PropertyName}")
            {
                // Extract the property name from "DataType.PropertyName" format
                var currentPropertyName = SelectedSourceColumn.MappedTo?.Split('.').LastOrDefault() ?? "Unknown";
                
                var confirmed = _dialogService.Confirm(
                    $"Column '{SelectedSourceColumn.ColumnName}' is already mapped to property '{currentPropertyName}'.\n\n" +
                    $"Do you want to replace the existing mapping with '{SelectedTargetProperty.PropertyName}'?",
                    "Replace Existing Mapping?");
                
                if (!confirmed)
                {
                    Log.Debug("User canceled replacing column mapping for {Column}", SelectedSourceColumn.ColumnName);
                    return; // User canceled
                }
                
                // Clear the old mapping from the target property
                var oldProperty = TargetProperties.FirstOrDefault(p => p.PropertyName == currentPropertyName);
                if (oldProperty != null)
                {
                    oldProperty.IsMapped = false;
                    oldProperty.MappedColumn = null;
                    
                    // Remove from document
                    _document.RemoveMapping(_dataType, oldProperty.PropertyName);
                }
                
                Log.Information("Replacing column mapping for {Column}: {OldProperty} ? {NewProperty}",
                    SelectedSourceColumn.ColumnName, currentPropertyName, SelectedTargetProperty.PropertyName);
            }

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

        /// <summary>
        /// Executes the auto-map command using intelligent fuzzy matching.
        /// </summary>
        /// <remarks>
        /// <para>
        /// CROSS-MODULE EDIT: 2025-01-17 Auto-Map Intelligence (Safeguard #9)
        /// Modified for: Implement intelligent column-to-property matching using fuzzy search
        /// Related modules: Core (IFuzzyMatcher service with Levenshtein + Jaro-Winkler)
        /// Rollback instructions: Revert to stub implementation (just log message)
        /// </para>
        /// <para>
        /// Auto-Map Algorithm:
        /// 1. Get all unmapped properties (target properties with no mapping)
        /// 2. Get all unmapped columns (source columns with no mapping)
        /// 3. For each unmapped property:
        ///    - Use FuzzyMatcher to find best matching column names
        ///    - If best match score >= 0.6 (60% confidence), create mapping
        ///    - Track results (success, low confidence, no match)
        /// 4. Show summary dialog with results
        /// 5. Update UI to reflect new mappings
        /// </para>
        /// <para>
        /// Threshold: 0.6 (60%) chosen as sweet spot:
        /// - Too low (0.4): Many false positives
        /// - Too high (0.8): Misses good matches like "Name" ? "BUS_NAME"
        /// - 0.6: Balances precision and recall
        /// </para>
        /// </remarks>
        private void ExecuteAutoMap()
        {
            try
            {
                if (SourceColumns.Count == 0)
                {
                    _dialogService.ShowMessage(
                        "No source columns available.\n\n" +
                        "Please select a table from the dropdown first.",
                        "Auto-Map");
                    Log.Information("Auto-Map: No source columns available for {DataType}", _dataType);
                    return;
                }

                Log.Information("Starting Auto-Map for {DataType} with {ColumnCount} columns and {PropertyCount} properties",
                    _dataType, SourceColumns.Count, TargetProperties.Count);

                // Get unmapped properties (targets)
                var unmappedProperties = TargetProperties
                    .Where(p => !p.IsMapped)
                    .ToList();

                // Get unmapped column names (sources)
                var unmappedColumnNames = SourceColumns
                    .Where(c => !c.IsMapped)
                    .Select(c => c.ColumnName)
                    .ToList();

                if (unmappedProperties.Count == 0)
                {
                    _dialogService.ShowMessage(
                        "All properties are already mapped!",
                        "Auto-Map");
                    Log.Information("Auto-Map: All properties already mapped for {DataType}", _dataType);
                    return;
                }

                // Track results for summary
                var successfulMappings = new List<(string Property, string Column, double Score)>();
                var lowConfidenceMappings = new List<(string Property, string Column, double Score)>();
                var noMatchProperties = new List<string>();

                const double confidenceThreshold = 0.6;

                // Match each unmapped property to best column
                foreach (var property in unmappedProperties)
                {
                    // STRATEGY 1: Match against PropertyName
                    var matches = _fuzzyMatcher.FindBestMatches(
                        query: property.PropertyName,
                        candidates: unmappedColumnNames,
                        maxResults: 1,
                        minScore: 0.4,
                        caseSensitive: false);

                    // STRATEGY 2: Also try matching against Description (friendly text) if available
                    // This handles cases where column names match friendly text like "1/2 Cycle Duty (kA)"
                    if (!string.IsNullOrWhiteSpace(property.Description))
                    {
                        var descriptionMatches = _fuzzyMatcher.FindBestMatches(
                            query: property.Description,
                            candidates: unmappedColumnNames,
                            maxResults: 1,
                            minScore: 0.4,
                            caseSensitive: false);

                        // Use description match if it's better than property name match
                        if (descriptionMatches.Any() && 
                            (!matches.Any() || descriptionMatches[0].Score > matches[0].Score))
                        {
                            matches = descriptionMatches;
                            Log.Debug("Auto-Map: Using Description match for '{Property}' (Description: '{Description}') - score {Score:P0}",
                                property.PropertyName, property.Description, descriptionMatches[0].Score);
                        }
                    }

                    if (!matches.Any())
                    {
                        noMatchProperties.Add(property.PropertyName);
                        Log.Debug("Auto-Map: No match found for property '{Property}'", property.PropertyName);
                        continue;
                    }

                    var bestMatch = matches[0];

                    if (bestMatch.Score >= confidenceThreshold)
                    {
                        // High confidence - create mapping automatically
                        var sourceColumn = SourceColumns.FirstOrDefault(c => c.ColumnName == bestMatch.Target);
                        if (sourceColumn != null)
                        {
                            CreateMapping(property, sourceColumn);
                            successfulMappings.Add((property.PropertyName, bestMatch.Target, bestMatch.Score));
                            
                            // Remove from unmapped list so it's not used again
                            unmappedColumnNames.Remove(bestMatch.Target);

                            Log.Information("Auto-Map: Mapped '{Property}' ? '{Column}' (score: {Score:P0}, reason: {Reason})",
                                property.PropertyName, bestMatch.Target, bestMatch.Score, bestMatch.Reason);
                        }
                    }
                    else
                    {
                        // Low confidence - don't map, but report it
                        lowConfidenceMappings.Add((property.PropertyName, bestMatch.Target, bestMatch.Score));
                        Log.Debug("Auto-Map: Low confidence match for '{Property}' ? '{Column}' (score: {Score:P0})",
                            property.PropertyName, bestMatch.Target, bestMatch.Score);
                    }
                }

                // Show summary dialog
                ShowAutoMapResults(successfulMappings, lowConfidenceMappings, noMatchProperties);

                Log.Information("Auto-Map complete for {DataType}: {Success} mapped, {LowConf} low confidence, {NoMatch} no match",
                    _dataType, successfulMappings.Count, lowConfidenceMappings.Count, noMatchProperties.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Auto-Map for {DataType}", _dataType);
                _dialogService.ShowMessage(
                    $"An error occurred during Auto-Map:\n\n{ex.Message}",
                    "Auto-Map Error");
            }
        }

        /// <summary>
        /// Shows the Auto-Map results summary dialog.
        /// </summary>
        private void ShowAutoMapResults(
            List<(string Property, string Column, double Score)> successfulMappings,
            List<(string Property, string Column, double Score)> lowConfidenceMappings,
            List<string> noMatchProperties)
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Auto-Map Results for {_dataType}\n");

            if (successfulMappings.Any())
            {
                summary.AppendLine($"? Successfully Mapped ({successfulMappings.Count}):");
                foreach (var (property, column, score) in successfulMappings.OrderBy(m => m.Property))
                {
                    summary.AppendLine($"  • {property} ? {column} ({score:P0} confidence)");
                }
                summary.AppendLine();
            }

            if (lowConfidenceMappings.Any())
            {
                summary.AppendLine($"? Low Confidence - Not Mapped ({lowConfidenceMappings.Count}):");
                foreach (var (property, column, score) in lowConfidenceMappings.OrderBy(m => m.Property))
                {
                    summary.AppendLine($"  • {property} ? {column}? ({score:P0} confidence - below 60% threshold)");
                }
                summary.AppendLine();
            }

            if (noMatchProperties.Any())
            {
                summary.AppendLine($"? No Match Found ({noMatchProperties.Count}):");
                foreach (var property in noMatchProperties.OrderBy(p => p))
                {
                    summary.AppendLine($"  • {property}");
                }
                summary.AppendLine();
            }

            if (!successfulMappings.Any() && !lowConfidenceMappings.Any() && !noMatchProperties.Any())
            {
                summary.AppendLine("No unmapped properties found.");
            }

            _dialogService.ShowMessage(summary.ToString(), "Auto-Map Results");
        }
        
        /// <summary>
        /// Executes the clear mappings command.
        /// </summary>
        /// <remarks>
        /// This command clears ALL mappings for this data type.
        /// Prompts the user for confirmation before proceeding.
        /// </remarks>
        private void ExecuteClearMappings()
        {
            try
            {
                if (MappedCount == 0)
                {
                    _dialogService.ShowMessage(
                        "No mappings to clear for this data type.",
                        "Clear Mappings");
                    return;
                }

                var confirmed = _dialogService.Confirm(
                    $"Clear all {MappedCount} mapping(s) for {_dataType}?\n\n" +
                    $"This cannot be undone.",
                    "Clear All Mappings?");
                
                if (!confirmed)
                {
                    Log.Debug("User cancelled clearing mappings for {DataType}", _dataType);
                    return; // User canceled
                }

                // Clear from document
                _document.ClearMappings(_dataType);

                // Update UI
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
                Log.Error(ex, "Failed to clear mappings for {DataType}", _dataType);
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

                // CROSS-MODULE EDIT: 2025-01-18 Property Selector Friendly Names
                // Modified for: Pass friendly data type description to property selector dialog
                // Related modules: Map (PropertyDiscoveryService.GetDataTypeDescription, PropertySelectorViewModel)
                // Rollback instructions: Remove dataTypeDisplayName parameter, revert to old constructor
                
                // Get the friendly display name for this data type (e.g., "LV Breakers" instead of "LVBreaker")
                var dataTypeDisplayName = _propertyDiscovery.GetDataTypeDescription(_dataType);

                // Create and show dialog
                var viewModel = new PropertySelectorViewModel(
                    _dataType,
                    dataTypeDisplayName,  // Pass friendly name
                    allProperties,
                    enabledProperties,
                    defaultProperties);

                var dialog = new Views.PropertySelectorDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                // Dialog code-behind handles DialogResult via PropertyChanged subscription
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

        /// <summary>
        /// Determines whether the reset table command can execute.
        /// </summary>
        /// <returns>True if a table is currently selected; otherwise false.</returns>
        private bool CanExecuteResetTable()
        {
            return HasTableSelected;
        }

        /// <summary>
        /// Executes the reset table command.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This command performs the following actions:
        /// 1. Prompts user to confirm (if mappings exist)
        /// 2. Clears ALL mappings for this data type
        /// 3. Clears the selected table (sets SelectedTable to null)
        /// 4. Clears the selected ComboBox item
        /// 5. Triggers the welcome overlay to reappear
        /// 6. Triggers the glow effect on the table dropdown
        /// 7. Clears the source columns list
        /// </para>
        /// <para>
        /// This is a "fresh start" operation - user can select a different table
        /// and create new mappings from scratch.
        /// </para>
        /// </remarks>
        private void ExecuteResetTable()
        {
            try
            {
                // CROSS-MODULE EDIT: 2025-01-16 Reset Table with Mapping Cleanup
                // Modified for: Clear all mappings when resetting table (fresh start)
                // Related modules: Core (IUserDialogService), Map (MapDocument)
                // Rollback instructions: Remove mapping cleanup logic, only clear table selection
                
                // Check if there are any mappings - if so, warn user they will be lost
                if (MappedCount > 0)
                {
                    var confirmed = _dialogService.Confirm(
                        $"Reset table selection for {_dataType}?\n\n" +
                        $"This will DELETE all {MappedCount} mapping(s) for this data type.\n" +
                        $"You'll start fresh with a blank slate.\n\n" +
                        $"Are you sure you want to continue?",
                        "Reset Table and Clear Mappings?");
                    
                    if (!confirmed)
                    {
                        Log.Debug("User canceled table reset for {DataType}", _dataType);
                        return; // User canceled
                    }
                }
                
                // Clear ALL mappings for this data type
                _document.ClearMappings(_dataType);
                
                // Update UI state for target properties
                foreach (var prop in TargetProperties)
                {
                    prop.IsMapped = false;
                    prop.MappedColumn = null;
                }
                
                // Clear table selection
                SelectedTable = null;
                SelectedComboBoxItem = null;
                
                // Clear source columns (will happen automatically via SelectedTable setter, but explicit is clearer)
                SourceColumns.Clear();
                
                // Refresh views
                TargetPropertiesView.Refresh();
                RaisePropertyChanged(nameof(MappedCount));
                
                // Update command states
                UpdateCommandStates();
                (ResetTableCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                
                // Update parent tab status
                UpdateTabStatus();
                
                Log.Information("Reset table selection and cleared {Count} mappings for {DataType}", MappedCount, _dataType);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to reset table selection for {DataType}", _dataType);
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

        /// <summary>
        /// Creates a mapping between the specified property and column, updating the document and UI.
        /// </summary>
        /// <remarks>
        /// This method performs the following actions:
        /// - Updates the document mapping
        /// - Updates the target property's mapping state
        /// - Updates the source column's mapping state
        /// - Refreshes the relevant collection views
        /// - Updates the mapped count property
        /// - Updates the command states
        /// - Updates the parent tab status
        /// </remarks>
        /// <param name="property">The target property to map.</param>
        /// <param name="column">The source column to map to the property.</param>
        private void CreateMapping(MapPropertyInfo property, ColumnInfo column)
        {
            // Update document mapping
            _document.UpdateMapping(_dataType, property.PropertyName, column.ColumnName);

            // Update target property state
            property.IsMapped = true;
            property.MappedColumn = column.ColumnName;

            // Update source column state
            column.IsMapped = true;
            column.MappedTo = $"{_dataType}.{property.PropertyName}";

            // Refresh views
            SourceColumnsView.Refresh();
            TargetPropertiesView.Refresh();
            RaisePropertyChanged(nameof(MappedCount));

            // Update command states immediately
            UpdateCommandStates();

            // Update parent tab status
            UpdateTabStatus();
        }

        /// <summary>
        /// Gets the user-friendly description for a data type (e.g., "Buses", "LV Breakers").
        /// </summary>
        /// <param name="dataType">The raw data type name (e.g., "Bus", "LVBreaker").</param>
        /// <returns>Friendly display name from [EasyPowerClass] attribute.</returns>
        public string GetFriendlyDataTypeName(string dataType)
        {
            return _propertyDiscovery.GetDataTypeDescription(dataType);
        }

        /// <summary>
        /// Gets the currently selected tables from all other tabs (excluding this one).
        /// </summary>
        /// <returns>Dictionary where key=dataType, value=TableReference.DisplayName</returns>
        /// <remarks>
        /// Used by the cross-tab table exclusivity feature to prevent the same table
        /// from being selected on multiple tabs simultaneously.
        /// This method is called when the table dropdown opens to dynamically update availability.
        /// </remarks>
        public Dictionary<string, string> GetSelectedTablesFromOtherTabs()
        {
            return _parentViewModel.GetSelectedTablesByDataType();
        }

        #endregion
    }
}
