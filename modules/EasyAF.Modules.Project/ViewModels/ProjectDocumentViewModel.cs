using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Modules.Project.Models;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// Main view model for the project document, managing tabs and coordinating child view models.
    /// </summary>
    /// <remarks>
    /// This VM acts as the coordinator for the entire project UI. It manages:
    /// - The tab collection (Summary tab + one tab per data type with data)
    /// - Tab selection and activation
    /// - Document-level state
    /// - Communication between child VMs
    /// </remarks>
    public class ProjectDocumentViewModel : BindableBase, IDisposable
    {
        private readonly ProjectDocument _document;
        private readonly IUserDialogService _dialogService;
        private bool _disposed;
        private object? _selectedTabContent;
        private int _selectedTabIndex;

        /// <summary>
        /// Initializes a new instance of the ProjectDocumentViewModel.
        /// </summary>
        /// <param name="document">The project document this VM represents.</param>
        /// <param name="dialogService">Service for showing user dialogs.</param>
        /// <exception cref="ArgumentNullException">If document or dialogService is null.</exception>
        public ProjectDocumentViewModel(ProjectDocument document, IUserDialogService dialogService)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Initialize collections
            TabHeaders = new ObservableCollection<TabHeaderInfo>();

            // Subscribe to document changes
            _document.PropertyChanged += OnDocumentPropertyChanged;

            // Initialize tabs
            InitializeTabs();

            Log.Debug("ProjectDocumentViewModel initialized with {TabCount} tabs", TabHeaders.Count);
        }

        #region Properties

        /// <summary>
        /// Gets the underlying project document.
        /// </summary>
        public ProjectDocument Document => _document;

        /// <summary>
        /// Gets the collection of tab headers (Summary tab + data type tabs).
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
        /// Gets the Summary ViewModel for ribbon command binding.
        /// </summary>
        public ProjectSummaryViewModel? Summary =>
            TabHeaders.FirstOrDefault(t => t.Header == "Summary")?.ViewModel as ProjectSummaryViewModel;

        #endregion

        #region Tab Management

        /// <summary>
        /// Initializes the tab collection with Summary tab.
        /// </summary>
        /// <remarks>
        /// Data type tabs will be added dynamically in Task 21 based on loaded data.
        /// For now, we only create the Summary tab.
        /// </remarks>
        private void InitializeTabs()
        {
            TabHeaders.Clear();

            // Summary tab (always first)
            var summaryVm = new ProjectSummaryViewModel(_document, _dialogService);
            TabHeaders.Add(new TabHeaderInfo
            {
                Header = "Summary",
                DisplayName = "Summary",
                ViewModel = summaryVm,
                DataType = null // Summary tab has no associated data type
            });

            // TODO Task 21: Add data type tabs dynamically based on loaded data
            // For each equipment type with data in NewData or OldData:
            //   - Create DataTypeTabViewModel
            //   - Add tab to TabHeaders collection

            // Select summary tab by default
            if (TabHeaders.Count > 0)
            {
                SelectedTabIndex = 0;
            }

            Log.Information("Tab initialization complete: {Count} tab(s) created", TabHeaders.Count);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles property changes on the document to update UI.
        /// </summary>
        private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectDocument.IsDirty) ||
                e.PropertyName == nameof(ProjectDocument.Title))
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
        /// Gets or sets the data type name (null for Summary tab).
        /// </summary>
        public string? DataType { get; set; }
    }
}
