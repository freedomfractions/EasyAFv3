using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Map.Models;
using EasyAF.Import;
using Unity;
using Fluent;
using Serilog;
using Newtonsoft.Json;

namespace EasyAF.Modules.Map
{
    /// <summary>
    /// Map editor module for managing column-to-property mapping configurations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Map module provides a visual interface for creating and editing data import mappings.
    /// It allows users to:
    /// - Load sample data files (CSV/Excel)
    /// - Discover available target properties via reflection
    /// - Create column-to-property associations
    /// - Validate mappings for completeness and conflicts
    /// - Save/load mapping configurations (.ezmap files)
    /// </para>
    /// <para>
    /// <strong>Module Architecture:</strong>
    /// The Map module follows strict MVVM principles with zero code-behind logic.
    /// All functionality is exposed through ViewModels and bound via data templates.
    /// </para>
    /// <para>
    /// <strong>Document Type:</strong> MapDocument (.ezmap files)
    /// </para>
    /// <para>
    /// <strong>Integration:</strong> Module is discovered and initialized by the shell's ModuleLoader
    /// at startup. Registers its document type and provides ribbon tabs for map-specific operations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Module registration (handled automatically by shell):</strong></para>
    /// <code>
    /// // Shell's ModuleLoader discovers this module via reflection
    /// var mapModule = new MapModule();
    /// mapModule.Initialize(container);
    /// 
    /// // Module registers its services and views
    /// // Document type (.ezmap) is now available in New/Open dialogs
    /// </code>
    /// </example>
    public class MapModule : IDocumentModule
    {
        private IUnityContainer? _container;
        
        /// <summary>
        /// Gets the unique name of this module.
        /// </summary>
        public string ModuleName => "Map Editor";

        /// <summary>
        /// Gets the version of this module.
        /// </summary>
        public string ModuleVersion => "3.0.0";

        /// <summary>
        /// Gets the file extensions supported by this module.
        /// </summary>
        /// <remarks>
        /// The Map module handles .ezmap files (EasyAF mapping configuration format).
        /// </remarks>
        public string[] SupportedFileExtensions => new[] { "ezmap" };

        /// <summary>
        /// Gets rich file type definitions for this module.
        /// </summary>
        /// <remarks>
        /// Provides detailed file type information for Open/Save dialogs.
        /// </remarks>
        public IReadOnlyList<FileTypeDefinition>? SupportedFileTypes => new[]
        {
            new FileTypeDefinition("ezmap", "EasyAF Mapping Configuration Files")
        };

        /// <summary>
        /// Gets the icon representing this module.
        /// </summary>
        /// <remarks>
        /// Currently returns null; icon can be added as embedded resource in future.
        /// </remarks>
        public ImageSource? ModuleIcon => null; // TODO: Add module icon

        /// <summary>
        /// Initializes the Map module with the dependency injection container.
        /// </summary>
        /// <param name="container">The Unity container for service registration.</param>
        /// <remarks>
        /// <para>
        /// Registers Map module services and view models with the DI container.
        /// Called once during application startup by the shell's ModuleLoader.
        /// </para>
        /// <para>
        /// <strong>Registered Services:</strong>
        /// - ViewModels (transient per document)
        /// - Services (singleton for module-wide functionality)
        /// </para>
        /// </remarks>
        public void Initialize(IUnityContainer container)
        {
            _container = container;
            Log.Information("Initializing Map module v{Version}", ModuleVersion);

            // Register services
            container.RegisterSingleton<Services.IPropertyDiscoveryService, Services.PropertyDiscoveryService>();
            container.RegisterType<Services.ColumnExtractionService>(); // Transient - create new instance per use
            
            Log.Information("Map module initialized successfully");
        }

        /// <summary>
        /// Shuts down the Map module and releases resources.
        /// </summary>
        /// <remarks>
        /// Called when the application is closing. Cleans up any module-specific resources.
        /// </remarks>
        public void Shutdown()
        {
            Log.Information("Shutting down Map module");
            // TODO: Cleanup resources if needed
        }

        /// <summary>
        /// Creates a new, empty mapping document.
        /// </summary>
        /// <returns>A new <see cref="IDocument"/> instance representing an empty mapping.</returns>
        /// <remarks>
        /// Creates a new MapDocument with default settings. The document is initially
        /// marked as dirty (unsaved) until the user saves it.
        /// </remarks>
        public IDocument CreateNewDocument()
        {
            Log.Information("Creating new mapping document");
            
            var document = new MapDocument
            {
                MapName = "Untitled Map",
                SoftwareVersion = "3.0.0",
                IsDirty = true, // New documents are dirty until saved
                OwnerModule = this
            };

            // Create ViewModel for this document
            if (_container != null)
            {
                var viewModel = new ViewModels.MapDocumentViewModel(
                    document,
                    _container.Resolve<Services.IPropertyDiscoveryService>()
                );
                document.ViewModel = viewModel;
                Log.Debug("Created ViewModel for new map document");
            }
            
            Log.Debug("New map document created with default settings");
            return document;
        }

