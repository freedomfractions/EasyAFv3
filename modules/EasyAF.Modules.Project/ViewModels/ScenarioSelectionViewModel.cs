using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Prism.Commands;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// ViewModel for the Scenario Selection Dialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allows users to:
    /// - Select which scenarios to import from a multi-scenario file
    /// - Rename scenarios during import (override names)
    /// - View scenario statistics (entry counts)
    /// - Detect conflicts with existing scenarios
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// <code>
    /// var viewModel = new ScenarioSelectionViewModel(
    ///     discoveredScenarios: new[] { "Main-Min", "Main-Max", "Service-Min", "Service-Max" },
    ///     existingScenarios: new[] { "Main-Min" }, // Conflict warning
    ///     scenarioCounts: new Dictionary&lt;string, int&gt; { { "Main-Min", 80 }, ... }
    /// );
    /// 
    /// var dialog = new ScenarioSelectionDialog { DataContext = viewModel };
    /// if (dialog.ShowDialog() == true)
    /// {
    ///     var selected = viewModel.GetSelectedScenarios();
    ///     var renames = viewModel.GetScenarioRenames();
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public class ScenarioSelectionViewModel : INotifyPropertyChanged
    {
        private bool _selectAll = true;

        /// <summary>
        /// Collection of scenario items (discovered from file).
        /// </summary>
        public ObservableCollection<ScenarioSelectionItem> Scenarios { get; } = new ObservableCollection<ScenarioSelectionItem>();

        /// <summary>
        /// Select All checkbox state.
        /// </summary>
        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                if (SetProperty(ref _selectAll, value))
                {
                    foreach (var scenario in Scenarios)
                    {
                        scenario.IsSelected = value;
                    }
                }
            }
        }

        /// <summary>
        /// Number of selected scenarios.
        /// </summary>
        public int SelectedCount => Scenarios.Count(s => s.IsSelected);

        /// <summary>
        /// Summary text for selected scenarios.
        /// </summary>
        public string SelectionSummary => $"{SelectedCount} of {Scenarios.Count} scenarios selected";

        /// <summary>
        /// Command to select all scenarios.
        /// </summary>
        public ICommand SelectAllCommand { get; }

        /// <summary>
        /// Command to deselect all scenarios.
        /// </summary>
        public ICommand DeselectAllCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioSelectionViewModel"/> class.
        /// </summary>
        /// <param name="discoveredScenarios">Scenarios found in the import file.</param>
        /// <param name="existingScenarios">Scenarios already in the target DataSet (for conflict detection).</param>
        /// <param name="scenarioCounts">Per-scenario entry counts from file audit.</param>
        public ScenarioSelectionViewModel(
            string[] discoveredScenarios,
            string[] existingScenarios,
            System.Collections.Generic.Dictionary<string, int>? scenarioCounts = null)
        {
            var existingSet = new System.Collections.Generic.HashSet<string>(
                existingScenarios,
                System.StringComparer.OrdinalIgnoreCase);

            foreach (var scenario in discoveredScenarios)
            {
                var entryCount = scenarioCounts?.ContainsKey(scenario) == true
                    ? scenarioCounts[scenario]
                    : 0;

                var item = new ScenarioSelectionItem
                {
                    OriginalName = scenario,
                    DisplayName = scenario,
                    IsSelected = true,
                    EntryCount = entryCount,
                    HasConflict = existingSet.Contains(scenario)
                };

                item.PropertyChanged += OnScenarioItemPropertyChanged;
                Scenarios.Add(item);
            }

            SelectAllCommand = new DelegateCommand(() => SelectAll = true);
            DeselectAllCommand = new DelegateCommand(() => SelectAll = false);
        }

        /// <summary>
        /// Gets the list of selected scenario names (after user renames).
        /// </summary>
        public string[] GetSelectedScenarios()
        {
            return Scenarios
                .Where(s => s.IsSelected)
                .Select(s => s.DisplayName)
                .ToArray();
        }

        /// <summary>
        /// Gets dictionary of scenario name overrides (originalName ? newName).
        /// </summary>
        /// <remarks>
        /// Only includes scenarios where user changed the name.
        /// </remarks>
        public System.Collections.Generic.Dictionary<string, string> GetScenarioRenames()
        {
            return Scenarios
                .Where(s => s.IsSelected && s.OriginalName != s.DisplayName)
                .ToDictionary(s => s.OriginalName, s => s.DisplayName);
        }

        private void OnScenarioItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScenarioSelectionItem.IsSelected))
            {
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(SelectionSummary));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Represents a single scenario in the selection dialog.
    /// </summary>
    public class ScenarioSelectionItem : INotifyPropertyChanged
    {
        private bool _isSelected = true;
        private string _displayName = string.Empty;

        /// <summary>
        /// Original scenario name from the file.
        /// </summary>
        public string OriginalName { get; set; } = string.Empty;

        /// <summary>
        /// User-editable display name (for renaming scenarios).
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (SetProperty(ref _displayName, value))
                {
                    OnPropertyChanged(nameof(HasRename));
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        /// <summary>
        /// Whether this scenario is selected for import.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Number of entries for this scenario.
        /// </summary>
        public int EntryCount { get; set; }

        /// <summary>
        /// Indicates whether this scenario name conflicts with existing data.
        /// </summary>
        public bool HasConflict { get; set; }

        /// <summary>
        /// Indicates whether the user renamed this scenario.
        /// </summary>
        public bool HasRename => OriginalName != DisplayName;

        /// <summary>
        /// Display text with entry count and conflict/rename indicators.
        /// </summary>
        public string DisplayText
        {
            get
            {
                var text = DisplayName;
                
                if (EntryCount > 0)
                    text += $" ({EntryCount} entries)";

                if (HasConflict)
                    text += " ? Exists";

                if (HasRename)
                    text += $" (was: {OriginalName})";

                return text;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
