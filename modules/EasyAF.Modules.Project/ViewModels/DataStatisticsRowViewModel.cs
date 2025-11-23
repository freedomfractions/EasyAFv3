using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// View model for a single row in the data statistics table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports a two-level hierarchy:
    /// - Root rows: Data types (e.g., "Arc Flash", "Buses")
    /// - Child rows: Scenarios (e.g., "Main-Max", "Emergency")
    /// </para>
    /// <para>
    /// <strong>Display Rules:</strong>
    /// - Zero values display as em-dash (—)
    /// - Delta shows +/- changes or em-dash for no change
    /// - Scenarios can be expanded/collapsed
    /// </para>
    /// </remarks>
    public class DataStatisticsRowViewModel : BindableBase
    {
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isNewCountHighlighted;
        private bool _isOldCountHighlighted;

        /// <summary>
        /// Gets or sets the display name of the data type or scenario.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data type name (e.g., "ArcFlash", "Bus").
        /// </summary>
        public string DataTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scenario name (null for root-level data type rows).
        /// </summary>
        public string? ScenarioName { get; set; }

        /// <summary>
        /// Gets or sets the count in New Data.
        /// </summary>
        public int NewCount { get; set; }

        /// <summary>
        /// Gets or sets the count in Old Data.
        /// </summary>
        public int OldCount { get; set; }

        /// <summary>
        /// Gets the delta (change) between New and Old data.
        /// </summary>
        public int Delta => NewCount - OldCount;

        /// <summary>
        /// Gets the formatted New Data count (em-dash for 0).
        /// </summary>
        public string NewCountDisplay => NewCount == 0 ? "—" : NewCount.ToString();

        /// <summary>
        /// Gets the formatted Old Data count (em-dash for 0).
        /// </summary>
        public string OldCountDisplay => OldCount == 0 ? "—" : OldCount.ToString();

        /// <summary>
        /// Gets the formatted Delta display (+/- or em-dash).
        /// </summary>
        public string DeltaDisplay
        {
            get
            {
                if (Delta == 0) return "—";
                if (Delta > 0) return $"+{Delta}";
                return Delta.ToString();
            }
        }

        /// <summary>
        /// Gets the foreground brush for Delta based on value.
        /// </summary>
        public string DeltaForeground
        {
            get
            {
                if (Delta > 0) return "SuccessBrush";
                if (Delta < 0) return "ErrorBrush";
                return "TextSecondaryBrush";
            }
        }

        /// <summary>
        /// Gets the font weight for Delta (SemiBold if changed, Normal if unchanged).
        /// </summary>
        public string DeltaFontWeight
        {
            get
            {
                return Delta != 0 ? "SemiBold" : "Normal";
            }
        }

        /// <summary>
        /// Gets whether this row has child scenarios.
        /// </summary>
        public bool HasChildren => Children.Count > 0;

        /// <summary>
        /// Gets the child rows (scenarios).
        /// </summary>
        public ObservableCollection<DataStatisticsRowViewModel> Children { get; } = new();

        /// <summary>
        /// Gets or sets whether this row is expanded (showing children).
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Gets or sets whether this row is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Gets or sets whether the New Count cell should be highlighted (after import).
        /// </summary>
        public bool IsNewCountHighlighted
        {
            get => _isNewCountHighlighted;
            set => SetProperty(ref _isNewCountHighlighted, value);
        }

        /// <summary>
        /// Gets or sets whether the Old Count cell should be highlighted (after import).
        /// </summary>
        public bool IsOldCountHighlighted
        {
            get => _isOldCountHighlighted;
            set => SetProperty(ref _isOldCountHighlighted, value);
        }

        /// <summary>
        /// Gets whether this is a root-level row (data type) vs. child row (scenario).
        /// </summary>
        public bool IsRootRow => ScenarioName == null;

        /// <summary>
        /// Gets the indentation level (0 for root, 1 for scenarios).
        /// </summary>
        public int IndentLevel => IsRootRow ? 0 : 1;

        /// <summary>
        /// Gets the expander visibility (visible only for rows with children).
        /// </summary>
        public bool ShowExpander => HasChildren;

        /// <summary>
        /// Gets whether scenarios have uniform distribution (all same count).
        /// </summary>
        /// <remarks>
        /// Used to show ?? warning indicator if counts are inconsistent.
        /// </remarks>
        public bool IsScenariosUniform { get; set; } = true;

        /// <summary>
        /// Gets the warning indicator text if scenarios are not uniform.
        /// </summary>
        public string? WarningIndicator => !IsScenariosUniform && HasChildren ? "\uE7BA" : null;

        /// <summary>
        /// Gets or sets whether this is the first child in its parent's collection.
        /// </summary>
        public bool IsFirstChild { get; set; }

        /// <summary>
        /// Gets or sets whether this is the last child in its parent's collection.
        /// </summary>
        public bool IsLastChild { get; set; }

        /// <summary>
        /// Gets whether this is a middle child (not first, not last).
        /// </summary>
        public bool IsMiddleChild => !IsRootRow && !IsFirstChild && !IsLastChild;
    }
}