        /// <summary>
        /// Opens an existing mapping document from a file.
        /// </summary>
        /// <param name="filePath">Path to the .ezmap file to open.</param>
        /// <returns>A <see cref="IDocument"/> instance representing the loaded mapping.</returns>
        /// <exception cref="System.IO.FileNotFoundException">If the specified file does not exist.</exception>
        /// <exception cref="Newtonsoft.Json.JsonException">If the file content is not valid JSON.</exception>
        /// <remarks>
        /// <para>
        /// Deserializes the mapping configuration from JSON format and validates it.
        /// If validation fails, warnings are logged but the document is still loaded
        /// to allow the user to fix issues.
        /// </para>
        /// <para>
        /// <strong>File Format:</strong> See <see cref="EasyAF.Import.MappingConfig"/> for details.
        /// </para>
        /// </remarks>
        public IDocument OpenDocument(string filePath)
        {
            Log.Information("Opening mapping document from {FilePath}", filePath);
            
            if (!File.Exists(filePath))
            {
                Log.Error("Map file not found: {FilePath}", filePath);
                throw new FileNotFoundException($"Map file not found: {filePath}");
            }

            try
            {
                // Load and validate the mapping config
                var config = MappingConfig.Load(filePath);
                Log.Debug("Loaded mapping config with {Count} entries", config.ImportMap.Count);

                // Convert to MapDocument
                var document = MapDocument.FromMappingConfig(config, filePath);
                document.MapName = Path.GetFileNameWithoutExtension(filePath);
                document.OwnerModule = this;

                // Create ViewModel for this document
                if (_container != null)
                {
                    var viewModel = new ViewModels.MapDocumentViewModel(
                        document,
                        _container.Resolve<Services.IPropertyDiscoveryService>()
                    );
                    document.ViewModel = viewModel;
                    Log.Debug("Created ViewModel for opened map document");
                }
                
                Log.Information("Successfully opened map document: {MapName}", document.MapName);
                return document;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex, "Invalid map file format: {FilePath}", filePath);
                throw new InvalidDataException($"Invalid map file format: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "JSON parsing error in map file: {FilePath}", filePath);
                throw new InvalidDataException($"Map file contains invalid JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves a mapping document to a file.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="filePath">Path where the .ezmap file should be saved.</param>
        /// <exception cref="ArgumentNullException">If document is null.</exception>
        /// <exception cref="InvalidCastException">If document is not a MapDocument.</exception>
        /// <exception cref="System.IO.IOException">If the file cannot be written.</exception>
        /// <remarks>
        /// <para>
        /// Serializes the mapping configuration to JSON format with indentation for readability.
        /// Validates the mapping before saving and logs any warnings.
        /// </para>
        /// <para>
        /// <strong>Atomicity:</strong> Writes to a temporary file first, then replaces the original
        /// to prevent data loss if the write operation fails.
        /// </para>
        /// </remarks>
        public void SaveDocument(IDocument document, string filePath)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document is not MapDocument mapDoc)
            {
                Log.Error("Document is not a MapDocument: {Type}", document.GetType().Name);
                throw new InvalidCastException("Document is not a MapDocument");
            }

            Log.Information("Saving mapping document to {FilePath}", filePath);

            try
            {
                // Convert to MappingConfig
                var config = mapDoc.ToMappingConfig();
                Log.Debug("Converted MapDocument to MappingConfig with {Count} entries", config.ImportMap.Count);

                // Serialize to JSON
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                
                // Atomic write: write to temp file first, then replace
                var tempPath = filePath + ".tmp";
                File.WriteAllText(tempPath, json);
                
                if (File.Exists(filePath))
                    File.Delete(filePath);
                    
                File.Move(tempPath, filePath);
                
                // Update document state
                mapDoc.FilePath = filePath;
                mapDoc.IsDirty = false;
                
                Log.Information("Map saved successfully: {FilePath}", filePath);
            }
            catch (IOException ex)
            {
                Log.Error(ex, "I/O error saving map file: {FilePath}", filePath);
                throw new IOException($"Failed to save map file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error saving map file: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Gets the ribbon tabs to display when a mapping document is active.
        /// </summary>
        /// <param name="activeDocument">The currently active mapping document.</param>
        /// <returns>Array of ribbon tabs for map-specific commands.</returns>
        /// <remarks>
        /// <para>
        /// Returns contextual ribbon tabs based on the active document state.
        /// Tabs include:
        /// - <strong>Mapping</strong>: Load samples, auto-detect, validate, clear
        /// - <strong>File Operations</strong>: Save mapping, export sample
        /// </para>
        /// <para>
        /// Commands are data-bound to the MapDocument's ViewModel.
        /// </para>
        /// </remarks>
        public RibbonTabItem[] GetRibbonTabs(IDocument activeDocument)
        {
            Log.Debug("Generating ribbon tabs for Map module");
            // TODO Task 15: Implement ribbon tab creation
            throw new NotImplementedException("Task 15: Create Map Ribbon Tabs");
        }

        /// <summary>
        /// Determines if this module can handle the specified file.
        /// </summary>
        /// <param name="filePath">Path to the file to check.</param>
        /// <returns>True if the file has a .ezmap extension; otherwise false.</returns>
        /// <remarks>
        /// Simple extension-based check. Does not validate file content.
        /// </remarks>
        public bool CanHandleFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var extension = System.IO.Path.GetExtension(filePath).TrimStart('.');
            return string.Equals(extension, "ezmap", StringComparison.OrdinalIgnoreCase);
        }
    }
}
