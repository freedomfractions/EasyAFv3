using Prism.Mvvm;
using System.IO;

namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents a table/sheet reference from a source file.
    /// </summary>
    /// <remarks>
    /// Used to track which file a table came from, enabling grouped ComboBox display.
    /// </remarks>
    public class TableReference : BindableBase
    {
        private string _tableName = string.Empty;
        private string _filePath = string.Empty;

        /// <summary>
        /// Gets or sets the name of the table/sheet (e.g., "Sheet1", "BusData").
        /// </summary>
        public string TableName
        {
            get => _tableName;
            set => SetProperty(ref _tableName, value);
        }

        /// <summary>
        /// Gets or sets the full path to the source file containing this table.
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// Gets the filename (without path) for display purposes.
        /// </summary>
        public string FileName => Path.GetFileName(FilePath);

        /// <summary>
        /// Gets a composite display name combining file and table (e.g., "data.xlsx ? Sheet1").
        /// </summary>
        public string DisplayName => $"{FileName} ? {TableName}";

        /// <summary>
        /// Returns a string representation of this table reference.
        /// </summary>
        public override string ToString() => DisplayName;
    }
}
