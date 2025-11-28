using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Mvvm;
using Prism.Commands;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services;
using EasyAF.Core.Contracts;
using EasyAF.Core.Services; // NEW: For DataTypeSettingsExtensions
using Microsoft.Win32;
using Serilog;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// View model for the map summary tab showing metadata and data type status.
    /// </summary>
    /// <remarks>
    /// The summary tab provides:
    /// - Map metadata (name, version, description)
    /// - Referenced file management
    /// - Data type mapping status overview
    /// </remarks>
    public class MapSummaryViewModel : BindableBase
    {
        private readonly MapDocument _document;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly ISettingsService _settingsService;
        private readonly MapDocumentViewModel _parentViewModel;
        private readonly ColumnExtractionService _columnExtraction;
        private ReferencedFile? _selectedFile;
        private DataTypePropertySummary? _selectedDataType;

        /// <summary>
        /// Initializes a new instance of the MapSummaryViewModel.
        /// </summary>
        public MapSummaryViewModel(
            MapDocument document,
            IPropertyDiscoveryService propertyDiscovery,
            ISettingsService settingsService,
            MapDocumentViewModel parentViewModel)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
            _columnExtraction = new ColumnExtractionService();

            // Initialize collections
            DataTypeProperties = new ObservableCollection<DataTypePropertySummary>();
            AvailableTables = new ObservableCollection<string>();

            // Initialize commands
            AddFileCommand = new DelegateCommand(ExecuteAddFile);
            RemoveFileCommand = new DelegateCommand(ExecuteRemoveFile, CanExecuteRemoveFile);
            BrowseFileCommand = new DelegateCommand(ExecuteBrowseFile);
            RefreshStatusCommand = new DelegateCommand(ExecuteRefreshStatus);

            // Initialize data
            InitializeDataTypeSummaries();

            Log.Debug("MapSummaryViewModel initialized");
        }

        #region Properties - Map Metadata

        /// <summary>
        /// Gets or sets the map name.
        /// </summary>
        public string MapName
        {
            get => _document.MapName;
            set
            {
                _document.MapName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the software version this map is for.
        /// </summary>
        public string SoftwareVersion
        {
            get => _document.SoftwareVersion;
            set
            {
                _document.SoftwareVersion = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the map description.
        /// </summary>
        public string Description
        {
            get => _document.Description;
            set
            {
                _document.Description = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the date the map was last modified.
        /// </summary>
        public DateTime DateModified => _document.DateModified;

        #endregion

        #region Properties - File Management

        /// <summary>
        /// Gets the collection of referenced files.
        /// </summary>
        public ObservableCollection<ReferencedFile> ReferencedFiles { get; private set; } = null!;

        /// <summary>
        /// Gets or sets the currently selected file.
        /// </summary>
        public ReferencedFile? SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (SetProperty(ref _selectedFile, value))
                {
                    (RemoveFileCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Properties - Data Type Status

        /// <summary>
        /// Gets the collection of data type summaries.
        /// </summary>
        public ObservableCollection<DataTypePropertySummary> DataTypeProperties { get; }

        /// <summary>
        /// Gets or sets the selected data type summary.
        /// </summary>
        public DataTypePropertySummary? SelectedDataType
        {
            get => _selectedDataType;
            set => SetProperty(ref _selectedDataType, value);
        }

        /// <summary>
        /// Gets the collection of available tables from loaded files.
        /// </summary>
        public ObservableCollection<string> AvailableTables { get; }

        #endregion

        #region Commands

        /// <summary>
        /// Command to add a new referenced file.
        /// </summary>
        public ICommand AddFileCommand { get; }

        /// <summary>
        /// Command to remove the selected file.
        /// </summary>
        public ICommand RemoveFileCommand { get; }

        /// <summary>
        /// Command to browse for a file.
        /// </summary>
        public ICommand BrowseFileCommand { get; }

        /// <summary>
        /// Command to refresh the mapping status.
        /// </summary>
        public ICommand RefreshStatusCommand { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the data type summaries collection.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-16 Hide Disabled Data Types from Summary
        /// Modified for: Only show statistics for enabled data types (respects user settings)
        /// Related modules: Core (ISettingsService), Map (MapSettingsExtensions)
        /// Rollback instructions: Remove IsDataTypeEnabled check
        /// </remarks>
        private void InitializeDataTypeSummaries()
        {
            DataTypeProperties.Clear();

            var dataTypes = _propertyDiscovery.GetAvailableDataTypes();
            Log.Debug("Initializing summaries for {Count} data types", dataTypes.Count);

            foreach (var dataType in dataTypes)
            {
                // Skip disabled data types (same filter used for tabs)
                // If Cable is disabled in settings, don't show its statistics
                if (!_settingsService.IsDataTypeEnabled(dataType))
                {
                    Log.Debug("Skipping summary for disabled data type: {DataType}", dataType);
                    continue;
                }

                var properties = _propertyDiscovery.GetPropertiesForType(dataType);
                var mappedCount = _document.MappingsByDataType.TryGetValue(dataType, out var mappings)
                    ? mappings.Count
                    : 0;

                var summary = new DataTypePropertySummary
                {
                    TypeName = dataType, // Raw class name (e.g., "Bus", "LVBreaker")
                    TypeDisplayName = _propertyDiscovery.GetDataTypeDescription(dataType), // Friendly name (e.g., "Buses", "LV Breakers")
                    FieldsAvailable = properties.Count,
                    FieldsMapped = mappedCount,
                    StatusColor = GetStatusColor(mappedCount, properties.Count)
                };

                DataTypeProperties.Add(summary);
            }

            // Update referenced files observable collection
            UpdateReferencedFilesCollection();
        }

        /// <summary>
        /// Updates the referenced files collection from the document.
        /// </summary>
        private void UpdateReferencedFilesCollection()
        {
            ReferencedFiles = new ObservableCollection<ReferencedFile>(_document.ReferencedFiles);
            RaisePropertyChanged(nameof(ReferencedFiles));
        }

        /// <summary>
        /// Gets the status color based on mapping completion.
        /// </summary>
        private string GetStatusColor(int mapped, int available)
        {
            if (mapped == 0) return "Red";
            if (mapped >= available) return "Green";
            return "Orange";
        }

        #endregion

        #region Command Implementations

        /// <summary>
        /// Executes the add file command.
        /// </summary>
        private void ExecuteAddFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Data Files (*.csv;*.xlsx;*.xls)|*.csv;*.xlsx;*.xls|All Files (*.*)|*.*",
                Title = "Add Sample File for Mapping",
                Multiselect = true  // Enable multi-select
            };

            if (dialog.ShowDialog() == true)
            {
                // Add all selected files
                foreach (var filePath in dialog.FileNames)
                {
                    AddReferencedFile(filePath);
                }
            }
        }

        /// <summary>
        /// Adds a referenced file to the document.
        /// </summary>
        private void AddReferencedFile(string filePath)
        {
            try
            {
                // Check if file already exists
                if (_document.ReferencedFiles.Any(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Warning("File already referenced: {FilePath}", filePath);
                    return;
                }

                // Extract columns to verify file is readable
                var columns = _columnExtraction.ExtractColumns(filePath);
                
                // Update available tables
                foreach (var tableName in columns.Keys)
                {
                    if (!AvailableTables.Contains(tableName))
                    {
                        AvailableTables.Add(tableName);
                    }
                }

                // Add to document
                var referencedFile = new ReferencedFile
                {
                    FileName = System.IO.Path.GetFileName(filePath),
                    FilePath = filePath,
                    Status = "Available",
                    LastAccessed = DateTime.Now
                };

                _document.ReferencedFiles.Add(referencedFile);
                _document.MarkDirty();

                // Refresh UI
                UpdateReferencedFilesCollection();
                
                // Notify DataType tabs to refresh their table lists
                _parentViewModel.RefreshAllTabStatuses();

                Log.Information("Added referenced file: {FileName} with {TableCount} tables",
                    referencedFile.FileName, columns.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add referenced file: {FilePath}", filePath);
                // TODO: Show error dialog to user
            }
        }

        /// <summary>
        /// Determines if the remove file command can execute.
        /// </summary>
        private bool CanExecuteRemoveFile()
        {
            return SelectedFile != null;
        }

        /// <summary>
        /// Executes the remove file command.
        /// </summary>
        private void ExecuteRemoveFile()
        {
            if (SelectedFile == null) return;

            try
            {
                // CROSS-MOD
