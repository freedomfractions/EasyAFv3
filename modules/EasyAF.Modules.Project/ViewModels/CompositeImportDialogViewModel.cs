using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Data.Models;
using EasyAF.Data.Extensions;
using EasyAF.Modules.Project.Helpers;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// ViewModel for the Composite Import Dialog.
    /// Allows users to selectively import scenarios from multiple files.
    /// </summary>
    public class CompositeImportDialogViewModel : BindableBase
    {
        private readonly List<string> _existingScenarios;

        /// <summary>
        /// Gets the collection of file groups containing scenarios to import.
        /// </summary>
        public ObservableCollection<FileScenarioGroup> FileGroups { get; } = new();

        /// <summary>
        /// Gets the list of existing scenarios in the target dataset (for validation).
        /// </summary>
        public List<string> ExistingScenarios => _existingScenarios;

        public ICommand ImportCommand { get; }
        public ICommand CancelCommand { get; }

        private bool _dialogResult;
        
        /// <summary>
        /// Gets or sets the dialog result.
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult ? (bool?)true : (bool?)false;
            private set
            {
                if (value.HasValue)
                {
                    _dialogResult = value.Value;
                    RaisePropertyChanged();
                }
            }
        }

        public CompositeImportDialogViewModel(
            Dictionary<string, FileScanResult> fileScanResults, 
            List<string> existingScenarios)
        {
            _existingScenarios = existingScenarios ?? new List<string>();

            // Build file groups
            foreach (var kvp in fileScanResults.OrderBy(x => x.Key))
            {
                var filePath = kvp.Key;
                var scanResult = kvp.Value;

                var fileGroup = new FileScenarioGroup(filePath, scanResult.DataTypes, this);

                if (!scanResult.IsNonScenarioFile && scanResult.Scenarios.Count > 0)
                {
                    // Scenario-based file
                    for (int i = 0; i < scanResult.Scenarios.Count; i++)
                    {
                        var scenario = scanResult.Scenarios[i];
                        var action = i == 0 ? ImportAction.AddNew : ImportAction.DoNotImport;
                        
                        fileGroup.Scenarios.Add(new ScenarioImportRow(scenario, action, fileGroup, this));
                    }
                }
                else
                {
                    // Non-scenario file
                    fileGroup.IsNonScenarioFile = true;
                }

                FileGroups.Add(fileGroup);
            }

            ImportCommand = new DelegateCommand(ExecuteImport, CanExecuteImport);
            CancelCommand = new DelegateCommand(ExecuteCancel);

            // Subscribe to changes for validation
            foreach (var fileGroup in FileGroups)
            {
                foreach (var row in fileGroup.Scenarios)
                {
                    row.PropertyChanged += (s, e) =>
                    {
                        // Re-evaluate Import button whenever any row changes
                        (ImportCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    };
                }
            }

            Log.Debug("CompositeImportDialogViewModel initialized with {FileCount} file(s), {ExistingCount} existing scenarios",
                FileGroups.Count, _existingScenarios.Count);
        }

        private bool CanExecuteImport()
        {
            // Check if any scenario has errors
            foreach (var fileGroup in FileGroups)
            {
                if (fileGroup.Scenarios.Any(s => s.HasError))
                    return false;
            }

            // Check if at least one scenario is selected for import
            var hasSelection = FileGroups.Any(fg => 
                fg.Scenarios.Any(s => s.Action != ImportAction.DoNotImport));

            return hasSelection || FileGroups.Any(fg => fg.IsNonScenarioFile);
        }

        private void ExecuteImport()
        {
            Log.Information("User confirmed composite import with {Count} file(s)", FileGroups.Count);
            DialogResult = true;
        }

        private void ExecuteCancel()
        {
            Log.Information("User cancelled composite import");
            DialogResult = false;
        }

        /// <summary>
        /// Gets the import plan for execution.
        /// </summary>
        public List<ScenarioImportPlan> GetImportPlan()
        {
            var plan = new List<ScenarioImportPlan>();

            foreach (var fileGroup in FileGroups)
            {
                if (fileGroup.IsNonScenarioFile)
                {
                    // Import entire file (no scenario filtering)
                    plan.Add(new ScenarioImportPlan
                    {
                        FilePath = fileGroup.FilePath,
                        OriginalScenario = null,
                        TargetScenario = null,
                        Action = ImportAction.AddNew
                    });
                }
                else
                {
                    // Import selected scenarios
                    foreach (var row in fileGroup.Scenarios.Where(s => s.Action != ImportAction.DoNotImport))
                    {
                        var targetScenario = row.Action == ImportAction.AddNew 
                            ? row.NewScenarioName 
                            : row.SelectedExistingScenario;

                        plan.Add(new ScenarioImportPlan
                        {
                            FilePath = fileGroup.FilePath,
                            OriginalScenario = row.ScenarioName,
                            TargetScenario = targetScenario,
                            Action = row.Action
                        });
                    }
                }
            }

            return plan;
        }

        /// <summary>
        /// Validates scenario name against existing scenarios and other rows in dialog.
        /// Only validates against scenarios within the same data type(s).
        /// </summary>
        public bool ValidateScenarioName(string scenarioName, ScenarioImportRow currentRow)
        {
            if (string.IsNullOrWhiteSpace(scenarioName))
                return false;

            var normalized = scenarioName.Trim();

            // Get the data types for the current row's file
            var currentDataTypes = currentRow.ParentFileGroup.DataTypes;

            // Check against existing scenarios in dataset (only for matching data types)
            // NOTE: _existingScenarios is a flat list, so we conservatively check all
            // This could be enhanced to be per-data-type if needed
            if (_existingScenarios.Any(s => string.Equals(s.Trim(), normalized, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Check against other "Add New" scenarios in this dialog (only same data types)
            foreach (var fileGroup in FileGroups)
            {
                // Skip if no data type overlap
                if (!currentDataTypes.Any(dt => fileGroup.DataTypes.Contains(dt)))
                    continue;

                foreach (var row in fileGroup.Scenarios)
                {
                    if (row == currentRow)
                        continue;

                    if (row.Action == ImportAction.AddNew && 
                        !string.IsNullOrWhiteSpace(row.NewScenarioName) &&
                        string.Equals(row.NewScenarioName.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a scenario name would conflict when overwriting.
        /// </summary>
        public bool ValidateOverwriteTarget(string scenarioName, ScenarioImportRow currentRow)
        {
            if (string.IsNullOrWhiteSpace(scenarioName))
                return false;

            var normalized = scenarioName.Trim();

            // Check if another row is also trying to overwrite the same scenario
            foreach (var fileGroup in FileGroups)
            {
                foreach (var row in fileGroup.Scenarios)
                {
                    if (row == currentRow)
                        continue;

                    if (row.Action == ImportAction.Overwrite && 
                        !string.IsNullOrWhiteSpace(row.SelectedExistingScenario) &&
                        string.Equals(row.SelectedExistingScenario.Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Represents a file and its scenarios for import.
    /// </summary>
    public class FileScenarioGroup
    {
        public string FilePath { get; }
        public string FileName => System.IO.Path.GetFileName(FilePath);
        public string FileHeader => $"{FileName} - {FilePath}";
        public List<string> DataTypes { get; }
        
        public ObservableCollection<ScenarioImportRow> Scenarios { get; } = new();
        
        public bool IsNonScenarioFile { get; set; }
        public string NonScenarioMessage => "(No scenarios - will import all data)";

        private readonly CompositeImportDialogViewModel _parentViewModel;

        public FileScenarioGroup(string filePath, List<string> dataTypes, CompositeImportDialogViewModel parentViewModel)
        {
            FilePath = filePath;
            DataTypes = dataTypes ?? new List<string>();
            _parentViewModel = parentViewModel;
        }
    }

    /// <summary>
    /// Represents a single scenario row in the import dialog.
    /// </summary>
    public class ScenarioImportRow : BindableBase
    {
        private readonly CompositeImportDialogViewModel _parentViewModel;
        public FileScenarioGroup ParentFileGroup { get; }

        public string ScenarioName { get; }

        private ImportAction _action;
        public ImportAction Action
        {
            get => _action;
            set
            {
                if (SetProperty(ref _action, value))
                {
                    // Auto-populate new scenario name when switching to "Add New"
                    if (value == ImportAction.AddNew && string.IsNullOrWhiteSpace(_newScenarioName))
                    {
                        _newScenarioName = ScenarioName;
                        RaisePropertyChanged(nameof(NewScenarioName));
                    }

                    RaisePropertyChanged(nameof(ShowTextBox));
                    RaisePropertyChanged(nameof(ShowComboBox));
                    RaisePropertyChanged(nameof(ShowNothing));
                    ValidateCurrentState();
                }
            }
        }

        private string? _newScenarioName;
        public string? NewScenarioName
        {
            get => _newScenarioName;
            set
            {
                if (SetProperty(ref _newScenarioName, value))
                {
                    ValidateCurrentState();
                }
            }
        }

        private string? _selectedExistingScenario;
        public string? SelectedExistingScenario
        {
            get => _selectedExistingScenario;
            set
            {
                if (SetProperty(ref _selectedExistingScenario, value))
                {
                    ValidateCurrentState();
                }
            }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            private set => SetProperty(ref _hasError, value);
        }

        public bool ShowTextBox => Action == ImportAction.AddNew;
        public bool ShowComboBox => Action == ImportAction.Overwrite;
        public bool ShowNothing => Action == ImportAction.DoNotImport;

        public List<string> ExistingScenarios => _parentViewModel.ExistingScenarios;

        public ScenarioImportRow(string scenarioName, ImportAction defaultAction, FileScenarioGroup parentFileGroup, CompositeImportDialogViewModel parentViewModel)
        {
            ScenarioName = scenarioName;
            _action = defaultAction;
            ParentFileGroup = parentFileGroup;
            _parentViewModel = parentViewModel;

            // Pre-fill new scenario name with original scenario name
            if (defaultAction == ImportAction.AddNew)
            {
                _newScenarioName = scenarioName;
            }

            ValidateCurrentState();
        }

        private void ValidateCurrentState()
        {
            switch (Action)
            {
                case ImportAction.AddNew:
                    HasError = !_parentViewModel.ValidateScenarioName(NewScenarioName ?? string.Empty, this);
                    break;

                case ImportAction.Overwrite:
                    HasError = !_parentViewModel.ValidateOverwriteTarget(SelectedExistingScenario ?? string.Empty, this);
                    break;

                case ImportAction.DoNotImport:
                    HasError = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Defines the action to take for a scenario.
    /// </summary>
    public enum ImportAction
    {
        AddNew,
        Overwrite,
        DoNotImport
    }

    /// <summary>
    /// Execution plan for a single scenario import.
    /// </summary>
    public class ScenarioImportPlan
    {
        public string FilePath { get; set; } = string.Empty;
        public string? OriginalScenario { get; set; }
        public string? TargetScenario { get; set; }
        public ImportAction Action { get; set; }
    }
}
