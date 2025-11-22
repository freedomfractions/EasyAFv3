using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// View model for a tree node representing a data type or scenario.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This view model supports a two-level tree structure:
    /// </para>
    /// <para>
    /// <strong>Level 1: Data Type Nodes</strong>
    /// - Display name: "Arc Flash (160)"
    /// - Children: Scenario nodes
    /// - Icon: Based on whether it has scenarios (??) or not (?)
    /// </para>
    /// <para>
    /// <strong>Level 2: Scenario Nodes</strong>
    /// - Display name: "Main-Max (40)"
    /// - No children
    /// - Icon: None (leaf node)
    /// </para>
    /// <para>
    /// <strong>Example Tree Structure:</strong>
    /// </para>
    /// <code>
    /// ?? Arc Flash (160)
    ///   ?? Main-Max (40)
    ///   ?? Main-Min (40)
    ///   ?? Emergency (40)
    ///   ?? Generator (40)
    /// ? Buses (14)
    /// ? Breakers (9)
    /// </code>
    /// </remarks>
    public class DataTypeNodeViewModel : BindableBase
    {
        private bool _isExpanded;
        private bool _isSelected;

        /// <summary>
        /// Gets the display name of this node.
        /// </summary>
        /// <remarks>
        /// Format:
        /// - Data type: "{DataTypeName} ({TotalCount})" e.g., "Arc Flash (160)"
        /// - Scenario: "{ScenarioName} ({Count})" e.g., "Main-Max (40)"
        /// </remarks>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the data type name (e.g., "ArcFlash", "Bus").
        /// </summary>
        public string DataTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the scenario name (for scenario nodes), or null for data type nodes.
        /// </summary>
        public string? ScenarioName { get; set; }

        /// <summary>
        /// Gets the entry count for this node.
        /// </summary>
        /// <remarks>
        /// - For data type nodes: Total count across all scenarios
        /// - For scenario nodes: Count for that specific scenario
        /// </remarks>
        public int Count { get; set; }

        /// <summary>
        /// Gets whether this node has scenarios (is expandable).
        /// </summary>
        public bool HasScenarios => Children.Count > 0;

        /// <summary>
        /// Gets the child nodes (scenarios for a data type node).
        /// </summary>
        public ObservableCollection<DataTypeNodeViewModel> Children { get; set; } = new();

        /// <summary>
        /// Gets or sets whether this node is expanded in the tree.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Gets or sets whether this node is selected in the tree.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Gets whether this is a root-level node (data type) vs. child node (scenario).
        /// </summary>
        public bool IsRootNode => ScenarioName == null;

        /// <summary>
        /// Gets the icon to display for this node.
        /// </summary>
        /// <remarks>
        /// - ?? (scenario-based): Has child scenarios
        /// - ? (simple): No scenarios, just a count
        /// - null: Scenario node (no icon)
        /// </remarks>
        public string? Icon
        {
            get
            {
                if (!IsRootNode)
                    return null; // Scenario nodes don't have icons

                return HasScenarios ? "??" : "?";
            }
        }

        /// <summary>
        /// Gets whether scenarios have uniform distribution (all same count).
        /// </summary>
        /// <remarks>
        /// Used to show ?? warning indicator if counts are inconsistent.
        /// Only applicable to data type nodes with scenarios.
        /// </remarks>
        public bool IsScenariosUniform { get; set; } = true;

        /// <summary>
        /// Gets the warning indicator text if scenarios are not uniform.
        /// </summary>
        public string? WarningIndicator => !IsScenariosUniform && HasScenarios ? "??" : null;
    }
}
