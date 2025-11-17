using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EasyAF.Core.Contracts;
using EasyAF.Import;
using Serilog;

namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents a mapping document (.ezmap) that defines associations between 
    /// source file columns and target data type properties.
    /// </summary>
    /// <remarks>
    /// This is the in-memory representation of a .ezmap file. It tracks all mappings,
    /// referenced sample files, and metadata. The document implements INotifyPropertyChanged
    /// to support WPF data binding and IDocument to integrate with the Shell's document management.
    /// </remarks>
    public class MapDocument : IDocument, INotifyPropertyChanged
    {
        private string? _filePath;
        private bool _isDirty;
        private string _mapName = "Untitled Map";
        private string _softwareVersion = "3.0.0";
        private string _description = string.Empty;
        private DateTime _dateModified = DateTime.Now;
        private IDocumentModule? _ownerModule;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the ViewModel for this document.
        /// Used by the Shell to display the document's UI.
        /// </summary>
        public object? ViewModel { get; set; }

        #region IDocument Implementation

        /// <summary>
        /// Gets or sets the file path where this document is saved.
        /// Null if the document has never been saved.
        /// </summary>
        public string? FilePath 
        { 
            get => _filePath;
            set 
            { 
                _filePath = value; 
                OnPropertyChanged(nameof(FilePath)); 
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Gets the display title for this document (shown in tabs).
        /// </summary>
        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(FilePath))
                    return System.IO.Path.GetFileNameWithoutExtension(FilePath);
                return string.IsNullOrEmpty(_mapName) ? "Untitled Map" : _mapName;
            }
        }

        /// <summary>
        /// Gets or sets whether the document has unsaved changes.
        /// </summary>
        public bool IsDirty 
        { 
            get => _isDirty;
            set 
            { 
                _isDirty = value; 
                OnPropertyChanged(nameof(IsDirty)); 
            }
        }

        /// <summary>
        /// Gets or sets the module that owns this document.
        /// </summary>
        public IDocumentModule OwnerModule
        {
            get => _ownerModule ?? throw new InvalidOperationException("OwnerModule not set");
            set => _ownerModule = value;
        }

        /// <summary>
        /// Marks the document as having unsaved changes.
        /// </summary>
        public void MarkDirty()
        {
            IsDirty = true;
            DateModified = DateTime.Now;
        }

        /// <summary>
        /// Marks the document as saved (clears dirty flag).
        /// </summary>
        public void MarkClean()
        {
            IsDirty = false;
        }

        #endregion

        #region Map Metadata

        /// <summary>
        /// Gets the display name for internal use (different from Title).
        /// </summary>
        public string DisplayName => Title;

        /// <summary>
        /// Gets or sets the name of this mapping configuration.
        /// </summary>
        public string MapName
        {
            get => _mapName;
            set 
            { 
                _mapName = value; 
                MarkDirty(); 
                OnPropertyChanged(nameof(MapName)); 
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(DisplayName)); 
            }
        }

        /// <summary>
        /// Gets or sets the EasyPower software version this map is designed for.
        /// </summary>
        /// <remarks>
        /// Different versions of EasyPower may use different column names,
        /// so maps are version-specific.
        /// </remarks>
        public string SoftwareVersion
        {
            get => _softwareVersion;
            set 
            { 
                _softwareVersion = value; 
                MarkDirty(); 
                OnPropertyChanged(nameof(SoftwareVersion)); 
            }
        }

        /// <summary>
        /// Gets or sets an optional long-form description of this mapping.
        /// </summary>
        public string Description
        {
            get => _description;
            set 
            { 
                _description = value; 
                MarkDirty(); 
                OnPropertyChanged(nameof(Description)); 
            }
        }

        /// <summary>
        /// Gets the date this document was last modified.
        /// </summary>
        /// <remarks>
        /// Updated automatically whenever the document is marked dirty.
        /// </remarks>
        public DateTime DateModified
        {
            get => _dateModified;
            private set 
            { 
                _dateModified = value; 
                OnPropertyChanged(nameof(DateModified)); 
            }
        }

        #endregion

        #region Mapping Data

        /// <summary>
        /// Gets the mappings organized by data type (e.g., "Bus", "LVCB", "ArcFlash").
        /// </summary>
        /// <remarks>
        /// Key = data type name (e.g., "Bus")
        /// Value = list of mapping entries for that data type
        /// </remarks>
        public Dictionary<string, List<MappingEntry>> MappingsByDataType { get; } = new();

        /// <summary>
        /// Gets the table references by data type, tracking which source table was used for each data type's mappings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Key = data type name (e.g., "Bus", "LVCB")
        /// Value = table identifier string (e.g., "Table1 - Sheet1 (Sample_Export.csv)")
        /// </para>
        /// <para>
        /// This allows the Map Editor to restore the user's table selection when reopening a saved map,
        /// providing better context about where each mapping came from.
        /// </para>
        /// </remarks>
        public Dictionary<string, string> TableReferencesByDataType { get; } = new();

        /// <summary>
        /// Gets the list of sample files referenced by this mapping.
        /// </summary>
        /// <remarks>
        /// These files are used to extract column names and provide sample data
        /// during the mapping process.
        /// </remarks>
        public List<ReferencedFile> ReferencedFiles { get; } = new();

        #endregion

        #region Mapping Operations

        /// <summary>
        /// Adds a mapping entry for a specific data type.
        /// </summary>
        /// <param name="dataType">The data type (e.g., "Bus", "LVCB").</param>
        /// <param name="entry">The mapping entry to add.</param>
        public void AddMapping(string dataType, MappingEntry entry)
        {
            if (!MappingsByDataType.ContainsKey(dataType))
                MappingsByDataType[dataType] = new List<MappingEntry>();
            
            MappingsByDataType[dataType].Add(entry);
            MarkDirty();
        }

        /// <summary>
        /// Removes a mapping for a specific property.
        /// </summary>
        /// <param name="dataType">The data type containing the mapping.</param>
        /// <param name="propertyName">The property name to unmap.</param>
        public void RemoveMapping(string dataType, string propertyName)
        {
            if (MappingsByDataType.TryGetValue(dataType, out var entries))
            {
                entries.RemoveAll(e => e.PropertyName == propertyName);
                MarkDirty();
            }
        }

        /// <summary>
        /// Clears all mappings for a specific data type.
        /// </summary>
        /// <param name="dataType">The data type to clear.</param>
        public void ClearMappings(string dataType)
        {
            if (MappingsByDataType.ContainsKey(dataType))
            {
                MappingsByDataType[dataType].Clear();
                MarkDirty();
            }
        }

        /// <summary>
        /// Updates an existing mapping or adds it if it doesn't exist.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="columnHeader">The column header to map to.</param>
        public void UpdateMapping(string dataType, string propertyName, string columnHeader)
        {
            if (!MappingsByDataType.TryGetValue(dataType, out var entries))
            {
                entries = new List<MappingEntry>();
                MappingsByDataType[dataType] = entries;
            }

            var existing = entries.FirstOrDefault(e => e.PropertyName == propertyName);
            if (existing != null)
            {
                existing.ColumnHeader = columnHeader;
            }
            else
            {
                entries.Add(new MappingEntry
                {
                    TargetType = dataType,
                    PropertyName = propertyName,
                    ColumnHeader = columnHeader,
                    Required = false,
                    Severity = MappingSeverity.Info
                });
            }
            MarkDirty();
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Converts this MapDocument to a MappingConfig for serialization.
        /// </summary>
        /// <returns>A MappingConfig instance ready to be saved as JSON.</returns>
        public MappingConfig ToMappingConfig()
        {
            var config = new MappingConfig
            {
                SoftwareVersion = SoftwareVersion,
                MapVersion = "1.0",
                ImportMap = new List<MappingEntry>()
            };

            // Flatten all mappings from all data types into a single list
            foreach (var kvp in MappingsByDataType)
            {
                config.ImportMap.AddRange(kvp.Value);
            }

            return config;
        }

        /// <summary>
        /// Creates a MapDocument from a loaded MappingConfig.
        /// </summary>
        /// <param name="config">The MappingConfig loaded from a .ezmap file.</param>
        /// <param name="filePath">Optional file path where the config was loaded from.</param>
        /// <returns>A MapDocument instance populated with the config data.</returns>
        public static MapDocument FromMappingConfig(MappingConfig config, string? filePath = null)
        {
            var doc = new MapDocument
            {
                SoftwareVersion = config.SoftwareVersion,
                FilePath = filePath,
                IsDirty = false
            };

            // Group mappings by data type
            foreach (var entry in config.ImportMap)
            {
                if (!doc.MappingsByDataType.ContainsKey(entry.TargetType))
                    doc.MappingsByDataType[entry.TargetType] = new List<MappingEntry>();
                
                doc.MappingsByDataType[entry.TargetType].Add(entry);
            }

            return doc;
        }

        #endregion

        #region File Validation

        /// <summary>
        /// Validates all referenced files and updates their status.
        /// </summary>
        /// <returns>A list of missing file paths, or empty list if all files are valid.</returns>
        /// <remarks>
        /// <para>
        /// This method checks each referenced file's existence and accessibility.
        /// Status is updated to one of:
        /// - "Valid": File exists and is accessible
        /// - "Missing": File does not exist (moved/deleted)
        /// - "Inaccessible": File exists but cannot be read (permissions, locked, etc.)
        /// </para>
        /// <para>
        /// Should be called after loading a document or when file references change.
        /// </para>
        /// </remarks>
        public List<string> ValidateReferencedFiles()
        {
            var missingFiles = new List<string>();

            foreach (var file in ReferencedFiles)
            {
                try
                {
                    if (!File.Exists(file.FilePath))
                    {
                        file.Status = "Missing";
                        missingFiles.Add(file.FilePath);
                        Log.Warning("Referenced file not found: {Path}", file.FilePath);
                    }
                    else
                    {
                        // Try to open the file to verify it's accessible
                        using (var stream = File.OpenRead(file.FilePath))
                        {
                            file.Status = "Valid";
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    file.Status = "Inaccessible";
                    Log.Warning("Referenced file is inaccessible (permissions): {Path}", file.FilePath);
                }
                catch (IOException)
                {
                    file.Status = "Inaccessible";
                    Log.Warning("Referenced file is inaccessible (locked or in use): {Path}", file.FilePath);
                }
                catch (Exception ex)
                {
                    file.Status = "Inaccessible";
                    Log.Warning(ex, "Error accessing referenced file: {Path}", file.FilePath);
                }
            }

            if (missingFiles.Count > 0)
            {
                Log.Information("Validation found {Count} missing referenced files", missingFiles.Count);
            }
            else
            {
                Log.Debug("All {Count} referenced files are valid", ReferencedFiles.Count);
            }

            return missingFiles;
        }

        /// <summary>
        /// Updates the file path for a referenced file (used when user relocates a file).
        /// </summary>
        /// <param name="oldPath">The old file path to replace.</param>
        /// <param name="newPath">The new file path.</param>
        /// <returns>True if the file was found and updated; false otherwise.</returns>
        public bool UpdateReferencedFilePath(string oldPath, string newPath)
        {
            var file = ReferencedFiles.FirstOrDefault(f => 
                string.Equals(f.FilePath, oldPath, StringComparison.OrdinalIgnoreCase));

            if (file == null)
            {
                Log.Warning("Could not find referenced file to update: {OldPath}", oldPath);
                return false;
            }

            file.FilePath = newPath;
            file.FileName = Path.GetFileName(newPath);
            file.Status = File.Exists(newPath) ? "Valid" : "Missing";
            
            MarkDirty();
            Log.Information("Updated referenced file path: {OldPath} -> {NewPath}", oldPath, newPath);
            
            return true;
        }

        /// <summary>
        /// Removes a referenced file from the document.
        /// </summary>
        /// <param name="filePath">The file path to remove.</param>
        /// <returns>True if the file was found and removed; false otherwise.</returns>
        /// <remarks>
        /// <para>
        /// WARNING: This does not remove mappings that reference columns from this file.
        /// Those mappings will become orphaned and may cause issues.
        /// Consider using RemoveReferencedFileAndCleanup() instead.
        /// </para>
        /// </remarks>
        public bool RemoveReferencedFile(string filePath)
        {
            var file = ReferencedFiles.FirstOrDefault(f => 
                string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

            if (file == null)
            {
                Log.Warning("Could not find referenced file to remove: {Path}", filePath);
                return false;
            }

            ReferencedFiles.Remove(file);
            MarkDirty();
            Log.Information("Removed referenced file: {Path}", filePath);
            
            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Raises the PropertyChanged event for data binding.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Represents a reference to a sample file used for mapping.
    /// </summary>
    /// <remarks>
    /// Sample files are CSV or Excel files that contain the actual data structure
    /// being mapped. They're used to extract column names and provide sample data.
    /// </remarks>
    public class ReferencedFile
    {
        /// <summary>
        /// Gets or sets the file name (without path).
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full file path.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the file (e.g., "Available", "Missing", "Locked").
        /// </summary>
        public string Status { get; set; } = "Unknown";

        /// <summary>
        /// Gets or sets when this file was last accessed for mapping.
        /// </summary>
        public DateTime LastAccessed { get; set; }
    }
}
