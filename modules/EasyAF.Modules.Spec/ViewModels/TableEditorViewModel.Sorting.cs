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
    /// Partial class containing sorting-related functionality for TableEditorViewModel.
    /// </summary>
    public partial class TableEditorViewModel
    {
        #region Sorting Properties

        /// <summary>
        /// Gets the collection of sort specs for this table.
        /// </summary>
        public ObservableCollection<SortSpecViewModel> SortSpecs { get; private set; } = new();

        private SortSpecViewModel? _selectedSortSpec;

        /// <summary>
        /// Gets or sets the currently selected sort spec.
        /// </summary>
        public SortSpecViewModel? SelectedSortSpec
        {
            get => _selectedSortSpec;
            set => SetProperty(ref _selectedSortSpec, value);
        }

        /// <summary>
        /// Gets the sort spec count for display in the expander header.
        /// </summary>
        public int SortSpecCount => SortSpecs.Count;

        #endregion

        #region Sorting Commands

        /// <summary>
        /// Command to add a new sort spec.
        /// </summary>
        public ICommand AddSortSpecCommand { get; private set; } = null!;

        /// <summary>
        /// Command to remove the selected sort spec.
        /// </summary>
        public ICommand RemoveSortSpecCommand { get; private set; } = null!;

        /// <summary>
        /// Command to move the selected sort spec up in priority.
        /// </summary>
        public ICommand MoveSortSpecUpCommand { get; private set; } = null!;

        /// <summary>
        /// Command to move the selected sort spec down in priority.
        /// </summary>
        public ICommand MoveSortSpecDownCommand { get; private set; } = null!;

        #endregion

        #region Sorting Initialization

        /// <summary>
        /// Initializes sorting-related commands and collections.
        /// Call this from the constructor.
        /// </summary>
        private void InitializeSorting()
        {
            SortSpecs = new ObservableCollection<SortSpecViewModel>();
            RefreshSortSpecs();

            AddSortSpecCommand = new DelegateCommand(ExecuteAddSortSpec);
            RemoveSortSpecCommand = new DelegateCommand(ExecuteRemoveSortSpec, CanExecuteRemoveSortSpec)
                .ObservesProperty(() => SelectedSortSpec);
            MoveSortSpecUpCommand = new DelegateCommand(ExecuteMoveSortSpecUp, CanExecuteMoveSortSpecUp)
                .ObservesProperty(() => SelectedSortSpec);
            MoveSortSpecDownCommand = new DelegateCommand(ExecuteMoveSortSpecDown, CanExecuteMoveSortSpecDown)
                .ObservesProperty(() => SelectedSortSpec);
        }

        #endregion

        #region Sorting Command Implementations

        private void ExecuteAddSortSpec()
        {
            // Ensure SortSpecs array exists
            if (_table.SortSpecs == null)
            {
                _table.SortSpecs = Array.Empty<SortSpec>();
            }

            // Create new sort spec (default to Column 1, ascending)
            var newSort = new SortSpec
            {
                Column = 1,
                Direction = "asc",
                Numeric = false
            };

            // Add to array
            var newArray = new List<SortSpec>(_table.SortSpecs) { newSort };
            _table.SortSpecs = newArray.ToArray();

            // Refresh UI
            RefreshSortSpecs();

            // Select the new sort spec
            SelectedSortSpec = SortSpecs.LastOrDefault();

            _document.MarkDirty();
            RaisePropertyChanged(nameof(SortSpecCount));

            Log.Information("Added new sort spec to table {TableId}", _table.Id);
        }

        private bool CanExecuteRemoveSortSpec() => SelectedSortSpec != null;

        private void ExecuteRemoveSortSpec()
        {
            if (SelectedSortSpec == null || _table.SortSpecs == null) return;

            var sortToRemove = SelectedSortSpec.SortSpec;
            var index = SortSpecs.IndexOf(SelectedSortSpec);

            // Remove from array
            var newArray = _table.SortSpecs.Where(s => s != sortToRemove).ToArray();
            _table.SortSpecs = newArray.Length > 0 ? newArray : null;

            // Refresh UI
            RefreshSortSpecs();

            // Select next or previous
            if (SortSpecs.Count > 0)
            {
                SelectedSortSpec = SortSpecs.ElementAtOrDefault(Math.Min(index, SortSpecs.Count - 1));
            }

            _document.MarkDirty();
            RaisePropertyChanged(nameof(SortSpecCount));

            Log.Information("Removed sort spec from table {TableId}", _table.Id);
        }

        private bool CanExecuteMoveSortSpecUp() =>
            SelectedSortSpec != null && SortSpecs.IndexOf(SelectedSortSpec) > 0;

        private void ExecuteMoveSortSpecUp()
        {
            if (SelectedSortSpec == null || _table.SortSpecs == null) return;

            var index = Array.IndexOf(_table.SortSpecs, SelectedSortSpec.SortSpec);
            if (index > 0)
            {
                // Swap in array
                var temp = _table.SortSpecs[index];
                _table.SortSpecs[index] = _table.SortSpecs[index - 1];
                _table.SortSpecs[index - 1] = temp;

                RefreshSortSpecs();
                SelectedSortSpec = SortSpecs[index - 1];
                _document.MarkDirty();

                Log.Debug("Moved sort spec up");
            }
        }

        private bool CanExecuteMoveSortSpecDown() =>
            SelectedSortSpec != null && SortSpecs.IndexOf(SelectedSortSpec) < SortSpecs.Count - 1;

        private void ExecuteMoveSortSpecDown()
        {
            if (SelectedSortSpec == null || _table.SortSpecs == null) return;

            var index = Array.IndexOf(_table.SortSpecs, SelectedSortSpec.SortSpec);
            if (index < _table.SortSpecs.Length - 1)
            {
                // Swap in array
                var temp = _table.SortSpecs[index];
                _table.SortSpecs[index] = _table.SortSpecs[index + 1];
                _table.SortSpecs[index + 1] = temp;

                RefreshSortSpecs();
                SelectedSortSpec = SortSpecs[index + 1];
                _document.MarkDirty();

                Log.Debug("Moved sort spec down");
            }
        }

        #endregion

        #region Sorting Helper Methods

        /// <summary>
        /// Refreshes the SortSpecs collection from the table spec.
        /// </summary>
        private void RefreshSortSpecs()
        {
            var selectedSort = SelectedSortSpec?.SortSpec;

            SortSpecs.Clear();

            if (_table.SortSpecs != null)
            {
                for (int i = 0; i < _table.SortSpecs.Length; i++)
                {
                    var sortVm = new SortSpecViewModel(_table.SortSpecs[i], OnSortSpecChanged, i + 1);
                    SortSpecs.Add(sortVm);
                }
            }

            // Restore selection if possible
            if (selectedSort != null)
            {
                SelectedSortSpec = SortSpecs.FirstOrDefault(s => s.SortSpec == selectedSort);
            }

            RaisePropertyChanged(nameof(SortSpecCount));
        }

        /// <summary>
        /// Called when a sort spec property changes.
        /// </summary>
        private void OnSortSpecChanged()
        {
            _document.MarkDirty();
        }

        #endregion
    }
}
