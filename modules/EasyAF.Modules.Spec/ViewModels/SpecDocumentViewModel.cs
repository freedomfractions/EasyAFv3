using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;
using EasyAF.Modules.Spec.Models;
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
        private bool _disposed;
        private object? _selectedTabContent;
        private int _selectedTabIndex;

        /// <summary>
        /// Initializes a new instance of the SpecDocumentViewModel.
        /// </summary>
        /// <param name="document">The spec document this VM represents.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        /// <exception cref="ArgumentNullException">If document or dialogService is null.</exception>
        public SpecDocumentViewModel(SpecDocument document, IUserDialogService dialogService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

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
        /// Table editor tabs will be added dynamically in Task 26 based on tables in spec.
        /// For now, we only create the Setup tab.
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

            // TODO Task 26: Add table editor tabs dynamically based on tables in spec
            // For each table in Spec.Tables:
            //   - Create TableEditorViewModel
            //   - Add tab to TabHeaders collection

            // Select setup tab by default
            if (TabHeaders.Count > 0)
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
            // TODO Task 26: Rebuild table tabs when tables are added/removed
            Log.Debug("Tables changed, tab rebuild deferred to Task 26");
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
