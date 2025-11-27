using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// <para>
    /// This VM manages the mapping interface for one data type, providing:
    /// - Source column list (from sample files)
    /// - Target property list (from reflection)
    /// - Mapping operations (Map, Unmap, Auto-Map)
    /// - Filtering and search
    /// - Validation
    /// </para>
    /// <para>
    /// REFACTORING NOTE: This class is split into multiple partial files for maintainability:
    /// - Core.cs: Fields, properties, constructor, commands (this file)
    /// - Operations.cs: Manual mapping operations (Map, Unmap, Clear)
    /// - AutoMap.cs: Auto-map intelligence algorithm
    /// - DataLoader.cs: Data loading and refresh logic
    /// </para>
    /// </remarks>
    public partial class DataTypeMappingViewModel : BindableBase
    {
        #region Private Fields

        private readonly MapDocument _document;
        private readonly string _dataType;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly MapDocumentViewModel _parentViewModel;
        private readonly ISettingsService _settingsService;
        private readonly IUserDialogService _dialogService;
        private readonly IFuzzyMatcher _fuzzyMatcher;
        private readonly ColumnExtractionService _columnExtraction;

        private string _sourceFilter = string.Empty;
        private string _targetFilter = string.Empty;
        private ColumnInfo? _selectedSourceColumn;
        private MapPropertyInfo? _selectedTargetProperty;
        private TableReference _selectedTable;
        private bool _isInitializing = true;

        #endregion

        #region Constructor

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
            
            // Initialization complete - allow dirty marking from now on
            _isInitializing = false;

            Log.Debug("DataTypeMappingViewModel initialized for {DataType} (display: {DisplayName})", 
                _dataType, _propertyDiscovery.GetDataTypeDescription(_dataType));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data type this VM represents (raw class name, e.g., "Bus", "LVBreaker").
        /// </summary>
        public string DataType => _dataType;

        /// <summary>
        /// Gets the user-friendly display name for this data type (e.g., "Buses", "LV Breakers").
        /// </summary>
        public string DataTypeDisplayName => _propertyDiscovery.GetDataTypeDescription(_dataType);

        public ObservableCollection<ColumnInfo> SourceColumns { get; }
        public ICollectionView SourceColumnsView { get; }
        public ObservableCollection<MapPropertyInfo> TargetProperties { get; }
        public ICollectionView TargetPropertiesView { get; }
        public ObservableCollection<TableReference> AvailableTables { get; }
        public ObservableCollection<ComboBoxItemBase> ComboBoxItems { get; }

        private ComboBoxItemBase? _selectedComboBoxItem;
        public ComboBoxItemBase? SelectedComboBoxItem
        {
            get => _selectedComboBoxItem;
            set
            {
                if (SetProperty(ref _selectedComboBoxItem, value))
                {
                    if (value is TableItem tableItem)
                    {
                        SelectedTable = tableItem.TableReference;
                    }
                }
            }
        }

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

        public TableReference SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    if (value != null && !string.IsNullOrEmpty(value.DisplayName))
                    {
                        _document.TableReferencesByDataType[_dataType] = value.DisplayName;
                        
                        if (!_isInitializing)
                        {
                            _document.MarkDirty();
                        }
                        
                        Log.Debug("Saved table reference for {DataType}: {TableRef}", _dataType, value.DisplayName);
                    }
                    
                    RaisePropertyChanged(nameof(HasTableSelected));
                    RaisePropertyChanged(nameof(NoTableMessage));
                    UpdateCommandStates();
                    (ResetTableCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    OnTableChanged(value);
                }
            }
        }

        public int MappedCount =>
            _document.MappingsByDataType.TryGetValue(_dataType, out var mappings)
                ? mappings.Count
                : 0;

        public int AvailableCount => TargetProperties.Count;

        public bool HasTableSelected => SelectedTable != null && !string.IsNullOrEmpty(SelectedTable.TableName);

        public string NoTableMessage => "Please select a source table from the dropdown above to begin mapping.";

        #endregion

        #region Commands

        public ICommand MapSelectedCommand { get; }
        public ICommand UnmapSelectedCommand { get; }
        public ICommand AutoMapCommand { get; }
        public ICommand ClearMappingsCommand { get; }
        public ICommand ManageFieldsCommand { get; }
        public ICommand ResetTableCommand { get; }

        #endregion

        #region Filtering

        private bool FilterSourceColumn(object obj)
        {
            if (obj is not ColumnInfo column) return false;
            if (string.IsNullOrWhiteSpace(_sourceFilter)) return true;
            return column.ColumnName.Contains(_sourceFilter, StringComparison.OrdinalIgnoreCase);
        }

        private bool FilterTargetProperty(object obj)
        {
            if (obj is not MapPropertyInfo property) return false;
            if (string.IsNullOrWhiteSpace(_targetFilter)) return true;
            return property.PropertyName.Contains(_targetFilter, StringComparison.OrdinalIgnoreCase) ||
                   (property.Description?.Contains(_targetFilter, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        #endregion

        #region Helper Methods

        private void UpdateCommandStates()
        {
            (MapSelectedCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (UnmapSelectedCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        private void UpdateTabStatus()
        {
            int total = TargetProperties.Count;
            int mapped = MappedCount;
            MappingStatus status = mapped == 0 ? MappingStatus.Unmapped :
                                   mapped >= total ? MappingStatus.Complete :
                                   MappingStatus.Partial;
            _parentViewModel.UpdateTabStatus(_dataType, status);
        }

        public Dictionary<string, string> GetSelectedTablesFromOtherTabs()
        {
            return _parentViewModel.GetSelectedTablesByDataType();
        }

        public string GetFriendlyDataTypeName(string dataType)
        {
            return _propertyDiscovery.GetDataTypeDescription(dataType);
        }

        #endregion
    }
}
