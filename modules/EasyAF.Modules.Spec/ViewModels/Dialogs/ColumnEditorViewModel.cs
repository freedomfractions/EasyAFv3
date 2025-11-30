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

        private bool _isPropertyPathMode = true;
        private bool _isFormatMode;
        private bool _isExpressionMode;
        private bool _isLiteralMode;

        private string? _selectedPropertyPath;
        private string _joinWith = "\n";
        private string _format = string.Empty;
        private string _expression = string.Empty;
        private string? _numberFormat;
        private string _literal = string.Empty;

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

            // Initialize collections
            PropertyPaths = new ObservableCollection<string>();

            // Load current values from ColumnSpec
            LoadFromColumnSpec();

            // Initialize commands
            AddPropertyPathCommand = new DelegateCommand(ExecuteAddPropertyPath);
            RemovePropertyPathCommand = new DelegateCommand(ExecuteRemovePropertyPath, CanExecuteRemovePropertyPath)
                .ObservesProperty(() => SelectedPropertyPath);
            MovePropertyPathUpCommand = new DelegateCommand(ExecuteMovePropertyPathUp, CanExecuteMovePropertyPathUp)
                .ObservesProperty(() => SelectedPropertyPath);
            MovePropertyPathDownCommand = new DelegateCommand(ExecuteMovePropertyPathDown, CanExecuteMovePropertyPathDown)
                .ObservesProperty(() => SelectedPropertyPath);

            InsertFormatTokenCommand = new DelegateCommand(ExecuteInsertFormatToken);

            OkCommand = new DelegateCommand(ExecuteOk, CanExecuteOk);
            CancelCommand = new DelegateCommand(ExecuteCancel);

            Log.Information("ColumnEditorViewModel initialized for column: {Header}", 
                _columnSpec.Header ?? "(new column)");
        }

        #region Properties

        public ObservableCollection<string> PropertyPaths { get; }

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

        public bool IsPropertyPathMode
        {
            get => _isPropertyPathMode;
            set
            {
                if (SetProperty(ref _isPropertyPathMode, value) && value)
                {
                    // When selecting this mode, deselect others
                    IsFormatMode = IsExpressionMode = IsLiteralMode = false;
                }
            }
        }

        public bool IsFormatMode
        {
            get => _isFormatMode;
            set
            {
                if (SetProperty(ref _isFormatMode, value) && value)
                {
                    IsPropertyPathMode = IsExpressionMode = IsLiteralMode = false;
                }
            }
        }

        public bool IsExpressionMode
        {
            get => _isExpressionMode;
            set
            {
                if (SetProperty(ref _isExpressionMode, value) && value)
                {
                    IsPropertyPathMode = IsFormatMode = IsLiteralMode = false;
                }
            }
        }

        public bool IsLiteralMode
        {
            get => _isLiteralMode;
            set
            {
                if (SetProperty(ref _isLiteralMode, value) && value)
                {
                    IsPropertyPathMode = IsFormatMode = IsExpressionMode = false;
                }
            }
        }

        public string JoinWith
        {
            get => _joinWith;
            set => SetProperty(ref _joinWith, value);
        }

        public string Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        public string Expression
        {
            get => _expression;
            set => SetProperty(ref _expression, value);
        }

        public string? NumberFormat
        {
            get => _numberFormat;
            set => SetProperty(ref _numberFormat, value);
        }

        public string Literal
        {
            get => _literal;
            set => SetProperty(ref _literal, value);
        }

        public bool? DialogResult { get; private set; }

        public string? SelectedPropertyPath
        {
            get => _selectedPropertyPath;
            set => SetProperty(ref _selectedPropertyPath, value);
        }

        #endregion

        #region Commands

        public ICommand AddPropertyPathCommand { get; }
        public ICommand RemovePropertyPathCommand { get; }
        public ICommand MovePropertyPathUpCommand { get; }
        public ICommand MovePropertyPathDownCommand { get; }
        public ICommand InsertFormatTokenCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteAddPropertyPath()
        {
            try
            {
                // Open PropertyPath picker dialog (MULTI-SELECT for column building)
                var viewModel = new PropertyPathPickerViewModel(
                    Array.Empty<string>(),
                    _document,
                    _propertyDiscovery,
                    _settingsService,
                    allowMultiSelect: true); // MULTI-SELECT for columns

                var dialog = new Views.Dialogs.PropertyPathPickerDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    foreach (var path in viewModel.ResultPaths)
                    {
                        if (!PropertyPaths.Contains(path))
                        {
                            PropertyPaths.Add(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open property path picker for column");
            }
        }

        private bool CanExecuteRemovePropertyPath() => SelectedPropertyPath != null;

        private void ExecuteRemovePropertyPath()
        {
            if (SelectedPropertyPath != null)
            {
                PropertyPaths.Remove(SelectedPropertyPath);
            }
        }

        private bool CanExecuteMovePropertyPathUp()
        {
            if (SelectedPropertyPath == null) return false;
            var index = PropertyPaths.IndexOf(SelectedPropertyPath);
            return index > 0;
        }

        private void ExecuteMovePropertyPathUp()
        {
            if (SelectedPropertyPath == null) return;
            var index = PropertyPaths.IndexOf(SelectedPropertyPath);
            if (index > 0)
            {
                PropertyPaths.Move(index, index - 1);
            }
        }

        private bool CanExecuteMovePropertyPathDown()
        {
            if (SelectedPropertyPath == null) return false;
            var index = PropertyPaths.IndexOf(SelectedPropertyPath);
            return index >= 0 && index < PropertyPaths.Count - 1;
        }

        private void ExecuteMovePropertyPathDown()
        {
            if (SelectedPropertyPath == null) return;
            var index = PropertyPaths.IndexOf(SelectedPropertyPath);
            if (index >= 0 && index < PropertyPaths.Count - 1)
            {
                PropertyPaths.Move(index, index + 1);
            }
        }

        private void ExecuteInsertFormatToken()
        {
            try
            {
                // Open PropertyPath picker for token insertion (SINGLE SELECT)
                var viewModel = new PropertyPathPickerViewModel(
                    Array.Empty<string>(),
                    _document,
                    _propertyDiscovery,
                    _settingsService,
                    allowMultiSelect: false);

                var dialog = new Views.Dialogs.PropertyPathPickerDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    var path = viewModel.ResultPaths.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        // Insert token at end (could enhance with cursor position tracking)
                        Format += $"{{{path}}}";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to insert format token");
            }
        }

        private bool CanExecuteOk()
        {
            // Must have a column header
            if (string.IsNullOrWhiteSpace(ColumnHeader))
                return false;

            // Must have content in the selected mode
            if (IsPropertyPathMode && PropertyPaths.Count == 0)
                return false;
            if (IsFormatMode && string.IsNullOrWhiteSpace(Format))
                return false;
            if (IsExpressionMode && string.IsNullOrWhiteSpace(Expression))
                return false;
            if (IsLiteralMode && string.IsNullOrWhiteSpace(Literal))
                return false;

            return true;
        }

        private void ExecuteOk()
        {
            // Update the ColumnSpec with current values
            _columnSpec.Header = ColumnHeader;
            _columnSpec.WidthPercent = WidthPercent;
            _columnSpec.MergeVertically = MergeVertically;

            // Set content based on selected mode
            if (IsPropertyPathMode)
            {
                _columnSpec.PropertyPaths = PropertyPaths.ToArray();
                _columnSpec.JoinWith = JoinWith;
                _columnSpec.Format = null;
                _columnSpec.Expression = null;
                _columnSpec.Literal = null;
            }
            else if (IsFormatMode)
            {
                _columnSpec.Format = Format;
                _columnSpec.PropertyPaths = null;
                _columnSpec.Expression = null;
                _columnSpec.Literal = null;
            }
            else if (IsExpressionMode)
            {
                _columnSpec.Expression = Expression;
                _columnSpec.NumberFormat = NumberFormat;
                _columnSpec.PropertyPaths = null;
                _columnSpec.Format = null;
                _columnSpec.Literal = null;
            }
            else if (IsLiteralMode)
            {
                _columnSpec.Literal = Literal;
                _columnSpec.PropertyPaths = null;
                _columnSpec.Format = null;
                _columnSpec.Expression = null;
            }

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
            _joinWith = _columnSpec.JoinWith ?? "\n";
            _format = _columnSpec.Format ?? string.Empty;
            _expression = _columnSpec.Expression ?? string.Empty;
            _numberFormat = _columnSpec.NumberFormat;
            _literal = _columnSpec.Literal ?? string.Empty;

            // Load PropertyPaths if present
            if (_columnSpec.PropertyPaths != null && _columnSpec.PropertyPaths.Length > 0)
            {
                foreach (var path in _columnSpec.PropertyPaths)
                {
                    PropertyPaths.Add(path);
                }
            }

            // Determine content mode based on what's populated in the spec
            if (!string.IsNullOrWhiteSpace(_columnSpec.Literal))
            {
                IsLiteralMode = true;
            }
            else if (!string.IsNullOrWhiteSpace(_columnSpec.Expression))
            {
                IsExpressionMode = true;
            }
            else if (!string.IsNullOrWhiteSpace(_columnSpec.Format))
            {
                IsFormatMode = true;
            }
            else if (_columnSpec.PropertyPaths != null && _columnSpec.PropertyPaths.Length > 0)
            {
                IsPropertyPathMode = true;
            }
            else
            {
                // Default to PropertyPath mode for new columns
                IsPropertyPathMode = true;
            }

            RaisePropertyChanged(string.Empty); // Refresh all bindings
        }

        #endregion
    }
}
