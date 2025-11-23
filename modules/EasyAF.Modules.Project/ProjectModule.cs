using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Project.Models;
using Unity;
using Serilog;

namespace EasyAF.Modules.Project
{
    /// <summary>
    /// Project Editor module for managing EasyAF project files (.ezaf).
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
        private IUnityContainer? _container;

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
        /// The Project module handles .ezaf files (EasyAF Project files).
        /// </remarks>
        public string[] SupportedFileExtensions => new[] { "ezaf" };

        /// <summary>
        /// Gets the file type definitions for use in file dialogs.
        /// </summary>
        /// <remarks>
        /// Provides friendly names and filter strings for Open/Save dialogs.
        /// </remarks>
        public IReadOnlyList<FileTypeDefinition> SupportedFileTypes => new[]
        {
            new FileTypeDefinition("ezaf", "EasyAF Project Files")
        };

        /// <summary>
        /// Gets the icon for this module (null for now - can add embedded resource later).
        /// </summary>
        public ImageSource? ModuleIcon => new BitmapImage(
            new Uri("pack://application:,,,/EasyAF.Modules.Project;component/Resources/project-icon.png"));

        /// <summary>
        /// Initializes the module and registers services with the DI container.
        /// </summary>
        /// <param name="container">The Unity container for dependency injection.</param>
        public void Initialize(IUnityContainer container)
        {
            Log.Information("Initializing Project module v{Version}", ModuleVersion);
            
            // Store container reference for later use
            _container = container ?? throw new ArgumentNullException(nameof(container));
            
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
            
            // Resolve IUserDialogService from container
            var dialogService = _container?.Resolve<IUserDialogService>() 
                ?? throw new InvalidOperationException("IUserDialogService not registered in container");
            
            // Create ViewModel for the document
            var viewModel = new ViewModels.ProjectDocumentViewModel(document, dialogService);
            
            // Store ViewModel in a way the shell can access it
            // (Shell uses DataTemplate to render based on document type)
            document.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ViewModel")
                {
                    // ViewModel attached, nothing to do
                }
            };
            
            // Attach ViewModel to document for shell's DataTemplate system
            // (We'll use a custom property or tag for this)
            SetDocumentViewModel(document, viewModel);
            
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
            
            // Resolve IUserDialogService from container
            var dialogService = _container?.Resolve<IUserDialogService>() 
                ?? throw new InvalidOperationException("IUserDialogService not registered in container");
            
            // Create ViewModel for the document
            var viewModel = new ViewModels.ProjectDocumentViewModel(document, dialogService);
            SetDocumentViewModel(document, viewModel);
            
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
        /// Gets the ribbon tabs to display when a project document is active.
        /// </summary>
        /// <param name="activeDocument">The currently active document.</param>
        /// <returns>Array of ribbon tabs specific to project documents.</returns>
        public Fluent.RibbonTabItem[] GetRibbonTabs(IDocument activeDocument)
        {
            Log.Debug("Generating ribbon tabs for Project module");
            
            if (activeDocument is not ProjectDocument projectDoc)
            {
                Log.Debug("No active ProjectDocument (document is {Type}), skipping ribbon tab generation", 
                    activeDocument?.GetType().Name ?? "null");
                return Array.Empty<Fluent.RibbonTabItem>();
            }

            var viewModel = projectDoc.ViewModel as ViewModels.ProjectDocumentViewModel;
            if (viewModel == null)
            {
                Log.Warning("ProjectDocument has no ViewModel");
                return Array.Empty<Fluent.RibbonTabItem>();
            }

            // Create Project tab with placeholder groups
            var projectTab = CreateProjectTab(viewModel);

            Log.Debug("Created {Count} ribbon tabs for Project module", 1);
            return new[] { projectTab };
        }

