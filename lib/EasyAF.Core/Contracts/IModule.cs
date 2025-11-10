using System.Windows.Media;
using Unity;
using System.Collections.Generic;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Base interface for all EasyAF modules.
/// </summary>
/// <remarks>
/// All modules must implement this interface to be discovered and loaded by the shell.
/// Modules are isolated and communicate via EventAggregator only.
/// </remarks>
public interface IModule
{
    /// <summary>
    /// Gets the unique name of the module.
    /// </summary>
    /// <value>
    /// A unique identifier for the module (e.g., "MapEditor", "ProjectEditor").
    /// </value>
    string ModuleName { get; }

    /// <summary>
    /// Gets the version of the module.
    /// </summary>
    /// <value>
    /// Semantic version string (e.g., "1.0.0").
    /// </value>
    string ModuleVersion { get; }

    /// <summary>
    /// Gets the file extensions supported by this module.
    /// </summary>
    /// <value>
    /// Array of file extensions without the dot (e.g., ["ezmap", "map"]).
    /// </value>
    string[] SupportedFileExtensions { get; }

    /// <summary>
    /// Gets rich file type definitions (extension + description) supported by this module.
    /// </summary>
    /// <remarks>
    /// Used by the shell for dynamically building file dialogs and association lists.
    /// If not provided (null), the shell will fall back to <see cref="SupportedFileExtensions"/> with generic descriptions.
    /// </remarks>
    IReadOnlyList<FileTypeDefinition>? SupportedFileTypes { get; }

    /// <summary>
    /// Gets the icon representing this module.
    /// </summary>
    /// <value>
    /// An ImageSource that will be displayed in tabs and dialogs.
    /// </value>
    ImageSource? ModuleIcon { get; }

    /// <summary>
    /// Initializes the module with the dependency injection container.
    /// </summary>
    /// <param name="container">The Unity container for registering module services.</param>
    /// <remarks>
    /// Called once when the module is loaded. Register all module-specific services here.
    /// </remarks>
    void Initialize(IUnityContainer container);

    /// <summary>
    /// Shuts down the module and releases resources.
    /// </summary>
    /// <remarks>
    /// Called when the application is closing or the module is being unloaded.
    /// Clean up any resources, save state, and unsubscribe from events.
    /// </remarks>
    void Shutdown();
}

// CROSS-MODULE EDIT: 2025-01-11T17:15:00-06:00 Task 10
// Modified for: Add SupportedFileTypes metadata for dynamic file type discovery in File Management System
// Related modules: Core (IModule consumers: ModuleLoader, ModuleCatalog), future Map/Project/Spec modules
// Rollback instructions: Remove SupportedFileTypes property & associated using directive; delete FileTypeDefinition record file.
