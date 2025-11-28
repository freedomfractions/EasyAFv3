using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasyAF.Core.Contracts;
using EasyAF.Engine;
using Serilog;

namespace EasyAF.Modules.Spec.Models
{
    /// <summary>
    /// Document wrapper for EasyAF Spec files (.ezspec).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class wraps the existing <see cref="EasyAF.Engine.SpecFileRoot"/> class
    /// and implements the <see cref="IDocument"/> interface to integrate with the
    /// EasyAF shell's document management system.
    /// </para>
    /// <para>
    /// The SpecDocument provides:
    /// - Dirty state tracking for unsaved changes
    /// - File path management
    /// - Title display logic
    /// - Property change notifications (MVVM support)
    /// </para>
    /// <para>
    /// This is a lightweight wrapper - all actual spec data lives in the wrapped
    /// <see cref="SpecFileRoot"/> instance. The wrapper only adds shell integration metadata.
    /// </para>
    /// </remarks>
    public class SpecDocument : IDocument, INotifyPropertyChanged
    {
        private readonly SpecFileRoot _spec;
        private string? _filePath;
        private bool _isDirty;
        private IDocumentModule? _ownerModule;

        /// <summary>
        /// Initializes a new instance of the SpecDocument class.
        /// </summary>
        /// <param name="spec">The underlying SpecFileRoot instance to wrap.</param>
        /// <param name="filePath">Optional file path if this document was loaded from disk.</param>
        /// <exception cref="ArgumentNullException">If spec is null.</exception>
        public SpecDocument(SpecFileRoot spec, string? filePath = null)
        {
            _spec = spec ?? throw new ArgumentNullException(nameof(spec));
            _filePath = filePath;
            _isDirty = false;

            Log.Debug("SpecDocument created for: {FilePath}", filePath ?? "(new document)");
        }

        #region IDocument Implementation

