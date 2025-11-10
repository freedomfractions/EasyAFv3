using System.Windows.Media;
using Unity;

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
