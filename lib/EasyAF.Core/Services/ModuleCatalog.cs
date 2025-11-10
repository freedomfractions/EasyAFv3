using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Core.Services;

/// <summary>
/// Implementation of the module catalog for managing module registration and discovery.
/// </summary>
public class ModuleCatalog : IModuleCatalog
{
    private readonly List<IModule> _modules;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleCatalog"/> class.
    /// </summary>
    public ModuleCatalog()
    {
        _modules = new List<IModule>();
    }

    /// <inheritdoc/>
    public IReadOnlyList<IModule> Modules
    {
        get
        {
            lock (_lockObject)
            {
                return _modules.ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IDocumentModule> DocumentModules
    {
        get
        {
            lock (_lockObject)
            {
                return _modules.OfType<IDocumentModule>().ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc/>
    public void RegisterModule(IModule module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        lock (_lockObject)
        {
            if (_modules.Any(m => m.ModuleName.Equals(module.ModuleName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A module with the name '{module.ModuleName}' is already registered.");
            }

            _modules.Add(module);
            Log.Information("Module registered: {ModuleName} v{ModuleVersion}", module.ModuleName, module.ModuleVersion);
        }
    }

    /// <inheritdoc/>
    public bool UnregisterModule(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            return false;

        lock (_lockObject)
        {
            var module = _modules.FirstOrDefault(m => m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            if (module == null)
                return false;

            try
            {
                module.Shutdown();
                Log.Information("Module shutdown: {ModuleName}", module.ModuleName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error shutting down module: {ModuleName}", module.ModuleName);
            }

            _modules.Remove(module);
            Log.Information("Module unregistered: {ModuleName}", module.ModuleName);
            return true;
        }
    }

    /// <inheritdoc/>
    public IModule? GetModule(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            return null;

        lock (_lockObject)
        {
            return _modules.FirstOrDefault(m => m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <inheritdoc/>
    public IDocumentModule? FindModuleForFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        lock (_lockObject)
        {
            return _modules
                .OfType<IDocumentModule>()
                .FirstOrDefault(m => m.CanHandleFile(filePath));
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IDocumentModule> GetModulesByExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return Enumerable.Empty<IDocumentModule>();

        // Normalize extension (remove leading dot if present)
        var normalizedExtension = extension.TrimStart('.');

        lock (_lockObject)
        {
            return _modules
                .OfType<IDocumentModule>()
                .Where(m => m.SupportedFileExtensions.Any(ext => 
                    ext.Equals(normalizedExtension, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