        /// <summary>
        /// Gets or sets the file path where this document is stored.
        /// </summary>
        /// <remarks>
        /// Null if the document has never been saved (new document).
        /// Setting this property does NOT save the file - it only updates the path metadata.
        /// </remarks>
        public string? FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Title));
                    Log.Debug("SpecDocument FilePath changed to: {FilePath}", value);
                }
            }
        }

        /// <summary>
        /// Gets the display title for this document.
        /// </summary>
        /// <remarks>
        /// Title is derived from:
        /// 1. If FilePath is set: filename without extension
        /// 2. If FilePath is null: "Untitled Spec"
        /// </remarks>
        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath))
                    return "Untitled Spec";

                return System.IO.Path.GetFileNameWithoutExtension(_filePath);
            }
        }

        /// <summary>
        /// Gets or sets whether this document has unsaved changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// IsDirty tracking is managed by the SpecDocument wrapper:
        /// - Set to true when any property on the wrapped SpecFileRoot changes
        /// - Set to false after successful save
        /// - Prompts user before closing if true
        /// </para>
        /// <para>
        /// IMPORTANT: ViewModels must manually set IsDirty = true when making changes
        /// to the SpecFileRoot instance. Automatic change tracking is not implemented.
        /// </para>
        /// </remarks>
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                    Log.Debug("SpecDocument IsDirty changed to: {IsDirty}", value);
                }
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
        }

        /// <summary>
        /// Marks the document as saved (clears dirty flag).
        /// </summary>
        public void MarkClean()
        {
            IsDirty = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the underlying SpecFileRoot instance.
        /// </summary>
        /// <remarks>
        /// This exposes the wrapped SpecFileRoot for ViewModels to bind to.
        /// ViewModels should bind directly to Spec.* properties and
        /// manually call MarkDirty() when making changes.
        /// </remarks>
        public SpecFileRoot Spec => _spec;

        /// <summary>
        /// Gets or sets the ViewModel for this document.
        /// </summary>
        /// <remarks>
        /// Used by the Shell to display the document's UI via DataTemplate.
        /// The shell's ContentControl will bind to this property and use
        /// a DataTemplate to render the appropriate view.
        /// </remarks>
        public object? ViewModel { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed (auto-filled by compiler).</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates a new, empty SpecDocument.
        /// </summary>
        /// <returns>A new SpecDocument wrapping a default SpecFileRoot instance.</returns>
        public static SpecDocument CreateNew()
        {
            var spec = new SpecFileRoot
            {
                SpecVersion = "1.0",
                Tables = Array.Empty<TableSpec>(),
                PropertyMappings = null
            };

            var doc = new SpecDocument(spec);
            Log.Information("Created new SpecDocument");
            return doc;
        }

        /// <summary>
        /// Loads a SpecDocument from disk.
        /// </summary>
        /// <param name="filePath">Path to the .ezspec file to load.</param>
        /// <returns>A SpecDocument wrapping the loaded SpecFileRoot instance.</returns>
        /// <exception cref="ArgumentNullException">If filePath is null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException">If the file does not exist.</exception>
        /// <exception cref="InvalidOperationException">If deserialization fails.</exception>
        public static SpecDocument LoadFrom(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException($"Spec file not found: {filePath}", filePath);

            Log.Information("Loading SpecDocument from: {FilePath}", filePath);

            // Use SpecLoader to deserialize JSON
            var json = System.IO.File.ReadAllText(filePath);
            var loadResult = SpecLoader.LoadFromJson(json);
            
            if (loadResult.HasErrors)
            {
                var errorMsg = string.Join(Environment.NewLine, loadResult.Errors);
                throw new InvalidOperationException($"Failed to load spec file: {filePath}{Environment.NewLine}{errorMsg}");
            }

            // Create SpecFileRoot from loaded tables
            var spec = new SpecFileRoot
            {
                SpecVersion = loadResult.SpecVersion,
                SpecChecksum = loadResult.SpecChecksum,
                Tables = loadResult.Tables.Select(td => ConvertToTableSpec(td)).ToArray(),
                PropertyMappings = loadResult.PropertyMappings?.ToArray()
            };

            var doc = new SpecDocument(spec, filePath)
            {
                IsDirty = false // Just loaded, no unsaved changes
            };

            Log.Information("SpecDocument loaded successfully: {FilePath}", filePath);
            return doc;
        }

        /// <summary>
        /// Converts a TableDefinition (runtime) to a TableSpec (serialization).
        /// </summary>
        private static TableSpec ConvertToTableSpec(TableDefinition tableDef)
        {
            // TODO: Task 25 - Implement proper conversion
            // For now, create a basic TableSpec with minimal data
            return new TableSpec
            {
                Id = tableDef.Id,
                AltText = tableDef.AltText,
                Columns = Array.Empty<ColumnSpec>() // TODO: Convert columns
            };
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves the document to its current FilePath.
        /// </summary>
        /// <exception cref="InvalidOperationException">If FilePath is null (document has never been saved).</exception>
        /// <remarks>
        /// After successful save, IsDirty is set to false.
        /// Use SaveAs() for new documents or to save to a different location.
        /// </remarks>
        public void Save()
        {
            if (string.IsNullOrEmpty(_filePath))
                throw new InvalidOperationException("Cannot save document: FilePath is null. Use SaveAs() instead.");

            SaveAs(_filePath);
        }

        /// <summary>
        /// Saves the document to the specified file path.
        /// </summary>
        /// <param name="filePath">Path where the file should be saved.</param>
        /// <exception cref="ArgumentNullException">If filePath is null or empty.</exception>
        /// <remarks>
        /// <para>
        /// After successful save:
        /// - FilePath is updated to the new path
        /// - IsDirty is set to false
        /// - Title is updated (derived from new FilePath)
        /// </para>
        /// </remarks>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            Log.Information("Saving SpecDocument to: {FilePath}", filePath);

            // Serialize SpecFileRoot to JSON
            var json = System.Text.Json.JsonSerializer.Serialize(_spec, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            // Write to file
            System.IO.File.WriteAllText(filePath, json);

            // Update metadata
            FilePath = filePath; // This will also trigger Title update
            IsDirty = false;

            Log.Information("SpecDocument saved successfully: {FilePath}", filePath);
        }

        #endregion
    }
}
