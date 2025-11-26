using System;
using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// View model for a node in the import history tree.
    /// Represents either an import session (parent) or an individual file (child).
    /// </summary>
    public class ImportHistoryNodeViewModel : BindableBase
    {
        private bool _isExpanded;

        /// <summary>
        /// Gets or sets the display text for this node.
        /// </summary>
        /// <remarks>
        /// Format examples:
        /// - Parent: "Jan 15, 2025 2:30 PM - New Data (3 files, 450 entries)"
        /// - Child: "equipment_data.csv - Bus (120), ArcFlash (40)"
        /// </remarks>
        public string DisplayText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon to display for this node.
        /// Uses Segoe MDL2 Assets font glyphs.
        /// </summary>
        /// <remarks>
        /// Icon mapping:
        /// - Parent (New Data): \uE8B7 (Database icon)
        /// - Parent (Old Data): \uE823 (Archive icon)
        /// - Child (File): \uE8A5 (Document icon)
        /// </remarks>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tooltip text providing additional details.
        /// </summary>
        /// <remarks>
        /// Tooltip examples:
        /// - Parent: "Imported to New Data\nMapping: standard_mapping.ezmap\n450 total entries"
        /// - Child: "File: C:\data\equipment.csv\nData Types: Bus, ArcFlash\nScenarios: Main-Max ? Service-Max"
        /// </remarks>
        public string Tooltip { get; set; } = string.Empty;

        /// <summary>
        /// Gets whether this is a parent node (import session).
        /// </summary>
        public bool IsParent => Children.Count > 0;

        /// <summary>
        /// Gets whether this is a child node (individual file).
        /// </summary>
        public bool IsChild => Children.Count == 0;

        /// <summary>
        /// Gets or sets whether this node is expanded in the tree view.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Gets the collection of child nodes (files under this import session).
        /// Empty for child nodes.
        /// </summary>
        public ObservableCollection<ImportHistoryNodeViewModel> Children { get; } = new();

        /// <summary>
        /// Gets or sets the timestamp of the import session (parent nodes only).
        /// Used for sorting.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets whether this import was to New Data (true) or Old Data (false).
        /// Parent nodes only.
        /// </summary>
        public bool? IsNewData { get; set; }

        /// <summary>
        /// Gets or sets the mapping file path used for this import.
        /// Parent nodes only.
        /// </summary>
        public string? MappingPath { get; set; }

        /// <summary>
        /// Gets or sets the total number of entries imported in this session.
        /// Parent nodes only.
        /// </summary>
        public int? TotalEntries { get; set; }

        /// <summary>
        /// Gets or sets the full file path (child nodes only).
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Gets or sets the comma-separated list of data types in this file.
        /// Child nodes only.
        /// Example: "Bus, ArcFlash, ShortCircuit"
        /// </summary>
        public string? DataTypes { get; set; }

        /// <summary>
        /// Gets or sets the scenario mapping summary for this file.
        /// Child nodes only.
        /// Example: "Scenario1 ? ScenarioA, Scenario2 ? ScenarioB"
        /// </summary>
        public string? ScenarioMappings { get; set; }

        /// <summary>
        /// Gets or sets the number of entries imported from this file.
        /// Child nodes only.
        /// </summary>
        public int? FileEntryCount { get; set; }
    }
}
