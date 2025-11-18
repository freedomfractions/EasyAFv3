namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents a column discovered in a source data file (CSV or Excel).
    /// </summary>
    /// <remarks>
    /// This class is used to display available columns from sample files in the mapping UI,
    /// allowing users to map them to target properties.
    /// </remarks>
    public class ColumnInfo
    {
        /// <summary>
        /// Gets or sets the column header name as it appears in the source file.
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the zero-based index of the column in the source file.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets the source table/sheet name (for Excel files with multiple sheets).
        /// </summary>
        /// <remarks>
        /// For CSV files, this is typically "Sheet1" or the filename.
        /// For Excel files, this is the actual sheet name.
        /// </remarks>
        public string SourceTable { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this column has been mapped to a target property.
        /// </summary>
        public bool IsMapped { get; set; }

        /// <summary>
        /// Gets or sets the target property this column is mapped to (if any).
        /// </summary>
        /// <remarks>
        /// Format: "DataType.PropertyName" (e.g., "Bus.Id", "LVCB.Manufacturer")
        /// </remarks>
        public string? MappedTo { get; set; }

        /// <summary>
        /// Gets or sets the number of non-empty values in this column (from sample data).
        /// </summary>
        /// <remarks>
        /// Used to help users identify which columns contain meaningful data.
        /// </remarks>
        public int SampleValueCount { get; set; }
    }
}
