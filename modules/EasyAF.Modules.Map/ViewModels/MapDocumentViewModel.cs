using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Mvvm;
using Prism.Commands;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services;
using Serilog;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// Main view model for the map document, managing tabs and coordinating child view models.
    /// </summary>
    /// <remarks>
    /// This VM acts as the coordinator for the entire mapping UI. It manages:
    /// - The tab collection (Summary tab + one tab per data type)
    /// - Tab selection and activation
    /// - Document-level state
    /// - Communication between child VMs
    /// </remarks>
    public class MapDocumentViewModel : BindableBase, IDisposable
    {
        private readonly MapDocument _document;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly IUserDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private bool _disposed;
        private object? _selectedTabContent;
        private int _selectedTabIndex;

        /// <summary>
        /// Initializes a new instance of the MapDocumentViewModel.
        /// </summary>
        /// <param name="document">The map document this VM represents.</param>
        /// <param name="propertyDiscovery">Service for discovering data type properties.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        /// <param name="settingsService">Service for accessing module settings.</param>
        /// <exception cref="ArgumentNullException">If document, propertyDiscovery, or settingsService is null.</exception>
        public MapDocumentViewModel(
            MapDocument document,
            IPropertyDiscoveryService propertyDiscovery,
            IUserDialogService dialogService,
            ISettingsService settingsService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // Initialize collections
            TabHeaders = new ObservableCollection<TabHeaderInfo>();
            
            // Subscribe to document changes
            _document.PropertyChanged += OnDocumentPropertyChanged;

            // SUBSCRIBE to settings reload so tabs/properties refresh after Options changes
            // (removes mappings to newly hidden properties and updates lists)
            _settingsService.SettingsReloaded += OnSettingsReloaded;

            // Initialize ribbon commands
            AutoMapCommand = new DelegateCommand(ExecuteAutoMap);
            ValidateMappingsCommand = new DelegateCommand(ExecuteValidateMappings);
            ClearAllMappingsCommand = new DelegateCommand(ExecuteClearAllMappings, CanExecuteClearAllMappings);

            // Subscribe to settings changes for live property visibility updates
            _settingsService.SettingsReloaded += OnSettingsReloaded;

            // Initialize tabs
            InitializeTabs();

            Log.Debug("MapDocumentViewModel initialized with {TabCount} tabs", TabHeaders.Count);
        }

        #region Properties

        /// <summary>
        /// Gets the underlying map document.
        /// </summary>
        public MapDocument Document => _document;

        /// <summary>
        /// Gets the collection of tab headers (one for Summary, one per data type).
        /// </summary>
        public ObservableCollection<TabHeaderInfo> TabHeaders { get; }

        /// <summary>
        /// Gets or sets the currently selected tab's view model.
        /// </summary>
        public object? SelectedTabContent
        {
            get => _selectedTabContent;
            set => SetProperty(ref _selectedTabContent, value);
        }

        /// <summary>
        /// Gets or sets the index of the currently selected tab.
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (SetProperty(ref _selectedTabIndex, value) && value >= 0 && value < TabHeaders.Count)
                {
                    SelectedTabContent = TabHeaders[value].ViewModel;
                    Log.Debug("Selected tab: {TabName}", TabHeaders[value].Header);
                    
                    // Auto-refresh Summary tab statistics asynchronously when it gains focus
                    if (TabHeaders[value].Header == "Summary" && TabHeaders[value].ViewModel is MapSummaryViewModel summaryVm)
                    {
                        _ = summaryVm.RefreshStatusAsync();
                        Log.Debug("Triggered async auto-refresh of Summary tab statistics");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the title for the document window.
        /// </summary>
        public string DocumentTitle => _document.Title + (_document.IsDirty ? "*" : "");

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command to automatically map columns using intelligent matching.
        /// </summary>
        public ICommand AutoMapCommand { get; }

        /// <summary>
        /// Gets the command to validate all mappings.
        /// </summary>
        public ICommand ValidateMappingsCommand { get; }

        /// <summary>
        /// Gets the command to clear all mappings.
        /// </summary>
        public ICommand ClearAllMappingsCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteAutoMap()
        {
            // Delegate to currently selected data type tab
            if (SelectedTabContent is DataTypeMappingViewModel dataTypeVm)
            {
                dataTypeVm.AutoMapCommand.Execute(null);
            }
            else
            {
                Log.Information("Auto-Map: Please select a data type tab first");
                // TODO: Show user dialog
            }
        }

        private void ExecuteValidateMappings()
        {
            // TODO: Implement validation across all data types
            Log.Information("Validate Mappings requested - validation feature to be implemented");
            
            // For now, just log mapping status for each data type
            foreach (var tab in TabHeaders.Where(t => t.DataType != null))
            {
                if (tab.DataType == null) continue;
                
                var status = CalculateMappingStatus(tab.DataType);
                Log.Information("{DataType} mapping status: {Status}", tab.DataType, status);
            }
        }

        private bool CanExecuteClearAllMappings()
        {
            // Enable if any mappings exist
            return _document.MappingsByDataType.Values.Any(m => m.Count > 0);
        }

        private void ExecuteClearAllMappings()
        {
            // Delegate to currently selected data type tab
            if (SelectedTabContent is DataTypeMappingViewModel dataTypeVm)
            {
                dataTypeVm.ClearMappingsCommand.Execute(null);
            }
            else
            {
                Log.Information("Clear Mappings: Please select a data type tab first");
                // TODO: Show user dialog
            }
        }

        #endregion

        #region Tab Management

        /// <summary>
        /// Initializes the tab collection with Summary + enabled data type tabs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only creates tabs for data types where Enabled = true in settings.
        /// This allows users to hide data types they don't use (e.g., ShortCircuit).
        /// </para>
        /// </remarks>
        private void InitializeTabs()
        {
            TabHeaders.Clear();

            // Summary tab (always first)
            var summaryVm = new MapSummaryViewModel(_document, _propertyDiscovery, this);
            TabHeaders.Add(new TabHeaderInfo
            {
                Header = "Summary",
                Status = string.Empty,
                ViewModel = summaryVm,
                DataType = null
            });

            // Data type tabs (one per discovered type, filtered by settings)
            var dataTypes = _propertyDiscovery.GetAvailableDataTypes();
            Log.Information("Creating tabs for enabled data types (total discovered: {Count})", dataTypes.Count);

            foreach (var dataType in dataTypes)
            {
                // CROSS-MODULE EDIT: 2025-01-16 Map Module Settings Feature - Step 5
                // Modified for: Check IsDataTypeEnabled before creating tabs
                // Related modules: Core (ISettingsService), Map (DataTypeVisibilitySettings, MapSettingsExtensions)
                // Rollback instructions: Remove IsDataTypeEnabled check, revert to creating all tabs
                
                // Skip disabled data types
                if (!_settingsService.IsDataTypeEnabled(dataType))
                {
                    Log.Debug("Skipping tab for disabled data type: {DataType}", dataType);
                    continue;
                }

                // CROSS-MODULE EDIT: 2025-01-16 Duplicate Mapping Prevention
                // Modified for: Pass IUserDialogService to DataTypeMappingViewModel for confirmation dialogs
                // Related modules: Core (IUserDialogService), Map (DataTypeMappingViewModel)
                // Rollback instructions: Remove _dialogService parameter from constructor call
                
                var dataTypeVm = new DataTypeMappingViewModel(_document, dataType, _propertyDiscovery, this, _settingsService, _dialogService);
                
                TabHeaders.Add(new TabHeaderInfo
                {
                    Header = dataType,
                    Status = GetInitialStatus(dataType),
                    ViewModel = dataTypeVm,
                    DataType = dataType
                });
                
                Log.Debug("Created tab for enabled data type: {DataType}", dataType);
            }

            Log.Information("Tab initialization complete: {EnabledCount} tabs created", TabHeaders.Count);

            // CROSS-MODULE EDIT: 2025-01-16 Table Reference Persistence
            // Modified for: Restore saved table selections after tabs are created
            // Related modules: Map (MapDocument, MapDocumentSerializer, DataTypeMappingViewModel)
            // Rollback instructions: Remove this restoration block
            
            // Restore table selections for each data type from saved references
            foreach (var tab in TabHeaders.Where(t => t.ViewModel is DataTypeMappingViewModel))
            {
                if (tab.ViewModel is DataTypeMappingViewModel dataTypeVm)
                {
                    dataTypeVm.RestoreTableSelection();
                }
            }

            // Select summary tab by default
            if (TabHeaders.Count > 0)
            {
                SelectedTabIndex = 0;
            }
        }

        /// <summary>
        /// Gets the initial mapping status indicator for a data type.
        /// </summary>
        private string GetInitialStatus(string dataType)
        {
            if (!_document.MappingsByDataType.TryGetValue(dataType, out var mappings))
                return "?"; // Empty circle = unmapped

            if (mappings.Count == 0)
                return "?"; // Empty circle = unmapped

            var properties = _propertyDiscovery.GetPropertiesForType(dataType);
            if (properties.Count == 0)
                return "?";

            // Check mapping completion
            if (mappings.Count >= properties.Count)
                return "?"; // Filled circle = complete

            return "?"; // Half-filled circle = partial
        }

        /// <summary>
        /// Updates the status indicator for a specific data type tab.
        /// </summary>
        /// <param name="dataType">The data type whose status changed.</param>
        /// <param name="status">The new mapping status.</param>
        public void UpdateTabStatus(string dataType, MappingStatus status)
        {
            var tab = TabHeaders.FirstOrDefault(t => t.DataType == dataType);
            if (tab == null)
            {
                Log.Warning("Attempted to update status for unknown data type: {DataType}", dataType);
                return;
            }

            tab.Status = status switch
            {
                MappingStatus.Unmapped => "?",
                MappingStatus.Partial => "?",
                MappingStatus.Complete => "?",
                _ => "?"
            };

            Log.Debug("Updated tab status for {DataType}: {Status}", dataType, tab.Status);
        }

        /// <summary>
        /// Refreshes the status for all data type tabs.
        /// </summary>
        public void RefreshAllTabStatuses()
        {
            foreach (var tab in TabHeaders.Where(t => t.DataType != null))
            {
                if (tab.DataType == null) continue;

                var status = CalculateMappingStatus(tab.DataType);
                tab.Status = status switch
                {
                    MappingStatus.Unmapped => "?",
                    MappingStatus.Partial => "?",
                    MappingStatus.Complete => "?",
                    _ => "?"
                };

                // Refresh available tables when files are added/removed
                if (tab.ViewModel is DataTypeMappingViewModel dataTypeVm)
                {
                    dataTypeVm.RefreshAvailableTables();
                }
            }
        }

        /// <summary>
        /// Asynchronously refreshes the status for all data type tabs.
        /// </summary>
        /// <remarks>
        /// This version runs on a background thread to avoid blocking the UI.
        /// Used by the Summary tab auto-refresh to prevent lag.
        /// NOTE: Does NOT refresh available tables since that requires file I/O.
        /// Table refresh only happens when files are added/removed in Summary tab.
        /// </remarks>
        public async Task RefreshAllTabStatusesAsync()
        {
            await Task.Run(() =>
            {
                foreach (var tab in TabHeaders.Where(t => t.DataType != null))
                {
                    if (tab.DataType == null) continue;

                    var status = CalculateMappingStatus(tab.DataType);
                    var statusIcon = status switch
                    {
                        MappingStatus.Unmapped => "?",
                        MappingStatus.Partial => "?",
                        MappingStatus.Complete => "?",
                        _ => "?"
                    };

                    // Update on UI thread
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        tab.Status = statusIcon;
                    });

                    // REMOVED: RefreshAvailableTables() call
                    // Refreshing tables requires file I/O (ExtractColumns) which is expensive
                    // Tables should only refresh when files are actually added/removed
                }
            });
        }

        /// <summary>
        /// Calculates the current mapping status for a data type.
        /// </summary>
        private MappingStatus CalculateMappingStatus(string dataType)
        {
            if (!_document.MappingsByDataType.TryGetValue(dataType, out var mappings))
                return MappingStatus.Unmapped;

            if (mappings.Count == 0)
                return MappingStatus.Unmapped;

            var properties = _propertyDiscovery.GetPropertiesForType(dataType);
            if (properties.Count == 0)
                return MappingStatus.Unmapped;

            if (mappings.Count >= properties.Count)
                return MappingStatus.Complete;

            return MappingStatus.Partial;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles property changes on the document to update UI.
        /// </summary>
        private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapDocument.IsDirty) || 
                e.PropertyName == nameof(MapDocument.Title))
            {
                RaisePropertyChanged(nameof(DocumentTitle));
            }
        }

        /// <summary>
        /// Handles settings reload event to refresh property visibility in all tabs.
        /// </summary>
        private void OnSettingsReloaded(object? sender, EventArgs e)
        {
            Log.Information("Settings reloaded, refreshing property visibility for all data types");
            
            foreach (var tab in TabHeaders.Where(t => t.ViewModel is DataTypeMappingViewModel))
            {
                if (tab.ViewModel is DataTypeMappingViewModel dataTypeVm)
                {
                    dataTypeVm.RefreshTargetProperties();
                }
            }
        }

        #endregion

        #region File Validation

        /// <summary>
        /// Checks for missing referenced files and shows a resolution dialog if any are found.
        /// </summary>
        /// <remarks>
        /// This method should be called after loading a document to ensure all referenced files are accessible.
        /// If missing files are detected, a dialog is shown allowing the user to browse for relocated files,
        /// remove missing references, or continue anyway.
        /// </remarks>
        public void ValidateMissingFiles()
        {
            // CROSS-MODULE EDIT: 2025-01-16 Missing File Detection
            // Modified for: Add file validation on document load with resolution dialog
            // Related modules: Map (MapDocument, MissingFilesDialogViewModel, MissingFilesDialog)
            // Rollback instructions: Remove this method and related dialog files
            
            var missingFiles = _document.ValidateReferencedFiles();
            
            if (missingFiles.Count == 0)
            {
                Log.Debug("All referenced files are valid");
                return;
            }

            Log.Warning("Found {Count} missing referenced files", missingFiles.Count);

            // Show missing files dialog
            try
            {
                var dialogVm = new MissingFilesDialogViewModel(missingFiles);
                var dialog = new Views.MissingFilesDialog
                {
                    DataContext = dialogVm,
                    Owner = System.Windows.Application.Current?.MainWindow
                };

                var result = dialog.ShowDialog();
                
                if (result != true)
                {
                    Log.Information("User cancelled missing files resolution");
                    return;
                }

                // Process user actions
                foreach (var entry in dialogVm.MissingFiles)
                {
                    switch (entry.Status)
                    {
                        case "Resolved":
                            // User located the file - update path in document
                            if (!string.IsNullOrEmpty(entry.NewPath))
                            {
                                _document.UpdateReferencedFilePath(entry.OriginalPath, entry.NewPath);
                                Log.Information("Updated file reference: {Old} -> {New}", entry.OriginalPath, entry.NewPath);
                            }
                            break;

                        case "Removed":
                            // User wants to remove this file reference
                            _document.RemoveReferencedFile(entry.OriginalPath);
                            Log.Information("Removed missing file reference: {Path}", entry.OriginalPath);
                            break;

                        case "Missing":
                            // User chose to continue with missing file - leave as is
                            Log.Warning("Continuing with missing file: {Path}", entry.OriginalPath);
                            break;
                    }
                }

                // Refresh the UI after changes
                RefreshAllTabStatuses();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing missing files dialog");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Unsubscribe from settings events
            _settingsService.SettingsReloaded -= OnSettingsReloaded;

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Gets the user dialog service for child ViewModels.
        /// </summary>
        /// <returns>The IUserDialogService instance.</returns>
        /// <remarks>
        /// This is a helper method to allow child ViewModels (like MapSummaryViewModel)
        /// to access the dialog service without requiring it in their constructors.
        /// </remarks>
        public IUserDialogService GetDialogService()
        {
            return _dialogService;
        }
    }

    /// <summary>
    /// Represents information about a tab header (display text and status).
    /// </summary>
    public class TabHeaderInfo : BindableBase
    {
        private string _header = string.Empty;
        private string _status = string.Empty;

        /// <summary>
        /// Gets or sets the tab header text (e.g., "Summary", "Bus", "LVCB").
        /// </summary>
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// Gets or sets the status indicator glyph (?, ?, ?).
        /// </summary>
        /// <remarks>
        /// ? = Unmapped (empty circle)
        /// ? = Partial (half-filled circle)
        /// ? = Complete (filled circle)
        /// </remarks>
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// Gets or sets the view model for this tab's content.
        /// </summary>
        public object? ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the data type name (null for Summary tab).
        /// </summary>
        public string? DataType { get; set; }
    }

    /// <summary>
    /// Enumeration of mapping completion statuses.
    /// </summary>
    public enum MappingStatus
    {
        /// <summary>No mappings defined for this data type.</summary>
        Unmapped,
        
        /// <summary>Some but not all properties are mapped.</summary>
        Partial,
        
        /// <summary>All properties have mappings.</summary>
        Complete
    }
}
