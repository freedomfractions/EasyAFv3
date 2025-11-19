using System;
using System.Collections.Generic;
using System.Windows.Media;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Project.Models;
using Unity;
using Serilog;

namespace EasyAF.Modules.Project
{
    /// <summary>
    /// Project Editor module for managing EasyAF project files (.ezproj).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Project module provides a complete interface for:
    /// - Editing project metadata (name, client, location, dates, etc.)
    /// - Importing data into Old/New datasets
    /// - Viewing equipment data in DiffGrid (Old vs New comparison)
    /// - Generating reports from imported data
    /// </para>
    /// <para>
    /// This module wraps the existing Project class from EasyAF.Data and provides
    /// a rich WPF UI for interacting with project data. It integrates with the
    /// Import module for data loading and the Export module for report generation.
    /// </para>
    /// </remarks>
    public class ProjectModule : IDocumentModule
    {
        /// <summary>
        /// Gets the display name of this module.
        /// </summary>
        public string ModuleName => "Project Editor";

        /// <summary>
        /// Gets the version of this module.
        /// </summary>
        public string ModuleVersion => "3.0.0";

        /// <summary>
        /// Gets the file extensions this module can handle.
        /// </summary>
        /// <remarks>
        /// The Project module handles .ezproj files (EasyAF Project files).
        /// </remarks>
        public string[] SupportedFileExtensions => new[] { "ezproj" };

        /// <summary>
        /// Gets the file type definitions for use in file dialogs.
        /// </summary>
        /// <remarks>
        /// Provides friendly names and filter strings for Open/Save dialogs.
        /// </remarks>
        public IReadOnlyList<FileTypeDefinition> SupportedFileTypes => new[]
        {
            new FileTypeDefinition("ezproj", "EasyAF Project Files")
        };

        /// <summary>
        /// Gets the icon for this module (null for now - can add embedded resource later).
        /// </summary>
        public ImageSource? ModuleIcon => null;

        /// <summary>
        /// Initializes the module and registers services with the DI container.
        /// </summary>
        /// <param name="container">The Unity container for dependency injection.</param>
        public void Initialize(IUnityContainer container)
        {
            Log.Information("Initializing Project module v{Version}", ModuleVersion);
            
            // TODO Task 20: Register ProjectDocumentViewModel
            // TODO Task 20: Register ProjectSummaryViewModel
            // TODO Task 21: Register DataTypeTabViewModel
            // TODO Task 22: Register ribbon commands
            
            Log.Information("Project module initialized successfully");
        }

        /// <summary>
        /// Creates a new, empty project document.
        /// </summary>
        /// <returns>A new IDocument instance wrapping an empty Project.</returns>
        public IDocument CreateNewDocument()
        {
            Log.Debug("Creating new project document");
            
            var document = ProjectDocument.CreateNew();
            document.OwnerModule = this;
            document.MarkDirty(); // New documents are dirty until saved
            
            Log.Information("New project document created");
            
            return document;
        }

        /// <summary>
        /// Opens an existing project file from disk.
        /// </summary>
        /// <param name="filePath">Path to the .ezproj file to open.</param>
        /// <returns>An IDocument instance wrapping the loaded Project.</returns>
        /// <exception cref="ArgumentNullException">If filePath is null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException">If the file does not exist.</exception>
        public IDocument OpenDocument(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            Log.Information("Opening project document: {FilePath}", filePath);
            
            var document = ProjectDocument.LoadFrom(filePath);
            document.OwnerModule = this;
            
            Log.Information("Project document opened successfully: {FilePath}", filePath);
            
            return document;
        }

        /// <summary>
        /// Saves a project document to disk.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="filePath">Path where the file should be saved.</param>
        /// <exception cref="ArgumentNullException">If document or filePath is null.</exception>
        /// <exception cref="InvalidCastException">If document is not a ProjectDocument.</exception>
        public void SaveDocument(IDocument document, string filePath)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            Log.Information("Saving project document: {FilePath}", filePath);
            
            if (document is not ProjectDocument projectDoc)
                throw new InvalidCastException($"Document must be a ProjectDocument, but was {document.GetType().Name}");
            
            projectDoc.SaveAs(filePath);
            Log.Information("Project document saved successfully: {FilePath}", filePath);
        }

        /// <summary>
        /// Gets the ribbon tabs to display when this document type is active.
        /// </summary>
        /// <param name="activeDocument">The currently active document.</param>
        /// <returns>Array of ribbon tabs specific to project documents.</returns>
        public Fluent.RibbonTabItem[] GetRibbonTabs(IDocument activeDocument)
        {
            // TODO Task 22: Implement ribbon tabs
            // - Data Management tab (Import, Clear, Refresh)
            // - Output Generation tab (Generate Report, Export)
            // - Analysis Tools tab (Statistics, Validation)
            
            Log.Debug("GetRibbonTabs called for project document");
            return Array.Empty<Fluent.RibbonTabItem>();
        }

        /// <summary>
        /// Determines if this module can handle a given file.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>True if the file extension is .ezproj; otherwise false.</returns>
        public bool CanHandleFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = System.IO.Path.GetExtension(filePath)?.TrimStart('.');
            var canHandle = Array.Exists(SupportedFileExtensions, ext => 
                ext.Equals(extension, StringComparison.OrdinalIgnoreCase));

            Log.Debug("CanHandleFile({FilePath}): {CanHandle}", filePath, canHandle);
            return canHandle;
        }

        /// <summary>
        /// Shuts down the module and releases resources.
        /// </summary>
        public void Shutdown()
        {
            Log.Information("Shutting down Project module");
            // Cleanup if needed
        }
    }
}
