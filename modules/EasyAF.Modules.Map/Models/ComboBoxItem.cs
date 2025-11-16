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
    }

    /// <summary>
    /// Represents a file header in the ComboBox (non-selectable).
    /// </summary>
    public class FileHeaderItem : ComboBoxItemBase
    {
        public string FileName { get; set; } = string.Empty;

        public override bool IsSelectable => false;

        public override string DisplayText => FileName;
    }

    /// <summary>
    /// Represents a selectable table item in the ComboBox.
    /// </summary>
    public class TableItem : ComboBoxItemBase
    {
        public TableItem(TableReference tableReference)
        {
            TableReference = tableReference;
        }

        public TableReference TableReference { get; set; }

        public override bool IsSelectable => true;

        /// <summary>
        /// Gets the display text - shows filename for single-table files, table name for multi-table files.
        /// </summary>
        public override string DisplayText => TableReference.IsMultiTableFile 
            ? TableReference.TableName 
            : TableReference.FileName;

        /// <summary>
        /// Gets whether this table should be indented (from a multi-table file).
        /// </summary>
        public bool ShouldIndent => TableReference.IsMultiTableFile;
    }
}
