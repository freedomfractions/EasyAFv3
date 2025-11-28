using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Spec.Models;
using EasyAF.Modules.Map.Services;
using Unity;
using Serilog;

namespace EasyAF.Modules.Spec
{
    /// <summary>
    /// Spec Editor module for managing EasyAF spec files (.ezspec).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Spec module provides a complete interface for:
    /// - Creating and editing table specifications for reports and labels
    /// - Defining columns with property paths, expressions, and formatting
    /// - Configuring conditional formatting rules
    /// - Validating specs against map files
    /// - WYSIWYG table preview with sample data
    /// </para>
    /// <para>
    /// This module wraps the existing SpecFileRoot class from EasyAF.Engine and provides
    /// a rich WPF UI for creating table specifications that define how data flows
    /// from EasyAF projects into Word document templates.
    /// </para>
    /// </remarks>
    public class SpecModule : IDocumentModule
    {
        private IUnityContainer? _container;

        /// <summary>
        /// Gets the display name of this module.
        /// </summary>
        public string ModuleName => "Spec Editor";

        /// <summary>
        /// Gets the version of this module.
        /// </summary>
        public string ModuleVersion => "3.0.0";

        /// <summary>
        /// Gets the file extensions this module can handle.
        /// </summary>
        /// <remarks>
        /// The Spec module handles .ezspec files (EasyAF Specification files).
        /// </remarks>
        public string[] SupportedFileExtensions => new[] { "ezspec" };

        /// <summary>
        /// Gets the file type definitions for use in file dialogs.
        /// </summary>
        /// <remarks>
        /// Provides friendly names and filter strings for Open/Save dialogs.
        /// </remarks>
        public IReadOnlyList<FileTypeDefinition> SupportedFileTypes => new[]
        {
            new FileTypeDefinition("ezspec", "EasyAF Specification Files")
        };

        /// <summary>
        /// Gets the icon for this module (null for now - can add embedded resource later).
        /// </summary>
        public ImageSource? ModuleIcon => new BitmapImage(
            new Uri("pack://application:,,,/EasyAF.Modules.Spec;component/Resources/spec-icon.png"));

        /// <summary>
        /// Initializes the module and registers services with the DI container.
        /// </summary>
        /// <param name="container">The Unity container for dependency injection.</param>
        public void Initialize(IUnityContainer container)
        {
            Log.Information("Initializing Spec module v{Version}", ModuleVersion);
            
            // Store container reference for later use
            _container = container ?? throw new ArgumentNullException(nameof(container));
            
            // TODO: Register services (PropertyDiscoveryService, MapValidationService)
            // TODO: Register settings ViewModel for Options dialog (Task 25+)
            
            Log.Information("Spec module initialized successfully");
        }

        /// <summary>
        /// Creates a new, empty spec document.
        /// </summary>
        /// <returns>A new IDocument instance wrapping an empty SpecFileRoot.</returns>
        public IDocument CreateNewDocument()
        {
            Log.Debug("Creating new spec document");
            
            var document = SpecDocument.CreateNew();
            document.OwnerModule = this;
            
            // Resolve services from container
            var dialogService = _container?.Resolve<IUserDialogService>() 
                ?? throw new InvalidOperationException("IUserDialogService not registered in container");
            
            var propertyDiscovery = _container?.Resolve<IPropertyDiscoveryService>() 
                ?? throw new InvalidOperationException("IPropertyDiscoveryService not registered in container");
            
            var settingsService = _container?.Resolve<ISettingsService>() 
                ?? throw new InvalidOperationException("ISettingsService not registered in container");
            
            var viewModel = new ViewModels.SpecDocumentViewModel(document, dialogService, propertyDiscovery, settingsService);
            document.ViewModel = viewModel;
            
            Log.Information("New spec document created");
            
            return document;
        }

        /// <summary>
        /// Opens an existing spec file from disk.
        /// </summary>
        /// <param name="filePath">Path to the .ezspec file to open.</param>
        /// <returns>An IDocument instance wrapping the loaded SpecFileRoot.</returns>
        /// <exception cref="ArgumentNullException">If filePath is null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException">If the file does not exist.</exception>
        public IDocument OpenDocument(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            Log.Information("Opening spec document: {FilePath}", filePath);
            
            var document = SpecDocument.LoadFrom(filePath);
            document.OwnerModule = this;
            
            // Resolve services from container
            var dialogService = _container?.Resolve<IUserDialogService>() 
                ?? throw new InvalidOperationException("IUserDialogService not registered in container");
            
            var propertyDiscovery = _container?.Resolve<IPropertyDiscoveryService>() 
                ?? throw new InvalidOperationException("IPropertyDiscoveryService not registered in container");
            
            var settingsService = _container?.Resolve<ISettingsService>() 
                ?? throw new InvalidOperationException("ISettingsService not registered in container");
            
            var viewModel = new ViewModels.SpecDocumentViewModel(document, dialogService, propertyDiscovery, settingsService);
            document.ViewModel = viewModel;
            
            Log.Information("Spec document opened successfully: {FilePath}", filePath);
            
            return document;
        }

        /// <summary>
        /// Saves a spec document to disk.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="filePath">Path where the file should be saved.</param>
        /// <exception cref="ArgumentNullException">If document or filePath is null.</exception>
        /// <exception cref="InvalidCastException">If document is not a SpecDocument.</exception>
        public void SaveDocument(IDocument document, string filePath)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            Log.Information("Saving spec document: {FilePath}", filePath);
            
            if (document is not SpecDocument specDoc)
                throw new InvalidCastException($"Document must be a SpecDocument, but was {document.GetType().Name}");
            
            specDoc.SaveAs(filePath);
            Log.Information("Spec document saved successfully: {FilePath}", filePath);
        }

        /// <summary>
        /// Gets the ribbon tabs to display when a spec document is active.
        /// </summary>
        /// <param name="activeDocument">The currently active document.</param>
        /// <returns>Array of ribbon tabs specific to spec documents.</returns>
        /// <remarks>
        /// TODO: Implement ribbon tabs in Task 27.
        /// </remarks>
        public Fluent.RibbonTabItem[] GetRibbonTabs(IDocument activeDocument)
        {
            Log.Debug("Generating ribbon tabs for Spec module");
            
            // TODO: Task 27 - Create ribbon tabs (Table Management, Column Operations, Validation)
            return Array.Empty<Fluent.RibbonTabItem>();
        }

        /// <summary>
        /// Shuts down the module and releases resources.
        /// </summary>
        public void Shutdown()
        {
            Log.Information("Shutting down Spec module");
            // Cleanup if needed
        }

        /// <summary>
        /// Determines whether this module can handle the specified file based on extension.
        /// </summary>
        public bool CanHandleFile(string filePath)
        {
            var extension = System.IO.Path.GetExtension(filePath);
            return SupportedFileExtensions.Any(ext => ext.Equals(extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a suggested filename for the document based on its content.
        /// </summary>
        /// <param name="document">The document to suggest a name for.</param>
        /// <returns>Suggested filename without extension, or null to use default Title.</returns>
        public string? GetSuggestedFileName(IDocument document)
        {
            // Use default Title-based naming for spec files
            // Could enhance later to suggest names like "[Software Version] Spec"
            return null;
        }
    }
}
