using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasyAF.Data.Models;
using EasyAF.Import;
using EasyAF.Import.Models;
using Microsoft.Win32;
using Prism.Commands;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// ViewModel for the Import Data Dialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multi-step wizard for importing data into a project:
    /// 1. Select source file (CSV/Excel)
    /// 2. Select mapping configuration (.ezmap)
    /// 3. Preview scenarios (if applicable)
    /// 4. Choose import target (New Data / Old Data)
    /// 5. Confirm and execute import
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// <code>
    /// var viewModel = new ImportDataViewModel(project.NewData);
    /// var dialog = new ImportDataDialog { DataContext = viewModel };
    /// if (dialog.ShowDialog() == true)
    /// {
    ///     // Import completed successfully
    ///     project.MarkDirty();
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public class ImportDataViewModel : INotifyPropertyChanged
    {
        private readonly DataSet _targetDataSet;
        private string? _selectedFilePath;
        private string? _selectedMappingPath;
        private ImportAuditResult? _auditResult;
        private bool _isAuditing;
        private bool _isImporting;
        private string? _statusMessage;
        private bool _importToNewData = true;

        /// <summary>
        /// Path to the selected source file.
        /// </summary>
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                if (SetProperty(ref _selectedFilePath, value))
                {
                    OnPropertyChanged(nameof(HasFileSelected));
                    OnPropertyChanged(nameof(CanProceed));
                    AuditFileAsync();
                }
            }
        }

        /// <summary>
        /// Path to the selected mapping configuration file.
        /// </summary>
        public string? SelectedMappingPath
        {
            get => _selectedMappingPath;
            set
            {
                if (SetProperty(ref _selectedMappingPath, value))
                {
                    OnPropertyChanged(nameof(HasMappingSelected));
                    OnPropertyChanged(nameof(CanProceed));
                    AuditFileAsync();
                }
            }
        }

        /// <summary>
        /// Results from auditing the selected file.
        /// </summary>
        public ImportAuditResult? AuditResult
        {
            get => _auditResult;
            private set
            {
                if (SetProperty(ref _auditResult, value))
                {
                    OnPropertyChanged(nameof(HasAuditResult));
                    OnPropertyChanged(nameof(CanProceed));
                }
            }
        }

        /// <summary>
        /// Indicates whether file audit is in progress.
        /// </summary>
        public bool IsAuditing
        {
            get => _isAuditing;
            private set => SetProperty(ref _isAuditing, value);
        }

        /// <summary>
        /// Indicates whether import is in progress.
        /// </summary>
        public bool IsImporting
        {
            get => _isImporting;
            private set
            {
                if (SetProperty(ref _isImporting, value))
                {
                    OnPropertyChanged(nameof(CanProceed));
                }
            }
        }

        /// <summary>
        /// Status message for user feedback.
        /// </summary>
        public string? StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Whether to import to New Data (true) or Old Data (false).
        /// </summary>
        public bool ImportToNewData
        {
            get => _importToNewData;
            set => SetProperty(ref _importToNewData, value);
        }

        /// <summary>
        /// Indicates whether a file has been selected.
        /// </summary>
        public bool HasFileSelected => !string.IsNullOrWhiteSpace(SelectedFilePath);

        /// <summary>
        /// Indicates whether a mapping has been selected.
        /// </summary>
        public bool HasMappingSelected => !string.IsNullOrWhiteSpace(SelectedMappingPath);

        /// <summary>
        /// Indicates whether an audit result is available.
        /// </summary>
        public bool HasAuditResult => AuditResult != null;

        /// <summary>
        /// Indicates whether the user can proceed with import.
        /// </summary>
        public bool CanProceed =>
            HasFileSelected &&
            HasMappingSelected &&
            HasAuditResult &&
            AuditResult!.CanImport &&
            !IsImporting;

        /// <summary>
        /// Recent mapping files (loaded from settings).
        /// </summary>
        public ObservableCollection<string> RecentMappings { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Command to browse for source file.
        /// </summary>
        public ICommand BrowseFileCommand { get; }

        /// <summary>
        /// Command to browse for mapping file.
        /// </summary>
        public ICommand BrowseMappingCommand { get; }

        /// <summary>
        /// Command to show scenario selection dialog.
        /// </summary>
        public ICommand SelectScenariosCommand { get; }

        /// <summary>
        /// Selected scenarios (from scenario selection dialog).
        /// </summary>
        public string[]? SelectedScenarios { get; private set; }

        /// <summary>
        /// Scenario rename overrides (from scenario selection dialog).
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string>? ScenarioRenames { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDataViewModel"/> class.
        /// </summary>
        /// <param name="targetDataSet">The DataSet to import into.</param>
        public ImportDataViewModel(DataSet targetDataSet)
        {
            _targetDataSet = targetDataSet ?? throw new ArgumentNullException(nameof(targetDataSet));

            BrowseFileCommand = new DelegateCommand(ExecuteBrowseFile);
            BrowseMappingCommand = new DelegateCommand(ExecuteBrowseMapping);
            SelectScenariosCommand = new DelegateCommand(ExecuteSelectScenarios, CanExecuteSelectScenarios)
                .ObservesProperty(() => HasAuditResult)
                .ObservesProperty(() => AuditResult);

            // TODO: Load recent mappings from settings
            // For now, empty collection
        }

        private void ExecuteBrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Data File to Import",
                Filter = "Data Files (*.csv;*.xls;*.xlsx)|*.csv;*.xls;*.xlsx|CSV Files (*.csv)|*.csv|Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedFilePath = dialog.FileName;
            }
        }

        private void ExecuteBrowseMapping()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Mapping Configuration",
                Filter = "Mapping Files (*.ezmap)|*.ezmap|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedMappingPath = dialog.FileName;
            }
        }

        private bool CanExecuteSelectScenarios()
        {
            return HasAuditResult && AuditResult!.HasScenarios;
        }

        private void ExecuteSelectScenarios()
        {
            if (AuditResult == null || !AuditResult.HasScenarios)
                return;

            var existingScenarios = _targetDataSet.GetAvailableScenarios().ToArray();

            var viewModel = new ScenarioSelectionViewModel(
                discoveredScenarios: AuditResult.DiscoveredScenarios.ToArray(),
                existingScenarios: existingScenarios,
                scenarioCounts: AuditResult.ScenarioCounts
            );

            var dialog = new Views.ScenarioSelectionDialog
            {
                DataContext = viewModel,
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedScenarios = viewModel.GetSelectedScenarios();
                ScenarioRenames = viewModel.GetScenarioRenames();
                
                StatusMessage = $"Selected {SelectedScenarios.Length} of {AuditResult.DiscoveredScenarios.Count} scenarios";
            }
        }

        private async void AuditFileAsync()
        {
            if (!HasFileSelected || !HasMappingSelected)
            {
                AuditResult = null;
                StatusMessage = null;
                return;
            }

            IsAuditing = true;
            StatusMessage = "Analyzing file...";

            try
            {
                // TODO: Implement actual file audit using ImportManager
                // For now, create a mock result
                await System.Threading.Tasks.Task.Delay(500); // Simulate async work

                // PLACEHOLDER: Replace with actual ImportManager.AuditFile call
                var mockResult = new ImportAuditResult
                {
                    FilePath = SelectedFilePath!,
                    DetectedDataTypes = new System.Collections.Generic.List<string> { "ArcFlash", "ShortCircuit" },
                    DiscoveredScenarios = new System.Collections.Generic.List<string> { "Main-Min", "Main-Max" },
                    DataTypeCounts = new System.Collections.Generic.Dictionary<string, int>
                    {
                        { "ArcFlash", 84 },
                        { "ShortCircuit", 76 }
                    },
                    ScenarioCounts = new System.Collections.Generic.Dictionary<string, int>
                    {
                        { "Main-Min", 80 },
                        { "Main-Max", 80 }
                    },
                    TotalRows = 162,
                    ValidRows = 160
                };

                AuditResult = mockResult;
                StatusMessage = mockResult.Summary;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to audit file {FilePath}", SelectedFilePath);
                StatusMessage = $"Error: {ex.Message}";
                AuditResult = null;
            }
            finally
            {
                IsAuditing = false;
            }
        }

        /// <summary>
        /// Executes the import operation.
        /// </summary>
        /// <returns>True if import succeeded; false otherwise.</returns>
        public async System.Threading.Tasks.Task<bool> ExecuteImportAsync()
        {
            if (!CanProceed)
                return false;

            IsImporting = true;
            StatusMessage = "Importing data...";

            try
            {
                // TODO: Implement actual import using ImportManager
                await System.Threading.Tasks.Task.Delay(1000); // Simulate import

                // PLACEHOLDER: Replace with actual ImportManager.Import call
                /*
                var options = new ImportOptions
                {
                    SelectedScenarios = SelectedScenarios?.ToList(),
                    ScenarioOverrides = ScenarioRenames
                };

                var importManager = new ImportManager();
                var mapping = MappingConfig.Load(SelectedMappingPath!);
                
                importManager.Import(SelectedFilePath!, mapping, _targetDataSet, options);
                */

                StatusMessage = "Import completed successfully";
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to import file {FilePath}", SelectedFilePath);
                StatusMessage = $"Import failed: {ex.Message}";
                return false;
            }
            finally
            {
                IsImporting = false;
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
