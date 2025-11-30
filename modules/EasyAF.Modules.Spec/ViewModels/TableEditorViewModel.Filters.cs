using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using EasyAF.Engine;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// Partial class containing filter-related functionality for TableEditorViewModel.
    /// </summary>
    public partial class TableEditorViewModel
    {
        #region Filter Properties

        /// <summary>
        /// Gets the collection of filter specs for this table.
        /// </summary>
        public ObservableCollection<FilterSpecViewModel> Filters { get; private set; } = new();

        private FilterSpecViewModel? _selectedFilter;

        /// <summary>
        /// Gets or sets the currently selected filter.
        /// </summary>
        public FilterSpecViewModel? SelectedFilter
        {
            get => _selectedFilter;
            set => SetProperty(ref _selectedFilter, value);
        }

        /// <summary>
        /// Gets the available filter operators for dropdown binding.
        /// </summary>
        public ObservableCollection<OperatorOption> AvailableOperators { get; private set; } = new();

        /// <summary>
        /// Gets the filter count for display in the expander header.
        /// </summary>
        public int FilterCount => Filters.Count;

        private string _filterLogic = "AND";
        private string? _advancedFilterExpression;

        /// <summary>
        /// Gets or sets the filter logic mode (AND, OR, or Advanced).
        /// </summary>
        public string FilterLogic
        {
            get => _filterLogic;
            set
            {
                if (SetProperty(ref _filterLogic, value))
                {
                    RaisePropertyChanged(nameof(IsAdvancedFilterLogic));
                    
                    // When switching TO Advanced mode, initialize with current AND/OR if empty
                    if (value == "Advanced" && string.IsNullOrWhiteSpace(_advancedFilterExpression))
                    {
                        // Initialize with simple AND logic showing all filter numbers
                        if (_table.FilterSpecs != null && _table.FilterSpecs.Length > 0)
                        {
                            var numbers = Enumerable.Range(1, _table.FilterSpecs.Length);
                            _advancedFilterExpression = string.Join(" & ", numbers);
                            RaisePropertyChanged(nameof(AdvancedFilterExpression));
                        }
                    }
                    
                    // Update the table spec FilterLogic
                    UpdateTableFilterLogic();
                    
                    _document.MarkDirty();
                    Log.Information("Filter logic changed to: {Logic} for table {TableId}", value, _table.Id);
                }
            }
        }

        /// <summary>
        /// Gets whether Advanced filter logic mode is enabled.
        /// </summary>
        public bool IsAdvancedFilterLogic => FilterLogic?.Equals("Advanced", StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// Gets or sets the advanced filter expression when FilterLogic is "Advanced".
        /// This is a free-form text field for complex filter logic like "(1 | 2) & 3".
        /// Stored in TableDefinition.FilterLogic property in the Engine.
        /// </summary>
        public string? AdvancedFilterExpression
        {
            get => _advancedFilterExpression;
            set
            {
                if (SetProperty(ref _advancedFilterExpression, value))
                {
                    UpdateTableFilterLogic();
                    _document.MarkDirty();
                    Log.Debug("Advanced filter expression changed to: {Expression}", value);
                }
            }
        }

        /// <summary>
        /// Updates the TableDefinition.FilterLogic based on current UI state.
        /// </summary>
        private void UpdateTableFilterLogic()
        {
            if (IsAdvancedFilterLogic && !string.IsNullOrWhiteSpace(_advancedFilterExpression))
            {
                _table.FilterLogic = _advancedFilterExpression;
            }
            else if (_filterLogic == "OR")
            {
                _table.FilterLogic = "OR";
            }
            else
            {
                _table.FilterLogic = "AND"; // Default
            }
        }

        #endregion

        #region Filter Commands

        /// <summary>
        /// Command to add a new filter.
        /// </summary>
        public ICommand AddFilterCommand { get; private set; } = null!;

        /// <summary>
        /// Command to remove the selected filter.
        /// </summary>
        public ICommand RemoveFilterCommand { get; private set; } = null!;

        /// <summary>
        /// Command to pick a property path for the selected filter.
        /// </summary>
        public ICommand PickFilterPropertyCommand { get; private set; } = null!;

        /// <summary>
        /// Command to pick a "compare to" property for the selected filter.
        /// </summary>
        public ICommand PickCompareToPropertyCommand { get; private set; } = null!;

        /// <summary>
        /// Command to edit the selected filter (opens dialog).
        /// </summary>
        public ICommand EditFilterCommand { get; private set; } = null!;

        #endregion

        #region Filter Initialization

        /// <summary>
        /// Initializes filter-related commands and collections.
        /// Call this from the constructor.
        /// </summary>
        private void InitializeFilters()
        {
            Filters = new ObservableCollection<FilterSpecViewModel>();
            AvailableOperators = OperatorOption.GetStandardOperators();
            RefreshFilters();

            // Load FilterLogic from table
            LoadFilterLogicFromTable();

            AddFilterCommand = new DelegateCommand(ExecuteAddFilter);
            RemoveFilterCommand = new DelegateCommand(ExecuteRemoveFilter, CanExecuteRemoveFilter)
                .ObservesProperty(() => SelectedFilter);
            PickFilterPropertyCommand = new DelegateCommand(ExecutePickFilterProperty, CanExecutePickFilterProperty)
                .ObservesProperty(() => SelectedFilter);
            PickCompareToPropertyCommand = new DelegateCommand(ExecutePickCompareToProperty, CanExecutePickCompareToProperty)
                .ObservesProperty(() => SelectedFilter);
            EditFilterCommand = new DelegateCommand(ExecuteEditFilter, CanExecuteEditFilter)
                .ObservesProperty(() => SelectedFilter);
        }

        /// <summary>
        /// Loads the FilterLogic setting from the table into the ViewModel.
        /// </summary>
        private void LoadFilterLogicFromTable()
        {
            if (!string.IsNullOrWhiteSpace(_table.FilterLogic))
            {
                // Check if it's an advanced expression
                if (!_table.FilterLogic.Equals("AND", StringComparison.OrdinalIgnoreCase) &&
                    !_table.FilterLogic.Equals("OR", StringComparison.OrdinalIgnoreCase))
                {
                    // It's an advanced expression
                    _filterLogic = "Advanced";
                    _advancedFilterExpression = _table.FilterLogic;
                }
                else
                {
                    // Simple AND or OR
                    _filterLogic = _table.FilterLogic;
                }
            }
            else
            {
                // Default to AND
                _filterLogic = "AND";
            }

            RaisePropertyChanged(nameof(FilterLogic));
            RaisePropertyChanged(nameof(IsAdvancedFilterLogic));
            RaisePropertyChanged(nameof(AdvancedFilterExpression));
        }

        #endregion

        #region Filter Command Implementations

        private void ExecuteAddFilter()
        {
            // Ensure FilterSpecs array exists
            if (_table.FilterSpecs == null)
            {
                _table.FilterSpecs = Array.Empty<FilterSpec>();
            }

            // Create new filter spec
            var newFilter = new FilterSpec
            {
                PropertyPath = string.Empty,
                Operator = "eq",
                Value = string.Empty,
                Numeric = false
            };

            // Add to array
            var newArray = new List<FilterSpec>(_table.FilterSpecs) { newFilter };
            _table.FilterSpecs = newArray.ToArray();

            // Refresh UI
            RefreshFilters();

            // Select the new filter
            SelectedFilter = Filters.LastOrDefault();

            _document.MarkDirty();
            RaisePropertyChanged(nameof(FilterCount));

            Log.Information("Added new filter to table {TableId}", _table.Id);
        }

        private bool CanExecuteRemoveFilter() => SelectedFilter != null;

        private void ExecuteRemoveFilter()
        {
            if (SelectedFilter == null || _table.FilterSpecs == null) return;

            var filterToRemove = SelectedFilter.FilterSpec;
            var index = Filters.IndexOf(SelectedFilter);

            // Remove from array
            var newArray = _table.FilterSpecs.Where(f => f != filterToRemove).ToArray();
            _table.FilterSpecs = newArray.Length > 0 ? newArray : null;

            // Refresh UI
            RefreshFilters();

            // Select next filter or previous if at end
            if (Filters.Count > 0)
            {
                SelectedFilter = Filters.ElementAtOrDefault(Math.Min(index, Filters.Count - 1));
            }

            _document.MarkDirty();
            RaisePropertyChanged(nameof(FilterCount));

            Log.Information("Removed filter from table {TableId}", _table.Id);
        }

        private bool CanExecutePickFilterProperty() => SelectedFilter != null;

        private void ExecutePickFilterProperty()
        {
            if (SelectedFilter == null) return;

            try
            {
                // Open PropertyPath picker dialog for filter property (SINGLE SELECT)
                var currentPath = string.IsNullOrWhiteSpace(SelectedFilter.PropertyPath) 
                    ? Array.Empty<string>() 
                    : new[] { SelectedFilter.PropertyPath };
                    
                var viewModel = new Dialogs.PropertyPathPickerViewModel(
                    currentPath, 
                    _document, 
                    _propertyDiscovery, 
                    _settingsService,
                    allowMultiSelect: false); // SINGLE SELECT for filters
                    
                var dialog = new Views.Dialogs.PropertyPathPickerDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    // Take first selected path (or empty if none)
                    SelectedFilter.PropertyPath = viewModel.ResultPaths.FirstOrDefault() ?? string.Empty;

                    _document.MarkDirty();

                    Log.Information("Updated filter property path: {Path}", SelectedFilter.PropertyPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open filter property path picker");
                _dialogService.ShowError("Failed to open filter property path picker", ex.Message);
            }
        }

        private bool CanExecutePickCompareToProperty() => SelectedFilter?.IsValueMode == false;

        private void ExecutePickCompareToProperty()
        {
            if (SelectedFilter == null) return;

            try
            {
                // Open PropertyPath picker dialog for compare-to property (SINGLE SELECT)
                var currentPath = string.IsNullOrWhiteSpace(SelectedFilter.RightPropertyPath)
                    ? Array.Empty<string>()
                    : new[] { SelectedFilter.RightPropertyPath };

                var viewModel = new Dialogs.PropertyPathPickerViewModel(
                    currentPath, 
                    _document, 
                    _propertyDiscovery, 
                    _settingsService,
                    allowMultiSelect: false); // SINGLE SELECT for filters
                    
                var dialog = new Views.Dialogs.PropertyPathPickerDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    // Take first selected path (or empty if none)
                    var selectedPath = viewModel.ResultPaths.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(selectedPath))
                    {
                        SelectedFilter.RightPropertyPath = selectedPath;
                        // When setting RightPropertyPath, clear Value
                        SelectedFilter.Value = null;
                    }
                    else
                    {
                        // Clearing the property comparison - revert to value mode
                        SelectedFilter.RightPropertyPath = null;
                    }

                    _document.MarkDirty();

                    Log.Information("Updated filter compare-to property path: {Path}", SelectedFilter.RightPropertyPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open compare-to property path picker");
                _dialogService.ShowError("Failed to open compare-to property path picker", ex.Message);
            }
        }

        #endregion

        #region Filter Helper Methods

        /// <summary>
        /// Refreshes the Filters collection from the table spec.
        /// </summary>
        private void RefreshFilters()
        {
            var selectedFilter = SelectedFilter?.FilterSpec;

            Filters.Clear();

            if (_table.FilterSpecs != null)
            {
                for (int i = 0; i < _table.FilterSpecs.Length; i++)
                {
                    var filterVm = new FilterSpecViewModel(_table.FilterSpecs[i], OnFilterChanged, i + 1);
                    Filters.Add(filterVm);
                }
            }

            // Restore selection if possible
            if (selectedFilter != null)
            {
                SelectedFilter = Filters.FirstOrDefault(f => f.FilterSpec == selectedFilter);
            }

            RaisePropertyChanged(nameof(FilterCount));
        }

        /// <summary>
        /// Called when a filter property changes.
        /// </summary>
        private void OnFilterChanged()
        {
            _document.MarkDirty();
        }

        private bool CanExecuteEditFilter() => SelectedFilter != null;

        private void ExecuteEditFilter()
        {
            if (SelectedFilter == null) return;

            try
            {
                // Store the filter for re-selection
                var filterSpec = SelectedFilter.FilterSpec;
                var index = Filters.IndexOf(SelectedFilter);

                // Open Filter Editor Dialog
                var viewModel = new Dialogs.FilterEditorViewModel(
                    filterSpec,
                    _document,
                    _propertyDiscovery,
                    _settingsService,
                    OnFilterChanged);

                var dialog = new Views.Dialogs.FilterEditorDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    // Refresh the selected filter's properties to update UI bindings
                    SelectedFilter.RefreshProperties();
                    
                    Log.Information("Edited filter #{RuleNumber}: {Summary}", 
                        SelectedFilter.RuleNumber, SelectedFilter.Summary);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to edit filter");
                _dialogService.ShowError("Failed to edit filter", ex.Message);
            }
        }

        #endregion
    }
}
