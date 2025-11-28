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

            AddFilterCommand = new DelegateCommand(ExecuteAddFilter);
            RemoveFilterCommand = new DelegateCommand(ExecuteRemoveFilter, CanExecuteRemoveFilter)
                .ObservesProperty(() => SelectedFilter);
            PickFilterPropertyCommand = new DelegateCommand(ExecutePickFilterProperty, CanExecutePickFilterProperty)
                .ObservesProperty(() => SelectedFilter);
            PickCompareToPropertyCommand = new DelegateCommand(ExecutePickCompareToProperty, CanExecutePickCompareToProperty)
                .ObservesProperty(() => SelectedFilter);
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
                // Open PropertyPath picker dialog for filter property (single select)
                var currentPath = string.IsNullOrWhiteSpace(SelectedFilter.PropertyPath) 
                    ? Array.Empty<string>() 
                    : new[] { SelectedFilter.PropertyPath };
                    
                var viewModel = new Dialogs.PropertyPathPickerViewModel(currentPath, _document, _propertyDiscovery, _settingsService);
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
                // Open PropertyPath picker dialog for compare-to property (single select)
                var currentPath = string.IsNullOrWhiteSpace(SelectedFilter.RightPropertyPath)
                    ? Array.Empty<string>()
                    : new[] { SelectedFilter.RightPropertyPath };

                var viewModel = new Dialogs.PropertyPathPickerViewModel(currentPath, _document, _propertyDiscovery, _settingsService);
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
                foreach (var filterSpec in _table.FilterSpecs)
                {
                    var filterVm = new FilterSpecViewModel(filterSpec, OnFilterChanged);
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

        #endregion
    }
}
