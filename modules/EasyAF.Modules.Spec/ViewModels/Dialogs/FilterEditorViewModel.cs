using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using EasyAF.Engine;
using EasyAF.Modules.Spec.Models;
using EasyAF.Modules.Map.Services;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels.Dialogs
{
    /// <summary>
    /// ViewModel for the Filter Editor dialog.
    /// </summary>
    /// <remarks>
    /// This dialog allows editing all properties of a FilterSpec:
    /// - PropertyPath (with picker)
    /// - Operator (dropdown)
    /// - Value (literal) OR RightPropertyPath (property comparison)
    /// - Numeric flag
    /// - IgnoreCase flag
    /// </remarks>
    public class FilterEditorViewModel : BindableBase
    {
        private readonly FilterSpec _filterSpec;
        private readonly SpecDocument _document;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly ISettingsService _settingsService;
        private readonly Action _onFilterChanged;

        private string _propertyPath;
        private string _operator;
        private string? _value;
        private string? _rightPropertyPath;
        private bool _numeric;
        private bool _ignoreCase;
        private bool _isValueMode = true;

        /// <summary>
        /// Initializes a new instance of the FilterEditorViewModel.
        /// </summary>
        public FilterEditorViewModel(
            FilterSpec filterSpec,
            SpecDocument document,
            IPropertyDiscoveryService propertyDiscovery,
            ISettingsService settingsService,
            Action onFilterChanged)
        {
            _filterSpec = filterSpec ?? throw new ArgumentNullException(nameof(filterSpec));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _onFilterChanged = onFilterChanged ?? throw new ArgumentNullException(nameof(onFilterChanged));

            // Initialize available operators
            AvailableOperators = new ObservableCollection<OperatorOption>(OperatorOption.GetStandardOperators());

            // Load current values from FilterSpec
            _propertyPath = filterSpec.PropertyPath ?? string.Empty;
            _operator = filterSpec.Operator ?? "eq";
            _value = filterSpec.Value;
            _rightPropertyPath = filterSpec.RightPropertyPath;
            _numeric = filterSpec.Numeric;
            _ignoreCase = filterSpec.IgnoreCase;

            // Determine mode (value vs property comparison)
            _isValueMode = string.IsNullOrWhiteSpace(_rightPropertyPath);

            // Initialize commands
            BrowsePropertyCommand = new DelegateCommand(ExecuteBrowseProperty);
            BrowseRightPropertyCommand = new DelegateCommand(ExecuteBrowseRightProperty);
            OkCommand = new DelegateCommand(ExecuteOk, CanExecuteOk);
            CancelCommand = new DelegateCommand(ExecuteCancel);

            // Watch for changes to update summary
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(Summary))
                {
                    RaisePropertyChanged(nameof(Summary));
                }
                
                if (e.PropertyName == nameof(PropertyPath) ||
                    e.PropertyName == nameof(Operator) ||
                    e.PropertyName == nameof(Value) ||
                    e.PropertyName == nameof(RightPropertyPath))
                {
                    (OkCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            };
        }

        #region Properties

        public ObservableCollection<OperatorOption> AvailableOperators { get; }

        public string PropertyPath
        {
            get => _propertyPath;
            set => SetProperty(ref _propertyPath, value);
        }

        public string Operator
        {
            get => _operator;
            set => SetProperty(ref _operator, value);
        }

        public string? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string? RightPropertyPath
        {
            get => _rightPropertyPath;
            set => SetProperty(ref _rightPropertyPath, value);
        }

        public bool Numeric
        {
            get => _numeric;
            set
            {
                if (SetProperty(ref _numeric, value))
                {
                    // If switching to numeric, disable IgnoreCase
                    if (value)
                    {
                        IgnoreCase = false;
                    }
                }
            }
        }

        public bool IgnoreCase
        {
            get => _ignoreCase;
            set => SetProperty(ref _ignoreCase, value);
        }

        public bool IsValueMode
        {
            get => _isValueMode;
            set
            {
                if (SetProperty(ref _isValueMode, value))
                {
                    RaisePropertyChanged(nameof(IsPropertyMode));
                    
                    // Clear the unused field
                    if (value)
                    {
                        RightPropertyPath = null;
                    }
                    else
                    {
                        Value = null;
                    }
                }
            }
        }

        public bool IsPropertyMode
        {
            get => !_isValueMode;
            set => IsValueMode = !value;
        }

        /// <summary>
        /// Gets a human-readable summary of the filter rule.
        /// </summary>
        public string Summary
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PropertyPath))
                {
                    return "No property selected.";
                }

                var opDisplay = AvailableOperators.FirstOrDefault(o => o.Value == Operator)?.DisplayName ?? Operator;
                
                string rightSide;
                if (IsValueMode)
                {
                    rightSide = string.IsNullOrWhiteSpace(Value) ? "(empty)" : $"\"{Value}\"";
                }
                else
                {
                    rightSide = string.IsNullOrWhiteSpace(RightPropertyPath) 
                        ? "(no property selected)" 
                        : RightPropertyPath;
                }

                var typeFlag = Numeric ? " (numeric)" : (IgnoreCase ? " (ignore case)" : "");
                
                return $"{PropertyPath} {opDisplay} {rightSide}{typeFlag}";
            }
        }

        public bool? DialogResult { get; private set; }

        #endregion

        #region Commands

        public ICommand BrowsePropertyCommand { get; }
        public ICommand BrowseRightPropertyCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteBrowseProperty()
        {
            try
            {
                var currentPath = string.IsNullOrWhiteSpace(PropertyPath)
                    ? Array.Empty<string>()
                    : new[] { PropertyPath };

                var viewModel = new PropertyPathPickerViewModel(currentPath, _document, _propertyDiscovery, _settingsService);
                var dialog = new Views.Dialogs.PropertyPathPickerDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    PropertyPath = viewModel.ResultPaths.FirstOrDefault() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open property path picker for filter property");
            }
        }

        private void ExecuteBrowseRightProperty()
        {
            try
            {
                var currentPath = string.IsNullOrWhiteSpace(RightPropertyPath)
                    ? Array.Empty<string>()
                    : new[] { RightPropertyPath };

                var viewModel = new PropertyPathPickerViewModel(currentPath, _document, _propertyDiscovery, _settingsService);
                var dialog = new Views.Dialogs.PropertyPathPickerDialog
                {
                    DataContext = viewModel,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    var selectedPath = viewModel.ResultPaths.FirstOrDefault();
                    RightPropertyPath = string.IsNullOrWhiteSpace(selectedPath) ? null : selectedPath;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open property path picker for compare-to property");
            }
        }

        private bool CanExecuteOk()
        {
            // Must have a property path
            if (string.IsNullOrWhiteSpace(PropertyPath))
                return false;

            // Must have either a value OR a right property path
            if (IsValueMode)
            {
                // Value mode: Value can be empty for some operators (like "eq" for empty check)
                return true;
            }
            else
            {
                // Property mode: must have a right property path
                return !string.IsNullOrWhiteSpace(RightPropertyPath);
            }
        }

        private void ExecuteOk()
        {
            // Update the FilterSpec
            _filterSpec.PropertyPath = PropertyPath;
            _filterSpec.Operator = Operator;
            _filterSpec.Value = IsValueMode ? Value : null;
            _filterSpec.RightPropertyPath = IsPropertyMode ? RightPropertyPath : null;
            _filterSpec.Numeric = Numeric;
            _filterSpec.IgnoreCase = IgnoreCase;

            // Notify parent that filter changed
            _onFilterChanged?.Invoke();

            DialogResult = true;

            Log.Information("Filter updated: {Summary}", Summary);

            // Close the dialog
            CloseDialog();
        }

        private void ExecuteCancel()
        {
            DialogResult = false;
            CloseDialog();
        }

        private void CloseDialog()
        {
            // Find the window and close it
            var window = System.Windows.Application.Current.Windows.OfType<Views.Dialogs.FilterEditorDialog>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }

        #endregion
    }
}
