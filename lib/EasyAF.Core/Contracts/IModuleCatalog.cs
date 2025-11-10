namespace EasyAF.Core.Contracts;

/// <summary>
/// Service for managing module registration and discovery.
/// </summary>
/// <remarks>
/// The module catalog maintains a registry of all available modules
/// and provides methods for module lookup and enumeration.
/// </remarks>
public interface IModuleCatalog
{
    /// <summary>
    /// Gets all registered modules.
    /// </summary>
    /// <value>
    /// Read-only collection of all modules loaded by the application.
    /// </value>
    IReadOnlyList<IModule> Modules { get; }

    /// <summary>
    /// Gets all registered document modules.
    /// </summary>
    /// <value>
    /// Read-only collection of modules that implement IDocumentModule.
    /// </value>
    IReadOnlyList<IDocumentModule> DocumentModules { get; }

    /// <summary>
    /// Registers a module with the catalog.
    /// </summary>
    /// <param name="module">The module to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when module is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a module with the same name is already registered.</exception>
    /// <remarks>
    /// Modules must be registered before they can be discovered by the shell.
    /// </remarks>
    void RegisterModule(IModule module);

    /// <summary>
    /// Unregisters a module from the catalog.
    /// </summary>
    /// <param name="moduleName">The name of the module to unregister.</param>
    /// <returns>True if the module was found and removed; otherwise, false.</returns>
    /// <remarks>
    /// Calls Shutdown() on the module before removing it from the catalog.
    /// </remarks>
    bool UnregisterModule(string moduleName);

    /// <summary>
    /// Gets a module by its name.
    /// </summary>
    /// <param name="moduleName">The unique name of the module.</param>
    /// <returns>The module instance, or null if not found.</returns>
    IModule? GetModule(string moduleName);

    /// <summary>
    /// Finds the document module that can handle the specified file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>The first module that can handle the file, or null if none found.</returns>
    /// <remarks>
    /// Queries all document modules using their CanHandleFile method.
    /// Returns the first match found.
    /// </remarks>
    IDocumentModule? FindModuleForFile(string filePath);

    /// <summary>
    /// Gets all modules that support the specified file extension.
    /// </summary>
    /// <param name="extension">The file extension (without dot).</param>
    /// <returns>Collection of modules that support this extension.</returns>
    IEnumerable<IDocumentModule> GetModulesByExtension(string extension);
}
