using System;
using System.Collections.Generic;
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
        private readonly IFuzzyMatcher _fuzzyMatcher;  // Fuzzy search for Auto-Map
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
        /// <param name="fuzzyMatcher">Service for fuzzy string matching (Auto-Map).</param>
        /// <exception cref="ArgumentNullException">If document, propertyDiscovery, or settingsService is null.</exception>
        public MapDocumentViewModel(
            MapDocument document,
            IPropertyDiscoveryService propertyDiscovery,
            IUserDialogService dialogService,
            ISettingsService settingsService,
            IFuzzyMatcher fuzzyMatcher)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _fuzzyMatcher = fuzzyMatcher ?? throw new ArgumentNullException(nameof(fuzzyMatcher));

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
            var summaryVm = new MapSummaryViewModel(_document, _propertyDiscovery, _settingsService, this);
            TabHeaders.Add(new TabHeaderInfo
            {
                Header = "Summary",
                DisplayName = "Summary", // Summary tab uses same name
                Status = MappingStatus.Unmapped, // Summary tab doesn't have a status
                ViewModel = summaryVm,
                DataType = null
            });

            // Data type tabs (one per discovered type, filtered by settings)
            var dataTypes = _propertyDiscovery.GetAvailableDataTypes();
            Log.Information("Creating tabs for enabled data types (total discovered: {Count})", dataTypes.Count);

            foreach (var dataType in dataTypes)
            {
                // Skip disabled data types
                if (!_settingsService.IsDataTypeEnabled(dataType))
                {
                    Log.Debug("Skipping tab for disabled data type: {DataType}", dataType);
                    continue;
                }

                var dataTypeVm = new DataTypeMappingViewModel(_document, dataType, _propertyDiscovery, this, _settingsService, _dialogService, _fuzzyMatcher);
                
                TabHeaders.Add(new TabHeaderInfo
                {
                    Header = dataType, // Internal name (e.g., "Bus", "LVBreaker")
                    DisplayName = _propertyDiscovery.GetDataTypeDescription(dataType), // Friendly name (e.g., "Buses", "LV Breakers")
                    Status = GetInitialStatus(dataType),
                    ViewModel = dataTypeVm,
                    DataType = dataType
                });
                
                Log.Debug("Created tab for enabled data type: {DataType} (display: {DisplayName})", 
                    dataType, _propertyDiscovery.GetDataTypeDescription(dataType));
            }

            Log.Information("Tab initialization complete: {EnabledCount} tabs created", TabHeaders.Count);

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
        private MappingStatus GetInitialStatus(string dataType)
        {
            if (!_document.MappingsByDataType.TryGetValue(dataType, out var mappings))
                return MappingStatus.Unmapped;

            if (mappings.Count == 0)
                return MappingStatus.Unmapped;

            var properties = _propertyDiscovery.GetPropertiesForType(dataType);
            if (properties.Count == 0)
                return MappingStatus.Unmapped;

            // Check mapping completion
            if (mappings.Count >= properties.Count)
                return MappingStatus.Complete;

            return MappingStatus.Partial;
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

            tab.Status = status;

            Log.Debug("Updated tab status for {DataType}: {Status}", dataType, status);
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
                tab.Status = status;

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

                    // Update on UI thread
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        tab.Status = status;
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
            
            // CROSS-MODULE EDIT: 2025-01-16 Property Count Fix
            // Modified for: Clear property cache when settings change so counts update correctly
            // Related modules: Map (PropertyDiscoveryService)
            // Rollback instructions: Remove ClearCache call
            
            // Clear cache so property counts reflect new visibility settings
            // This ensures Summary tab and data type tabs show correct available property counts
            _propertyDiscovery.ClearCache();
            
            // CROSS-MODULE EDIT: 2025-01-16 Invalid Mapping Detection
            // Modified for: Detect and clean up mappings to hidden properties after settings change
            // Related modules: Map (InvalidMappingDetector)
            // Rollback instructions: Remove invalid mapping detection logic
            
            // Check for invalid mappings BEFORE refreshing tabs
            // (properties may have been hidden, leaving orphaned mappings)
            var invalidDetector = new InvalidMappingDetector(_propertyDiscovery);
            var invalidMappings = invalidDetector.FindInvalidMappings(_document);
            
            if (invalidMappings.Any())
            {
                var totalInvalid = invalidMappings.Sum(kvp => kvp.Value.Count);
                var summary = invalidDetector.GetInvalidMappingsSummary(invalidMappings);
                
                var confirmed = _dialogService.Confirm(
                    $"Invalid Mappings Detected\n\n" +
                    $"{summary}" +
                    $"Total: {totalInvalid} mapping(s) reference hidden properties.\n\n" +
                    $"Remove these invalid mappings?",
                    "Remove Invalid Mappings?");
                
                if (confirmed)
                {
                    var removedCount = invalidDetector.RemoveInvalidMappings(_document, invalidMappings);
                    Log.Information("Removed {Count} invalid mappings after settings change", removedCount);
                }
                else
                {
                    Log.Information("User chose to keep {Count} invalid mappings", totalInvalid);
                }
            }
            
            // Now refresh tabs (this will update UI to reflect property visibility changes)
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
                // Don't return yet - continue to check for invalid mappings
            }
            else
            {
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
                        // Continue to check invalid mappings anyway
                    }
                    else
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error showing missing files dialog");
                }
            }

            // CROSS-MODULE EDIT: 2025-01-16 Invalid Mapping Detection
            // Modified for: Check for invalid mappings on document load
            // Related modules: Map (InvalidMappingDetector)
            // Rollback instructions: Remove invalid mapping detection logic below
            
            // Check for mappings to properties that are now hidden (settings may have changed since save)
            var invalidDetector = new InvalidMappingDetector(_propertyDiscovery);
            var invalidMappings = invalidDetector.FindInvalidMappings(_document);
            
            if (invalidMappings.Any())
            {
                var totalInvalid = invalidMappings.Sum(kvp => kvp.Value.Count);
                var summary = invalidDetector.GetInvalidMappingsSummary(invalidMappings);
                
                Log.Warning("Found {Count} invalid mappings on document load", totalInvalid);
                
                var confirmed = _dialogService.Confirm(
                    $"Invalid Mappings Detected\n\n" +
                    $"{summary}" +
                    $"Total: {totalInvalid} mapping(s) reference properties that are currently hidden in settings.\n\n" +
                    $"This may occur if settings were changed since this map was saved.\n\n" +
                    $"Remove these invalid mappings?",
                    "Remove Invalid Mappings?");
                
                if (confirmed)
                {
                    var removedCount = invalidDetector.RemoveInvalidMappings(_document, invalidMappings);
                    Log.Information("Removed {Count} invalid mappings on document load", removedCount);
                }
                else
                {
                    Log.Information("User chose to keep {Count} invalid mappings (properties remain hidden)", totalInvalid);
                }
            }

            // Refresh the UI after any changes
            RefreshAllTabStatuses();
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

        #region Helper Methods

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

        /// <summary>
        /// Gets a dictionary of currently selected tables by data type.
        /// </summary>
        /// <returns>Dictionary where key=dataType, value=TableReference.DisplayName</returns>
        /// <remarks>
        /// Used by the cross-tab table exclusivity feature to prevent the same table
        /// from being selected on multiple tabs simultaneously.
        /// </remarks>
        public Dictionary<string, string> GetSelectedTablesByDataType()
        {
            var selectedTables = new Dictionary<string, string>();
            
            foreach (var tab in TabHeaders.Where(t => t.ViewModel is DataTypeMappingViewModel))
            {
                if (tab.ViewModel is DataTypeMappingViewModel dataTypeVm && 
                    tab.DataType != null &&
                    dataTypeVm.SelectedTable != null &&
                    !string.IsNullOrEmpty(dataTypeVm.SelectedTable.DisplayName))
                {
                    selectedTables[tab.DataType] = dataTypeVm.SelectedTable.DisplayName;
                }
            }
            
            return selectedTables;
        }

        #endregion
    }

    /// <summary>
    /// Represents information about a tab header (display text and status).
    /// </summary>
    public class TabHeaderInfo : BindableBase
    {
        private string _header = string.Empty;
        private string _displayName = string.Empty; // NEW: User-friendly display name
        private MappingStatus _status = MappingStatus.Unmapped;

        /// <summary>
        /// Gets or sets the tab header text (e.g., "Summary", "Bus", "LVBreaker") - raw class name.
        /// </summary>
        /// <remarks>
        /// This is the internal data type name used for lookups. For UI display, use <see cref="DisplayName"/>.
        /// </remarks>
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// Gets or sets the user-friendly display name from [EasyPowerClass] attribute (e.g., "Buses", "LV Breakers").
        /// </summary>
        /// <remarks>
        /// This is what should be displayed in the tab header. Falls back to <see cref="Header"/> if not set.
        /// </remarks>
        public string DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? _header : _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Gets or sets the mapping status (enum instead of string glyph).
        /// </summary>
        public MappingStatus Status
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
