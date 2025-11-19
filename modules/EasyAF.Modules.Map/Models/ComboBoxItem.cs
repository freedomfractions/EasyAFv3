using Prism.Mvvm;

namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Base class for ComboBox items that can be headers or tables.
    /// </summary>
    public abstract class ComboBoxItemBase : BindableBase
    {
        /// <summary>
        /// Gets whether this item is selectable.
        /// </summary>
        public abstract bool IsSelectable { get; }

        /// <summary>
        /// Gets the display text for this item.
        /// </summary>
        public abstract string DisplayText { get; }
        
        /// <summary>
        /// Gets whether this item should be indented (for multi-table file children).
        /// </summary>
        public abstract bool ShouldIndent { get; }
    }

    /// <summary>
    /// Represents a file header in the ComboBox (non-selectable).
    /// </summary>
    public class FileHeaderItem : ComboBoxItemBase
    {
        public string FileName { get; set; } = string.Empty;

        public override bool IsSelectable => false;

        public override string DisplayText => FileName;
        
        /// <summary>
        /// Headers are never indented (they're top-level file names).
        /// </summary>
        public override bool ShouldIndent => false;
        
        /// <summary>
        /// Gets the "used by" status - always null for headers (only tables can be "used").
        /// </summary>
        /// <remarks>
        /// This property exists to prevent binding errors in the XAML tooltip.
        /// Headers are never "used" by tabs, so this is always null.
        /// </remarks>
        public string? UsedByDataType => null;
    }

    /// <summary>
    /// Represents a selectable table item in the ComboBox.
    /// </summary>
    public class TableItem : ComboBoxItemBase
    {
        private string? _usedByDataType;

        public TableItem(TableReference tableReference)
        {
            TableReference = tableReference;
        }

        public TableReference TableReference { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the data type that's currently using this table.
        /// </summary>
        /// <remarks>
        /// When set, indicates this table is assigned to another data type and should be disabled.
        /// Example: "LV Breakers" means the table is currently assigned to the LV Breakers tab.
        /// This is set dynamically when the dropdown opens, not persisted.
        /// </remarks>
        public string? UsedByDataType
        {
            get => _usedByDataType;
            set => SetProperty(ref _usedByDataType, value);
        }

        /// <summary>
        /// Gets whether this item is selectable.
        /// </summary>
        /// <remarks>
        /// A table is selectable only if it's not currently assigned to another data type.
        /// </remarks>
        public override bool IsSelectable => string.IsNullOrEmpty(_usedByDataType);

        /// <summary>
        /// Gets the display text - shows filename for single-table files, table name for multi-table files.
        /// </summary>
        public override string DisplayText => TableReference.IsMultiTableFile 
            ? TableReference.TableName 
            : TableReference.FileName;

        /// <summary>
        /// Gets whether this table should be indented (from a multi-table file).
        /// </summary>
        public override bool ShouldIndent => TableReference.IsMultiTableFile;
    }
}
