using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Mvvm;
using Prism.Commands;
using EasyAF.Modules.Spec.Models;
using EasyAF.Core.Contracts;
using EasyAF.Engine;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// View model for the spec setup tab showing table management and statistics.
    /// </summary>
    /// <remarks>
    /// The setup tab provides:
    /// - Table definitions grid (name, data types, mode)
    /// - Statistics panel (field usage by data type)
    /// - Map validation panel
    /// </remarks>
    public class SpecSetupViewModel : BindableBase, IDisposable
    {
        private readonly SpecDocument _document;
        private readonly IUserDialogService _dialogService;
        private TableDefinitionViewModel? _selectedTable;
        private string? _selectedMapFile;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the SpecSetupViewModel.
        /// </summary>
        public SpecSetupViewModel(
            SpecDocument document,
            IUserDialogService dialogService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Initialize collections
            Tables = new ObservableCollection<TableDefinitionViewModel>();
            Statistics = new ObservableCollection<DataTypeStatistic>();
            ValidationResults = new ObservableCollection<ValidationResult>();

            // Initialize commands
            AddTableCommand = new DelegateCommand(ExecuteAddTable);
            RemoveTableCommand = new DelegateCommand(ExecuteRemoveTable, CanExecuteRemoveTable);
            MoveTableUpCommand = new DelegateCommand(ExecuteMoveTableUp, CanExecuteMoveTableUp);
            MoveTableDownCommand = new DelegateCommand(ExecuteMoveTableDown, CanExecuteMoveTableDown);
            ValidateMapCommand = new DelegateCommand(ExecuteValidateMap, CanExecuteValidateMap);

            // Load existing tables from document
            LoadTablesFromDocument();

            // Subscribe to table changes
            Tables.CollectionChanged += (s, e) =>
            {
                TablesChanged?.Invoke(this, EventArgs.Empty);
                RefreshStatistics();
            };

            Log.Debug("SpecSetupViewModel initialized with {TableCount} tables", Tables.Count);
        }

        #region Events

        /// <summary>
        /// Event fired when tables collection changes (add/remove).
        /// </summary>
        public event EventHandler? TablesChanged;

        #endregion

        #region Properties - Table Management

        /// <summary>
        /// Gets the collection of table definitions.
        /// </summary>
        public ObservableCollection<TableDefinitionViewModel> Tables { get; }

        /// <summary>
        /// Gets or sets the currently selected table.
        /// </summary>
        public TableDefinitionViewModel? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    (RemoveTableCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    (MoveTableUpCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    (MoveTableDownCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Properties - Statistics

        /// <summary>
        /// Gets the collection of data type statistics.
        /// </summary>
        public ObservableCollection<DataTypeStatistic> Statistics { get; }

        #endregion

        #region Properties - Map Validation

        /// <summary>
        /// Gets the collection of available map files.
        /// </summary>
        /// <remarks>
        /// Populated from Documents\EasyAF\Maps\ folder.
        /// </remarks>
        public ObservableCollection<string> AvailableMapFiles { get; } = new();

        /// <summary>
        /// Gets or sets the selected map file for validation.
        /// </summary>
        public string? SelectedMapFile
        {
            get => _selectedMapFile;
            set
            {
                if (SetProperty(ref _selectedMapFile, value))
                {
                    (ValidateMapCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of validation results.
        /// </summary>
        public ObservableCollection<ValidationResult> ValidationResults { get; }

        #endregion

        #region Commands

        /// <summary>
        /// Command to add a new table.
        /// </summary>
        public ICommand AddTableCommand { get; }

        /// <summary>
        /// Command to remove the selected table.
        /// </summary>
        public ICommand RemoveTableCommand { get; }

        /// <summary>
        /// Command to move selected table up in order.
        /// </summary>
        public ICommand MoveTableUpCommand { get; }

        /// <summary>
        /// Command to move selected table down in order.
        /// </summary>
        public ICommand MoveTableDownCommand { get; }

        /// <summary>
        /// Command to validate spec against map file.
        /// </summary>
        public ICommand ValidateMapCommand { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Loads existing tables from the document into the collection.
        /// </summary>
        private void LoadTablesFromDocument()
        {
            Tables.Clear();

            if (_document.Spec.Tables != null)
            {
                foreach (var tableSpec in _document.Spec.Tables)
                {
                    var vm = new TableDefinitionViewModel(tableSpec);
                    
                    // Subscribe to property changes for dirty tracking
                    vm.PropertyChanged += OnTablePropertyChanged;
                    
                    Tables.Add(vm);
                }
            }

            RefreshStatistics();
            Log.Debug("Loaded {Count} tables from document", Tables.Count);
        }

        /// <summary>
        /// Handles property changes on table ViewModels to mark document dirty.
        /// </summary>
        private void OnTablePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _document.MarkDirty();
        }

        /// <summary>
        /// Refreshes the statistics display based on current tables.
        /// </summary>
        private void RefreshStatistics()
        {
            Statistics.Clear();

            // TODO: Task 25 - Implement statistics calculation
            // For each data type used across all tables:
            //   - Count unique properties referenced
            //   - Calculate completeness percentage
            //   - Determine status color

            Log.Debug("Statistics refreshed (placeholder implementation)");
        }

        #endregion

        #region Command Implementations - Table Management

        /// <summary>
        /// Executes the add table command.
        /// </summary>
        private void ExecuteAddTable()
        {
            var newTable = new TableSpec
            {
                Id = Guid.NewGuid().ToString(),
                AltText = $"Table {Tables.Count + 1}",
                Columns = Array.Empty<ColumnSpec>()
            };

            var vm = new TableDefinitionViewModel(newTable);
            vm.PropertyChanged += OnTablePropertyChanged;
            
            Tables.Add(vm);
            SelectedTable = vm;
            
            _document.MarkDirty();
            Log.Information("Added new table: {Name}", newTable.AltText);
        }

        /// <summary>
        /// Determines if the remove table command can execute.
        /// </summary>
        private bool CanExecuteRemoveTable() => SelectedTable != null;

        /// <summary>
        /// Executes the remove table command.
        /// </summary>
        private void ExecuteRemoveTable()
        {
            if (SelectedTable == null) return;

            var confirmed = _dialogService.Confirm(
                $"Remove table '{SelectedTable.TableName}'?",
                "Remove Table");

            if (!confirmed) return;

            SelectedTable.PropertyChanged -= OnTablePropertyChanged;
            Tables.Remove(SelectedTable);
            SelectedTable = null;
            
            _document.MarkDirty();
            Log.Information("Removed table");
        }

        /// <summary>
        /// Determines if move up command can execute.
        /// </summary>
        private bool CanExecuteMoveTableUp()
        {
            if (SelectedTable == null) return false;
            var index = Tables.IndexOf(SelectedTable);
            return index > 0;
        }

        /// <summary>
        /// Executes move table up command.
        /// </summary>
        private void ExecuteMoveTableUp()
        {
            if (SelectedTable == null) return;
            
            var index = Tables.IndexOf(SelectedTable);
            if (index <= 0) return;

            Tables.Move(index, index - 1);
            _document.MarkDirty();
        }

        /// <summary>
        /// Determines if move down command can execute.
        /// </summary>
        private bool CanExecuteMoveTableDown()
        {
            if (SelectedTable == null) return false;
            var index = Tables.IndexOf(SelectedTable);
            return index >= 0 && index < Tables.Count - 1;
        }

        /// <summary>
        /// Executes move table down command.
        /// </summary>
        private void ExecuteMoveTableDown()
        {
            if (SelectedTable == null) return;
            
            var index = Tables.IndexOf(SelectedTable);
            if (index < 0 || index >= Tables.Count - 1) return;

            Tables.Move(index, index + 1);
            _document.MarkDirty();
        }

        #endregion

        #region Command Implementations - Map Validation

        /// <summary>
        /// Determines if validate map command can execute.
        /// </summary>
        private bool CanExecuteValidateMap() => !string.IsNullOrWhiteSpace(SelectedMapFile);

        /// <summary>
        /// Executes the validate map command.
        /// </summary>
        private void ExecuteValidateMap()
        {
            if (string.IsNullOrWhiteSpace(SelectedMapFile)) return;

            ValidationResults.Clear();

            // TODO: Task 28 - Implement map validation
            // 1. Load selected .ezmap file
            // 2. Extract all PropertyPaths from spec tables
            // 3. Check if each PropertyPath is mapped in the map file
            // 4. Populate ValidationResults with color-coded results

            Log.Information("Map validation executed (placeholder)");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Unsubscribe from table property changes
            foreach (var table in Tables)
            {
                table.PropertyChanged -= OnTablePropertyChanged;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// View model wrapper for a table definition.
    /// </summary>
    public class TableDefinitionViewModel : BindableBase
    {
        private readonly TableSpec _tableSpec;

        public TableDefinitionViewModel(TableSpec tableSpec)
        {
            _tableSpec = tableSpec ?? throw new ArgumentNullException(nameof(tableSpec));
        }

        public TableSpec TableSpec => _tableSpec;

        public string TableName
        {
            get => _tableSpec.AltText ?? string.Empty;
            set
            {
                if (_tableSpec.AltText != value)
                {
                    _tableSpec.AltText = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO: Task 25 - Add DataTypes property (multi-select)
        // TODO: Task 25 - Add Mode property (Label/Report)
        // TODO: Task 25 - Add ColumnCount property (read-only, calculated)
    }

    /// <summary>
    /// Represents statistics for a data type's usage across tables.
    /// </summary>
    public class DataTypeStatistic : BindableBase
    {
        private string _dataTypeName = string.Empty;
        private int _fieldsUsed;
        private int _fieldsAvailable;
        private string _statusColor = "Gray";

        public string DataTypeName
        {
            get => _dataTypeName;
            set => SetProperty(ref _dataTypeName, value);
        }

        public int FieldsUsed
        {
            get => _fieldsUsed;
            set => SetProperty(ref _fieldsUsed, value);
        }

        public int FieldsAvailable
        {
            get => _fieldsAvailable;
            set => SetProperty(ref _fieldsAvailable, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }
    }

    /// <summary>
    /// Represents a map validation result entry.
    /// </summary>
    public class ValidationResult : BindableBase
    {
        private string _propertyPath = string.Empty;
        private string _status = "Unknown";
        private string _statusColor = "Gray";
        private string? _message;

        public string PropertyPath
        {
            get => _propertyPath;
            set => SetProperty(ref _propertyPath, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string? Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
    }
}
