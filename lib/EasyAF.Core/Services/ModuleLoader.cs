using System.Reflection;
using System.IO;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Core.Services;

/// <summary>
/// Reflection-based module loader that locates types implementing IModule and registers them with the catalog.
/// </summary>
public class ModuleLoader : IModuleLoader
{
    private readonly IModuleCatalog _catalog;
    private readonly ISettingsService _settings;
    private readonly List<IModule> _loaded = new();
    private Unity.IUnityContainer? _container; // Store container reference

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleLoader"/> class.
    /// </summary>
    public ModuleLoader(IModuleCatalog catalog, ISettingsService settings)
    {
        _catalog = catalog;
        _settings = settings;
    }

    /// <inheritdoc />
    public event EventHandler<IModule>? ModuleLoaded;

    /// <inheritdoc />
    public IEnumerable<IModule> LoadedModules => _loaded;

    /// <summary>
    /// Sets the Unity container for module initialization.
    /// </summary>
    /// <param name="container">The Unity container.</param>
    public void SetContainer(Unity.IUnityContainer container)
    {
        _container = container;
    }

    /// <inheritdoc />
    public void DiscoverAndLoadModules(string? searchPath = null)
    {
        try
        {
            var assemblies = new List<Assembly>();

            // 1. Use already loaded application domain assemblies first
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic));

            // 2. Optionally probe a Modules folder on disk
            if (string.IsNullOrWhiteSpace(searchPath))
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                searchPath = Path.Combine(baseDir, "Modules");
            }

            if (Directory.Exists(searchPath))
            {
                foreach (var file in Directory.GetFiles(searchPath, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(file);
                        assemblies.Add(asm);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to load module assembly from {File}", file);
                    }
                }
            }

            // Discover IModule implementations
            foreach (var asm in assemblies.Distinct())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    types = rtle.Types.Where(t => t != null).ToArray()!;
                }
                catch
                {
                    continue; // Skip bad assembly
                }

                foreach (var t in types)
                {
                    if (t == null || t.IsAbstract || t.IsInterface) continue;
                    if (!typeof(IModule).IsAssignableFrom(t)) continue;

                    try
                    {
                        // Create instance
                        var module = (IModule)Activator.CreateInstance(t)!;

                        // Initialize with container (if available)
                        if (_container != null)
                        {
                            module.Initialize(_container);
                        }
                        else
                        {
                            Log.Warning("Module {ModuleName} initialized without container - services may not be available", module.ModuleName);
                            module.Initialize(null!); // Fallback for early initialization
                        }

                        _catalog.RegisterModule(module);
                        _loaded.Add(module);

                        Log.Information("Module loaded: {ModuleName} v{Version}", module.ModuleName, module.ModuleVersion);
                        ModuleLoaded?.Invoke(this, module);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to load module type {TypeName} from {Assembly}", t.FullName, asm.GetName().Name);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception during module discovery");
        }
    }
}
