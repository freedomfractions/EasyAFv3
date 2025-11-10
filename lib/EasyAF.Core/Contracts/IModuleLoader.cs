using System.Collections.Generic;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Discovers and loads EasyAF modules via reflection and registers them with the module catalog.
/// </summary>
public interface IModuleLoader
{
    /// <summary>
    /// Occurs when a module has been successfully loaded and registered.
    /// </summary>
    event EventHandler<IModule>? ModuleLoaded;

    /// <summary>
    /// Returns the collection of loaded modules.
    /// </summary>
    IEnumerable<IModule> LoadedModules { get; }

    /// <summary>
    /// Discovers and loads modules from a search path. If null, uses default module probing (Modules folder & loaded assemblies).
    /// </summary>
    /// <param name="searchPath">Optional directory to search for module assemblies.</param>
    void DiscoverAndLoadModules(string? searchPath = null);
}
