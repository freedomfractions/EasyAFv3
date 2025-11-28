using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;
using EasyAF.Modules.Spec.Models;
using EasyAF.Modules.Map.Services;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// Main view model for the spec document, managing tabs and coordinating child view models.
    /// </summary>
    /// <remarks>
    /// This VM acts as the coordinator for the entire spec UI. It manages:
    /// - The tab collection (Setup tab + one tab per table)
    /// - Tab selection and activation
    /// - Document-level state
    /// - Communication between child VMs
    /// </remarks>
    public class SpecDocumentViewModel : BindableBase, IDisposable
    {
        private readonly SpecDocument _document;
        private readonly IUserDialogService _dialogService;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly ISettingsService _settingsService; // NEW: For global settings access
        private bool _disposed;
        private object? _selectedTabContent;
        private int _selectedTabIndex;

        /// <summary>
        /// Initializes a new instance of the SpecDocumentViewModel.
        /// </summary>
        /// <param name="document">The spec document this VM represents.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        /// <param name="propertyDiscovery">Service for discovering data type properties.</param>
        /// <param name="settingsService">Service for accessing settings (property visibility).</param>
        /// <exception cref="ArgumentNullException">If document or dialogService is null.</exception>
        public SpecDocumentViewModel(SpecDocument document, IUserDialogService dialogService, IPropertyDiscoveryService propertyDiscovery, ISettingsService settingsService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // Initialize collections
            TabHeaders = new ObservableCollection<TabHeaderInfo>();

            // Subscribe to document changes
            _document.PropertyChanged += OnDocumentPropertyChanged;

            // Initialize tabs
            InitializeTabs();

            Log.Debug("SpecDocumentViewModel initialized with {TabCount} tabs", TabHeaders.Count);
        }

        #region Properties

        /// <summary>
        /// Gets the underlying spec document.
        /// </summary>
        public SpecDocument Document => _document;

        /// <summary>
        /// Gets the collection of tab headers (Setup tab + table tabs).
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
                }
            }
        }

        /// <summary>
        /// Gets the title for the document window.
        /// </summary>
        public string DocumentTitle => _document.Title + (_document.IsDirty ? "*" : "");

        /// <summary>
        /// Gets the Setup ViewModel for ribbon command binding.
        /// </summary>
        public SpecSetupViewModel? Setup =>
            TabHeaders.FirstOrDefault(t => t.Header == "Setup")?.ViewModel as SpecSetupViewModel;

        #endregion

        #region Tab Management

        /// <summary>
        /// Initializes the tab collection with Setup tab.
        /// </summary>
        /// <remarks>
        /// Table editor tabs are added dynamically based on tables in spec (Task 26).
        /// </remarks>
        private void InitializeTabs()
        {
            TabHeaders.Clear();

            // Setup tab (always first)
            var setupVm = new SpecSetupViewModel(_document, _dialogService);
            
            // Subscribe to table changes so we can update tabs dynamically
            setupVm.TablesChanged += OnTablesChanged;
            
            TabHeaders.Add(new TabHeaderInfo
            {
                Header = "Setup",
                DisplayName = "Setup",
                ViewModel = setupVm,
                TableId = null // Setup tab has no associated table
            });

            // Task 26: Add table editor tabs dynamically
            OnTablesChanged(this, EventArgs.Empty);

            // Select setup tab by default
            if (TabHeaders.Count > 0 && SelectedTabIndex < 0)
            {
                SelectedTabIndex = 0;
            }

            Log.Information("Tab initialization complete: {Count} tab(s) created", TabHeaders.Count);
        }

        /// <summary>
        /// Handles changes to the table collection (add/remove tables).
        /// </summary>
        private void OnTablesChanged(object? sender, EventArgs e)
        {
            // CROSS-MODULE EDIT: 2025-11-28 Task 26 - Dynamic Table Tabs
            // Modified for: Rebuild table tabs when tables are added/removed
            // Related modules: Spec (TableEditorViewModel, TableEditorView)
            // Rollback instructions: Restore original TODO comment
            
            Log.Debug("Tables changed, rebuilding table tabs");
            
            // Preserve current selection
            var currentTab = SelectedTabIndex >= 0 && SelectedTabIndex < TabHeaders.Count 
                ? TabHeaders[SelectedTabIndex] 
                : null;
            
            // Remove all table tabs (keep Setup tab)
            for (int i = TabHeaders.Count - 1; i >= 1; i--)
            {
                if (TabHeaders[i].ViewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                TabHeaders.RemoveAt(i);
            }
            
            // Add table tabs
            if (_document.Spec?.Tables != null)
            {
                foreach (var table in _document.Spec.Tables)
                {
                    var editorVm = new TableEditorViewModel(table, _document, _dialogService, _propertyDiscovery, _settingsService);
                    TabHeaders.Add(new TabHeaderInfo
                    {
                        Header = !string.IsNullOrEmpty(table.AltText) ? table.AltText : table.Id,
                        DisplayName = !string.IsNullOrEmpty(table.AltText) ? table.AltText : table.Id,
                        ViewModel = editorVm,
                        TableId = table.Id // Use table ID as unique identifier
                    });
                }
            }
            
            // Restore selection or default to Setup
            if (currentTab != null && currentTab.TableId == null)
            {
                // Was on Setup tab, stay there
                SelectedTabIndex = 0;
            }
            else if (currentTab != null)
            {
                // Try to restore to same table
                var restoredIndex = TabHeaders.Select((t, i) => new { t, i })
                    .FirstOrDefault(x => x.t.TableId == currentTab.TableId)?.i;
                SelectedTabIndex = restoredIndex ?? 0;
            }
            else
            {
                SelectedTabIndex = 0;
            }
            
            Log.Information("Tab rebuild complete: {Count} total tabs ({TableCount} table editors)", 
                TabHeaders.Count, TabHeaders.Count - 1);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles property changes on the document to update UI.
        /// </summary>
        private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SpecDocument.IsDirty) ||
                e.PropertyName == nameof(SpecDocument.Title))
            {
                RaisePropertyChanged(nameof(DocumentTitle));
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

            _document.PropertyChanged -= OnDocumentPropertyChanged;

            // Unsubscribe from setup VM
            if (Setup != null)
            {
                Setup.TablesChanged -= OnTablesChanged;
            }

            // Dispose child VMs
            foreach (var tab in TabHeaders)
            {
                if (tab.ViewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents information about a tab header (display text).
    /// </summary>
    public class TabHeaderInfo : BindableBase
    {
        private string _header = string.Empty;
        private string _displayName = string.Empty;

        /// <summary>
        /// Gets or sets the tab header text (internal name).
        /// </summary>
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// Gets or sets the user-friendly display name.
        /// </summary>
        public string DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? _header : _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Gets or sets the view model for this tab's content.
        /// </summary>
        public object? ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the table ID (null for Setup tab).
        /// </summary>
        public string? TableId { get; set; }
    }
}
