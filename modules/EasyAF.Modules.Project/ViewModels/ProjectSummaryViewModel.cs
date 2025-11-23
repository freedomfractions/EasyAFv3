using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Modules.Project.Models;
using EasyAF.Data.Models;
using EasyAF.Data.Extensions;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// View model for the project summary tab showing metadata and statistics.
    /// </summary>
    /// <remarks>
    /// This tab provides:
    /// - Project metadata editing (LB Project Number, Site Name, Client, etc.)
    /// - File management (add/remove referenced files)
    /// - Data statistics (equipment counts for New/Old data)
    /// - Project type selection (Standard vs Composite pipeline)
    /// </remarks>
    public class ProjectSummaryViewModel : BindableBase, IDisposable
    {
        private readonly ProjectDocument _document;
        private readonly IUserDialogService _dialogService;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the ProjectSummaryViewModel.
        /// </summary>
        /// <param name="document">The project document.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        /// <exception cref="ArgumentNullException">If document or dialogService is null.</exception>
        public ProjectSummaryViewModel(ProjectDocument document, IUserDialogService dialogService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Commands
            AddFileCommand = new DelegateCommand(ExecuteAddFile);
            RemoveFileCommand = new DelegateCommand(ExecuteRemoveFile, CanExecuteRemoveFile);
            BrowseMapCommand = new DelegateCommand(ExecuteBrowseMap);
            BrowseSpecCommand = new DelegateCommand(ExecuteBrowseSpec);
            BrowseTemplateCommand = new DelegateCommand(ExecuteBrowseTemplate);
            BrowseOutputCommand = new DelegateCommand(ExecuteBrowseOutput);
            ImportNewDataCommand = new DelegateCommand(ExecuteImportNewData);
            ImportOldDataCommand = new DelegateCommand(ExecuteImportOldData);

            // Build initial tree nodes (will be empty if no data loaded)
            RefreshStatistics();

            Log.Debug("ProjectSummaryViewModel initialized");
        }

        #region Metadata Properties

        /// <summary>
        /// Gets or sets the LB Project Number.
        /// </summary>
        public string? LBProjectNumber
        {
            get => _document.Project.LBProjectNumber;
            set
            {
                if (_document.Project.LBProjectNumber != value)
                {
                    _document.Project.LBProjectNumber = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Site Name.
        /// </summary>
        public string? SiteName
        {
            get => _document.Project.SiteName;
            set
            {
                if (_document.Project.SiteName != value)
                {
                    _document.Project.SiteName = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Client name.
        /// </summary>
        public string? Client
        {
            get => _document.Project.Client;
            set
            {
                if (_document.Project.Client != value)
                {
                    _document.Project.Client = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Study Engineer.
        /// </summary>
        public string? StudyEngineer
        {
            get => _document.Project.StudyEngineer;
            set
            {
                if (_document.Project.StudyEngineer != value)
                {
                    _document.Project.StudyEngineer = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets Address Line 1.
        /// </summary>
        public string? AddressLine1
        {
            get => _document.Project.AddressLine1;
            set
            {
                if (_document.Project.AddressLine1 != value)
                {
                    _document.Project.AddressLine1 = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets Address Line 2.
        /// </summary>
        public string? AddressLine2
        {
            get => _document.Project.AddressLine2;
            set
            {
                if (_document.Project.AddressLine2 != value)
                {
                    _document.Project.AddressLine2 = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets Address Line 3.
        /// </summary>
        public string? AddressLine3
        {
            get => _document.Project.AddressLine3;
            set
            {
                if (_document.Project.AddressLine3 != value)
                {
                    _document.Project.AddressLine3 = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        public string? City
        {
            get => _document.Project.City;
            set
            {
                if (_document.Project.City != value)
                {
                    _document.Project.City = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the State (2-letter abbreviation).
        /// </summary>
        public string? State
        {
            get => _document.Project.State;
            set
            {
                if (_document.Project.State != value)
                {
                    _document.Project.State = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Zip Code.
        /// </summary>
        public string? Zip
        {
            get => _document.Project.Zip;
            set
            {
                if (_document.Project.Zip != value)
                {
                    _document.Project.Zip = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Study Date.
        /// </summary>
        public string? StudyDate
        {
            get => _document.Project.StudyDate;
            set
            {
                if (_document.Project.StudyDate != value)
                {
                    _document.Project.StudyDate = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(StudyDateValue)); // Notify DatePicker binding
                }
            }
        }

        /// <summary>
        /// Gets or sets the Study Date as a DateTime for DatePicker binding.
        /// </summary>
        /// <remarks>
        /// This property converts between the string StudyDate (stored in Project)
        /// and DateTime? (required by DatePicker).
        /// Always displays in "MMM dd, yyyy" format regardless of user's regional settings.
        /// When a date is selected and Revision is blank, auto-populates Revision with "MMMM yyyy".
        /// </remarks>
        public DateTime? StudyDateValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StudyDate))
                    return null;
                
                if (DateTime.TryParse(StudyDate, out var date))
                    return date;
                
                return null;
            }
            set
            {
                // Store in consistent "MMM dd, yyyy" format for all users
                StudyDate = value?.ToString("MMM dd, yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                // Auto-populate Revision (Study Month) if blank and date is selected
                if (value.HasValue && string.IsNullOrWhiteSpace(Revision))
                {
                    Revision = value.Value.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    Log.Debug("Auto-populated Revision from Study Date: {Revision}", Revision);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Revision (Revision Month).
        /// </summary>
        public string? Revision
        {
            get => _document.Project.Revision;
            set
            {
                if (_document.Project.Revision != value)
                {
                    _document.Project.Revision = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Comments.
        /// </summary>
        public string? Comments
        {
            get => _document.Project.Comments;
            set
            {
                if (_document.Project.Comments != value)
                {
                    _document.Project.Comments = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Project Type (Standard or Composite pipeline).
        /// </summary>
        /// <remarks>
        /// CRITICAL: Changing this value requires purging all dataset entries to prevent invalid data.
        /// User will be prompted with a confirmation dialog if datasets contain data.
        /// Summary information is preserved.
        /// </remarks>
        public ProjectType ProjectType
        {
            get => _document.Project.ProjectType;
            set
            {
                if (_document.Project.ProjectType != value)
                {
                    // Check if datasets have data
                    if (HasDatasetEntries())
                    {
                        // Show confirmation dialog
                        var confirmed = _dialogService.Confirm(
                            "Changing the project type will delete all imported data to prevent invalid reports.\n\n" +
                            "Summary information (metadata, file paths) will be preserved.\n\n" +
                            "All equipment entries and calculation results will be lost.\n\n" +
                            "Are you sure you want to continue?",
                            "Confirm Project Type Change");

                        if (!confirmed)
                        {
                            // User cancelled - revert the UI
                            RaisePropertyChanged();
                            Log.Information("User cancelled project type change");
                            return;
                        }

                        // User confirmed - purge datasets
                        PurgeDatasets();
                        Log.Warning("Project type changed from {Old} to {New} - datasets purged",
                            _document.Project.ProjectType, value);
                    }

                    _document.Project.ProjectType = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsStandardProjectType));
                    RaisePropertyChanged(nameof(IsCompositeProjectType));
                    RefreshStatistics(); // Update counts to show 0

                    Log.Information("Project type set to: {ProjectType}", value);
                }
            }
        }

        /// <summary>
        /// Gets whether the Standard project type is selected.
        /// </summary>
        public bool IsStandardProjectType
        {
            get => ProjectType == ProjectType.Standard;
            set
            {
                if (value)
                    ProjectType = ProjectType.Standard;
            }
        }

        /// <summary>
        /// Gets whether the Composite project type is selected.
        /// </summary>
        public bool IsCompositeProjectType
        {
            get => ProjectType == ProjectType.Composite;
            set
            {
                if (value)
                    ProjectType = ProjectType.Composite;
            }
        }

        #endregion

        #region File Paths (Report Section)

        /// <summary>
        /// Gets or sets the Map file path.
        /// </summary>
        public string? MapPath
        {
            get => _document.Project.MapPathHistory?.FirstOrDefault();
            set
            {
                var current = _document.Project.MapPathHistory?.FirstOrDefault();
                if (current != value && !string.IsNullOrWhiteSpace(value))
                {
                    if (_document.Project.MapPathHistory == null)
                        _document.Project.MapPathHistory = new System.Collections.Generic.List<string>();
                    
                    _document.Project.MapPathHistory.Remove(value); // Remove if exists
                    _document.Project.MapPathHistory.Insert(0, value); // Add at front
                    
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Spec file path.
        /// </summary>
        public string? SpecPath
        {
            get => _document.Project.SpecPathHistory?.FirstOrDefault();
            set
            {
                var current = _document.Project.SpecPathHistory?.FirstOrDefault();
                if (current != value && !string.IsNullOrWhiteSpace(value))
                {
                    if (_document.Project.SpecPathHistory == null)
                        _document.Project.SpecPathHistory = new System.Collections.Generic.List<string>();
                    
                    _document.Project.SpecPathHistory.Remove(value);
                    _document.Project.SpecPathHistory.Insert(0, value);
                    
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Template file path.
        /// </summary>
        public string? TemplatePath
        {
            get => _document.Project.TemplatePath;
            set
            {
                if (_document.Project.TemplatePath != value)
                {
                    _document.Project.TemplatePath = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Output folder path.
        /// </summary>
        public string? OutputPath { get; set; } // Not persisted in Project, just UI state

        #endregion

        #region Statistics

        /// <summary>
        /// Gets the table rows for data statistics (unified New/Old comparison).
        /// </summary>
        public ObservableCollection<DataStatisticsRowViewModel> DataStatisticsRows { get; } = new();

        /// <summary>
        /// Gets the flattened list of visible rows (including expanded children).
        /// </summary>
        public ObservableCollection<DataStatisticsRowViewModel> VisibleStatisticsRows { get; } = new();

        /// <summary>
        /// Gets the tree nodes for New data.
        /// </summary>
        public ObservableCollection<DataTypeNodeViewModel> NewDataTreeNodes { get; } = new();

        /// <summary>
        /// Gets the tree nodes for Old data.
        /// </summary>
        public ObservableCollection<DataTypeNodeViewModel> OldDataTreeNodes { get; } = new();

        /// <summary>
        /// Gets the count of buses in New data.
        /// </summary>
        public int NewBusCount => _document.Project.NewData?.BusEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of breakers in New data.
        /// </summary>
        public int NewBreakerCount => _document.Project.NewData?.LVBreakerEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of fuses in New data.
        /// </summary>
        public int NewFuseCount => _document.Project.NewData?.FuseEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of short circuit entries in New data.
        /// </summary>
        public int NewShortCircuitCount => _document.Project.NewData?.ShortCircuitEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of arc flash entries in New data.
        /// </summary>
        public int NewArcFlashCount => _document.Project.NewData?.ArcFlashEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of buses in Old data.
        /// </summary>
        public int OldBusCount => _document.Project.OldData?.BusEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of breakers in Old data.
        /// </summary>
        public int OldBreakerCount => _document.Project.OldData?.LVBreakerEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of fuses in Old data.
        /// </summary>
        public int OldFuseCount => _document.Project.OldData?.FuseEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of short circuit entries in Old data.
        /// </summary>
        public int OldShortCircuitCount => _document.Project.OldData?.ShortCircuitEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of arc flash entries in Old data.
        /// </summary>
        public int OldArcFlashCount => _document.Project.OldData?.ArcFlashEntries?.Count ?? 0;

        #endregion

        #region Commands

        public ICommand AddFileCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand BrowseMapCommand { get; }
        public ICommand BrowseSpecCommand { get; }
        public ICommand BrowseTemplateCommand { get; }
        public ICommand BrowseOutputCommand { get; }
        public ICommand ImportNewDataCommand { get; }
        public ICommand ImportOldDataCommand { get; }

        private void ExecuteAddFile()
        {
            // TODO Task 21: Implement file import (CSV/Excel to DataSet)
            Log.Information("AddFile command - To be implemented in Task 21");
        }

        private bool CanExecuteRemoveFile()
        {
            // TODO Task 21: Check if files are selected
            return false;
        }

        private void ExecuteRemoveFile()
        {
            // TODO Task 21: Remove selected files
            Log.Information("RemoveFile command - To be implemented in Task 21");
        }

        private void ExecuteBrowseMap()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Map File",
                Filter = "Map Files (*.ezmap)|*.ezmap|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                MapPath = dialog.FileName;
                Log.Information("Map file selected: {Path}", dialog.FileName);
            }
        }

        private void ExecuteBrowseSpec()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Spec File",
                Filter = "Spec Files (*.ezspec)|*.ezspec|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                SpecPath = dialog.FileName;
                Log.Information("Spec file selected: {Path}", dialog.FileName);
            }
        }

        private void ExecuteBrowseTemplate()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Template File",
                Filter = "Word Documents (*.docx;*.dotx)|*.docx;*.dotx|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                TemplatePath = dialog.FileName;
                Log.Information("Template file selected: {Path}", dialog.FileName);
            }
        }

        private void ExecuteBrowseOutput()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Output Folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputPath = dialog.SelectedPath;
                RaisePropertyChanged(nameof(OutputPath));
                Log.Information("Output folder selected: {Path}", dialog.SelectedPath);
            }
        }

        private void ExecuteImportNewData()
        {
            ExecuteImport(isNewData: true);
        }

        private void ExecuteImportOldData()
        {
            ExecuteImport(isNewData: false);
        }

        private void ExecuteImport(bool isNewData)
        {
            try
            {
                // Step 1: Validate mapping file exists
                if (string.IsNullOrWhiteSpace(MapPath) || !System.IO.File.Exists(MapPath))
                {
                    _dialogService.ShowError(
                        "Please select a mapping file first using the 'Map' field in the Report section below.",
                        "No Mapping File");
                    return;
                }

                // Step 2: Select data file(s) to import (MULTI-SELECT enabled)
                var dialog = new OpenFileDialog
                {
                    Title = $"Select Data File(s) to Import ({(isNewData ? "New" : "Old")} Data)",
                    Filter = "Data Files (*.csv;*.xlsx;*.xls)|*.csv;*.xlsx;*.xls|CSV Files (*.csv)|*.csv|Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = true // Enable multi-select
                };

                if (dialog.ShowDialog() != true)
                    return; // User cancelled

                var fileNames = dialog.FileNames; // Get all selected files
                if (fileNames == null || fileNames.Length == 0)
                    return;

                Log.Information("Importing {Count} file(s) into {Target} data", fileNames.Length, isNewData ? "New" : "Old");

                // Step 3: Load mapping config once (reuse for all files)
                var mappingConfig = EasyAF.Import.MappingConfig.Load(MapPath);
                var validation = mappingConfig.Validate();
                
                if (validation.HasErrors)
                {
                    var validationErrors = string.Join("\n", validation.Errors);
                    _dialogService.ShowError($"Mapping configuration has errors:\n\n{validationErrors}", "Invalid Mapping");
                    return;
                }

                if (validation.Warnings.Any())
                {
                    var warnings = string.Join("\n", validation.Warnings);
                    Log.Warning("Mapping validation warnings: {Warnings}", warnings);
                }

                // Step 4: Ensure target dataset exists
                var targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                if (targetDataSet == null)
                {
                    targetDataSet = new DataSet();
                    if (isNewData)
                        _document.Project.NewData = targetDataSet;
                    else
                        _document.Project.OldData = targetDataSet;
                }

                // Step 5: Import each file
                var importManager = new EasyAF.Import.ImportManager();
                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var filePath in fileNames)
                {
                    try
                    {
                        Log.Information("Importing file: {File}", System.IO.Path.GetFileName(filePath));
                        importManager.Import(filePath, mappingConfig, targetDataSet);
                        successCount++;
                    }
                    catch (Exception fileEx)
                    {
                        var fileName = System.IO.Path.GetFileName(filePath);
                        errors.Add($"{fileName}: {fileEx.Message}");
                        Log.Error(fileEx, "Error importing file: {File}", filePath);
                    }
                }

                // Step 6: Report results
                _document.MarkDirty();
                RefreshStatistics();

                // Log scenario discovery for verification
                LogScenarioDiscovery(targetDataSet, isNewData);

                if (successCount == fileNames.Length)
                {
                    // All files imported successfully
                    _dialogService.ShowMessage(
                        $"Import completed successfully!\n\n{successCount} file(s) imported into {(isNewData ? "New" : "Old")} Data.",
                        "Import Successful");
                }
                else if (successCount > 0)
                {
                    // Some files failed
                    var errorSummary = string.Join("\n", errors);
                    _dialogService.ShowWarning(
                        $"{successCount} of {fileNames.Length} file(s) imported successfully.\n\nErrors:\n{errorSummary}",
                        "Partial Import");
                }
                else
                {
                    // All files failed
                    var errorSummary = string.Join("\n", errors);
                    _dialogService.ShowError($"All imports failed:\n\n{errorSummary}", "Import Failed");
                }

                Log.Information("Import completed: {Success} of {Total} files imported successfully", successCount, fileNames.Length);
            }
            catch (System.IO.IOException ioEx)
            {
                Log.Error(ioEx, "I/O error during import");
                _dialogService.ShowError(
                    $"File error: {ioEx.Message}\n\nMake sure the file is not open in another program.",
                    "Import Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during import");
                _dialogService.ShowError($"Import failed: {ex.Message}", "Import Failed");
            }
        }

        /// <summary>
        /// Executes import operation triggered by drag-and-drop.
        /// </summary>
        /// <param name="filePaths">Array of file paths dropped on the zone.</param>
        /// <param name="isNewData">True to import into New Data, false for Old Data.</param>
        /// <remarks>
        /// This method is called by the ImportDropBehavior when files are dropped.
        /// It reuses the same import logic as the button-triggered import but skips
        /// the file picker dialog since files are already provided.
        /// </remarks>
        public void ExecuteDropImport(string[] filePaths, bool isNewData)
        {
            try
            {
                if (filePaths == null || filePaths.Length == 0)
                {
                    Log.Warning("ExecuteDropImport called with no files");
                    return;
                }

                // Step 1: Validate mapping file exists
                if (string.IsNullOrWhiteSpace(MapPath) || !System.IO.File.Exists(MapPath))
                {
                    _dialogService.ShowError(
                        "Please select a mapping file first using the 'Map' field in the Report section below.",
                        "No Mapping File");
                    return;
                }

                Log.Information("Drop import: {Count} file(s) into {Target} data", filePaths.Length, isNewData ? "New" : "Old");

                // Step 2: Load mapping config once (reuse for all files)
                var mappingConfig = EasyAF.Import.MappingConfig.Load(MapPath);
                var validation = mappingConfig.Validate();
                
                if (validation.HasErrors)
                {
                    var validationErrors = string.Join("\n", validation.Errors);
                    _dialogService.ShowError($"Mapping configuration has errors:\n\n{validationErrors}", "Invalid Mapping");
                    return;
                }

                if (validation.Warnings.Any())
                {
                    var warnings = string.Join("\n", validation.Warnings);
                    Log.Warning("Mapping validation warnings: {Warnings}", warnings);
                }

                // Step 3: Ensure target dataset exists
                var targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                if (targetDataSet == null)
                {
                    targetDataSet = new DataSet();
                    if (isNewData)
                        _document.Project.NewData = targetDataSet;
                    else
                        _document.Project.OldData = targetDataSet;
                }

                // Step 4: Import each file
                var importManager = new EasyAF.Import.ImportManager();
                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var filePath in filePaths)
                {
                    try
                    {
                        Log.Information("Importing dropped file: {File}", System.IO.Path.GetFileName(filePath));
                        importManager.Import(filePath, mappingConfig, targetDataSet);
                        successCount++;
                    }
                    catch (Exception fileEx)
                    {
                        var fileName = System.IO.Path.GetFileName(filePath);
                        errors.Add($"{fileName}: {fileEx.Message}");
                        Log.Error(fileEx, "Error importing dropped file: {File}", filePath);
                    }
                }

                // Step 5: Report results
                _document.MarkDirty();
                RefreshStatistics();

                // Log scenario discovery for verification
                LogScenarioDiscovery(targetDataSet, isNewData);

                if (successCount == filePaths.Length)
                {
                    // All files imported successfully
                    _dialogService.ShowMessage(
                        $"Import completed successfully!\n\n{successCount} file(s) imported into {(isNewData ? "New" : "Old")} Data.",
                        "Import Successful");
                }
                else if (successCount > 0)
                {
                    // Some files failed
                    var errorSummary = string.Join("\n", errors);
                    _dialogService.ShowWarning(
                        $"{successCount} of {filePaths.Length} file(s) imported successfully.\n\nErrors:\n{errorSummary}",
                        "Partial Import");
                }
                else
                {
                    // All files failed
                    var errorSummary = string.Join("\n", errors);
                    _dialogService.ShowError($"All imports failed:\n\n{errorSummary}", "Import Failed");
                }

                Log.Information("Drop import completed: {Success} of {Total} files imported successfully", successCount, filePaths.Length);
            }
            catch (System.IO.IOException ioEx)
            {
                Log.Error(ioEx, "I/O error during drop import");
                _dialogService.ShowError(
                    $"File error: {ioEx.Message}\n\nMake sure the file is not open in another program.",
                    "Import Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during drop import");
                _dialogService.ShowError($"Import failed: {ex.Message}", "Import Failed");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Refreshes statistics display.
        /// </summary>
        /// <remarks>
        /// This should be called manually after data import operations.
        /// Since Project class doesn't implement INotifyPropertyChanged,
        /// we can't automatically detect changes.
        /// </remarks>
        public void RefreshStatistics()
        {
            // Capture old counts before refreshing (for change detection)
            var oldCounts = new System.Collections.Generic.Dictionary<string, (int newCount, int oldCount)>();
            foreach (var row in DataStatisticsRows)
            {
                var key = row.ScenarioName != null 
                    ? $"{row.DataTypeName}:{row.ScenarioName}" 
                    : row.DataTypeName;
                oldCounts[key] = (row.NewCount, row.OldCount);
            }

            // Debug logging to diagnose load issues
            Log.Debug("RefreshStatistics called. NewData null: {NewDataNull}, OldData null: {OldDataNull}", 
                _document.Project.NewData == null, 
                _document.Project.OldData == null);
            
            if (_document.Project.NewData != null)
            {
                Log.Debug("NewData stats - Arc Flash: {AF}, Short Circuit: {SC}, Buses: {Bus}, Breakers: {BR}, Fuses: {F}",
                    _document.Project.NewData.ArcFlashEntries?.Count ?? 0,
                    _document.Project.NewData.ShortCircuitEntries?.Count ?? 0,
                    _document.Project.NewData.BusEntries?.Count ?? 0,
                    _document.Project.NewData.LVBreakerEntries?.Count ?? 0,
                    _document.Project.NewData.FuseEntries?.Count ?? 0);
            }

            // Rebuild table rows (new unified view)
            DataStatisticsRows.Clear();
            foreach (var row in BuildStatisticsRows(_document.Project.NewData, _document.Project.OldData))
            {
                DataStatisticsRows.Add(row);
                // Subscribe to IsExpanded changes
                row.PropertyChanged += StatisticsRow_PropertyChanged;

                // Detect changes and trigger highlights
                var key = row.ScenarioName != null 
                    ? $"{row.DataTypeName}:{row.ScenarioName}" 
                    : row.DataTypeName;

                if (oldCounts.TryGetValue(key, out var oldValues))
                {
                    // Check if New count changed
                    if (row.NewCount != oldValues.newCount && row.NewCount > 0)
                    {
                        row.IsNewCountHighlighted = true;
                        Log.Debug("Highlighting New count change for {Key}: {Old} ? {New}", 
                            key, oldValues.newCount, row.NewCount);
                    }

                    // Check if Old count changed
                    if (row.OldCount != oldValues.oldCount && row.OldCount > 0)
                    {
                        row.IsOldCountHighlighted = true;
                        Log.Debug("Highlighting Old count change for {Key}: {Old} ? {New}", 
                            key, oldValues.oldCount, row.OldCount);
                    }
                }
                else if (row.NewCount > 0 || row.OldCount > 0)
                {
                    // New row appeared - highlight whichever has data
                    if (row.NewCount > 0) row.IsNewCountHighlighted = true;
                    if (row.OldCount > 0) row.IsOldCountHighlighted = true;
                    Log.Debug("Highlighting new row {Key}: New={New}, Old={Old}", 
                        key, row.NewCount, row.OldCount);
                }
            }

            // Build flattened visible list
            RebuildVisibleRows();

            // Rebuild tree nodes (legacy view - kept for compatibility)
            NewDataTreeNodes.Clear();
            OldDataTreeNodes.Clear();

            foreach (var node in BuildTreeNodes(_document.Project.NewData))
                NewDataTreeNodes.Add(node);

            foreach (var node in BuildTreeNodes(_document.Project.OldData))
                OldDataTreeNodes.Add(node);

            // Legacy flat properties (kept for backward compatibility)
            RaisePropertyChanged(nameof(NewBusCount));
            RaisePropertyChanged(nameof(NewBreakerCount));
            RaisePropertyChanged(nameof(NewFuseCount));
            RaisePropertyChanged(nameof(NewShortCircuitCount));
            RaisePropertyChanged(nameof(NewArcFlashCount));
            RaisePropertyChanged(nameof(OldBusCount));
            RaisePropertyChanged(nameof(OldBreakerCount));
            RaisePropertyChanged(nameof(OldFuseCount));
            RaisePropertyChanged(nameof(OldShortCircuitCount));
            RaisePropertyChanged(nameof(OldArcFlashCount));
        }

        private void StatisticsRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataStatisticsRowViewModel.IsExpanded))
            {
                RebuildVisibleRows();
            }
        }

        private void RebuildVisibleRows()
        {
            VisibleStatisticsRows.Clear();
            foreach (var row in DataStatisticsRows)
            {
                VisibleStatisticsRows.Add(row);
                if (row.IsExpanded)
                {
                    foreach (var child in row.Children)
                    {
                        VisibleStatisticsRows.Add(child);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if either NewData or OldData contains any entries.
        /// </summary>
        /// <returns>True if any data exists, false if both datasets are empty.</returns>
        private bool HasDatasetEntries()
        {
            return HasDatasetEntriesInternal(_document.Project.NewData) ||
                   HasDatasetEntriesInternal(_document.Project.OldData);
        }

        /// <summary>
        /// Checks if a specific DataSet has any entries.
        /// </summary>
        private bool HasDatasetEntriesInternal(DataSet? dataset)
        {
            if (dataset == null) return false;

            // Check all dictionary properties for entries
            return (dataset.BusEntries?.Count ?? 0) > 0 ||
                   (dataset.LVBreakerEntries?.Count ?? 0) > 0 ||
                   (dataset.FuseEntries?.Count ?? 0) > 0 ||
                   (dataset.CableEntries?.Count ?? 0) > 0 ||
                   (dataset.ArcFlashEntries?.Count ?? 0) > 0 ||
                   (dataset.ShortCircuitEntries?.Count ?? 0) > 0 ||
                   (dataset.AFDEntries?.Count ?? 0) > 0 ||
                   (dataset.ATSEntries?.Count ?? 0) > 0 ||
                   (dataset.BatteryEntries?.Count ?? 0) > 0 ||
                   (dataset.BuswayEntries?.Count ?? 0) > 0 ||
                   (dataset.CapacitorEntries?.Count ?? 0) > 0 ||
                   (dataset.CLReactorEntries?.Count ?? 0) > 0 ||
                   (dataset.CTEntries?.Count ?? 0) > 0 ||
                   (dataset.FilterEntries?.Count ?? 0) > 0 ||
                   (dataset.GeneratorEntries?.Count ?? 0) > 0 ||
                   (dataset.HVBreakerEntries?.Count ?? 0) > 0 ||
                   (dataset.InverterEntries?.Count ?? 0) > 0 ||
                   (dataset.LoadEntries?.Count ?? 0) > 0 ||
                   (dataset.MCCEntries?.Count ?? 0) > 0 ||
                   (dataset.MeterEntries?.Count ?? 0) > 0 ||
                   (dataset.MotorEntries?.Count ?? 0) > 0 ||
                   (dataset.PanelEntries?.Count ?? 0) > 0 ||
                   (dataset.PhotovoltaicEntries?.Count ?? 0) > 0 ||
                   (dataset.POCEntries?.Count ?? 0) > 0 ||
                   (dataset.RectifierEntries?.Count ?? 0) > 0 ||
                   (dataset.RelayEntries?.Count ?? 0) > 0 ||
                   (dataset.ShuntEntries?.Count ?? 0) > 0 ||
                   (dataset.SwitchEntries?.Count ?? 0) > 0 ||
                   (dataset.Transformer2WEntries?.Count ?? 0) > 0 ||
                   (dataset.Transformer3WEntries?.Count ?? 0) > 0 ||
                   (dataset.TransmissionLineEntries?.Count ?? 0) > 0 ||
                   (dataset.UPSEntries?.Count ?? 0) > 0 ||
                   (dataset.UtilityEntries?.Count ?? 0) > 0 ||
                   (dataset.ZigzagTransformerEntries?.Count ?? 0) > 0;
        }

        /// <summary>
        /// Purges all entries from NewData and OldData datasets.
        /// Summary information is preserved.
        /// </summary>
        private void PurgeDatasets()
        {
            // Create fresh empty datasets
            _document.Project.NewData = new DataSet();
            _document.Project.OldData = new DataSet();

            Log.Information("Datasets purged due to project type change");
        }

        /// <summary>
        /// Logs scenario discovery results for verification and debugging.
        /// </summary>
        private void LogScenarioDiscovery(DataSet dataSet, bool isNewData)
        {
            var target = isNewData ? "New" : "Old";
            
            // Get available scenarios
            var scenarios = dataSet.GetAvailableScenarios();
            
            if (scenarios.Count > 0)
            {
                Log.Information("{Target} Data: Discovered {Count} scenario(s): {Scenarios}", 
                    target, scenarios.Count, string.Join(", ", scenarios));

                // Get detailed statistics per scenario
                var stats = dataSet.GetStatisticsByScenario();
                
                foreach (var dataType in stats.Keys.OrderBy(k => k))
                {
                    var scenarioStats = stats[dataType];
                    
                    if (scenarioStats.ContainsKey("(All)"))
                    {
                        // Non-scenario type
                        Log.Debug("{Target} Data: {DataType} = {Count} entries (no scenarios)",
                            target, dataType, scenarioStats["(All)"]);
                    }
                    else
                    {
                        // Scenario-based type
                        var isUniform = dataSet.IsScenariosUniform(dataType);
                        var uniformIndicator = isUniform ? "?" : "?";
                        
                        var scenarioSummary = string.Join(", ", 
                            scenarioStats.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                        
                        Log.Information("{Target} Data: {DataType} {Indicator} = {Summary}",
                            target, dataType, uniformIndicator, scenarioSummary);
                    }
                }
            }
            else
            {
                Log.Debug("{Target} Data: No scenarios discovered (non-scenario data types only)", target);
            }
        }

        #endregion

        #region Tree Building

        /// <summary>
        /// Builds tree nodes from DataSet statistics.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <returns>Observable collection of root-level tree nodes.</returns>
        private ObservableCollection<DataTypeNodeViewModel> BuildTreeNodes(DataSet? dataSet)
        {
            var nodes = new ObservableCollection<DataTypeNodeViewModel>();

            if (dataSet == null)
                return nodes;

            var stats = dataSet.GetStatisticsByScenario();

            // Sort data types: scenario-based types first, then simple types
            var scenarioTypes = stats.Where(kvp => !kvp.Value.ContainsKey("(All)")).OrderBy(kvp => kvp.Key);
            var simpleTypes = stats.Where(kvp => kvp.Value.ContainsKey("(All)")).OrderBy(kvp => kvp.Key);

            // Add scenario-based types (Arc Flash, Short Circuit)
            foreach (var kvp in scenarioTypes)
            {
                var dataTypeName = kvp.Key;
                var scenarioCounts = kvp.Value;
                var totalCount = scenarioCounts.Values.Sum();

                var node = new DataTypeNodeViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = $"{GetFriendlyName(dataTypeName)} ({totalCount})",
                    Count = totalCount,
                    IsScenariosUniform = dataSet.IsScenariosUniform(dataTypeName)
                };

                // Add child nodes for each scenario
                foreach (var scenario in scenarioCounts.OrderBy(s => s.Key))
                {
                    node.Children.Add(new DataTypeNodeViewModel
                    {
                        DataTypeName = dataTypeName,
                        ScenarioName = scenario.Key,
                        DisplayName = $"{scenario.Key} ({scenario.Value})",
                        Count = scenario.Value
                    });
                }

                nodes.Add(node);
            }

            // Add simple types (Bus, LVCB, Fuse, etc.)
            foreach (var kvp in simpleTypes)
            {
                var dataTypeName = kvp.Key;
                var count = kvp.Value["(All)"];

                nodes.Add(new DataTypeNodeViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = $"{GetFriendlyName(dataTypeName)} ({count})",
                    Count = count
                });
            }

            return nodes;
        }

        /// <summary>
        /// Converts internal data type names to user-friendly display names.
        /// </summary>
        private string GetFriendlyName(string dataTypeName)
        {
            return dataTypeName switch
            {
                "ArcFlash" => "Arc Flash",
                "ShortCircuit" => "Short Circuit",
                "LVCB" => "Breakers",
                "HVBreaker" => "HV Breakers",
                "Transformer2W" => "2-Winding Transformers",
                "Transformer3W" => "3-Winding Transformers",
                "TransmissionLine" => "Transmission Lines",
                "CLReactor" => "Current Limiting Reactors",
                "ZigzagTransformer" => "Zigzag Transformers",
                _ => dataTypeName // Use as-is for others (Bus, Fuse, Cable, etc.)
            };
        }

        /// <summary>
        /// Builds statistics table rows comparing New and Old data.
        /// </summary>
        /// <param name="newData">New DataSet.</param>
        /// <param name="oldData">Old DataSet.</param>
        /// <returns>Observable collection of table rows with hierarchical scenario support.</returns>
        private ObservableCollection<DataStatisticsRowViewModel> BuildStatisticsRows(DataSet? newData, DataSet? oldData)
        {
            var rows = new ObservableCollection<DataStatisticsRowViewModel>();

            // Get statistics from both datasets
            var newStats = newData?.GetStatisticsByScenario() ?? new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>();
            var oldStats = oldData?.GetStatisticsByScenario() ?? new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>();

            // Collect all unique data type names from both datasets
            var allDataTypes = newStats.Keys.Union(oldStats.Keys).OrderBy(k => k);

            // Separate scenario-based types from simple types
            var scenarioTypes = allDataTypes.Where(dt => 
                (newStats.ContainsKey(dt) && !newStats[dt].ContainsKey("(All)")) ||
                (oldStats.ContainsKey(dt) && !oldStats[dt].ContainsKey("(All)")))
                .OrderBy(dt => dt);

            var simpleTypes = allDataTypes.Where(dt =>
                (newStats.ContainsKey(dt) && newStats[dt].ContainsKey("(All)")) ||
                (oldStats.ContainsKey(dt) && oldStats[dt].ContainsKey("(All)")))
                .OrderBy(dt => dt);

            // Add scenario-based types first (Arc Flash, Short Circuit)
            foreach (var dataTypeName in scenarioTypes)
            {
                var newScenarios = newStats.ContainsKey(dataTypeName) ? newStats[dataTypeName] : new System.Collections.Generic.Dictionary<string, int>();
                var oldScenarios = oldStats.ContainsKey(dataTypeName) ? oldStats[dataTypeName] : new System.Collections.Generic.Dictionary<string, int>();

                var newTotal = newScenarios.Values.Sum();
                var oldTotal = oldScenarios.Values.Sum();

                var row = new DataStatisticsRowViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = GetFriendlyName(dataTypeName),
                    NewCount = newTotal,
                    OldCount = oldTotal,
                    IsScenariosUniform = newData?.IsScenariosUniform(dataTypeName) ?? true
                };

                // Add child rows for each scenario
                var allScenarios = newScenarios.Keys.Union(oldScenarios.Keys).OrderBy(s => s);
                var scenarioList = allScenarios.ToList();
                
                for (int i = 0; i < scenarioList.Count; i++)
                {
                    var scenario = scenarioList[i];
                    var newCount = newScenarios.ContainsKey(scenario) ? newScenarios[scenario] : 0;
                    var oldCount = oldScenarios.ContainsKey(scenario) ? oldScenarios[scenario] : 0;

                    row.Children.Add(new DataStatisticsRowViewModel
                    {
                        DataTypeName = dataTypeName,
                        ScenarioName = scenario,
                        DisplayName = scenario,
                        NewCount = newCount,
                        OldCount = oldCount,
                        IsFirstChild = i == 0,
                        IsLastChild = i == scenarioList.Count - 1
                    });
                }

                rows.Add(row);
            }

            // Add simple types (Bus, Breakers, Fuses, etc.)
            foreach (var dataTypeName in simpleTypes)
            {
                var newCount = newStats.ContainsKey(dataTypeName) && newStats[dataTypeName].ContainsKey("(All)") 
                    ? newStats[dataTypeName]["(All)"] 
                    : 0;
                var oldCount = oldStats.ContainsKey(dataTypeName) && oldStats[dataTypeName].ContainsKey("(All)") 
                    ? oldStats[dataTypeName]["(All)"] 
                    : 0;

                rows.Add(new DataStatisticsRowViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = GetFriendlyName(dataTypeName),
                    NewCount = newCount,
                    OldCount = oldCount
                });
            }

            return rows;
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
}
