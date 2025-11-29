using System;
using System.Collections.ObjectModel;
using EasyAF.Engine;
using Prism.Mvvm;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// ViewModel wrapper for FilterSpec DTO.
    /// Provides UI-friendly properties and operator display logic.
    /// </summary>
    public class FilterSpecViewModel : BindableBase
    {
        private readonly FilterSpec _filterSpec;
        private readonly Action _onChanged;
        private int _ruleNumber;

        public FilterSpecViewModel(FilterSpec filterSpec, Action onChanged, int ruleNumber = 0)
        {
            _filterSpec = filterSpec ?? throw new ArgumentNullException(nameof(filterSpec));
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _ruleNumber = ruleNumber;
        }

        /// <summary>
        /// Gets the rule number for display (1-based index).
        /// </summary>
        public int RuleNumber
        {
            get => _ruleNumber;
            set => SetProperty(ref _ruleNumber, value);
        }

        /// <summary>
        /// Gets or sets the property path to filter on (e.g., "TripUnit.Adjustable").
        /// </summary>
        public string PropertyPath
        {
            get => _filterSpec.PropertyPath;
            set
            {
                if (_filterSpec.PropertyPath != value)
                {
                    _filterSpec.PropertyPath = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Summary));
                    Log.Debug("FilterSpec PropertyPath changed to: {Path}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter operator (eq, neq, gt, lt, gte, lte, contains).
        /// </summary>
        public string Operator
        {
            get => _filterSpec.Operator;
            set
            {
                if (_filterSpec.Operator != value)
                {
                    _filterSpec.Operator = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Summary));
                    RaisePropertyChanged(nameof(OperatorDisplay));
                    Log.Debug("FilterSpec Operator changed to: {Operator}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the literal value to compare against.
        /// </summary>
        public string? Value
        {
            get => _filterSpec.Value;
            set
            {
                if (_filterSpec.Value != value)
                {
                    _filterSpec.Value = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Summary));
                    RaisePropertyChanged(nameof(IsValueMode));
                    RaisePropertyChanged(nameof(IsPropertyCompareMode));
                    Log.Debug("FilterSpec Value changed to: {Value}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the right-hand property path for property-to-property comparison.
        /// </summary>
        public string? RightPropertyPath
        {
            get => _filterSpec.RightPropertyPath;
            set
            {
                if (_filterSpec.RightPropertyPath != value)
                {
                    _filterSpec.RightPropertyPath = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Summary));
                    RaisePropertyChanged(nameof(IsValueMode));
                    RaisePropertyChanged(nameof(IsPropertyCompareMode));
                    RaisePropertyChanged(nameof(CompareToDisplay));
                    Log.Debug("FilterSpec RightPropertyPath changed to: {Path}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this is a numeric comparison.
        /// </summary>
        public bool Numeric
        {
            get => _filterSpec.Numeric;
            set
            {
                if (_filterSpec.Numeric != value)
                {
                    _filterSpec.Numeric = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Summary));
                    Log.Debug("FilterSpec Numeric changed to: {Numeric}", value);
                }
            }
        }

        /// <summary>
        /// Gets whether this filter uses a literal value (not property comparison).
        /// </summary>
        public bool IsValueMode => string.IsNullOrWhiteSpace(RightPropertyPath);

        /// <summary>
        /// Gets whether this filter uses property-to-property comparison.
        /// </summary>
        public bool IsPropertyCompareMode => !string.IsNullOrWhiteSpace(RightPropertyPath);

        /// <summary>
        /// Gets the human-readable operator display text.
        /// </summary>
        public string OperatorDisplay => Operator switch
        {
            "eq" => "equals (=)",
            "neq" => "not equals (!=)",
            "gt" => "greater than (>)",
            "lt" => "less than (<)",
            "gte" => "greater than or equal (>=)",
            "lte" => "less than or equal (<=)",
            "contains" => "contains",
            _ => Operator
        };

        /// <summary>
        /// Gets the display text for the "Compare To" button/field.
        /// </summary>
        public string CompareToDisplay => IsPropertyCompareMode 
            ? RightPropertyPath ?? "(none)" 
            : "(none)";

        /// <summary>
        /// Gets a summary display of this filter (e.g., "TripUnit.Adjustable = true").
        /// </summary>
        public string Summary
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PropertyPath))
                    return "(empty filter)";

                var op = Operator switch
                {
                    "eq" => "=",
                    "neq" => "!=",
                    "gt" => ">",
                    "lt" => "<",
                    "gte" => ">=",
                    "lte" => "<=",
                    "contains" => "contains",
                    _ => Operator
                };

                var rightSide = IsPropertyCompareMode 
                    ? RightPropertyPath 
                    : (Value ?? "(empty)");

                return $"{PropertyPath} {op} {rightSide}";
            }
        }

        /// <summary>
        /// Gets the underlying FilterSpec DTO.
        /// </summary>
        public FilterSpec FilterSpec => _filterSpec;

        /// <summary>
        /// Refreshes all properties to update UI after external changes to FilterSpec.
        /// </summary>
        public void RefreshProperties()
        {
            RaisePropertyChanged(nameof(PropertyPath));
            RaisePropertyChanged(nameof(Operator));
            RaisePropertyChanged(nameof(Value));
            RaisePropertyChanged(nameof(RightPropertyPath));
            RaisePropertyChanged(nameof(Numeric));
            RaisePropertyChanged(nameof(Summary));
            RaisePropertyChanged(nameof(OperatorDisplay));
            RaisePropertyChanged(nameof(CompareToDisplay));
            RaisePropertyChanged(nameof(IsValueMode));
            RaisePropertyChanged(nameof(IsPropertyCompareMode));
        }
    }

    /// <summary>
    /// Represents a filter operator option for dropdown binding.
    /// </summary>
    public class OperatorOption
    {
        public string Value { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public string DisplayName => Display; // Alias for XAML binding

        public OperatorOption() { }

        public OperatorOption(string value, string display)
        {
            Value = value;
            Display = display;
        }

        /// <summary>
        /// Gets the standard operator options.
        /// </summary>
        public static ObservableCollection<OperatorOption> GetStandardOperators()
        {
            return new ObservableCollection<OperatorOption>
            {
                new("eq", "equals (=)"),
                new("neq", "not equals (!=)"),
                new("gt", "greater than (>)"),
                new("lt", "less than (<)"),
                new("gte", "greater than or equal (>=)"),
                new("lte", "less than or equal (<=)"),
                new("contains", "contains")
            };
        }
    }
}
