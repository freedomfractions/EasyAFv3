using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services; // For MapSettingsExtensions
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
    public class MapModule : IDocumentModule, IHelpProvider
    {
        private IUnityContainer? _container;
        
        /// <summary>
        /// Gets the unique name of this module.
        /// </summary>
        public string ModuleName => "Map Editor";

        /// <summary>
        /// Gets the version of this module.
        /// </summary>
        public string ModuleVersion => "0.1.0";

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
        /// Gets the help pages provided by this module.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Provides comprehensive user documentation for the Map Editor including:
        /// - Introduction and quick start
        /// - Creating and managing mappings
        /// - Property management features
        /// - Troubleshooting common issues
        /// </para>
        /// <para>
        /// All pages are embedded Markdown resources in the Help/ folder.
        /// </para>
        /// </remarks>
        public IEnumerable<HelpPageDescriptor> GetHelpPages() => new[]
        {
            new HelpPageDescriptor(
                Id: "map.intro",
                Title: "Map Editor Introduction",
                Category: "Getting Started",
                ResourcePath: "Help/Introduction.md",
                Keywords: new[] { "overview", "quick start", "basics", "map editor", "mapping" }
            ),
            new HelpPageDescriptor(
                Id: "map.mapping",
                Title: "Creating Mappings",
                Category: "Mapping",
                ResourcePath: "Help/CreatingMappings.md",
                Keywords: new[] { "create", "map", "associate", "link", "columns", "properties", "auto-map", "drag-drop" }
            ),
            new HelpPageDescriptor(
                Id: "map.properties",
                Title: "Property Management",
                Category: "Mapping",
                ResourcePath: "Help/PropertyManagement.md",
                Keywords: new[] { "manage fields", "show", "hide", "properties", "fields", "visibility", "selector" }
            ),
            new HelpPageDescriptor(
                Id: "map.troubleshooting",
                Title: "Troubleshooting",
                Category: "Help",
                ResourcePath: "Help/Troubleshooting.md",
                Keywords: new[] { "problems", "errors", "issues", "bugs", "not working", "fix", "solve", "help" }
            )
        };

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

            // CROSS-MODULE EDIT: 2025-01-16 Map Module Settings Feature
            // Modified for: Add property visibility settings filtering
            // Related modules: Core (ISettingsService), Map (PropertyDiscoveryService, MapDocumentViewModel, MapSettingsExtensions)
            // Rollback instructions: Remove DataTypeVisibilitySettings, MapSettingsExtensions, PropertySelectorDialog, MapModuleSettingsView files

            // Register services - PropertyDiscoveryService now requires ISettingsService
            container.RegisterSingleton<Services.IPropertyDiscoveryService, Services.PropertyDiscoveryService>();
            container.RegisterType<Services.ColumnExtractionService>(); // Transient - create new instance per use
            
            // Register settings ViewModel (singleton - one instance for Options dialog)
            container.RegisterSingleton<ViewModels.MapModuleSettingsViewModel>();
            
            // Initialize default settings if not present (first run or corrupted settings)
            var settingsService = container.Resolve<ISettingsService>();
            InitializeDefaultSettings(settingsService);
            
            Log.Information("Map module initialized successfully");
        }

        /// <summary>
        /// Initializes default visibility settings for data types and properties.
        /// </summary>
        /// <param name="settingsService">The settings service.</param>
        /// <remarks>
        /// <para>
        /// Creates default settings if none exist. This serves as:
        /// - Initial configuration on first run
        /// - Fallback if settings are corrupted
        /// - Template that UI will later override with user preferences
        /// </para>
        /// <para>
        /// <strong>Default Configuration:</strong>
        /// - All data types: Enabled = true
        /// - All properties: Wildcard mode ("*" = show all)
        /// </para>
        /// <para>
        /// Users can later customize via Options dialog (when UI is implemented).
        /// </para>
        /// </remarks>
        private void InitializeDefaultSettings(ISettingsService settingsService)
        {
            try
            {
                var existing = settingsService.GetMapVisibilitySettings();
                
                // If settings already exist and have data types configured, don't overwrite
                if (existing.DataTypes.Count > 0)
                {
                    Log.Debug("Map visibility settings already initialized ({Count} data types configured)", existing.DataTypes.Count);
                    return;
                }

                Log.Information("Initializing default Map visibility settings (first run or corrupted settings)");

                // Create default settings: all data types enabled with all properties visible
                var defaults = new Models.DataTypeVisibilitySettings();
                
                // List of known data types from EasyAF.Data.Models
                var dataTypes = new[] { "Bus", "LVCB", "Fuse", "Cable", "ArcFlash", "ShortCircuit" };
                
                foreach (var dataType in dataTypes)
                {
                    defaults.DataTypes[dataType] = new Models.DataTypeConfig
                    {
                        Enabled = true,
                        EnabledProperties = new List<string> { "*" } // Wildcard = show all properties
                    };
                }

                // Save defaults to settings
                settingsService.SetMapVisibilitySettings(defaults);
                
                Log.Information("Default Map visibility settings created: {Count} data types, all properties enabled", dataTypes.Length);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize default Map visibility settings - will use fallback (all enabled)");
                // Don't throw - module should still function with all properties visible
            }
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
                SoftwareVersion = "0.1.0",
                IsDirty = true, // New documents are dirty until saved
                OwnerModule = this
            };

            // Create ViewModel for this document
            if (_container != null)
            {
                // CROSS-MODULE EDIT: 2025-01-16 Auto-Map Intelligence
                // Modified for: Resolve IFuzzyMatcher and pass to MapDocumentViewModel
                // Related modules: Core (IFuzzyMatcher), Map (DataTypeMappingViewModel)
                // Rollback instructions: Remove fuzzyMatcher resolution
                
                var fuzzyMatcher = _container.Resolve<IFuzzyMatcher>();
                var viewModel = new ViewModels.MapDocumentViewModel(
                    document,
                    _container.Resolve<Services.IPropertyDiscoveryService>(),
                    _container.Resolve<IUserDialogService>(),
                    _container.Resolve<ISettingsService>(),
                    fuzzyMatcher);
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
        /// <exception cref="InvalidDataException">If the file content is not valid JSON or has invalid schema.</exception>
        /// <remarks>
        /// <para>
        /// Deserializes the mapping configuration from JSON format including Map Editor metadata.
        /// Uses MapDocumentSerializer to load both mappings and metadata (MapName, Description, etc.).
        /// </para>
        /// <para>
        /// <strong>Backward Compatibility:</strong> Can load files created by older tools (missing metadata defaults to empty).
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
                // Use MapDocumentSerializer to load the document
                var serializer = new MapDocumentSerializer();
                var document = serializer.Load(filePath);
                
                // Set file path and module reference
                document.FilePath = filePath;
                document.OwnerModule = this;
                document.IsDirty = false; // Freshly loaded = clean

                // Create ViewModel for this document
                if (_container != null)
                {
                    // CROSS-MODULE EDIT: 2025-01-16 Auto-Map Intelligence
                    // Modified for: Resolve IFuzzyMatcher and pass to MapDocumentViewModel
                    // Related modules: Core (IFuzzyMatcher), Map (DataTypeMappingViewModel)
                    // Rollback instructions: Remove fuzzyMatcher resolution
                    
                    var fuzzyMatcher = _container.Resolve<IFuzzyMatcher>();
                    var viewModel = new ViewModels.MapDocumentViewModel(
                        document,
                        _container.Resolve<Services.IPropertyDiscoveryService>(),
                        _container.Resolve<IUserDialogService>(),
                        _container.Resolve<ISettingsService>(),
                        fuzzyMatcher);
                    document.ViewModel = viewModel;
                    Log.Debug("Created ViewModel for opened map document");

                    // CROSS-MODULE EDIT: 2025-01-16 Missing File Detection
                    // Modified for: Validate referenced files after document load
                    // Related modules: Map (MapDocument, MapDocumentViewModel, MissingFilesDialog)
                    // Rollback instructions: Remove ValidateMissingFiles call below
                    
                    // Check for missing referenced files and show resolution dialog if needed
                    // Pass isInitialLoad: true to prevent marking document dirty unless user makes changes
                    viewModel.ValidateMissingFiles(isInitialLoad: true);
                }
                
                Log.Information("Successfully opened map document: {MapName} ({MappingCount} mappings)", 
                    document.MapName, document.MappingsByDataType.Values.Sum(v => v.Count));
                return document;
            }
            catch (InvalidDataException ex)
            {
                Log.Error(ex, "Invalid map file format: {FilePath}", filePath);
                throw;
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
        /// Serializes the mapping configuration to JSON format including Map Editor metadata.
        /// Uses MapDocumentSerializer to save both mappings and metadata (MapName, Description, ReferencedFiles, etc.).
        /// </para>
        /// <para>
        /// <strong>Format Compatibility:</strong> Saved files can be loaded by ImportManager (metadata is ignored).
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
                // CROSS-MODULE EDIT: 2025-01-16 Required Property Validation
                // Modified for: Validate required properties before saving
                // Related modules: Map (MappingValidator service, PropertyDiscoveryService)
                // Rollback instructions: Remove validation check below
                
                // Validate that all required properties are mapped before saving
                if (_container != null)
                {
                    var propertyDiscovery = _container.Resolve<Services.IPropertyDiscoveryService>();
                    var settingsService = _container.Resolve<ISettingsService>();
                    var validator = new Services.MappingValidator(propertyDiscovery, settingsService);
                    var validationResult = validator.ValidateRequiredMappings(mapDoc);
                    
                    if (!validationResult.IsValid)
                    {
                        var summary = validator.GetValidationSummary(validationResult);
                        var totalUnmapped = validationResult.UnmappedRequired.Sum(kvp => kvp.Value.Count);
                        
                        Log.Warning("Validation failed: {Count} required properties unmapped", totalUnmapped);
                        
                        // Show warning dialog to user
                        var dialogService = _container.Resolve<IUserDialogService>();
                        var confirmed = dialogService.Confirm(
                            $"Incomplete Mapping Detected\n\n" +
                            $"{summary}\n" +
                            $"Saving this map without mapping all required properties may cause import errors.\n\n" +
                            $"Do you want to save anyway?",
                            "Required Properties Not Mapped");
                        
                        if (!confirmed)
                        {
                            Log.Information("User cancelled save due to validation warnings");
                            return; // User chose not to save
                        }
                        
                        Log.Information("User chose to save despite validation warnings");
                    }
                    else
                    {
                        Log.Debug("Validation passed: all required properties are mapped");
                    }
                }

                // Use MapDocumentSerializer to save the document
                var serializer = new MapDocumentSerializer();
                serializer.Save(mapDoc, filePath);
                
                // Update document state
                mapDoc.FilePath = filePath;
                mapDoc.IsDirty = false;
                
                Log.Information("Map saved successfully: {FilePath} ({MappingCount} mappings)",
                    filePath, mapDoc.MappingsByDataType.Values.Sum(v => v.Count));
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
        /// </para>
        /// <para>
        /// Commands are data-bound to the MapDocument's ViewModel.
        /// </para>
        /// </remarks>
        public RibbonTabItem[] GetRibbonTabs(IDocument activeDocument)
        {
            Log.Debug("Generating ribbon tabs for Map module");
            
            if (activeDocument is not MapDocument mapDoc)
            {
                Log.Debug("No active MapDocument (document is {Type}), skipping ribbon tab generation", 
                    activeDocument?.GetType().Name ?? "null");
                return Array.Empty<RibbonTabItem>();
            }

            var viewModel = mapDoc.ViewModel as ViewModels.MapDocumentViewModel;
            if (viewModel == null)
            {
                Log.Warning("MapDocument has no ViewModel");
                return Array.Empty<RibbonTabItem>();
            }

            // Create Mapping tab
            var mappingTab = CreateMappingTab(viewModel);

            Log.Debug("Created {Count} ribbon tabs for Map module", 1);
            return new[] { mappingTab };
        }

        /// <summary>
        /// Creates the Mapping ribbon tab with sample loading and mapping tools.
        /// </summary>
        private RibbonTabItem CreateMappingTab(ViewModels.MapDocumentViewModel viewModel)
        {
            var tab = new RibbonTabItem
            {
                Header = "Mapping",
                DataContext = viewModel
            };

            // Create ribbon groups
            var samplesGroup = CreateSamplesGroup();
            var toolsGroup = CreateToolsGroup();

            tab.Groups.Add(samplesGroup);
            tab.Groups.Add(toolsGroup);

            return tab;
        }

        /// <summary>
        /// Creates the Samples group with Load Sample button.
        /// </summary>
        private RibbonGroupBox CreateSamplesGroup()
        {
            var group = new RibbonGroupBox
            {
                Header = "Samples"
            };

            // Load Sample button
            var loadSampleButton = new Fluent.Button
            {
                Header = "Load Sample",
                Icon = CreateGlyphIcon("\uE8E5"), // OpenFile glyph
                LargeIcon = CreateGlyphIcon("\uE8E5", 32),
                SizeDefinition = "Large",
                ToolTip = "Load a sample data file to preview columns"
            };
            loadSampleButton.SetBinding(Fluent.Button.CommandProperty, new System.Windows.Data.Binding("LoadFromFileCommand"));

            group.Items.Add(loadSampleButton);
            
            return group;
        }

        /// <summary>
        /// Creates the Tools group with Auto-Map, Validate, and Clear buttons.
        /// </summary>
        private RibbonGroupBox CreateToolsGroup()
        {
            var group = new RibbonGroupBox
            {
                Header = "Mapping Tools"
            };

            // Auto-Map button
            var autoMapButton = new Fluent.Button
            {
                Header = "Auto-Map",
                Icon = CreateGlyphIcon("\uE895"), // Link glyph
                LargeIcon = CreateGlyphIcon("\uE895", 32),
                SizeDefinition = "Large",
                ToolTip = "Automatically match columns to properties using intelligent algorithms"
            };
            autoMapButton.SetBinding(Fluent.Button.CommandProperty, new System.Windows.Data.Binding("AutoMapCommand"));

            // Validate button
            var validateButton = new Fluent.Button
            {
                Header = "Validate",
                Icon = CreateGlyphIcon("\uE73E"), // Checkmark glyph
                LargeIcon = CreateGlyphIcon("\uE73E", 32),
                SizeDefinition = "Large",
                ToolTip = "Check mapping completeness and identify issues"
            };
            validateButton.SetBinding(Fluent.Button.CommandProperty, new System.Windows.Data.Binding("ValidateMappingsCommand"));

            // Clear button
            var clearButton = new Fluent.Button
            {
                Header = "Clear",
                Icon = CreateGlyphIcon("\uE894"), // Delete glyph
                LargeIcon = CreateGlyphIcon("\uE894", 32),
                SizeDefinition = "Large",
                ToolTip = "Remove all mappings"
            };
            clearButton.SetBinding(Fluent.Button.CommandProperty, new System.Windows.Data.Binding("ClearAllMappingsCommand"));

            group.Items.Add(autoMapButton);
            group.Items.Add(validateButton);
            group.Items.Add(clearButton);

            return group;
        }

        /// <summary>
        /// Creates a glyph icon from Segoe MDL2 Assets font.
        /// </summary>
        private System.Windows.Media.ImageSource CreateGlyphIcon(string glyph, int size = 16)
        {
            var visual = new System.Windows.Media.DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var formattedText = new System.Windows.Media.FormattedText(
                    glyph,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface("Segoe MDL2 Assets"),
                    size,
                    System.Windows.Media.Brushes.Black,
                    System.Windows.Media.VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                context.DrawText(formattedText, new System.Windows.Point(0, 0));
            }

            var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(
                size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            bitmap.Render(visual);

            return bitmap;
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

        /// <summary>
        /// Gets a suggested filename for the document based on its content.
        /// </summary>
        /// <param name="document">The document to suggest a filename for.</param>
        /// <returns>
        /// Null (Map module uses default Title-based naming for now).
        /// </returns>
        /// <remarks>
        /// <para>
        /// The Map module currently does not have a specific naming convention.
        /// Future enhancement could use "[Data Type] Mapping" if data type is known.
        /// </para>
        /// </remarks>
        public string? GetSuggestedFileName(IDocument document)
        {
            // Map module doesn't have a naming convention yet
            // Could enhance later with "[Data Type] Mapping" if applicable
            return null;
        }
    }
}
