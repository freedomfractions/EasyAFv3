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

            // Initialize commands
            OkCommand = new DelegateCommand(ExecuteOk, CanExecuteOk);
            CancelCommand = new DelegateCommand(ExecuteCancel);

            Log.Information("ColumnEditorViewModel initialized for column: {Header}", 
                _columnSpec.Header ?? "(new column)");
        }

        #region Properties

        public bool? DialogResult { get; private set; }

        #endregion

        #region Commands

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Command Implementations

        private bool CanExecuteOk()
        {
            // For now, always allow OK
            // Will add validation as we build panels
            return true;
        }

        private void ExecuteOk()
        {
            Log.Information("Column editor OK clicked (no changes yet - panels not implemented)");

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
    }
}
