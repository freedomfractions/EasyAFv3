using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Engine;
using EasyAF.Modules.Spec.Models;
using EasyAF.Modules.Map.Services;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// View model for editing a single table's column definitions.
    /// </summary>
    /// <remarks>
    /// This VM provides a WYSIWYG interface for defining columns in a report or label table.
    /// Users can add, remove, reorder, and configure columns with formatting options.
    /// </remarks>
    public partial class TableEditorViewModel : BindableBase, IDisposable
    {
        private readonly TableSpec _table;
        private readonly SpecDocument _document;
        private readonly IUserDialogService _dialogService;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly ISettingsService _settingsService; // NEW: For property visibility filtering
        private ColumnViewModel? _selectedColumn;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the TableEditorViewModel.
        /// </summary>
        /// <param name="table">The table spec to edit.</param>
        /// <param name="document">The parent spec document.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        /// <param name="propertyDiscovery">Service for discovering data type properties.</param>
        /// <param name="settingsService">Service for accessing settings (property visibility).</param>
        public TableEditorViewModel(TableSpec table, SpecDocument document, IUserDialogService dialogService, IPropertyDiscoveryService propertyDiscovery, ISettingsService settingsService)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // Initialize column collection
            Columns = new ObservableCollection<ColumnViewModel>();
            RefreshColumns();

            // Initialize filter functionality (from partial class)
            InitializeFilters();

            // Initialize sorting functionality (from partial class)
            InitializeSorting();

            // Initialize empty message functionality (from partial class)
            InitializeEmptyMessage();

            // Initialize commands
            AddColumnCommand = new DelegateCommand(ExecuteAddColumn);
            RemoveColumnCommand = new DelegateCommand(ExecuteRemoveColumn, CanExecuteRemoveColumn)
                .ObservesProperty(() => SelectedColumn);
            MoveColumnUpCommand = new DelegateCommand(ExecuteMoveColumnUp, CanExecuteMoveColumnUp)
                .ObservesProperty(() => SelectedColumn);
            MoveColumnDownCommand = new DelegateCommand(ExecuteMoveColumnDown, CanExecuteMoveColumnDown)
                .ObservesProperty(() => SelectedColumn);
            EditColumnCommand = new DelegateCommand(ExecuteEditColumn, CanExecuteEditColumn)
                .ObservesProperty(() => SelectedColumn);

            Log.Debug("TableEditorViewModel initialized for table: {TableId}/{AltText}", _table.Id, _table.AltText);
        }

        #region Properties

        /// <summary>
        /// Gets or sets the table display name (AltText).
        /// </summary>
        public string TableName
        {
            get => !string.IsNullOrEmpty(_table.AltText) ? _table.AltText : _table.Id;
            set
            {
                if (_table.AltText != value)
                {
                    _table.AltText = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                    Log.Debug("Table name changed to: {Name}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the table mode (new/diff).
        /// </summary>
        public string Mode
        {
            get => _table.Mode ?? "new";
            set
            {
                if (_table.Mode != value)
                {
                    _table.Mode = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsDiffMode));
                    Log.Debug("Table mode changed to: {Mode}", value);
                }
            }
        }

        /// <summary>
        /// Gets whether the table is in diff mode (for enabling HideIfNoDiff checkbox).
        /// </summary>
        public bool IsDiffMode => string.Equals(Mode, "diff", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets whether to hide the table if no differences are found (diff mode only).
        /// </summary>
        public bool HideIfNoDiff
        {
            get => _table.HideIfNoDiff ?? false;
            set
            {
                if (_table.HideIfNoDiff != value)
                {
                    _table.HideIfNoDiff = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                    Log.Debug("HideIfNoDiff changed to: {Value}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether rows can break across pages.
        /// </summary>
        public bool AllowRowBreakAcrossPages
        {
            get => _table.AllowRowBreakAcrossPages ?? false;
            set
            {
                if (_table.AllowRowBreakAcrossPages != value)
                {
                    _table.AllowRowBreakAcrossPages = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                    Log.Debug("AllowRowBreakAcrossPages changed to: {Value}", value);
                }
            }
        }

        /// <summary>
        /// Gets the collection of columns for this table.
        /// </summary>
        public ObservableCollection<ColumnViewModel> Columns { get; }

        /// <summary>
        /// Gets or sets the currently selected column.
        /// </summary>
        public ColumnViewModel? SelectedColumn
        {
            get => _selectedColumn;
            set => SetProperty(ref _selectedColumn, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to add a new column.
        /// </summary>
        public ICommand AddColumnCommand { get; }

        /// <summary>
        /// Command to remove the selected column.
        /// </summary>
        public ICommand RemoveColumnCommand { get; }

        /// <summary>
        /// Command to move the selected column up.
        /// </summary>
        public ICommand MoveColumnUpCommand { get; }

        /// <summary>
        /// Command to move the selected column down.
        /// </summary>
        public ICommand MoveColumnDownCommand { get; }

        /// <summary>
        /// Command to edit the selected column's properties.
        /// </summary>
        public ICommand EditColumnCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteAddColumn()
        {
            // Create new column spec
            var newColumn = new ColumnSpec
            {
                Header = $"Column{_table.Columns.Length + 1}",
                PropertyPaths = new[] { string.Empty },
                WidthPercent = 10.0
            };

            // Add to array (need to create new array since it's not a collection)
            var newArray = new List<ColumnSpec>(_table.Columns) { newColumn };
            _table.Columns = newArray.ToArray();
            
            RefreshColumns();

            // Select the new column
            SelectedColumn = Columns.LastOrDefault();

            Log.Information("Added new column: {Header} to table {TableId}", newColumn.Header, _table.Id);
        }

        private bool CanExecuteRemoveColumn() => SelectedColumn != null;

        private void ExecuteRemoveColumn()
        {
            if (SelectedColumn == null) return;

            var columnToRemove = SelectedColumn.Column;
            var index = Columns.IndexOf(SelectedColumn);

            // Remove from array
            var newArray = _table.Columns.Where(c => c != columnToRemove).ToArray();
            _table.Columns = newArray;
            
            RefreshColumns();

            // Select next column or previous if at end
            if (Columns.Count > 0)
            {
                SelectedColumn = Columns.ElementAtOrDefault(Math.Min(index, Columns.Count - 1));
            }

            Log.Information("Removed column: {Header} from table {TableId}", columnToRemove.Header, _table.Id);
        }

        private bool CanExecuteMoveColumnUp() =>
            SelectedColumn != null && Columns.IndexOf(SelectedColumn) > 0;

        private void ExecuteMoveColumnUp()
        {
            if (SelectedColumn == null) return;

            var index = Array.IndexOf(_table.Columns, SelectedColumn.Column);
            if (index > 0)
            {
                // Swap in array
                var temp = _table.Columns[index];
                _table.Columns[index] = _table.Columns[index - 1];
                _table.Columns[index - 1] = temp;
                
                RefreshColumns();
                SelectedColumn = Columns[index - 1];
                Log.Debug("Moved column up: {Header}", SelectedColumn.Column.Header);
            }
        }

        private bool CanExecuteMoveColumnDown() =>
            SelectedColumn != null && Columns.IndexOf(SelectedColumn) < Columns.Count - 1;

        private void ExecuteMoveColumnDown()
        {
            if (SelectedColumn == null) return;

            var index = Array.IndexOf(_table.Columns, SelectedColumn.Column);
            if (index < _table.Columns.Length - 1)
            {
                // Swap in array
                var temp = _table.Columns[index];
                _table.Columns[index] = _table.Columns[index + 1];
                _table.Columns[index + 1] = temp;
                
                RefreshColumns();
                SelectedColumn = Columns[index + 1];
                Log.Debug("Moved column down: {Header}", SelectedColumn.Column.Header);
            }
        }

        private bool CanExecuteEditColumn() => SelectedColumn != null;

        private void ExecuteEditColumn()
        {
            if (SelectedColumn == null)
            {
                Log.Warning("Edit column requested but no column selected");
                return;
            }

            try
            {
                // Open PropertyPath picker dialog with real property discovery
                var currentPaths = SelectedColumn.Column.PropertyPaths ?? Array.Empty<string>();
                var viewModel = new Dialogs.PropertyPathPickerViewModel(currentPaths, _document, _propertyDiscovery, _settingsService);
                var dialog = new Views.Dialogs.PropertyPathPickerDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();
                
                if (result == true)
                {
                    // Apply selected property paths
                    SelectedColumn.Column.PropertyPaths = viewModel.ResultPaths;
                    
                    // Refresh the display
                    SelectedColumn.RaisePropertyChanged(nameof(ColumnViewModel.PropertyPathsDisplay));
                    
                    _document.MarkDirty();
                    
                    Log.Information("Updated property paths for column '{Header}': {Paths}",
                        SelectedColumn.ColumnHeader,
                        string.Join(", ", viewModel.ResultPaths));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open property path picker");
                _dialogService.ShowError("Failed to open property path picker", ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Refreshes the Columns collection from the table spec.
        /// </summary>
        private void RefreshColumns()
        {
            var selectedColumn = SelectedColumn?.Column;

            Columns.Clear();
            for (int i = 0; i < _table.Columns.Length; i++)
            {
                var colVm = new ColumnViewModel(_table.Columns[i], i + 1);
                Columns.Add(colVm);
            }

            // Restore selection if possible
            if (selectedColumn != null)
            {
                SelectedColumn = Columns.FirstOrDefault(c => c.Column == selectedColumn);
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// View model wrapper for a single column in the table editor.
    /// </summary>
    public class ColumnViewModel : BindableBase
    {
        private readonly int _orderIndex;

        /// <summary>
        /// Initializes a new instance of the ColumnViewModel.
        /// </summary>
        /// <param name="column">The underlying column spec.</param>
        /// <param name="orderIndex">The 1-based display order.</param>
        public ColumnViewModel(ColumnSpec column, int orderIndex)
        {
            Column = column ?? throw new ArgumentNullException(nameof(column));
            _orderIndex = orderIndex;
        }

        /// <summary>
        /// Gets the underlying column spec.
        /// </summary>
        public ColumnSpec Column { get; }

        /// <summary>
        /// Gets the display order (1-based).
        /// </summary>
        public int OrderIndex => _orderIndex;

        /// <summary>
        /// Gets or sets the column header (editable).
        /// </summary>
        public string ColumnHeader
        {
            get => Column.Header ?? $"Column{_orderIndex}";
            set
            {
                if (Column.Header != value)
                {
                    Column.Header = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the property paths display (read-only - use Edit button to change).
        /// </summary>
        public string PropertyPathsDisplay
        {
            get
            {
                if (Column.PropertyPaths == null || Column.PropertyPaths.Length == 0)
                    return "(none)";
                return string.Join(", ", Column.PropertyPaths);
            }
        }

        /// <summary>
        /// Gets or sets the column width percent (editable).
        /// </summary>
        public string WidthPercent
        {
            get => Column.WidthPercent.HasValue ? Column.WidthPercent.Value.ToString("F1") : "";
            set
            {
                if (double.TryParse(value, out var parsed))
                {
                    Column.WidthPercent = parsed;
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    Column.WidthPercent = null;
                }
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the format string (editable).
        /// </summary>
        public string FormatDisplay
        {
            get => Column.Format ?? "";
            set
            {
                if (Column.Format != value)
                {
                    Column.Format = string.IsNullOrWhiteSpace(value) ? null : value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Manually raises PropertyChanged for a specific property name.
        /// Used when PropertyPaths is updated via dialog.
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
        }
    }
}
