using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Modules.Project.Models;
using EasyAF.Modules.Project.Views;
using EasyAF.Modules.Project.Helpers;
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
    public partial class ProjectSummaryViewModel : BindableBase, IDisposable
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
            BrowseSpecCommand = new DelegateCommand(ExecuteBrowseSpec);
            BrowseTemplateCommand = new DelegateCommand(ExecuteBrowseTemplate);
            BrowseOutputCommand = new DelegateCommand(ExecuteBrowseOutput);
            ImportNewDataCommand = new DelegateCommand(ExecuteImportNewData);
            ImportOldDataCommand = new DelegateCommand(ExecuteImportOldData);
            ClearNewDataCommand = new DelegateCommand(ExecuteClearNewData, CanExecuteClearNewData);
            ClearOldDataCommand = new DelegateCommand(ExecuteClearOldData, CanExecuteClearOldData);

            // Build initial tree nodes (will be empty if no data loaded)
            RefreshStatistics();
            
            // Load available mappings from history
            RefreshAvailableMappings();

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
                    
                    // Refresh available mappings when map path changes
                    RefreshAvailableMappings();
                }
            }
        }

        /// <summary>
        /// Gets the collection of available mapping files.
        /// </summary>
        public ObservableCollection<MappingFileItem> AvailableMappings { get; } = new();

        private MappingFileItem? _selectedMapping;
        
        /// <summary>
        /// Gets or sets the selected mapping file.
        /// </summary>
        public MappingFileItem? SelectedMapping
        {
            get => _selectedMapping;
            set
            {
                // Check if user selected Browse...
                if (value != null && value.IsBrowseItem)
                {
                    // Trigger browse dialog
                    ExecuteBrowseCustomMapping();
                    
                    // Don't change selection to Browse item - keep previous selection
                    RaisePropertyChanged();  // Force UI update without changing value
                    return;
                }
                
                if (SetProperty(ref _selectedMapping, value))
                {
                    // Update MapPath when selection changes
                    if (value != null && !string.IsNullOrEmpty(value.FilePath))
                    {
                        // If it's a custom file item, update CustomMapPath
                        if (value.IsCustomFileItem)
                        {
                            _document.Project.CustomMapPath = value.FilePath;
                            _document.MarkDirty();
                        }
                        
                        MapPath = value.FilePath;
                    }
                    
                    RaisePropertyChanged(nameof(MappingPathDisplay));
                    RaisePropertyChanged(nameof(MappingValidationIcon));
                    RaisePropertyChanged(nameof(MappingValidationBrush));
                    RaisePropertyChanged(nameof(MappingValidationTooltip));
                }
            }
        }

        /// <summary>
        /// Gets the mapping file path for display.
        /// </summary>
        public string MappingPathDisplay
        {
            get
            {
                if (SelectedMapping == null || string.IsNullOrEmpty(SelectedMapping.FilePath))
                    return "No mapping selected";
                
                return SelectedMapping.FilePath;
            }
        }

        /// <summary>
        /// Gets the validation icon for the selected mapping.
        /// </summary>
        public string MappingValidationIcon
        {
            get
            {
                if (SelectedMapping == null || string.IsNullOrEmpty(SelectedMapping.FilePath))
                    return "";
                
                return SelectedMapping.IsValid ? "\uE73E" : "\uE711"; // Checkmark : X
            }
        }

        /// <summary>
        /// Gets the validation brush color for the selected mapping.
        /// </summary>
        public Brush MappingValidationBrush
        {
            get
            {
                if (SelectedMapping == null || string.IsNullOrEmpty(SelectedMapping.FilePath))
                    return Brushes.Gray;
                
                return SelectedMapping.IsValid ? Brushes.Green : Brushes.Red;
            }
        }

        /// <summary>
        /// Gets the validation tooltip for the selected mapping.
        /// </summary>
        public string MappingValidationTooltip
        {
            get
            {
                if (SelectedMapping == null || string.IsNullOrEmpty(SelectedMapping.FilePath))
                    return "No mapping file selected";
                
                return SelectedMapping.IsValid ? "Mapping file is valid" : "Mapping file has errors";
            }
        }

        /// <summary>
        /// Gets the action to refresh available mappings (for behavior binding).
        /// </summary>
        public Action? RefreshMappingsAction => RefreshAvailableMappings;

        /// <summary>
        /// Refreshes the available mappings list from the map path history.
        /// </summary>
        private void RefreshAvailableMappings()
        {
            AvailableMappings.Clear();
            
            // Add mapping files from history
            if (_document.Project.MapPathHistory != null && _document.Project.MapPathHistory.Count > 0)
            {
                foreach (var path in _document.Project.MapPathHistory)
                {
                    if (string.IsNullOrWhiteSpace(path))
                        continue;
                    
                    var fileName = System.IO.Path.GetFileName(path);
                    var item = MappingFileItem.CreateCustomFileItem(path, fileName);
                    AvailableMappings.Add(item);
                }
            }
            
            // Add custom file entry if it exists (project-specific)
            if (!string.IsNullOrWhiteSpace(_document.Project.CustomMapPath) &&
                System.IO.File.Exists(_document.Project.CustomMapPath))
            {
                var fileName = System.IO.Path.GetFileName(_document.Project.CustomMapPath);
                var customItem = MappingFileItem.CreateCustomFileItem(_document.Project.CustomMapPath, fileName);
                customItem.IsCustomFileItem = true;  // Mark it specially
                
                // Only add if not already in history
                if (!AvailableMappings.Any(m => string.Equals(m.FilePath, _document.Project.CustomMapPath, StringComparison.OrdinalIgnoreCase)))
                {
                    AvailableMappings.Add(customItem);
                }
            }
            
            // Always add Browse... button at the end
            AvailableMappings.Add(MappingFileItem.CreateBrowseItem());
            
            // Select the first non-browse item (most recent) if available
            // Do this silently without marking dirty (just loading saved state)
            var firstMapping = AvailableMappings.FirstOrDefault(m => !m.IsBrowseItem);
            if (firstMapping != null)
            {
                _selectedMapping = firstMapping; // Set backing field directly to avoid setter
                RaisePropertyChanged(nameof(SelectedMapping));
                RaisePropertyChanged(nameof(MappingPathDisplay));
                RaisePropertyChanged(nameof(MappingValidationIcon));
                RaisePropertyChanged(nameof(MappingValidationBrush));
                RaisePropertyChanged(nameof(MappingValidationTooltip));
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

        private bool _allowCellHighlights = true;
        
        /// <summary>
        /// Gets or sets whether cell highlights are allowed to trigger.
        /// Set to false during UI state changes (expand/collapse) to prevent animation.
        /// Set to true during data import to allow highlights.
        /// </summary>
        public bool AllowCellHighlights
        {
            get => _allowCellHighlights;
            set => SetProperty(ref _allowCellHighlights, value);
        }

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
        public ICommand BrowseSpecCommand { get; }
        public ICommand BrowseTemplateCommand { get; }
        public ICommand BrowseOutputCommand { get; }
        public ICommand ImportNewDataCommand { get; }
        public ICommand ImportOldDataCommand { get; }
        public ICommand ClearNewDataCommand { get; }
        public ICommand ClearOldDataCommand { get; }

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

        private void ExecuteBrowseCustomMapping()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Custom Mapping File",
                Filter = "Map Files (*.ezmap)|*.ezmap|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                // Store as custom map path (project-specific)
                _document.Project.CustomMapPath = dialog.FileName;
                _document.MarkDirty();
                
                // Also update MapPath to use this file
                MapPath = dialog.FileName;
                
                Log.Information("Custom mapping file selected for project: {Path}", dialog.FileName);
                
                // Refresh the dropdown to show the new custom file
                RefreshAvailableMappings();
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

        private bool CanExecuteClearNewData()
        {
            return _document.Project.NewData != null && 
                   HasDatasetEntriesInternal(_document.Project.NewData);
        }

        private void ExecuteClearNewData()
        {
            var confirmed = _dialogService.Confirm(
                "This will permanently delete all imported New Data.\n\n" +
                "All equipment entries and calculation results will be lost.\n\n" +
                "Project metadata (LB Project Number, Site Name, etc.) will be preserved.\n\n" +
                "Are you sure you want to continue?",
                "Confirm Clear New Data");

            if (!confirmed)
            {
                Log.Information("User cancelled clear New Data operation");
                return;
            }

            // Create fresh empty dataset
            _document.Project.NewData = new DataSet();
            _document.MarkDirty();
            RefreshStatistics(triggerHighlights: false);

            Log.Information("New Data cleared by user");
            _dialogService.ShowMessage("New Data has been cleared.", "Data Cleared");

            // Raise CanExecute changed for commands
            (ClearNewDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteClearOldData()
        {
            return _document.Project.OldData != null && 
                   HasDatasetEntriesInternal(_document.Project.OldData);
        }

        private void ExecuteClearOldData()
        {
            var confirmed = _dialogService.Confirm(
                "This will permanently delete all imported Old Data.\n\n" +
                "All equipment entries and calculation results will be lost.\n\n" +
                "Project metadata (LB Project Number, Site Name, etc.) will be preserved.\n\n" +
                "Are you sure you want to continue?",
                "Confirm Clear Old Data");

            if (!confirmed)
            {
                Log.Information("User cancelled clear Old Data operation");
                return;
            }

            // Create fresh empty dataset
            _document.Project.OldData = new DataSet();
            _document.MarkDirty();
            RefreshStatistics(triggerHighlights: false);

            Log.Information("Old Data cleared by user");
            _dialogService.ShowMessage("Old Data has been cleared.", "Data Cleared");

            // Raise CanExecute changed for commands
            (ClearOldDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        private void ExecuteImport(bool isNewData)
        {
            try
            {
                // Step 1: Validate mapping file exists
                if (string.IsNullOrWhiteSpace(MapPath) || !System.IO.File.Exists(MapPath))
                {
                    _dialogService.ShowError(
                        "Please select a mapping file using the 'Import Map' dropdown above.",
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

                // Step 2.5: COMPOSITE MODE - Show scenario selection dialog
                if (ProjectType == ProjectType.Composite)
                {
                    var fileScanResults = CompositeImportHelper.PreScanFilesForScenarios(fileNames, mappingConfig);
                    var existingDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                    var existingScenarios = existingDataSet?.GetAvailableScenarios().ToList() ?? new System.Collections.Generic.List<string>();
                    
                    var compositeDialog = new CompositeImportDialog(fileScanResults, existingScenarios)
                    {
                        Owner = System.Windows.Application.Current.MainWindow
                    };

                    if (compositeDialog.ShowDialog() != true)
                    {
                        Log.Information("User cancelled composite import");
                        return;
                    }

                    var importPlan = compositeDialog.ViewModel.GetImportPlan();
                    ExecuteCompositeImport(importPlan, mappingConfig, isNewData);
                    return;
                }

                // STANDARD MODE - Continue with existing logic
                // Step 2.5: Smart conflict detection - check if files will actually overwrite existing data
                var targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                if (targetDataSet != null && HasDatasetEntriesInternal(targetDataSet))
                {
                    var dataTypeName = isNewData ? "New" : "Old";
                    var (willOverwrite, affectedTypes) = WillImportOverwriteData(fileNames, mappingConfig, targetDataSet);
                    
                    if (willOverwrite)
                    {
                        var fileCount = fileNames.Length;
                        var fileWord = fileCount == 1 ? "file" : "files";
                        var affectedList = string.Join("\n  • ", affectedTypes);
                        
                        var behaviorNote = ProjectType == ProjectType.Standard
                            ? "Existing data for these types will be REPLACED."
                            : "New scenarios will be added to existing data.";
                        
                        var confirmed = _dialogService.Confirm(
                            $"?? WARNING: {dataTypeName} Data Conflict\n\n" +
                            $"Affected data types:\n  • {affectedList}\n\n" +
                            $"{behaviorNote}\n\n" +
                            $"Continue with import?",
                            $"Confirm Import");

                        if (!confirmed)
                        {
                            Log.Information("User cancelled import due to {DataType} data conflict with {Count} type(s)", 
                                dataTypeName, affectedTypes.Count);
                            return;
                        }
                        
                        // STANDARD MODE: Clear affected data types before importing
                        if (ProjectType == ProjectType.Standard)
                        {
                            ClearDataTypes(targetDataSet!, affectedTypes);
                            Log.Warning("Cleared {Count} data type(s) for Standard mode replacement import", affectedTypes.Count);
                        }
                    }
                    else
                    {
                        Log.Information("Smart detection: Import will not affect existing {DataType} data - proceeding without warning", dataTypeName);
                    }
                }

                // Step 4: Ensure target dataset exists
                targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
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
                RefreshStatistics(triggerHighlights: true); // Trigger highlights on import

                // Update command states
                (ClearNewDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (ClearOldDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();

                // Log scenario discovery for verification
                LogScenarioDiscovery(targetDataSet, isNewData);
                
                // Record import in history (AFTER successful import)
                if (successCount > 0)
                {
                    RecordImportInHistory(fileNames, isNewData, mappingConfig);
                }

                if (successCount == fileNames.Length)
                {
                    // All files imported successfully - UI feedback via cell highlighting is enough
                    Log.Information("Import completed successfully: {Count} file(s) imported into {Target} data", 
                        successCount, isNewData ? "New" : "Old");
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
                        "Please select a mapping file using the 'Import Map' dropdown above.",
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


                // Step 2.5: COMPOSITE MODE - Show scenario selection dialog
                if (ProjectType == ProjectType.Composite)
                {
                    var fileScanResults = CompositeImportHelper.PreScanFilesForScenarios(filePaths, mappingConfig);
                    var existingDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                    var existingScenarios = existingDataSet?.GetAvailableScenarios().ToList() ?? new System.Collections.Generic.List<string>();
                    
                    var compositeDialog = new CompositeImportDialog(fileScanResults, existingScenarios)
                    {
                        Owner = System.Windows.Application.Current.MainWindow
                    };

                    if (compositeDialog.ShowDialog() != true)
                    {
                        Log.Information("User cancelled composite drop import");
                        return;
                    }

                    var importPlan = compositeDialog.ViewModel.GetImportPlan();
                    ExecuteCompositeImport(importPlan, mappingConfig, isNewData);
                    return;
                }

                // STANDARD MODE - Continue with existing logic
                // Step 2.5: Smart conflict detection - check if files will actually overwrite existing data
                var targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
                if (targetDataSet != null && HasDatasetEntriesInternal(targetDataSet))
                {
                    var dataTypeName = isNewData ? "New" : "Old";
                    var (willOverwrite, affectedTypes) = WillImportOverwriteData(filePaths, mappingConfig, targetDataSet);
                    
                    if (willOverwrite)
                    {
                        var fileCount = filePaths.Length;
                        var fileWord = fileCount == 1 ? "file" : "files";
                        var affectedList = string.Join("\n  • ", affectedTypes);
                        
                        var behaviorNote = ProjectType == ProjectType.Standard
                            ? "Existing data for these types will be REPLACED."
                            : "New scenarios will be added to existing data.";
                        
                        var confirmed = _dialogService.Confirm(
                            $"?? WARNING: {dataTypeName} Data Conflict\n\n" +
                            $"Affected data types:\n  • {affectedList}\n\n" +
                            $"{behaviorNote}\n\n" +
                            $"Continue with import?",
                            $"Confirm Import");

                        if (!confirmed)
                        {
                            Log.Information("User cancelled drop import due to {DataType} data conflict with {Count} type(s)", 
                                dataTypeName, affectedTypes.Count);
                            return;
                        }
                        
                        // STANDARD MODE: Clear affected data types before importing
                        if (ProjectType == ProjectType.Standard)
                        {
                            ClearDataTypes(targetDataSet!, affectedTypes);
                            Log.Warning("Cleared {Count} data type(s) for Standard mode replacement import (drop)", affectedTypes.Count);
                        }
                    }
                    else
                    {
                        Log.Information("Smart detection: Drop import will not affect existing {DataType} data - proceeding without warning", dataTypeName);
                    }
                }

                // Step 3: Ensure target dataset exists
                targetDataSet = isNewData ? _document.Project.NewData : _document.Project.OldData;
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
                RefreshStatistics(triggerHighlights: true); // Trigger highlights on drop import

                // Update command states
                (ClearNewDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (ClearOldDataCommand as DelegateCommand)?.RaiseCanExecuteChanged();

                // Log scenario discovery for verification
                LogScenarioDiscovery(targetDataSet, isNewData);

                // Record import in history (AFTER successful import)
                if (successCount > 0)
                {
                    RecordImportInHistory(filePaths, isNewData, mappingConfig);
                }

                if (successCount == filePaths.Length)
                {
                    // All files imported successfully - UI feedback via cell highlighting is enough
                    Log.Information("Import completed successfully: {Count} file(s) imported into {Target} data", 
                        successCount, isNewData ? "New" : "Old");
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
