using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Engine;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Map.Services;
using EasyAF.Modules.Spec.Models;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels.Dialogs
{
    /// <summary>
    /// ViewModel for the Column Editor dialog.
    /// </summary>
    /// <remarks>
    /// Modular design - will add functionality incrementally:
    /// Phase 1: Basic shell + OK/Cancel
    /// Phase 2: Add panels one at a time
    /// Phase 3: Live preview integration
    /// </remarks>
    public class ColumnEditorViewModel : BindableBase
    {
        private readonly ColumnSpec _columnSpec;
        private readonly SpecDocument _document;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly ISettingsService _settingsService;

        private string _columnHeader = string.Empty;
        private double? _widthPercent;
        private bool _mergeVertically;

        /// <summary>
        /// Initializes a new instance of the ColumnEditorViewModel.
        /// </summary>
        public ColumnEditorViewModel(
            ColumnSpec columnSpec,
            SpecDocument document,
            IPropertyDiscoveryService propertyDiscovery,
            ISettingsService settingsService)
        {
            _columnSpec = columnSpec ?? throw new ArgumentNullException(nameof(columnSpec));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // Load current values from ColumnSpec
            LoadFromColumnSpec();

            // Initialize commands
            OkCommand = new DelegateCommand(ExecuteOk, CanExecuteOk);
            CancelCommand = new DelegateCommand(ExecuteCancel);

            Log.Information("ColumnEditorViewModel initialized for column: {Header}", 
                _columnSpec.Header ?? "(new column)");
        }

        #region Properties

        public string ColumnHeader
        {
            get => _columnHeader;
            set => SetProperty(ref _columnHeader, value);
        }

        public double? WidthPercent
        {
            get => _widthPercent;
            set => SetProperty(ref _widthPercent, value);
        }

        public bool MergeVertically
        {
            get => _mergeVertically;
            set => SetProperty(ref _mergeVertically, value);
        }

        public bool? DialogResult { get; private set; }

        #endregion

        #region Commands

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Command Implementations

        private bool CanExecuteOk()
        {
            // Must have a column header
            if (string.IsNullOrWhiteSpace(ColumnHeader))
                return false;

            return true;
        }

        private void ExecuteOk()
        {
            // Update the ColumnSpec with current values
            _columnSpec.Header = ColumnHeader;
            _columnSpec.WidthPercent = WidthPercent;
            _columnSpec.MergeVertically = MergeVertically;

            Log.Information("Column updated: {Header}", ColumnHeader);

            DialogResult = true;
            CloseDialog();
        }

        private void ExecuteCancel()
        {
            DialogResult = false;
            CloseDialog();
        }

        private void CloseDialog()
        {
            var window = System.Windows.Application.Current.Windows
                .OfType<Views.Dialogs.ColumnEditorDialog>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
            {
                window.DialogResult = DialogResult;
            }
        }

        #endregion

        #region Helper Methods

        private void LoadFromColumnSpec()
        {
            _columnHeader = _columnSpec.Header ?? string.Empty;
            _widthPercent = _columnSpec.WidthPercent;
            _mergeVertically = _columnSpec.MergeVertically;

            RaisePropertyChanged(string.Empty); // Refresh all bindings
        }

        #endregion
    }
}
