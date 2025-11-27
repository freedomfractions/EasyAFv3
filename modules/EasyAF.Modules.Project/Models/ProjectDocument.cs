using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EasyAF.Core.Contracts;
using EasyAF.Data.Models;
using EasyAF.Modules.Project.Services;
using Serilog;

namespace EasyAF.Modules.Project.Models
{
    /// <summary>
    /// Document wrapper for EasyAF Project files (.ezproj).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class wraps the existing <see cref="EasyAF.Data.Models.Project"/> class
    /// and implements the <see cref="IDocument"/> interface to integrate with the
    /// EasyAF shell's document management system.
    /// </para>
    /// <para>
    /// The ProjectDocument provides:
    /// - Dirty state tracking for unsaved changes
    /// - File path management
    /// - Title display logic
    /// - Property change notifications (MVVM support)
    /// </para>
    /// <para>
    /// This is a lightweight wrapper - all actual project data lives in the wrapped
    /// <see cref="Project"/> instance. The wrapper only adds shell integration metadata.
    /// </para>
    /// </remarks>
    public class ProjectDocument : IDocument, INotifyPropertyChanged
    {
        private readonly Data.Models.Project _project;
        private string? _filePath;
        private bool _isDirty;
        private IDocumentModule? _ownerModule;

        /// <summary>
        /// Initializes a new instance of the ProjectDocument class.
        /// </summary>
        /// <param name="project">The underlying Project instance to wrap.</param>
        /// <param name="filePath">Optional file path if this document was loaded from disk.</param>
        /// <exception cref="ArgumentNullException">If project is null.</exception>
        public ProjectDocument(Data.Models.Project project, string? filePath = null)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _filePath = filePath;
            _isDirty = false;

            Log.Debug("ProjectDocument created for: {FilePath}", filePath ?? "(new document)");
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
                    Log.Debug("ProjectDocument FilePath changed to: {FilePath}", value);
                }
            }
        }

        /// <summary>
        /// Gets the display title for this document.
        /// </summary>
        /// <remarks>
        /// Title is derived from:
        /// 1. If FilePath is set: filename without extension
        /// 2. If FilePath is null: "Untitled Project"
        /// </remarks>
        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath))
                    return "Untitled Project";

                return System.IO.Path.GetFileNameWithoutExtension(_filePath);
            }
        }

        /// <summary>
        /// Gets or sets whether this document has unsaved changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// IsDirty tracking is managed by the ProjectDocument wrapper:
        /// - Set to true when any property on the wrapped Project changes
        /// - Set to false after successful save
        /// - Prompts user before closing if true
        /// </para>
        /// <para>
        /// IMPORTANT: ViewModels must manually set IsDirty = true when making changes
        /// to the Project instance. Automatic change tracking is not implemented
        /// (would require deep property monitoring on Project class).
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
                    Log.Debug("ProjectDocument IsDirty changed to: {IsDirty}", value);
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
        /// Gets the underlying Project instance.
        /// </summary>
        /// <remarks>
        /// This exposes the wrapped Project for ViewModels to bind to.
        /// ViewModels should bind directly to Project.* properties and
        /// manually call MarkDirty() when making changes.
        /// </remarks>
        public Data.Models.Project Project => _project;

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
        /// Creates a new, empty ProjectDocument.
        /// </summary>
        /// <param name="settingsService">Optional settings service to apply defaults from.</param>
        /// <returns>A new ProjectDocument wrapping a default Project instance.</returns>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Default Import Map from Settings
        /// Modified for: Pre-populate MapPathHistory with default map from settings
        /// Related modules: Project (ProjectModuleSettings, ProjectSettingsExtensions)
        /// Rollback instructions: Remove settingsService parameter and default map logic
        /// 
        /// If settingsService is provided and a default map is configured,
        /// the new project will have that map pre-selected in the Import Map dropdown.
        /// </remarks>
        public static ProjectDocument CreateNew(ISettingsService? settingsService = null)
        {
            var project = new Data.Models.Project
            {
                NewData = new DataSet(),
                OldData = new DataSet(),
                ProjectLog = new System.Collections.Generic.Dictionary<long, string>(),
                MapPathHistory = new System.Collections.Generic.List<string>()
            };

            // Apply default map from settings if available
            if (settingsService != null)
            {
                try
                {
                    var projectSettings = settingsService.GetProjectModuleSettings();
                    
                    // CROSS-MODULE EDIT: 2025-01-27 Use Most Recent Map When No Default Set
                    // Modified for: If setting is null (None), find most recent .ezmap file by modified date
                    // Related modules: Core (CrossModuleSettingsExtensions.GetMapsDirectory)
                    // Rollback instructions: Remove auto-discovery logic, only use explicit setting
                    
                    string? mapToUse = null;
                    
                    if (!string.IsNullOrWhiteSpace(projectSettings.DefaultImportMapPath) &&
                        System.IO.File.Exists(projectSettings.DefaultImportMapPath))
                    {
                        // User has explicitly set a default map - use it
                        mapToUse = projectSettings.DefaultImportMapPath;
                        Log.Information("New project initialized with user-configured default map: {Path}", mapToUse);
                    }
                    else
                    {
                        // Setting is null or file doesn't exist - find most recent .ezmap file
                        var mapsFolder = System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            "EasyAF",
                            "Maps");
                        
                        if (System.IO.Directory.Exists(mapsFolder))
                        {
                            var mostRecentMap = System.IO.Directory.GetFiles(mapsFolder, "*.ezmap")
                                .Select(f => new System.IO.FileInfo(f))
                                .OrderByDescending(f => f.LastWriteTime)
                                .FirstOrDefault();
                            
                            if (mostRecentMap != null)
                            {
                                mapToUse = mostRecentMap.FullName;
                                Log.Information("New project initialized with most recent map (by date): {Path}", mapToUse);
                            }
                        }
                    }
                    
                    // Pre-populate MapPathHistory with selected map (if found)
                    if (mapToUse != null)
                    {
                        project.MapPathHistory.Add(mapToUse);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to apply default map from settings");
                    // Don't fail project creation - just log and continue
                }
            }

            var doc = new ProjectDocument(project);
            Log.Information("Created new ProjectDocument");
            return doc;
        }

        /// <summary>
        /// Loads a ProjectDocument from disk.
        /// </summary>
        /// <param name="filePath">Path to the .ezproj file to load.</param>
        /// <returns>A ProjectDocument wrapping the loaded Project instance.</returns>
        /// <exception cref="ArgumentNullException">If filePath is null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException">If the file does not exist.</exception>
        /// <exception cref="InvalidOperationException">If deserialization fails.</exception>
        public static ProjectDocument LoadFrom(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException($"Project file not found: {filePath}", filePath);

            Log.Information("Loading ProjectDocument from: {FilePath}", filePath);

            var project = Data.Models.Project.LoadFromFile(filePath);
            if (project == null)
                throw new InvalidOperationException($"Failed to deserialize project file: {filePath}");

            var doc = new ProjectDocument(project, filePath)
            {
                IsDirty = false // Just loaded, no unsaved changes
            };

            Log.Information("ProjectDocument loaded successfully: {FilePath}", filePath);
            return doc;
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

            Log.Information("Saving ProjectDocument to: {FilePath}", filePath);

            // Use the Project's built-in save method
            _project.SaveToFile(filePath);

            // Update metadata
            FilePath = filePath; // This will also trigger Title update
            IsDirty = false;

            Log.Information("ProjectDocument saved successfully: {FilePath}", filePath);
        }

        #endregion
    }
}
