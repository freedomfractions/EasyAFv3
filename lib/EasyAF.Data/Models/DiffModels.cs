using System.Collections.Generic;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Type of change detected.
    /// </summary>
    public enum ChangeType
    {
        Unchanged,
        Added,
        Removed,
        Modified
    }

    /// <summary>
    /// Represents a single property-level change.
    /// </summary>
    public class PropertyChange
    {
        public string PropertyPath { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public ChangeType ChangeType { get; set; }
    }

    /// <summary>
    /// Represents changes for a single dictionary entry (device/row).
    /// </summary>
    public class EntryDiff
    {
        public string EntryKey { get; set; } = string.Empty;
        public string EntryType { get; set; } = string.Empty;
        public ChangeType ChangeType { get; set; }
        public List<PropertyChange> PropertyChanges { get; set; } = new List<PropertyChange>();
    }

    /// <summary>
    /// Aggregated diff for a DataSet.
    /// </summary>
    public class DataSetDiff
    {
        public List<EntryDiff> EntryDiffs { get; set; } = new List<EntryDiff>();
        public int AddedCount => EntryDiffs.FindAll(e => e.ChangeType == ChangeType.Added).Count;
        public int RemovedCount => EntryDiffs.FindAll(e => e.ChangeType == ChangeType.Removed).Count;
        public int ModifiedCount => EntryDiffs.FindAll(e => e.ChangeType == ChangeType.Modified).Count;
    }

    /// <summary>
    /// Aggregated diff for a Project, including metadata changes and dataset diffs.
    /// </summary>
    public class ProjectDiff
    {
        /// <summary>Property-level changes for top-level project metadata.</summary>
        public List<PropertyChange> ProjectPropertyChanges { get; set; } = new List<PropertyChange>();

        /// <summary>Diff between OldData and NewData.</summary>
        public DataSetDiff DataDiff { get; set; } = new DataSetDiff();
    }
}
