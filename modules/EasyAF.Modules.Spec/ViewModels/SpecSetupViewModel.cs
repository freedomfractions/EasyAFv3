using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Modules.Spec.Models;
using EasyAF.Engine;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// View model for the Spec Setup tab (table management).
    /// </summary>
    public class SpecSetupViewModel : BindableBase, IDisposable
    {
        private readonly SpecDocument _document;
        private readonly IUserDialogService _dialogService;
        private TableDefinitionViewModel? _selectedTable;
        private string? _selectedMapFile;
        private bool _disposed;

        public SpecSetupViewModel(SpecDocument document, IUserDialogService dialogService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Initialize collections
            Tables = new ObservableCollection<TableDefinitionViewModel>();
            Statistics = new ObservableCollection<DataTypeStatistic>();
            ValidationResults = new ObservableCollection<ValidationResult>();
            AvailableMapFiles = new ObservableCollection<string>();

            // Load initial data
            RefreshTables();
            RefreshStatistics();
            LoadAvailableMapFiles(); // FIX: Auto-populate map files dropdown

            // Commands
            AddTableCommand = new DelegateCommand(ExecuteAddTable);
            RemoveTableCommand = new DelegateCommand(ExecuteRemoveTable, CanExecuteRemoveTable)
                .ObservesProperty(() => SelectedTable);
            MoveTableUpCommand = new DelegateCommand(ExecuteMoveTableUp, CanExecuteMoveTableUp)
                .ObservesProperty(() => SelectedTable);
            MoveTableDownCommand = new DelegateCommand(ExecuteMoveTableDown, CanExecuteMoveTableDown)
                .ObservesProperty(() => SelectedTable);
            ValidateMapCommand = new DelegateCommand(ExecuteValidateMap, CanExecuteValidateMap)
                .ObservesProperty(() => SelectedMapFile);

            Log.Debug("SpecSetupViewModel initialized");
        }

        #region Properties

        public ObservableCollection<TableDefinitionViewModel> Tables { get; }
        public ObservableCollection<DataTypeStatistic> Statistics { get; }
        public ObservableCollection<ValidationResult> ValidationResults { get; }
        public ObservableCollection<string> AvailableMapFiles { get; }

        public TableDefinitionViewModel? SelectedTable
        {
            get => _selectedTable;
            set => SetProperty(ref _selectedTable, value);
        }

        public string? SelectedMapFile
        {
            get => _selectedMapFile;
            set => SetProperty(ref _selectedMapFile, value);
        }

        #endregion

        #region Commands

        public ICommand AddTableCommand { get; }
        public ICommand RemoveTableCommand { get; }
        public ICommand MoveTableUpCommand { get; }
        public ICommand MoveTableDownCommand { get; }
        public ICommand ValidateMapCommand { get; }

        #endregion

        #region Events

        public event EventHandler? TablesChanged;

        #endregion

        #region Command Implementations

        private void ExecuteAddTable()
        {
            var newTable = new TableSpec
            {
                Id = $"Table{_document.Spec.Tables.Length + 1}",
                AltText = $"Table {_document.Spec.Tables.Length + 1}",
                Columns = Array.Empty<ColumnSpec>()
            };

            var newArray = new TableSpec[_document.Spec.Tables.Length + 1];
            Array.Copy(_document.Spec.Tables, newArray, _document.Spec.Tables.Length);
            newArray[^1] = newTable;
            _document.Spec.Tables = newArray;

            RefreshTables();
            RefreshStatistics();
            _document.MarkDirty();

            TablesChanged?.Invoke(this, EventArgs.Empty);

            Log.Information("Added new table: {TableId}", newTable.Id);
        }

        private bool CanExecuteRemoveTable() => SelectedTable != null;

        private void ExecuteRemoveTable()
        {
            if (SelectedTable == null) return;

            var index = Tables.IndexOf(SelectedTable);
            var newArray = _document.Spec.Tables.Where(t => t != SelectedTable.Table).ToArray();
            _document.Spec.Tables = newArray;

            RefreshTables();
            RefreshStatistics();
            _document.MarkDirty();

            if (Tables.Count > 0)
            {
                SelectedTable = Tables.ElementAtOrDefault(Math.Min(index, Tables.Count - 1));
            }

            TablesChanged?.Invoke(this, EventArgs.Empty);

            Log.Information("Removed table");
        }

        private bool CanExecuteMoveTableUp() =>
            SelectedTable != null && Tables.IndexOf(SelectedTable) > 0;

        private void ExecuteMoveTableUp()
        {
            if (SelectedTable == null) return;

            var index = Array.IndexOf(_document.Spec.Tables, SelectedTable.Table);
            if (index > 0)
            {
                var temp = _document.Spec.Tables[index];
                _document.Spec.Tables[index] = _document.Spec.Tables[index - 1];
                _document.Spec.Tables[index - 1] = temp;

                RefreshTables();
                _document.MarkDirty();
                SelectedTable = Tables[index - 1];
            }
        }

        private bool CanExecuteMoveTableDown() =>
            SelectedTable != null && Tables.IndexOf(SelectedTable) < Tables.Count - 1;

        private void ExecuteMoveTableDown()
        {
            if (SelectedTable == null) return;

            var index = Array.IndexOf(_document.Spec.Tables, SelectedTable.Table);
            if (index < _document.Spec.Tables.Length - 1)
            {
                var temp = _document.Spec.Tables[index];
                _document.Spec.Tables[index] = _document.Spec.Tables[index + 1];
                _document.Spec.Tables[index + 1] = temp;

                RefreshTables();
                _document.MarkDirty();
                SelectedTable = Tables[index + 1];
            }
        }

        private bool CanExecuteValidateMap() => !string.IsNullOrEmpty(SelectedMapFile);

        private void ExecuteValidateMap()
        {
            ValidationResults.Clear();
            Log.Information("Map validation not yet implemented");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// AUDIT FIX: Load available map files from Documents\EasyAF\Maps directory
        /// </summary>
        private void LoadAvailableMapFiles()
        {
            AvailableMapFiles.Clear();

            try
            {
                var mapsFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "EasyAF",
                    "Maps");

                if (Directory.Exists(mapsFolder))
                {
                    var mapFiles = Directory.GetFiles(mapsFolder, "*.ezmap")
                        .Select(Path.GetFileNameWithoutExtension)
                        .OrderBy(name => name)
                        .ToList();

                    foreach (var mapFile in mapFiles)
                    {
                        AvailableMapFiles.Add(mapFile);
                    }

                    Log.Debug("Loaded {Count} map files from {Folder}", mapFiles.Count, mapsFolder);
                }
                else
                {
                    Log.Warning("Maps folder does not exist: {Folder}", mapsFolder);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load available map files");
            }
        }

        private void RefreshTables()
        {
            var selected = SelectedTable?.Table;

            Tables.Clear();
            for (int i = 0; i < _document.Spec.Tables.Length; i++)
            {
                var tableVm = new TableDefinitionViewModel(_document.Spec.Tables[i], _document);
                Tables.Add(tableVm);
            }

            if (selected != null)
            {
                SelectedTable = Tables.FirstOrDefault(t => t.Table == selected);
            }
        }

        private void RefreshStatistics()
        {
            Statistics.Clear();
            // TODO: Populate with actual statistics
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// AUDIT FIX: Make TableDefinitionViewModel properties editable
    /// </summary>
    public class TableDefinitionViewModel : BindableBase
    {
        private readonly SpecDocument _document;

        public TableDefinitionViewModel(TableSpec table, SpecDocument document)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public TableSpec Table { get; }

        /// <summary>
        /// AUDIT FIX: Make table name editable (was read-only)
        /// </summary>
        public string TableName
        {
            get => !string.IsNullOrEmpty(Table.AltText) ? Table.AltText : Table.Id;
            set
            {
                if (Table.AltText != value)
                {
                    Table.AltText = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        public string DataTypesDisplay
        {
            get
            {
                // Extract data types from column PropertyPaths
                if (Table.Columns == null || Table.Columns.Length == 0)
                    return "(no columns)";

                var dataTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var column in Table.Columns)
                {
                    if (column.PropertyPaths != null)
                    {
                        foreach (var path in column.PropertyPaths)
                        {
                            // Extract data type from path like "Bus.Name" or "LVCB.TripUnit.Ltpu"
                            var firstDot = path.IndexOf('.');
                            if (firstDot > 0)
                            {
                                var dataType = path.Substring(0, firstDot);
                                dataTypes.Add(dataType);
                            }
                        }
                    }
                }

                return dataTypes.Count > 0
                    ? string.Join(", ", dataTypes.OrderBy(d => d))
                    : "(none)";
            }
        }

        public int ColumnCount => Table.Columns?.Length ?? 0;

        /// <summary>
        /// AUDIT FIX: Add Mode property for ComboBox binding
        /// </summary>
        public string Mode
        {
            get => Table.Mode ?? "new";
            set
            {
                if (Table.Mode != value)
                {
                    Table.Mode = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class DataTypeStatistic : BindableBase
    {
        private string _dataTypeName = string.Empty;
        private string _statusColor = "Gray";
        private int _fieldsUsed;
        private int _fieldsAvailable;

        public string DataTypeName
        {
            get => _dataTypeName;
            set => SetProperty(ref _dataTypeName, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
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
    }

    public class ValidationResult : BindableBase
    {
        private string _propertyPath = string.Empty;
        private string _status = string.Empty;
        private string _statusColor = "Gray";

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
    }
}