        /// <summary>
        /// Creates the Project ribbon tab with data management and output tools.
        /// </summary>
        private Fluent.RibbonTabItem CreateProjectTab(ViewModels.ProjectDocumentViewModel viewModel)
        {
            var tab = new Fluent.RibbonTabItem
            {
                Header = "Project",
                DataContext = viewModel
            };

            // Create Import group with Import New/Old Data buttons
            var importGroup = CreateImportGroup();
            
            // Create placeholder ribbon groups (will be populated in Task 22)
            var dataGroup = new Fluent.RibbonGroupBox
            {
                Header = "Data Management"
            };
            
            var outputGroup = new Fluent.RibbonGroupBox
            {
                Header = "Output"
            };

            tab.Groups.Add(importGroup);
            tab.Groups.Add(dataGroup);
            tab.Groups.Add(outputGroup);

            return tab;
        }

        /// <summary>
        /// Creates the Import group with Import New Data and Import Old Data buttons.
        /// </summary>
        private Fluent.RibbonGroupBox CreateImportGroup()
        {
            var group = new Fluent.RibbonGroupBox
            {
                Header = "Import"
            };

            // Import New Data button
            var importNewButton = new Fluent.Button
            {
                Header = "Import New Data",
                Icon = CreateGlyphIcon("\uE8B5"), // Download icon
                LargeIcon = CreateGlyphIcon("\uE8B5", 32),
                SizeDefinition = "Large",
                ToolTip = "Import data files into New Data dataset (multi-select supported)"
            };
            importNewButton.SetBinding(Fluent.Button.CommandProperty, 
                new System.Windows.Data.Binding("Summary.ImportNewDataCommand"));

            // Import Old Data button
            var importOldButton = new Fluent.Button
            {
                Header = "Import Old Data",
                Icon = CreateGlyphIcon("\uE8B5"), // Download icon
                LargeIcon = CreateGlyphIcon("\uE8B5", 32),
                SizeDefinition = "Large",
                ToolTip = "Import data files into Old Data dataset (multi-select supported)"
            };
            importOldButton.SetBinding(Fluent.Button.CommandProperty, 
                new System.Windows.Data.Binding("Summary.ImportOldDataCommand"));

            group.Items.Add(importNewButton);
            group.Items.Add(importOldButton);

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
        /// Shuts down the module and releases resources.
        /// </summary>
        public void Shutdown()
        {
            Log.Information("Shutting down Project module");
            // Cleanup if needed
        }

        /// <summary>
        /// Attaches a ViewModel to a document for the shell's DataTemplate rendering.
        /// </summary>
        private void SetDocumentViewModel(ProjectDocument document, ViewModels.ProjectDocumentViewModel viewModel)
        {
            // CROSS-MODULE EDIT: 2025-01-20 Task 20
            // Modified for: Attach ViewModel to document for shell rendering
            // Related modules: Project (ProjectDocument)
            // Rollback instructions: Remove ViewModel property from ProjectDocument
            
            document.ViewModel = viewModel;
            Log.Debug("ViewModel attached to document");
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
        /// Gets a suggested filename based on project metadata.
        /// </summary>
        /// <param name="document">The project document.</param>
        /// <returns>Suggested filename in format "[LB Project Number] - [Site Name]" or null.</returns>
        public string? GetSuggestedFileName(IDocument document)
        {
            if (document is not ProjectDocument projectDoc)
                return null;

            var lbNumber = projectDoc.Project.LBProjectNumber;
            var siteName = projectDoc.Project.SiteName;

            // Build filename from available metadata
            if (!string.IsNullOrWhiteSpace(lbNumber) && !string.IsNullOrWhiteSpace(siteName))
            {
                return $"{lbNumber.Trim()} - {siteName.Trim()}";
            }
            else if (!string.IsNullOrWhiteSpace(lbNumber))
            {
                return lbNumber.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(siteName))
            {
                return siteName.Trim();
            }

            // Fall back to default Title-based naming
            return null;
        }
    }
}
