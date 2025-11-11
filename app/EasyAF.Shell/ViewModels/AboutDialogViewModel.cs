using Prism.Mvvm;
using System.Windows.Input;
using Prism.Commands;
using System.Reflection;
using System.Collections.ObjectModel;
using System.IO;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for About dialog.
/// </summary>
public class AboutDialogViewModel : BindableBase
{
    private readonly EasyAF.Core.Contracts.IModuleCatalog _catalog;

    public AboutDialogViewModel(EasyAF.Core.Contracts.IModuleCatalog catalog)
    {
        _catalog = catalog;
        CloseCommand = new DelegateCommand(() => DialogResult = true);
        LoadVersionInfo();
        LoadModules();
    }

    /// <summary>
    /// Application version string (e.g., "v1.0.0.0")
    /// </summary>
    public string Version { get; private set; } = "v1.0.0.0";

    /// <summary>
    /// Build date formatted as "yyyy-MM-dd HH:mm"
    /// </summary>
    public string BuildDate { get; private set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

    /// <summary>
    /// Build number from assembly version
    /// </summary>
    public string BuildNumber { get; private set; } = "0";

    /// <summary>
    /// .NET runtime version
    /// </summary>
    public string DotNetVersion { get; private set; } = Environment.Version.ToString();

    /// <summary>
    /// Collection of loaded modules for display
    /// </summary>
    public ObservableCollection<EasyAF.Core.Contracts.IModule> LoadedModules { get; } = new();

    public ICommand CloseCommand { get; }

    private bool? _dialogResult;
    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    private void LoadVersionInfo()
    {
        try
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            Version = ver != null ? $"v{ver}" : "v1.0.0.0";
            
            var location = asm.Location;
            DateTime buildDate = DateTime.Now;
            try
            {
                buildDate = File.GetLastWriteTime(location);
            }
            catch { /* Use current time as fallback */ }
            
            BuildDate = buildDate.ToString("yyyy-MM-dd HH:mm");
            BuildNumber = ver?.Build.ToString() ?? "0";
            DotNetVersion = $".NET {Environment.Version}";
        }
        catch
        {
            // Fallback to defaults if reflection fails
        }
    }

    private void LoadModules()
    {
        LoadedModules.Clear();
        foreach (var module in _catalog.Modules)
        {
            LoadedModules.Add(module);
        }
    }
}
